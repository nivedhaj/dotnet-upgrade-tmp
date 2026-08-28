using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ct.Infrastructure.Persistence.Migrations;

public partial class InitialPostgres : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "stu");

        migrationBuilder.Sql(
            "CREATE EXTENSION IF NOT EXISTS pgcrypto;");

        migrationBuilder.CreateTable(
            name: "student",
            schema: "stu",
            columns: table => new
            {
                id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),

                student_id = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),

                name = table.Column<string>(
                    type: "character varying(250)",
                    maxLength: 250,
                    nullable: false),

                admission_number = table.Column<string>(
                    type: "text",
                    nullable: true),

                application_number = table.Column<string>(
                    type: "text",
                    nullable: true),

                date_of_birth = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: true),

                date_of_admission = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: true),

                email = table.Column<string>(
                    type: "text",
                    nullable: true),

                mobile = table.Column<string>(
                    type: "text",
                    nullable: true),

                gender = table.Column<string>(
                    type: "text",
                    nullable: true),

                aadhar_number = table.Column<string>(
                    type: "text",
                    nullable: true),

                status = table.Column<string>(
                    type: "text",
                    nullable: false),

                enrollments = table.Column<string>(
                    type: "jsonb",
                    nullable: false),

                contacts = table.Column<string>(
                    type: "jsonb",
                    nullable: false),

                metadata = table.Column<string>(
                    type: "jsonb",
                    nullable: false),

                created_at = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false),

                updated_at = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "pk_student",
                    x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "fee",
            schema: "stu",
            columns: table => new
            {
                id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),

                name = table.Column<string>(
                    type: "character varying(250)",
                    maxLength: 250,
                    nullable: false),

                amount = table.Column<decimal>(
                    type: "numeric(14,2)",
                    precision: 14,
                    scale: 2,
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "pk_fee",
                    x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "fee_transaction",
            schema: "stu",
            columns: table => new
            {
                id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),

                student_id = table.Column<Guid>(
                    type: "uuid",
                    nullable: false),

                tx_no = table.Column<string>(
                    type: "character varying(100)",
                    maxLength: 100,
                    nullable: false),

                tx_date = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false),

                amount = table.Column<decimal>(
                    type: "numeric(14,2)",
                    precision: 14,
                    scale: 2,
                    nullable: false),

                status = table.Column<string>(
                    type: "text",
                    nullable: true),

                installments_paid = table.Column<string>(
                    type: "jsonb",
                    nullable: false),

                created_at = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false),

                updated_at = table.Column<DateTime>(
                    type: "timestamp with time zone",
                    nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey(
                    "pk_fee_transaction",
                    x => x.id);

                table.ForeignKey(
                    name: "fk_fee_transaction_student",
                    column: x => x.student_id,
                    principalSchema: "stu",
                    principalTable: "student",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_student_student_id",
            schema: "stu",
            table: "student",
            column: "student_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_student_status",
            schema: "stu",
            table: "student",
            column: "status");

        migrationBuilder.CreateIndex(
            name: "ix_student_admission_number",
            schema: "stu",
            table: "student",
            column: "admission_number");

        migrationBuilder.CreateIndex(
            name: "ix_fee_transaction_student_tx_date",
            schema: "stu",
            table: "fee_transaction",
            columns: new[] { "student_id", "tx_date" });

        migrationBuilder.CreateIndex(
            name: "ix_fee_transaction_tx_no",
            schema: "stu",
            table: "fee_transaction",
            column: "tx_no",
            unique: true);

        migrationBuilder.Sql(
            """
            CREATE OR REPLACE VIEW stu.student_fee_summary_view AS
            WITH raw_paid_installments AS (
                SELECT
                    ft.id AS fee_transaction_id,
                    ft.student_id,
                    ft.tx_no,
                    ft.tx_date,
                    ft.amount AS transaction_amount,
                    ft.status AS transaction_status,
                    NULLIF(
                        COALESCE(
                            i->>'FeeId',
                            i->>'feeId',
                            i->>'Id',
                            i->>'id'
                        ),
                        ''
                    ) AS raw_fee_id,
                    NULLIF(
                        COALESCE(
                            i->>'Amount',
                            i->>'PaidAmount',
                            i->>'amount',
                            i->>'paidAmount'
                        ),
                        ''
                    ) AS raw_paid_amount
                FROM stu.fee_transaction ft
                LEFT JOIN LATERAL jsonb_array_elements(
                    CASE
                        WHEN jsonb_typeof(ft.installments_paid) = 'array'
                        THEN ft.installments_paid
                        ELSE '[]'::jsonb
                    END
                ) i ON TRUE
            ),
            paid_installments AS (
                SELECT
                    *,
                    CASE
                        WHEN raw_fee_id ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$'
                        THEN raw_fee_id::uuid
                    END AS fee_id,
                    CASE
                        WHEN raw_paid_amount ~ '^-?[0-9]+(\.[0-9]+)?$'
                        THEN raw_paid_amount::numeric(14,2)
                    END AS paid_amount
                FROM raw_paid_installments
            )
            SELECT
                s.id AS student_pk,
                s.student_id,
                s.name AS student_name,
                se.enrollment->>'CourseName' AS course_name,
                se.enrollment->>'TermName' AS term_name,
                se.enrollment->>'SectionName' AS section_name,
                f.id AS fee_id,
                f.name AS fee_name,
                f.amount AS fee_amount,
                pi.fee_transaction_id,
                pi.tx_no,
                pi.tx_date,
                COALESCE(
                    pi.paid_amount,
                    pi.transaction_amount
                ) AS paid_amount,
                pi.transaction_status
            FROM stu.student s
            LEFT JOIN LATERAL jsonb_array_elements(
                CASE
                    WHEN jsonb_typeof(s.enrollments) = 'array'
                    THEN s.enrollments
                    ELSE '[]'::jsonb
                END
            ) se(enrollment) ON TRUE
            LEFT JOIN paid_installments pi
                ON pi.student_id = s.id
            LEFT JOIN stu.fee f
                ON f.id = pi.fee_id;
            """);

        migrationBuilder.Sql(
            """
            DROP MATERIALIZED VIEW IF EXISTS stu.student_fee_summary_mv;

            CREATE MATERIALIZED VIEW stu.student_fee_summary_mv AS
            SELECT *
            FROM stu.student_fee_summary_view
            WITH NO DATA;

            REFRESH MATERIALIZED VIEW stu.student_fee_summary_mv;
            """);

        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION stu.refresh_student_fee_summary_mv()
            RETURNS trigger
            LANGUAGE plpgsql
            AS $$
            BEGIN
                REFRESH MATERIALIZED VIEW stu.student_fee_summary_mv;
                RETURN NULL;
            END;
            $$;

            DROP TRIGGER IF EXISTS trg_refresh_student_fee_summary_mv
                ON stu.fee_transaction;

            CREATE TRIGGER trg_refresh_student_fee_summary_mv
            AFTER INSERT OR UPDATE OR DELETE
            ON stu.fee_transaction
            FOR EACH STATEMENT
            EXECUTE FUNCTION stu.refresh_student_fee_summary_mv();
            """);

        migrationBuilder.Sql(
            """
            CREATE OR REPLACE FUNCTION stu.set_updated_at()
            RETURNS trigger
            LANGUAGE plpgsql
            AS $$
            BEGIN
                NEW.updated_at = CURRENT_TIMESTAMP;
                RETURN NEW;
            END;
            $$;

            DROP TRIGGER IF EXISTS trg_student_updated_at
                ON stu.student;

            CREATE TRIGGER trg_student_updated_at
            BEFORE UPDATE
            ON stu.student
            FOR EACH ROW
            EXECUTE FUNCTION stu.set_updated_at();

            DROP TRIGGER IF EXISTS trg_fee_transaction_updated_at
                ON stu.fee_transaction;

            CREATE TRIGGER trg_fee_transaction_updated_at
            BEFORE UPDATE
            ON stu.fee_transaction
            FOR EACH ROW
            EXECUTE FUNCTION stu.set_updated_at();
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DROP TRIGGER IF EXISTS trg_refresh_student_fee_summary_mv
                ON stu.fee_transaction;

            DROP FUNCTION IF EXISTS stu.refresh_student_fee_summary_mv();

            DROP MATERIALIZED VIEW IF EXISTS stu.student_fee_summary_mv;

            DROP VIEW IF EXISTS stu.student_fee_summary_view;

            DROP TRIGGER IF EXISTS trg_student_updated_at
                ON stu.student;

            DROP TRIGGER IF EXISTS trg_fee_transaction_updated_at
                ON stu.fee_transaction;

            DROP FUNCTION IF EXISTS stu.set_updated_at();
            """);

        migrationBuilder.DropTable(
            name: "fee_transaction",
            schema: "stu");

        migrationBuilder.DropTable(
            name: "fee",
            schema: "stu");

        migrationBuilder.DropTable(
            name: "student",
            schema: "stu");
    }
}
