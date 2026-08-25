using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using ct._Core.Repo.Resource;
using Repo.Resource;
using Repo.Dto;
using ct._Core.Util;
using Repo.Db.Index;

namespace ct._Core.Persistence;

public interface IPostgresSession : IDisposable
{
    T? Load<T>(string id);
    IEnumerable<T> Load<T>(IEnumerable<string> ids);
    void Store<T>(T entity);
    void SaveChanges();
    PostgresQuery<T> Query<T>();
    PostgresQuery<T> Query<T, TIndex>();
    PostgresDocumentQuery<T, TIndex> DocumentQuery<T, TIndex>();
    PostgresAdvancedOperations Advanced { get; }
}

public sealed class PostgresDocumentSession : IPostgresSession
{
    private readonly NpgsqlConnection _connection;
    private readonly List<object> _pending = new();
    private bool _disposed;

    public PostgresDocumentSession(string connectionString)
    {
        _connection = new NpgsqlConnection(connectionString);
        _connection.Open();
    }

    public PostgresAdvancedOperations Advanced => new(this);

    public T? Load<T>(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return default;
        var typeName = TypeName(typeof(T));
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT data FROM ct_documents WHERE document_type = @type AND id = @id";
        cmd.Parameters.AddWithValue("type", typeName);
        cmd.Parameters.AddWithValue("id", id);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return default;
        return Deserialize<T>(reader.GetString(0));
    }

    public IEnumerable<T> Load<T>(IEnumerable<string> ids)
    {
        var wanted = ids?.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray() ?? Array.Empty<string>();
        if (wanted.Length == 0) return Array.Empty<T>();
        var typeName = TypeName(typeof(T));
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT data FROM ct_documents WHERE document_type = @type AND id = ANY(@ids)";
        cmd.Parameters.AddWithValue("type", typeName);
        cmd.Parameters.AddWithValue("ids", wanted);
        var result = new List<T>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) result.Add(Deserialize<T>(reader.GetString(0))!);
        return result;
    }

    public void Store<T>(T entity)
    {
        if (entity is null) return;
        PrepareIdentity(entity);
        _pending.Add(entity!);
    }

    public void SaveChanges()
    {
        if (_pending.Count == 0) return;
        using var tx = _connection.BeginTransaction();
        foreach (var entity in _pending)
        {
            var type = entity.GetType();
            var id = GetProperty(entity, "Id")?.ToString();
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString();
                SetProperty(entity, "Id", id);
            }

            ApplyAudit(entity);
            var json = JsonConvert.SerializeObject(entity, JsonSettings);
            var owner = GetProperty(entity, "OwnerId")?.ToString();
            var parent = GetProperty(entity, "ParentId")?.ToString();
            var created = GetNullableDate(entity, "CreatedOn");
            var modified = GetNullableDate(entity, "ModifiedOn");

            using var cmd = _connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO ct_documents(document_type,id,owner_id,parent_id,created_on,modified_on,data,updated_at)
                VALUES(@type,@id,@owner,@parent,@created,@modified,@data::jsonb,now())
                ON CONFLICT(document_type,id) DO UPDATE SET
                    owner_id=EXCLUDED.owner_id,
                    parent_id=EXCLUDED.parent_id,
                    created_on=EXCLUDED.created_on,
                    modified_on=EXCLUDED.modified_on,
                    data=EXCLUDED.data,
                    updated_at=now();
                """;
            cmd.Parameters.AddWithValue("type", TypeName(type));
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("owner", (object?)owner ?? DBNull.Value);
            cmd.Parameters.AddWithValue("parent", (object?)parent ?? DBNull.Value);
            cmd.Parameters.AddWithValue("created", (object?)created ?? DBNull.Value);
            cmd.Parameters.AddWithValue("modified", (object?)modified ?? DBNull.Value);
            cmd.Parameters.AddWithValue("data", json);
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
        _pending.Clear();
    }

    public PostgresQuery<T> Query<T>()
    {
        return new PostgresQuery<T>(this, LoadAll(typeof(T), typeof(T)), null);
    }

    public PostgresQuery<T> Query<T, TIndex>()
    {
        var rows = PostgresIndexFactory.Build<T, TIndex>(this);
        return new PostgresQuery<T>(this, rows, typeof(TIndex));
    }

    public PostgresDocumentQuery<T, TIndex> DocumentQuery<T, TIndex>()
        => new(PostgresIndexFactory.Build<T, TIndex>(this), this);

    internal List<PostgresRow<T>> LoadAll<T>() => LoadAll(typeof(T), typeof(T)).Cast<PostgresRow<T>>().ToList();

    internal IEnumerable<PostgresRow<T>> LoadAll(Type requestedType, Type indexType)
    {
        var typeName = TypeName(requestedType);
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT data FROM ct_documents WHERE document_type = @type ORDER BY id";
        cmd.Parameters.AddWithValue("type", typeName);
        using var reader = cmd.ExecuteReader();
        var result = new List<PostgresRow<T>>();
        while (reader.Read())
        {
            var value = Deserialize(requestedType, reader.GetString(0));
            result.Add(new PostgresRow<T>((T)value!, value!));
        }
        return result;
    }

    internal IEnumerable<object> LoadRaw(string typeName)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT data FROM ct_documents WHERE document_type = @type ORDER BY id";
        cmd.Parameters.AddWithValue("type", typeName);
        using var reader = cmd.ExecuteReader();
        var result = new List<object>();
        while (reader.Read()) result.Add(JObject.Parse(reader.GetString(0)));
        return result;
    }

    internal static string TypeName(Type type) => type.FullName?.Replace('+', '.') ?? type.Name;

    private static object? Deserialize(Type type, string json)
    {
        if (type == typeof(object)) return JObject.Parse(json);
        return JsonConvert.DeserializeObject(json, type, JsonSettings);
    }

    private static T? Deserialize<T>(string json) => (T?)Deserialize(typeof(T), json);

    private static object? GetProperty(object entity, string name)
        => entity.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)?.GetValue(entity);

    private static void SetProperty(object entity, string name, object value)
        => entity.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)?.SetValue(entity, value);

    private static DateTime? GetNullableDate(object entity, string name)
    {
        var value = GetProperty(entity, name);
        if (value is DateTime dt) return dt == DateTime.MinValue ? null : dt;
        return null;
    }

    private static void PrepareIdentity(object entity)
    {
        if (entity is Entity e && string.IsNullOrWhiteSpace(e.Id)) e.SetId();
        else
        {
            var id = GetProperty(entity, "Id")?.ToString();
            if (string.IsNullOrWhiteSpace(id)) SetProperty(entity, "Id", Guid.NewGuid().ToString());
        }
    }

    private static void ApplyAudit(object entity)
    {
        if (entity is not Entity e || e.Audited) return;
        var now = DateTime.UtcNow;
        if (e.CreatedOn == DateTime.MinValue)
            e.CreatedOn = now;
        else
            e.ModifiedOn = now;
    }

    internal void ClearSessionState() => _pending.Clear();

    internal static readonly JsonSerializerSettings JsonSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Include,
        TypeNameHandling = TypeNameHandling.Auto,
        DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _connection.Dispose();
    }
}

public sealed record PostgresRow<T>(T Value, object Document);

public sealed class PostgresAdvancedOperations
{
    private readonly PostgresDocumentSession _session;
    internal PostgresAdvancedOperations(PostgresDocumentSession session) => _session = session;
    public PostgresDocumentQuery<T, object> DocumentQuery<T>() => new(_session.LoadAll<T>(), _session);
    public PostgresDocumentQuery<T, TIndex> DocumentQuery<T, TIndex>()
        => new(PostgresIndexFactory.Build<T, TIndex>(_session), _session);
    public PostgresStream<T> Stream<T>(IEnumerable<T> query) => new(query);
    public void Clear() => _session.ClearSessionState();
    public Dictionary<string, object> ExternalState { get; } = new();
}

public sealed class PostgresStream<T> : IDisposable
{
    private readonly IEnumerator<T> _enumerator;
    public PostgresStream(IEnumerable<T> query) => _enumerator = query.GetEnumerator();
    public bool MoveNext() => _enumerator.MoveNext();
    public PostgresStreamItem<T> Current => new(_enumerator.Current);
    public void Dispose() => _enumerator.Dispose();
}

public sealed record PostgresStreamItem<T>(T Document);
