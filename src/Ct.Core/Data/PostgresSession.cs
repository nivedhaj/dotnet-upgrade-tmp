using Npgsql;
using NpgsqlTypes;
using Newtonsoft.Json;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using ct._Core.Repo.Resource;
using ct._Core.Util;

namespace ct._Core.Data;

/// <summary>
/// PostgreSQL-backed persistence abstraction used by the Campus modules.
/// It deliberately keeps the existing aggregate/document programming model for complex
/// objects while moving persistence, projections and cross-module summaries to PostgreSQL.
/// </summary>
public interface IPostgresSession : IDisposable
{
    IQueryable<T> Query<T>();
    IQueryable<T> Query<T, TIndex>();
    T? Load<T>(string id);
    List<T> Load<T>(IEnumerable<string> ids);
    void Store<T>(T entity);
    Task StoreAsync<T>(T entity);
    void SaveChanges();
    Task SaveChangesAsync();
    PostgresAdvancedSession Advanced { get; }
    string UserId { get; set; }
}

public sealed class QueryStatistics
{
    public int TotalResults { get; internal set; }
}

public sealed class PostgresStreamResult<T>
{
    public T Document { get; init; } = default!;
}

public sealed class PostgresStreamEnumerator<T> : IDisposable
{
    private readonly IEnumerator<T> _enumerator;
    public PostgresStreamResult<T> Current => new() { Document = _enumerator.Current };
    public PostgresStreamEnumerator(IEnumerable<T> source) => _enumerator = source.GetEnumerator();
    public bool MoveNext() => _enumerator.MoveNext();
    public void Dispose() => _enumerator.Dispose();
}

public sealed class PostgresAdvancedSession
{
    private readonly PostgresSession _session;
    internal PostgresAdvancedSession(PostgresSession session) => _session = session;

    public PostgresStreamEnumerator<T> Stream<T>(IEnumerable<T> query) => new(query);

    public PostgresDocumentQuery<T> DocumentQuery<T, TIndex>() =>
        new(_session, _session.Query<T, TIndex>());

    public Dictionary<string, object> ExternalState { get; } = new();

    public void Clear() { }
}

public sealed class PostgresDocumentQuery<T> : IEnumerable<T>
{
    private readonly PostgresSession _session;
    private IQueryable<T> _query;

    internal PostgresDocumentQuery(PostgresSession session, IQueryable<T> query)
    {
        _session = session;
        _query = query;
    }

    public PostgresDocumentQuery<T> WhereLucene(string field, string predicate)
    {
        _query = PostgresQueryExtensions.ApplyLucene(_query, predicate);
        return this;
    }

    public PostgresDocumentQuery<T> WhereEquals(string field, object value)
    {
        _query = _query.Where(x => PostgresQueryExtensions.GetPropertyValue(x, field)?.ToString() == value?.ToString());
        return this;
    }

    public IEnumerator<T> GetEnumerator() => _query.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public List<T> ToList() => _query.ToList();
    public int Count() => _query.Count();

    // Kept as an instance method so existing legacy ".OfType<T>()" calls on
    // DocumentQuery continue to compile without a document-database dependency.
    public IEnumerable<TResult> OfType<TResult>() => _query.OfType<TResult>();
}

public static class PostgresQueryExtensions
{
    public static IQueryable<T> Statistics<T>(this IQueryable<T> query, out QueryStatistics statistics)
    {
        var materialized = query.ToList();
        statistics = new QueryStatistics { TotalResults = materialized.Count };
        return materialized.AsQueryable();
    }

    public static IQueryable<T> Search<T, TValue>(
        this IQueryable<T> query,
        Expression<Func<T, TValue>> selector,
        string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return query;
        var getter = selector.Compile();
        var terms = searchTerm.Trim().Trim('"').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return query.AsEnumerable()
            .Where(x =>
            {
                var value = getter(x)?.ToString() ?? string.Empty;
                return terms.All(term => value.Contains(term.TrimEnd('*'), StringComparison.OrdinalIgnoreCase));
            })
            .AsQueryable();
    }

    internal static object? GetPropertyValue(object? item, string property)
    {
        if (item == null || string.IsNullOrWhiteSpace(property)) return null;
        return item.GetType().GetProperty(property,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(item);
    }

    internal static IQueryable<T> ApplyLucene<T>(IQueryable<T> query, string predicate)
    {
        if (string.IsNullOrWhiteSpace(predicate)) return query;

        var matches = Regex.Matches(predicate, @"(?<field>[A-Za-z]+):(?<value>[^\s\)\(]+)")
            .Select(m => (field: m.Groups["field"].Value, value: m.Groups["value"].Value.Trim('"').TrimEnd('*')))
            .Where(x => !string.IsNullOrWhiteSpace(x.value))
            .ToList();

        if (matches.Count == 0) return query;

        var groups = matches.GroupBy(x => x.field, StringComparer.OrdinalIgnoreCase).ToList();
        return query.AsEnumerable().Where(item =>
        {
            foreach (var group in groups)
            {
                // Search predicates such as Name:x OR StudentId:x OR FatherName:x are represented
                // as separate fields. Within one field, multiple values are OR; different
                // constrained fields are AND.
                var actual = GetPropertyValue(item, group.Key)?.ToString() ?? string.Empty;
                if (group.Any(x => actual.Contains(x.value, StringComparison.OrdinalIgnoreCase)))
                    continue;

                // Tags and email-list fields are arrays/lists; ToString() isn't useful there.
                var property = item?.GetType().GetProperty(group.Key,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.GetValue(item);
                if (property is IEnumerable<string> strings &&
                    group.Any(x => strings.Any(s => s.Contains(x.value, StringComparison.OrdinalIgnoreCase))))
                    continue;

                return false;
            }
            return true;
        }).AsQueryable();
    }
}

public sealed class PostgresSession : IPostgresSession
{
    private readonly string _connectionString;
    private bool _disposed;

    private static readonly Dictionary<string, string> IndexViews = new(StringComparer.Ordinal)
    {
        ["Student_Inst_Index"] = "campus.v_student_inst_index",
        ["Student_Coursewise_Count"] = "campus.v_student_coursewise_count",
        ["Inst_Student_Count"] = "campus.v_inst_student_count",
        ["AttendanceEvent_IX"] = "campus.v_attendance_event_index",
        ["Student_Essentials_IX"] = "campus.v_student_essentials_index",
        ["Ping_Index"] = "campus.v_ping_index"
    };

    public PostgresAdvancedSession Advanced { get; }
    public string UserId { get; set; } = string.Empty;

    public PostgresSession(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Campus")
            ?? configuration["Postgres:ConnectionString"]
            ?? throw new InvalidOperationException("Postgres connection string 'Campus' is not configured.");
        Advanced = new PostgresAdvancedSession(this);
    }

    private NpgsqlConnection OpenConnection()
    {
        var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "select set_config('app.user_id', @user_id, true)";
        cmd.Parameters.AddWithValue("user_id", UserId ?? string.Empty);
        cmd.ExecuteNonQuery();
        return connection;
    }

    public IQueryable<T> Query<T>()
    {
        var type = typeof(T);
        if (type.Name == "FeesReceivable")
            return ReadRows<T>("select row_to_json(t)::text from (select \"Id\",\"StudentId\",\"InstId\",\"FeeId\",\"FeeName\",\"TxId\",\"TxStatus\",\"Amount\",\"OwnerId\",\"ParentId\",\"CreatedOn\",\"CreatedBy\",\"ModifiedOn\",\"ModifiedBy\" from campus.fees_receivables) t").AsQueryable();

        if (type.Name == "OpeningBalance")
            return ReadRows<T>("select row_to_json(t)::text from (select \"Id\",\"StudentId\",\"InstId\",\"FeeId\",\"FeeName\",\"Amount\",\"OwnerId\",\"ParentId\",\"CreatedOn\",\"CreatedBy\",\"ModifiedOn\",\"ModifiedBy\" from campus.opening_balances) t").AsQueryable();

        if (type.Name == "StudentFeeSummary")
            return ReadRows<T>("select row_to_json(v)::text from campus.v_student_fee_summary v").AsQueryable();

        var typeName = type.FullName ?? type.Name;
        var simpleName = type.Name;
        var sql = """
            select payload::text
            from campus.documents
            where type_name = @type_name or type_name = @simple_name
            """;
        return ReadPayloads<T>(sql, ("type_name", typeName), ("simple_name", simpleName)).AsQueryable();
    }

    public IQueryable<T> Query<T, TIndex>()
    {
        var indexName = typeof(TIndex).Name;
        if (!IndexViews.TryGetValue(indexName, out var view))
            throw new InvalidOperationException($"No PostgreSQL view is registered for index '{indexName}'.");

        return ReadRows<T>($"select row_to_json(v)::text from {view} v").AsQueryable();
    }

    private List<T> ReadPayloads<T>(string sql, params (string Name, object Value)[] parameters)
    {
        var result = new List<T>();
        using var connection = OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        foreach (var p in parameters) cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (reader.IsDBNull(0)) continue;
            var json = reader.GetString(0);
            result.Add(JsonConvert.DeserializeObject<T>(json)!);
        }
        return result;
    }

    private List<T> ReadRows<T>(string sql, params (string Name, object Value)[] parameters)
    {
        var result = new List<T>();
        using var connection = OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        foreach (var p in parameters) cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (reader.IsDBNull(0)) continue;
            var json = reader.GetString(0);

            var item = JsonConvert.DeserializeObject<T>(json)!;
            if (typeof(T).FullName?.Contains("Student_Inst_Index+Result", StringComparison.Ordinal) == true)
            {
                var row = Newtonsoft.Json.Linq.JObject.Parse(json);
                var payload = row["Payload"]?.ToString();
                if (!string.IsNullOrWhiteSpace(payload))
                    JsonConvert.PopulateObject(payload, item!);
                JsonConvert.PopulateObject(json, item!);
            }
            result.Add(item);
        }
        return result;
    }

    public T? Load<T>(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return default;

        if (typeof(T).Name == "FeesReceivable")
            return ReadOne<T>("select row_to_json(t)::text from (select \"Id\",\"StudentId\",\"InstId\",\"FeeId\",\"FeeName\",\"TxId\",\"TxStatus\",\"Amount\",\"OwnerId\",\"ParentId\",\"CreatedOn\",\"CreatedBy\",\"ModifiedOn\",\"ModifiedBy\" from campus.fees_receivables where \"Id\"=@id) t", ("id", id));

        if (typeof(T).Name == "OpeningBalance")
            return ReadOne<T>("select row_to_json(t)::text from (select \"Id\",\"StudentId\",\"InstId\",\"FeeId\",\"FeeName\",\"Amount\",\"OwnerId\",\"ParentId\",\"CreatedOn\",\"CreatedBy\",\"ModifiedOn\",\"ModifiedBy\" from campus.opening_balances where \"Id\"=@id) t", ("id", id));

        // Dynamic loads are used by UserContext for client/persona documents.
        var sql = "select payload::text from campus.documents where id=@id limit 1";
        return ReadOne<T>(sql, ("id", id));
    }

    public List<T> Load<T>(IEnumerable<string> ids)
    {
        var wanted = ids?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList() ?? new();
        if (wanted.Count == 0) return new();

        if (typeof(T).Name == "FeesReceivable")
            return Query<T>().Where(x => wanted.Contains((string?)GetPropertyValue(x, "Id"))).ToList();

        if (typeof(T).Name == "OpeningBalance")
            return Query<T>().Where(x => wanted.Contains((string?)GetPropertyValue(x, "Id"))).ToList();

        var result = new List<T>();
        using var connection = OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "select payload::text from campus.documents where id = any(@ids)";
        cmd.Parameters.AddWithValue("ids", wanted.ToArray());
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            if (!reader.IsDBNull(0))
                result.Add(JsonConvert.DeserializeObject<T>(reader.GetString(0))!);
        }
        return result;
    }

    private T? ReadOne<T>(string sql, params (string Name, object Value)[] parameters)
    {
        using var connection = OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        foreach (var p in parameters) cmd.Parameters.AddWithValue(p.Name, p.Value ?? DBNull.Value);
        var value = cmd.ExecuteScalar() as string;
        return string.IsNullOrEmpty(value) ? default : JsonConvert.DeserializeObject<T>(value);
    }

    public void Store<T>(T entity)
    {
        if (entity == null) return;

        if (entity.GetType().Name == "FeesReceivable")
        {
            UpsertFee(entity);
            return;
        }

        if (entity.GetType().Name == "OpeningBalance")
        {
            UpsertOpeningBalance(entity);
            return;
        }

        var idProp = entity.GetType().GetProperty("Id");
        var id = idProp?.GetValue(entity)?.ToString();
        if (string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString();
            idProp?.SetValue(entity, id);
        }

        var owner = entity.GetType().GetProperty("OwnerId")?.GetValue(entity)?.ToString() ?? string.Empty;
        var parent = entity.GetType().GetProperty("ParentId")?.GetValue(entity)?.ToString() ?? string.Empty;
        var createdOn = entity.GetType().GetProperty("CreatedOn")?.GetValue(entity) as DateTime? ?? DateTime.MinValue;
        var createdBy = entity.GetType().GetProperty("CreatedBy")?.GetValue(entity)?.ToString() ?? string.Empty;
        var modifiedOn = entity.GetType().GetProperty("ModifiedOn")?.GetValue(entity) as DateTime?;
        var modifiedBy = entity.GetType().GetProperty("ModifiedBy")?.GetValue(entity)?.ToString() ?? string.Empty;
        var payload = JsonConvert.SerializeObject(entity);

        using var connection = OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            insert into campus.documents
                (id,type_name,owner_id,parent_id,payload,created_on,created_by,modified_on,modified_by)
            values
                (@id,@type_name,@owner_id,@parent_id,@payload::jsonb,@created_on,@created_by,@modified_on,@modified_by)
            on conflict (id) do update set
                type_name=excluded.type_name,
                owner_id=excluded.owner_id,
                parent_id=excluded.parent_id,
                payload=excluded.payload,
                modified_on=excluded.modified_on,
                modified_by=excluded.modified_by
            """;
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("type_name", entity.GetType().FullName ?? entity.GetType().Name);
        cmd.Parameters.AddWithValue("owner_id", owner);
        cmd.Parameters.AddWithValue("parent_id", parent);
        cmd.Parameters.AddWithValue("payload", NpgsqlDbType.Jsonb, payload);
        cmd.Parameters.AddWithValue("created_on", createdOn == DateTime.MinValue ? DBNull.Value : createdOn);
        cmd.Parameters.AddWithValue("created_by", createdBy);
        cmd.Parameters.AddWithValue("modified_on", modifiedOn ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("modified_by", modifiedBy);
        cmd.ExecuteNonQuery();
    }

    private void UpsertFee(object entity)
    {
        var id = GetString(entity, "Id");
        if (string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString();
            SetProperty(entity, "Id", id);
        }

        using var connection = OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            insert into campus.fees_receivables
                ("Id","StudentId","InstId","FeeId","FeeName","TxId","TxStatus","Amount","OwnerId","ParentId","CreatedOn","CreatedBy","ModifiedOn","ModifiedBy")
            values (@id,@student_id,@inst_id,@fee_id,@fee_name,@tx_id,@status,@amount,@owner_id,@parent_id,@created_on,@created_by,@modified_on,@modified_by)
            on conflict ("Id") do update set
                "StudentId"=excluded."StudentId","InstId"=excluded."InstId","FeeId"=excluded."FeeId",
                "FeeName"=excluded."FeeName","TxId"=excluded."TxId","TxStatus"=excluded."TxStatus",
                "Amount"=excluded."Amount","ModifiedOn"=excluded."ModifiedOn","ModifiedBy"=excluded."ModifiedBy"
            """;
        AddEntityParameters(cmd, entity);
        cmd.ExecuteNonQuery();
    }

    private void UpsertOpeningBalance(object entity)
    {
        var id = GetString(entity, "Id");
        if (string.IsNullOrWhiteSpace(id))
        {
            id = Guid.NewGuid().ToString();
            SetProperty(entity, "Id", id);
        }

        using var connection = OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            insert into campus.opening_balances
                ("Id","StudentId","InstId","FeeId","FeeName","Amount","OwnerId","ParentId","CreatedOn","CreatedBy","ModifiedOn","ModifiedBy")
            values (@id,@student_id,@inst_id,@fee_id,@fee_name,@amount,@owner_id,@parent_id,@created_on,@created_by,@modified_on,@modified_by)
            on conflict ("Id") do update set
                "StudentId"=excluded."StudentId","InstId"=excluded."InstId",
                "Amount"=excluded."Amount","ModifiedOn"=excluded."ModifiedOn","ModifiedBy"=excluded."ModifiedBy"
            """;
        AddOpeningParameters(cmd, entity);
        cmd.ExecuteNonQuery();
    }

    private static void AddEntityParameters(NpgsqlCommand cmd, object entity)
    {
        var type = entity.GetType();
        object Value(string name) => type.GetProperty(name)?.GetValue(entity) ?? string.Empty;

        cmd.Parameters.AddWithValue("id", Value("Id"));
        cmd.Parameters.AddWithValue("student_id", Value("StudentId"));
        cmd.Parameters.AddWithValue("inst_id", Value("InstId"));
        cmd.Parameters.AddWithValue("fee_id", Value("FeeId"));
        cmd.Parameters.AddWithValue("fee_name", Value("FeeName"));
        cmd.Parameters.AddWithValue("tx_id", Value("TxId"));
        var status = Value("TxStatus");
        cmd.Parameters.AddWithValue("status", status is Enum e ? Convert.ToInt32(e) : Convert.ToInt32(status));
        cmd.Parameters.AddWithValue("amount", Value("Amount"));
        AddAuditParameters(cmd, entity);
    }

    private static void AddOpeningParameters(NpgsqlCommand cmd, object entity)
    {
        var type = entity.GetType();
        object Value(string name) => type.GetProperty(name)?.GetValue(entity) ?? string.Empty;

        cmd.Parameters.AddWithValue("id", Value("Id"));
        cmd.Parameters.AddWithValue("student_id", Value("StudentId"));
        cmd.Parameters.AddWithValue("inst_id", Value("InstId"));
        cmd.Parameters.AddWithValue("fee_id", Value("FeeId"));
        cmd.Parameters.AddWithValue("fee_name", Value("FeeName"));
        cmd.Parameters.AddWithValue("amount", Value("Amount"));
        AddAuditParameters(cmd, entity);
    }

    private static void AddAuditParameters(NpgsqlCommand cmd, object entity)
    {
        var type = entity.GetType();
        object Value(string name) => type.GetProperty(name)?.GetValue(entity) ?? string.Empty;
        var createdOn = Value("CreatedOn");
        cmd.Parameters.AddWithValue("owner_id", Value("OwnerId"));
        cmd.Parameters.AddWithValue("parent_id", Value("ParentId"));
        cmd.Parameters.AddWithValue("created_on", createdOn is DateTime dt && dt != DateTime.MinValue ? dt : DBNull.Value);
        cmd.Parameters.AddWithValue("created_by", Value("CreatedBy"));
        var modifiedOn = Value("ModifiedOn");
        cmd.Parameters.AddWithValue("modified_on", modifiedOn is DateTime modifiedDate ? modifiedDate : DBNull.Value);
        cmd.Parameters.AddWithValue("modified_by", Value("ModifiedBy"));
    }

    private static string? GetString(object entity, string property) =>
        entity.GetType().GetProperty(property)?.GetValue(entity)?.ToString();

    private static void SetProperty(object entity, string property, object value) =>
        entity.GetType().GetProperty(property)?.SetValue(entity, value);

    public Task StoreAsync<T>(T entity)
    {
        Store(entity);
        return Task.CompletedTask;
    }

    public void SaveChanges() { }
    public Task SaveChangesAsync() => Task.CompletedTask;

    public void Dispose()
    {
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
