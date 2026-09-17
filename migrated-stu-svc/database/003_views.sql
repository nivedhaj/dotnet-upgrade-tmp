SET search_path TO ct, public;

-- student_search_view (spec section 72) — replacement for the legacy Raven
-- student search index. Joins student/person/enrollment/relations without
-- N+1 access; aggregates tags/contacts so each student is exactly one row.
--
-- NOTE: because a student may hold enrollments across multiple institutes
-- (one row per inst_id, spec section 61), this view surfaces one row per
-- (student, enrollment) pair rather than collapsing to "the" enrollment.
-- The legacy standalone spec does not define a single-current-enrollment
-- rule, so this is a documented assumption (see docs/migration-matrix.md) —
-- callers wanting a single row per student should filter by inst_id.
CREATE OR REPLACE VIEW ct.student_search_view AS
SELECT
    s.id                                   AS student_guid,
    s.student_id                           AS student_id,
    trim(both ' ' from
        coalesce(p.first_name, '') || ' ' ||
        coalesce(p.middle_name || ' ', '') ||
        coalesce(p.last_name, ''))         AS name,
    p.gender,
    s.status,
    e.inst_id,
    e.course_name,
    e.branch,
    e.term_name,
    e.section_name,
    e.roll_no,
    e.admission_no,
    e.doa,
    p.dob,
    p.email                                AS student_email,
    p.mobile                               AS student_mobile,
    father.name                            AS father_name,
    father.email                           AS father_email,
    father.mobile                          AS father_mobile,
    mother.name                            AS mother_name,
    mother.email                           AS mother_email,
    mother.mobile                          AS mother_mobile,
    guardian.name                          AS guardian_name,
    guardian.email                         AS guardian_email,
    guardian.mobile                        AS guardian_mobile,
    tags.tags
FROM ct.students s
JOIN ct.persons p ON p.id = s.person_id
LEFT JOIN ct.student_enrollments e ON e.student_id = s.id
LEFT JOIN LATERAL (
    SELECT array_agg(t.tag) AS tags
    FROM ct.person_tags t
    WHERE t.person_id = p.id
) tags ON true
LEFT JOIN LATERAL (
    SELECT
        trim(both ' ' from coalesce(rp.first_name,'') || ' ' || coalesce(rp.last_name,'')) AS name,
        rp.email, rp.mobile
    FROM ct.student_relations r
    JOIN ct.persons rp ON rp.id = r.person_id
    WHERE r.student_id = s.id AND r.relation_type = 1 -- Father
) father ON true
LEFT JOIN LATERAL (
    SELECT
        trim(both ' ' from coalesce(rp.first_name,'') || ' ' || coalesce(rp.last_name,'')) AS name,
        rp.email, rp.mobile
    FROM ct.student_relations r
    JOIN ct.persons rp ON rp.id = r.person_id
    WHERE r.student_id = s.id AND r.relation_type = 2 -- Mother
) mother ON true
LEFT JOIN LATERAL (
    SELECT
        trim(both ' ' from coalesce(rp.first_name,'') || ' ' || coalesce(rp.last_name,'')) AS name,
        rp.email, rp.mobile
    FROM ct.student_relations r
    JOIN ct.persons rp ON rp.id = r.person_id
    WHERE r.student_id = s.id AND r.relation_type = 3 -- Guardian
) guardian ON true;

-- student_institute_summary_view (spec section 71) — backs facet/count
-- endpoints (spec section 44, 33 GetStudentCountInstitutewise) without
-- in-memory aggregation.
CREATE OR REPLACE VIEW ct.student_institute_summary_view AS
SELECT
    e.inst_id,
    s.status,
    p.gender,
    count(*) AS student_count
FROM ct.student_enrollments e
JOIN ct.students s ON s.id = e.student_id
JOIN ct.persons p ON p.id = s.person_id
GROUP BY e.inst_id, s.status, p.gender;
