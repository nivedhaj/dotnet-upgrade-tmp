using Npgsql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ct._Core.Persistence;

/// <summary>
/// PostgreSQL-backed document store used by ct.stu. It replaces RavenDB while
/// preserving the aggregate/document programming model used by the module.
/// Documents are stored as JSONB with relational metadata and targeted indexes.
/// </summary>
public sealed class PostgresDocumentStore
{
    private readonly string _connectionString;
    private readonly ILogger<PostgresDocumentStore> _logger;

    public PostgresDocumentStore(IConfiguration configuration, ILogger<PostgresDocumentStore> logger)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? configuration["ConnectionStrings:Postgres"]
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
        _logger = logger;
    }

    public string ConnectionString => _connectionString;

    public PostgresDocumentSession OpenSession() => new(_connectionString);

    public void EnsureSchema()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = SchemaSql;
        command.ExecuteNonQuery();
        _logger.LogInformation("ct.stu PostgreSQL schema verified.");
    }

    public const string SchemaSql = """
        CREATE EXTENSION IF NOT EXISTS pg_trgm;

        CREATE TABLE IF NOT EXISTS ct_documents
        (
            document_type text NOT NULL,
            id text NOT NULL,
            owner_id text NULL,
            parent_id text NULL,
            created_on timestamptz NULL,
            modified_on timestamptz NULL,
            data jsonb NOT NULL,
            created_at timestamptz NOT NULL DEFAULT now(),
            updated_at timestamptz NOT NULL DEFAULT now(),
            CONSTRAINT pk_ct_documents PRIMARY KEY (document_type, id)
        );

        CREATE INDEX IF NOT EXISTS ix_ct_documents_type ON ct_documents(document_type);
        CREATE INDEX IF NOT EXISTS ix_ct_documents_owner ON ct_documents(document_type, owner_id);
        CREATE INDEX IF NOT EXISTS ix_ct_documents_parent ON ct_documents(document_type, parent_id);
        CREATE INDEX IF NOT EXISTS ix_ct_documents_data_gin ON ct_documents USING gin(data jsonb_path_ops);
        CREATE INDEX IF NOT EXISTS ix_ct_documents_student_name_trgm
            ON ct_documents USING gin ((data->>'Name') gin_trgm_ops)
            WHERE document_type = 'Student';
        CREATE INDEX IF NOT EXISTS ix_ct_documents_student_studentid
            ON ct_documents ((data->>'StudentId'))
            WHERE document_type = 'Student';
        CREATE INDEX IF NOT EXISTS ix_ct_documents_attendance_date
            ON ct_documents ((data->>'Date'))
            WHERE document_type = 'AttendanceEvent';
        CREATE INDEX IF NOT EXISTS ix_ct_documents_attendance_student
            ON ct_documents ((data->>'StudentId'))
            WHERE document_type = 'AttendanceEvent';
        CREATE INDEX IF NOT EXISTS ix_ct_documents_course_inst
            ON ct_documents ((data->>'InstId'))
            WHERE document_type = 'Course';
        CREATE INDEX IF NOT EXISTS ix_ct_documents_essentials_student
            ON ct_documents ((data->>'StudentId'))
            WHERE document_type = 'StudentEssentials';

        CREATE OR REPLACE VIEW ct_students AS
        SELECT
            id,
            (data->>'Name')::text AS name,
            NULLIF(data->>'DOB','')::timestamptz AS dob,
            data->>'StudentId' AS student_id,
            data->>'Status' AS status,
            data->>'Gender' AS gender,
            data->>'AadharNumber' AS aadhar_number,
            data->>'UDID' AS udid,
            data->'Enrollments' AS enrollments,
            data AS document
        FROM ct_documents
        WHERE document_type = 'Student';

        CREATE OR REPLACE VIEW ct_attendance_events AS
        SELECT id,
               data->>'InstId' AS inst_id,
               data->>'CourseId' AS course_id,
               data->>'StudentId' AS student_id,
               NULLIF(data->>'Date','')::timestamptz AS event_date,
               data->>'PeriodNo' AS period_no,
               data->>'Attendance' AS attendance,
               data AS document
        FROM ct_documents
        WHERE document_type = 'AttendanceEvent';
        """;
}
