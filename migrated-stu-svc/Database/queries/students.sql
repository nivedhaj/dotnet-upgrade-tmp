-- PostgreSQL-native Student module queries.
-- Keep query text versioned with the application.

-- Recent students
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
ORDER BY s.name NULLS LAST, s.student_id NULLS LAST
LIMIT @limit;
