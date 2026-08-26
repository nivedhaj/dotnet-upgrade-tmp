using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Ct.Stu.Infrastructure.Persistence.Migrations;
public partial class InitialPostgres : Migration
{
 protected override void Up(MigrationBuilder m)
 {
  m.EnsureSchema("stu");
  m.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");
  m.CreateTable("student","stu",t=>new{ id=t.Column<Guid>(type:"uuid",nullable:false),student_id=t.Column<string>(type:"character varying(100)",maxLength:100,nullable:false),name=t.Column<string>(type:"character varying(250)",maxLength:250,nullable:false),admission_number=t.Column<string>(type:"text",nullable:true),application_number=t.Column<string>(type:"text",nullable:true),date_of_birth=t.Column<DateTime>(type:"timestamp with time zone",nullable:true),date_of_admission=t.Column<DateTime>(type:"timestamp with time zone",nullable:true),email=t.Column<string>(type:"text",nullable:true),mobile=t.Column<string>(type:"text",nullable:true),gender=t.Column<string>(type:"text",nullable:true),aadhar_number=t.Column<string>(type:"text",nullable:true),status=t.Column<string>(type:"text",nullable:false),enrollments=t.Column<string>(type:"jsonb",nullable:false),contacts=t.Column<string>(type:"jsonb",nullable:false),metadata=t.Column<string>(type:"jsonb",nullable:false),created_at=t.Column<DateTime>(type:"timestamp with time zone",nullable:false),updated_at=t.Column<DateTime>(type:"timestamp with time zone",nullable:false)},constraints:t=>t.PrimaryKey("pk_student",x=>x.id));
  m.CreateTable("fee","stu",t=>new{id=t.Column<Guid>(type:"uuid",nullable:false),name=t.Column<string>(type:"character varying(250)",maxLength:250,nullable:false),amount=t.Column<decimal>(type:"numeric(14,2)",precision:14,scale:2,nullable:false)},constraints:t=>t.PrimaryKey("pk_fee",x=>x.id));
  m.CreateTable("fee_transaction","stu",t=>new{id=t.Column<Guid>(type:"uuid",nullable:false),student_id=t.Column<Guid>(type:"uuid",nullable:false),tx_no=t.Column<string>(type:"character varying(100)",maxLength:100,nullable:false),tx_date=t.Column<DateTime>(type:"timestamp with time zone",nullable:false),amount=t.Column<decimal>(type:"numeric(14,2)",precision:14,scale:2,nullable:false),status=t.Column<string>(type:"text",nullable:true),installments_paid=t.Column<string>(type:"jsonb",nullable:false),created_at=t.Column<DateTime>(type:"timestamp with time zone",nullable:false),updated_at=t.Column<DateTime>(type:"timestamp with time zone",nullable:false)},constraints:t=>{t.PrimaryKey("pk_fee_transaction",x=>x.id);t.ForeignKey("fk_fee_transaction_student",x=>x.student_id,"stu","student","id",onDelete:ReferentialAction.Restrict);});
  m.CreateIndex("ix_student_student_id","stu","student","student_id",unique:true);m.CreateIndex("ix_student_status","stu","student","status");m.CreateIndex("ix_student_admission_number","stu","student","admission_number");m.CreateIndex("ix_fee_transaction_student_tx_date","stu","fee_transaction",new[]{"student_id","tx_date"});m.CreateIndex("ix_fee_transaction_tx_no","stu","fee_transaction","tx_no",unique:true);
  m.Sql("""
CREATE OR REPLACE VIEW stu.student_fee_summary_view AS
WITH raw_paid_installments AS (
 SELECT ft.id fee_transaction_id,ft.student_id,ft.tx_no,ft.tx_date,ft.amount transaction_amount,ft.status transaction_status,
 NULLIF(COALESCE(i->>'FeeId',i->>'feeId',i->>'Id',i->>'id'),'') raw_fee_id,
 NULLIF(COALESCE(i->>'Amount',i->>'PaidAmount',i->>'amount',i->>'paidAmount'),'') raw_paid_amount
 FROM stu.fee_transaction ft LEFT JOIN LATERAL jsonb_array_elements(CASE WHEN jsonb_typeof(ft.installments_paid)='array' THEN ft.installments_paid ELSE '[]'::jsonb END) i ON TRUE),
paid_installments AS (
 SELECT *,CASE WHEN raw_fee_id ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$' THEN raw_fee_id::uuid END fee_id,
 CASE WHEN raw_paid_amount ~ '^-?[0-9]+(\\.[0-9]+)?$' THEN raw_paid_amount::numeric(14,2) END paid_amount FROM raw_paid_installments)
SELECT s.id student_pk,s.student_id,s.name student_name,se.enrollment->>'CourseName' course_name,se.enrollment->>'TermName' term_name,se.enrollment->>'SectionName' section_name,
f.id fee_id,f.name fee_name,f.amount fee_amount,pi.fee_transaction_id,pi.tx_no,pi.tx_date,COALESCE(pi.paid_amount,pi.transaction_amount) paid_amount,pi.transaction_status
FROM stu.student s LEFT JOIN LATERAL jsonb_array_elements(CASE WHEN jsonb_typeof(s.enrollments)='array' THEN s.enrollments ELSE '[]'::jsonb END) se(enrollment) ON TRUE
LEFT JOIN paid_installments pi ON pi.student_id=s.id LEFT JOIN stu.fee f ON f.id=pi.fee_id;
""");
  m.Sql("DROP MATERIALIZED VIEW IF EXISTS stu.student_fee_summary_mv;");m.Sql("CREATE MATERIALIZED VIEW stu.student_fee_summary_mv AS SELECT * FROM stu.student_fee_summary_view WITH NO DATA;");m.Sql("REFRESH MATERIALIZED VIEW stu.student_fee_summary_mv;");
  m.Sql("""
CREATE OR REPLACE FUNCTION stu.refresh_student_fee_summary_mv() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
  REFRESH MATERIALIZED VIEW stu.student_fee_summary_mv;
  RETURN NULL;
END; $$;
DROP TRIGGER IF EXISTS trg_refresh_student_fee_summary_mv ON stu.fee_transaction;
CREATE TRIGGER trg_refresh_student_fee_summary_mv AFTER INSERT OR UPDATE OR DELETE ON stu.fee_transaction FOR EACH STATEMENT EXECUTE FUNCTION stu.refresh_student_fee_summary_mv();
""");
  m.Sql("""CREATE OR REPLACE FUNCTION stu.set_updated_at() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN NEW.updated_at=CURRENT_TIMESTAMP; RETURN NEW; END; $$; DROP TRIGGER IF EXISTS trg_student_updated_at ON stu.student; CREATE TRIGGER trg_student_updated_at BEFORE UPDATE ON stu.student FOR EACH ROW EXECUTE FUNCTION stu.set_updated_at(); DROP TRIGGER IF EXISTS trg_fee_transaction_updated_at ON stu.fee_transaction; CREATE TRIGGER trg_fee_transaction_updated_at BEFORE UPDATE ON stu.fee_transaction FOR EACH ROW EXECUTE FUNCTION stu.set_updated_at();""");
 }
 protected override void Down(MigrationBuilder m){m.Sql("DROP TRIGGER IF EXISTS trg_refresh_student_fee_summary_mv ON stu.fee_transaction; DROP FUNCTION IF EXISTS stu.refresh_student_fee_summary_mv(); DROP MATERIALIZED VIEW IF EXISTS stu.student_fee_summary_mv; DROP VIEW IF EXISTS stu.student_fee_summary_view; DROP TRIGGER IF EXISTS trg_student_updated_at ON stu.student; DROP TRIGGER IF EXISTS trg_fee_transaction_updated_at ON stu.fee_transaction; DROP FUNCTION IF EXISTS stu.set_updated_at();");m.DropTable("fee_transaction","stu");m.DropTable("fee","stu");m.DropTable("student","stu");}
}
