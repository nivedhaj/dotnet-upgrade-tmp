-- ct.stu PostgreSQL schema
-- Spec sections 57-68. This script covers the Admissions/Student vertical
-- delivered in this pass. Tables for Attendance, Fees, IEP, Essentials,
-- Documents, and Baseline are NOT yet included here — see
-- docs/migration-matrix.md for status. NOTE: this file has been authored
-- but NOT executed against a live PostgreSQL instance in this environment
-- (no database engine available in the build sandbox); run it yourself and
-- verify before treating it as validated (spec rule 121).

CREATE SCHEMA IF NOT EXISTS ct;
SET search_path TO ct, public;

CREATE EXTENSION IF NOT EXISTS pgcrypto;   -- gen_random_uuid()
CREATE EXTENSION IF NOT EXISTS pg_trgm;    -- trigram search (spec section 69)

-- =====================================================================
-- persons (spec section 59)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.persons (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    first_name      varchar(100) NOT NULL,
    middle_name     varchar(100),
    last_name       varchar(100),
    title           varchar(50),
    gender          smallint,
    dob             date,
    email           varchar(320),
    mobile          varchar(30),
    virtual_id      varchar(320),
    pan             varchar(20),
    attributes      jsonb NOT NULL DEFAULT '{}',
    created_on      timestamptz NOT NULL DEFAULT now(),
    created_by      varchar(100),
    modified_on     timestamptz,
    modified_by     varchar(100)
);

-- =====================================================================
-- students (spec section 60)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.students (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    person_id       uuid NOT NULL REFERENCES ct.persons(id),
    student_id      varchar(100),
    attendance      varchar(100),
    category        varchar(100),
    status          smallint NOT NULL DEFAULT 1,
    aadhar_number   varchar(50),
    udid            varchar(100),
    user_id         varchar(200),
    photo_url       text,
    attributes      jsonb NOT NULL DEFAULT '{}',
    created_on      timestamptz NOT NULL DEFAULT now(),
    created_by      varchar(100),
    modified_on     timestamptz,
    modified_by     varchar(100)
);

-- =====================================================================
-- student_relations (spec section 10)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.student_relations (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id      uuid NOT NULL REFERENCES ct.students(id) ON DELETE CASCADE,
    person_id       uuid NOT NULL REFERENCES ct.persons(id),
    relation_type   smallint NOT NULL,
    CONSTRAINT uq_student_relation UNIQUE (student_id, relation_type)
);

-- =====================================================================
-- person_contacts (spec section 11)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.person_contacts (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    person_id       uuid NOT NULL REFERENCES ct.persons(id) ON DELETE CASCADE,
    contact_type    smallint NOT NULL,
    location        smallint NOT NULL,
    info            varchar(320) NOT NULL,
    is_primary      boolean NOT NULL DEFAULT false,
    status          smallint NOT NULL DEFAULT 1,
    notes           text,
    archived_on     timestamptz,
    created_on      timestamptz NOT NULL DEFAULT now()
);

-- =====================================================================
-- person_addresses (spec section 12)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.person_addresses (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    person_id       uuid NOT NULL REFERENCES ct.persons(id) ON DELETE CASCADE,
    location        smallint NOT NULL,
    line1           varchar(200),
    line2           varchar(200),
    city            varchar(100),
    state           varchar(100),
    country         varchar(100),
    pincode         varchar(20),
    archived_on     timestamptz,
    status          smallint NOT NULL DEFAULT 1
);

-- =====================================================================
-- person_tags (spec section 13)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.person_tags (
    person_id       uuid NOT NULL REFERENCES ct.persons(id) ON DELETE CASCADE,
    tag             varchar(100) NOT NULL,
    PRIMARY KEY (person_id, tag)
);

-- =====================================================================
-- person_occupations (spec section 14)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.person_occupations (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    person_id       uuid NOT NULL REFERENCES ct.persons(id) ON DELETE CASCADE,
    title           varchar(200),
    particulars     text,
    as_on           date
);

-- =====================================================================
-- student_domiciles (spec section 15)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.student_domiciles (
    student_id      uuid PRIMARY KEY REFERENCES ct.students(id) ON DELETE CASCADE,
    caste           varchar(100),
    country         varchar(100),
    mother_tongue   varchar(100),
    state           varchar(100)
);

-- =====================================================================
-- courses / terms / sections / subjects (spec section 62)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.courses (
    id                  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    inst_id             varchar(100),
    name                varchar(200) NOT NULL,
    branch              varchar(100),
    affiliation         varchar(200),
    edu_level           smallint,
    status              smallint NOT NULL DEFAULT 1,
    sort_index          integer NOT NULL DEFAULT 0,
    rank                integer NOT NULL DEFAULT 0,
    exam_subject_order  jsonb NOT NULL DEFAULT '[]',
    marks_card_template text,
    attributes          jsonb NOT NULL DEFAULT '{}'
);

CREATE TABLE IF NOT EXISTS ct.course_terms (
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    course_id   uuid NOT NULL REFERENCES ct.courses(id) ON DELETE CASCADE,
    name        varchar(100) NOT NULL,
    date_from   date,
    date_to     date,
    sort_order  integer NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS ct.course_sections (
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    term_id     uuid NOT NULL REFERENCES ct.course_terms(id) ON DELETE CASCADE,
    name        varchar(100) NOT NULL,
    sort_order  integer NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS ct.course_subjects (
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    section_id  uuid NOT NULL REFERENCES ct.course_sections(id) ON DELETE CASCADE,
    name        varchar(200) NOT NULL,
    optional    boolean NOT NULL DEFAULT false,
    sort_order  integer NOT NULL DEFAULT 0
);

-- =====================================================================
-- student_enrollments (spec section 61)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.student_enrollments (
    id                  uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id          uuid NOT NULL REFERENCES ct.students(id),
    inst_id             varchar(100) NOT NULL,
    course_id           varchar(100),
    course_name         varchar(200),
    term_name           varchar(100),
    section_name        varchar(100),
    branch              varchar(100),
    student_inst_id     varchar(100),
    application_no      varchar(100),
    admission_no        varchar(100),
    doa                 date,
    roll_no             varchar(50),
    status              smallint NOT NULL DEFAULT 1,
    tc_no               varchar(100),
    tc_applied_on       timestamptz,
    tc_issued_on        timestamptz,
    character           text,
    conduct             text,
    remarks             text,
    attributes          jsonb NOT NULL DEFAULT '{}',
    CONSTRAINT uq_student_enrollment_inst UNIQUE (student_id, inst_id)
);

-- =====================================================================
-- event_logs (spec section 28)
-- =====================================================================
CREATE TABLE IF NOT EXISTS ct.event_logs (
    id              uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    entity_type     varchar(100) NOT NULL,
    entity_id       uuid NOT NULL,
    event_type      varchar(100) NOT NULL,
    event_data      jsonb NOT NULL DEFAULT '{}',
    created_on      timestamptz NOT NULL DEFAULT now(),
    created_by      varchar(100)
);
