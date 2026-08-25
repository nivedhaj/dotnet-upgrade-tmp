CREATE TABLE IF NOT EXISTS students (
    id uuid PRIMARY KEY,
    student_id varchar(100) NOT NULL,
    name varchar(300) NOT NULL,
    status varchar(30) NOT NULL DEFAULT 'Active',
    aadhar_number varchar(100),
    udid varchar(100),
    user_id varchar(100),
    photo_url text,
    category varchar(100),
    attendance text,
    domicile_json jsonb,
    documents_json jsonb,
    created_at_utc timestamptz NOT NULL DEFAULT now(),
    updated_at_utc timestamptz NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_students_student_id ON students(student_id);
CREATE INDEX IF NOT EXISTS ix_students_name ON students(name);
CREATE TABLE IF NOT EXISTS student_enrollments (
    id uuid PRIMARY KEY,
    student_id uuid NOT NULL REFERENCES students(id) ON DELETE CASCADE,
    inst_id varchar(100) NOT NULL,
    course_id varchar(100),
    course_name varchar(300),
    term_name varchar(200),
    section_name varchar(100),
    branch varchar(200),
    application_no varchar(100),
    admission_no varchar(100),
    assigned_student_id varchar(100),
    doa timestamptz
);
CREATE INDEX IF NOT EXISTS ix_student_enrollments_student_inst ON student_enrollments(student_id, inst_id);
CREATE INDEX IF NOT EXISTS ix_student_enrollments_inst_course_section ON student_enrollments(inst_id, course_id, section_name);
