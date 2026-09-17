SET search_path TO ct, public;

-- Core lookup indexes (spec section 70)
CREATE INDEX IF NOT EXISTS ix_students_student_id ON ct.students (student_id);
CREATE INDEX IF NOT EXISTS ix_students_status ON ct.students (status);
CREATE INDEX IF NOT EXISTS ix_students_user_id ON ct.students (user_id);

CREATE INDEX IF NOT EXISTS ix_enrollments_inst_id ON ct.student_enrollments (inst_id);
CREATE INDEX IF NOT EXISTS ix_enrollments_student_id ON ct.student_enrollments (student_id);
CREATE INDEX IF NOT EXISTS ix_enrollments_inst_course ON ct.student_enrollments (inst_id, course_id);
CREATE INDEX IF NOT EXISTS ix_enrollments_inst_section ON ct.student_enrollments (inst_id, section_name);
CREATE INDEX IF NOT EXISTS ix_enrollments_inst_status ON ct.student_enrollments (inst_id, status);
CREATE INDEX IF NOT EXISTS ix_enrollments_inst_admission_no ON ct.student_enrollments (inst_id, admission_no);

CREATE INDEX IF NOT EXISTS ix_courses_inst_id ON ct.courses (inst_id);
CREATE INDEX IF NOT EXISTS ix_courses_inst_status ON ct.courses (inst_id, status);

CREATE INDEX IF NOT EXISTS ix_person_contacts_person_id ON ct.person_contacts (person_id);
CREATE INDEX IF NOT EXISTS ix_person_contacts_type_info ON ct.person_contacts (contact_type, info);
CREATE INDEX IF NOT EXISTS ix_person_contacts_primary ON ct.person_contacts (person_id, contact_type) WHERE is_primary = true;

CREATE INDEX IF NOT EXISTS ix_person_tags_tag ON ct.person_tags (tag);

-- Trigram search indexes (spec section 69, 30 — replacement for Raven student search index)
CREATE INDEX IF NOT EXISTS ix_persons_first_name_trgm ON ct.persons USING GIN (first_name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS ix_persons_last_name_trgm ON ct.persons USING GIN (last_name gin_trgm_ops);
CREATE INDEX IF NOT EXISTS ix_students_student_id_trgm ON ct.students USING GIN (student_id gin_trgm_ops);

-- Placeholders for tables introduced by later phases (Attendance, Fees,
-- Essentials) are intentionally NOT declared here yet — see
-- docs/migration-matrix.md. Do not add indexes for tables that do not exist.
