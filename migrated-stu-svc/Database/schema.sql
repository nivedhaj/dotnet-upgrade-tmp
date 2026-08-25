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
CREATE INDEX IF NOT EXISTS ix_ct_documents_student_name_trgm ON ct_documents USING gin ((data->>'Name') gin_trgm_ops) WHERE document_type='Student';
CREATE INDEX IF NOT EXISTS ix_ct_documents_student_studentid ON ct_documents ((data->>'StudentId')) WHERE document_type='Student';
CREATE INDEX IF NOT EXISTS ix_ct_documents_attendance_date ON ct_documents ((data->>'Date')) WHERE document_type='AttendanceEvent';
CREATE INDEX IF NOT EXISTS ix_ct_documents_attendance_student ON ct_documents ((data->>'StudentId')) WHERE document_type='AttendanceEvent';
CREATE INDEX IF NOT EXISTS ix_ct_documents_course_inst ON ct_documents ((data->>'InstId')) WHERE document_type='Course';
CREATE INDEX IF NOT EXISTS ix_ct_documents_essentials_student ON ct_documents ((data->>'StudentId')) WHERE document_type='StudentEssentials';

CREATE OR REPLACE VIEW student AS
SELECT
    id::uuid AS id,
    data->>'StudentId' AS student_id,
    data->>'Name' AS name,
    data->'Enrollments' AS enrollments
FROM ct_documents
WHERE document_type = 'Student';

CREATE OR REPLACE VIEW ct_students AS
SELECT id,
       data->>'Name' AS name,
       NULLIF(data->>'DOB','')::timestamptz AS dob,
       data->>'StudentId' AS student_id,
       data->>'Status' AS status,
       data->>'Gender' AS gender,
       data->>'AadharNumber' AS aadhar_number,
       data->>'UDID' AS udid,
       data->'Enrollments' AS enrollments,
       data AS document
FROM ct_documents WHERE document_type='Student';

CREATE OR REPLACE VIEW ct_attendance_events AS
SELECT id,
       data->>'InstId' AS inst_id,
       data->>'CourseId' AS course_id,
       data->>'StudentId' AS student_id,
       NULLIF(data->>'Date','')::timestamptz AS event_date,
       data->>'PeriodNo' AS period_no,
       data->>'Attendance' AS attendance,
       data AS document
FROM ct_documents WHERE document_type='AttendanceEvent';

CREATE OR REPLACE VIEW ct_student_enrollments AS
SELECT s.id AS student_document_id,
       e->>'InstId' AS inst_id,
       e->>'CourseId' AS course_id,
       e->>'CourseName' AS course_name,
       e->>'TermName' AS term_name,
       e->>'SectionName' AS section_name,
       e->>'StudentId' AS student_id,
       e->>'AdmissionNo' AS admission_no,
       e->>'ApplicationNo' AS application_no,
       e->>'Status' AS enrollment_status,
       e AS enrollment
FROM ct_documents s
CROSS JOIN LATERAL jsonb_array_elements(COALESCE(s.data->'Enrollments','[]'::jsonb)) e
WHERE s.document_type='Student';

CREATE INDEX IF NOT EXISTS ix_ct_student_enrollments_inst_course_section
    ON ct_documents USING gin ((data->'Enrollments') jsonb_path_ops)
    WHERE document_type='Student';
