SET search_path TO ct, public;

-- One reusable trigger function for modification timestamps (spec section 73).
-- NOTE: the application (CtDbContext.SaveChanges) also stamps modified_on for
-- rows updated through EF Core. This trigger exists as a safety net for any
-- direct SQL writes (migrations, admin scripts, future services) and is
-- intentionally a no-op business-logic-wise — it only touches modified_on.
CREATE OR REPLACE FUNCTION ct.set_modified_on()
RETURNS trigger AS $$
BEGIN
    NEW.modified_on = now();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_persons_modified_on ON ct.persons;
CREATE TRIGGER trg_persons_modified_on
    BEFORE UPDATE ON ct.persons
    FOR EACH ROW EXECUTE FUNCTION ct.set_modified_on();

DROP TRIGGER IF EXISTS trg_students_modified_on ON ct.students;
CREATE TRIGGER trg_students_modified_on
    BEFORE UPDATE ON ct.students
    FOR EACH ROW EXECUTE FUNCTION ct.set_modified_on();

-- courses, student_enrollments, student_documents, baseline_data triggers
-- are deferred until those tables/columns land (student_enrollments has no
-- modified_on column in the current schema — see docs/migration-matrix.md
-- for the decision to add it in a follow-up migration rather than widen
-- scope silently here).
