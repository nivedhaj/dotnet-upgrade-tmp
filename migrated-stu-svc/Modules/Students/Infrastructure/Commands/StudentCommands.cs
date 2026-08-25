using System.Text.Json;
using Npgsql;

namespace ct.Modules.Students.Infrastructure.Commands;

/// <summary>
/// PostgreSQL-native write operations for the Student module.
/// Domain-specific commands should be added here instead of calling a document session.
/// </summary>
public sealed class StudentCommands
{
    private readonly NpgsqlDataSource _dataSource;

    public StudentCommands(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task UpsertDocumentAsync(
        Guid id,
        string json,
        string? ownerId = null,
        string? parentId = null,
        CancellationToken cancellationToken = default)
    {
        using var document = JsonDocument.Parse(json);
        var normalizedJson = document.RootElement.GetRawText();

        const string sql = """
            INSERT INTO ct_documents
                (document_type, id, owner_id, parent_id, data, updated_at)
            VALUES
                ('Student', @id, @owner_id, @parent_id, @data::jsonb, now())
            ON CONFLICT (document_type, id)
            DO UPDATE SET
                owner_id = EXCLUDED.owner_id,
                parent_id = EXCLUDED.parent_id,
                data = EXCLUDED.data,
                updated_at = now();
            """;

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id.ToString());
        command.Parameters.AddWithValue("owner_id", (object?)ownerId ?? DBNull.Value);
        command.Parameters.AddWithValue("parent_id", (object?)parentId ?? DBNull.Value);
        command.Parameters.AddWithValue("data", normalizedJson);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
