using Npgsql;

namespace ct.Modules.Students.Infrastructure.Queries;

public sealed record StudentRow(
    Guid Id,
    string? StudentId,
    string? Name,
    string? CourseName,
    string? TermName,
    string? SectionName
);

/// <summary>
/// PostgreSQL-native read queries for the Student module.
/// This is the target data-access pattern for the RavenDB migration.
/// </summary>
public sealed class StudentQueries
{
    private readonly NpgsqlDataSource _dataSource;

    public StudentQueries(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    public async Task<IReadOnlyList<StudentRow>> GetRecentStudentsAsync(
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        limit = Math.Clamp(limit, 1, 100);

        const string sql = """
            SELECT
                s.id::uuid,
                s.student_id AS student_id,
                s.name AS name,
                e.enrollment->>'CourseName' AS course_name,
                e.enrollment->>'TermName' AS term_name,
                e.enrollment->>'SectionName' AS section_name
            FROM student s
            LEFT JOIN LATERAL jsonb_array_elements(
                CASE
                    WHEN jsonb_typeof(s.enrollments) = 'array'
                    THEN s.enrollments
                    ELSE '[]'::jsonb
                END
            ) AS e(enrollment) ON TRUE
            ORDER BY s.name NULLS LAST,
                     s.student_id NULLS LAST
            LIMIT @limit;
            """;

        var students = new List<StudentRow>();
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("limit", limit);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            students.Add(new StudentRow(
                reader.GetGuid(0),
                reader.IsDBNull(1) ? null : reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetString(5)));
        }

        return students;
    }

    public async Task<StudentRow?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                s.id::uuid,
                s.student_id,
                s.name,
                e.enrollment->>'CourseName',
                e.enrollment->>'TermName',
                e.enrollment->>'SectionName'
            FROM student s
            LEFT JOIN LATERAL jsonb_array_elements(
                CASE
                    WHEN jsonb_typeof(s.enrollments) = 'array'
                    THEN s.enrollments
                    ELSE '[]'::jsonb
                END
            ) AS e(enrollment) ON TRUE
              AND s.id = @id
            ORDER BY e.enrollment->>'TermName' NULLS LAST
            LIMIT 1;
            """;

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            return null;

        return new StudentRow(
            reader.GetGuid(0),
            reader.IsDBNull(1) ? null : reader.GetString(1),
            reader.IsDBNull(2) ? null : reader.GetString(2),
            reader.IsDBNull(3) ? null : reader.GetString(3),
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5));
    }
}
