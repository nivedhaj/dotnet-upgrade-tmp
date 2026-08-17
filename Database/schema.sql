
create schema if not exists campus;

create table if not exists campus.documents
(
    id           text primary key,
    type_name    text not null,
    owner_id     text not null default '',
    parent_id    text not null default '',
    payload      jsonb not null,
    created_on   timestamptz,
    created_by   text not null default '',
    modified_on  timestamptz,
    modified_by  text not null default ''
);

create index if not exists ix_documents_type_name on campus.documents(type_name);
create index if not exists ix_documents_owner_id on campus.documents(owner_id);
create index if not exists ix_documents_payload_gin on campus.documents using gin(payload);

create table if not exists campus.fees_receivables
(
    "Id"          text primary key,
    "StudentId"   text not null,
    "InstId"      text not null,
    "FeeId"       text not null,
    "FeeName"     text not null,
    "TxId"        text not null default '',
    "TxStatus"    integer not null,
    "Amount"      numeric(18,2) not null,
    "OwnerId"     text not null default '',
    "ParentId"    text not null default '',
    "CreatedOn"   timestamptz,
    "CreatedBy"   text not null default '',
    "ModifiedOn"  timestamptz,
    "ModifiedBy"  text not null default ''
);

create index if not exists ix_fees_receivables_student
    on campus.fees_receivables("StudentId","InstId");
create index if not exists ix_fees_receivables_tx
    on campus.fees_receivables("StudentId","TxId");

create table if not exists campus.opening_balances
(
    "Id"          text primary key,
    "StudentId"   text not null,
    "InstId"      text not null,
    "FeeId"       text not null,
    "FeeName"     text not null,
    "Amount"      numeric(18,2) not null,
    "OwnerId"     text not null default '',
    "ParentId"    text not null default '',
    "CreatedOn"   timestamptz,
    "CreatedBy"   text not null default '',
    "ModifiedOn"  timestamptz,
    "ModifiedBy"  text not null default ''
);

create unique index if not exists ux_opening_balance_student_inst
    on campus.opening_balances("StudentId","InstId");

create table if not exists campus.student_fee_summary_cache
(
    "StudentId"         text not null,
    "InstId"            text not null,
    "TotalReceivable"   numeric(18,2) not null default 0,
    "TotalPaid"         numeric(18,2) not null default 0,
    "OpeningBalance"    numeric(18,2) not null default 0,
    "LastUpdatedOn"     timestamptz not null default now(),
    primary key ("StudentId","InstId")
);

create or replace function campus.current_user_id()
returns text
language sql
stable
as $$
    select coalesce(nullif(current_setting('app.user_id', true), ''), 'system')
$$;

create or replace function campus.audit_documents()
returns trigger
language plpgsql
as $$
declare
    uid text := campus.current_user_id();
begin
    if tg_op = 'INSERT' then
        if new.created_on is null then new.created_on := now(); end if;
        if new.created_by = '' or new.created_by is null then new.created_by := uid; end if;

        new.payload := jsonb_set(new.payload, '{CreatedOn}', to_jsonb(new.created_on), true);
        new.payload := jsonb_set(new.payload, '{CreatedBy}', to_jsonb(new.created_by), true);
        return new;
    end if;

    new.modified_on := now();
    new.modified_by := uid;
    new.payload := jsonb_set(new.payload, '{ModifiedOn}', to_jsonb(new.modified_on), true);
    new.payload := jsonb_set(new.payload, '{ModifiedBy}', to_jsonb(new.modified_by), true);
    return new;
end;
$$;

drop trigger if exists trg_audit_documents on campus.documents;
create trigger trg_audit_documents
before insert or update on campus.documents
for each row execute function campus.audit_documents();

create or replace function campus.audit_fee_row()
returns trigger
language plpgsql
as $$
declare
    uid text := campus.current_user_id();
begin
    if tg_op = 'INSERT' then
        if new."CreatedOn" is null then new."CreatedOn" := now(); end if;
        if new."CreatedBy" = '' or new."CreatedBy" is null then new."CreatedBy" := uid; end if;
        return new;
    end if;

    new."ModifiedOn" := now();
    new."ModifiedBy" := uid;
    return new;
end;
$$;

drop trigger if exists trg_audit_fees_receivables on campus.fees_receivables;
create trigger trg_audit_fees_receivables
before insert or update on campus.fees_receivables
for each row execute function campus.audit_fee_row();

drop trigger if exists trg_audit_opening_balances on campus.opening_balances;
create trigger trg_audit_opening_balances
before insert or update on campus.opening_balances
for each row execute function campus.audit_fee_row();

create or replace function campus.refresh_student_fee_summary()
returns trigger
language plpgsql
as $$
declare
    sid text := coalesce(new."StudentId", old."StudentId");
    iid text := coalesce(new."InstId", old."InstId");
begin
    insert into campus.student_fee_summary_cache
        ("StudentId","InstId","TotalReceivable","TotalPaid","OpeningBalance","LastUpdatedOn")
    select
        sid,
        iid,
        coalesce((
            select sum(fr."Amount")
            from campus.fees_receivables fr
            where fr."StudentId" = sid
              and fr."InstId" = iid
              and fr."TxStatus" = 1
        ),0),
        coalesce((
            select sum(fr."Amount")
            from campus.fees_receivables fr
            where fr."StudentId" = sid
              and fr."InstId" = iid
              and fr."TxStatus" = 99
        ),0),
        coalesce((
            select sum(ob."Amount")
            from campus.opening_balances ob
            where ob."StudentId" = sid
              and ob."InstId" = iid
        ),0),
        now()
    on conflict ("StudentId","InstId") do update set
        "TotalReceivable" = excluded."TotalReceivable",
        "TotalPaid" = excluded."TotalPaid",
        "OpeningBalance" = excluded."OpeningBalance",
        "LastUpdatedOn" = excluded."LastUpdatedOn";

    return coalesce(new, old);
end;
$$;

drop trigger if exists trg_refresh_student_fee_summary on campus.fees_receivables;
create trigger trg_refresh_student_fee_summary
after insert or update or delete on campus.fees_receivables
for each row execute function campus.refresh_student_fee_summary();

drop trigger if exists trg_refresh_student_fee_summary_opening on campus.opening_balances;
create trigger trg_refresh_student_fee_summary_opening
after insert or update or delete on campus.opening_balances
for each row execute function campus.refresh_student_fee_summary();

create or replace view campus.v_student_fee_summary as
select
    "StudentId",
    "InstId",
    "TotalReceivable",
    "TotalPaid",
    "OpeningBalance",
    ("TotalReceivable" - "TotalPaid") as "OutstandingBalance",
    "LastUpdatedOn"
from campus.student_fee_summary_cache;

-- Student_Inst_Index replacement.
create or replace view campus.v_student_inst_index as
with students as
(
    select id, payload
    from campus.documents
    where type_name in ('Repo.Resource.Student','Student')
),
courses as
(
    select id, payload
    from campus.documents
    where type_name in ('Repo.Resource.Course','Course')
),
institutes as
(
    select id, owner_id
    from campus.documents
    where type_name in ('Institute','Repo.Db.Index.Institute')
)
select
    s.id as "Id",
    concat_ws(' ', nullif(s.payload->>'FirstName',''),
                   nullif(s.payload->>'MiddleName',''),
                   nullif(s.payload->>'LastName','')) as "Name",
    nullif(s.payload->>'Gender','')::integer as "Gender",
    coalesce((s.payload->>'Status')::integer, -1) as "Status",
    e->>'InstId' as "InstId",
    e->>'StudentId' as "StudentId",
    e->>'CourseId' as "CourseId",
    c.payload->>'Name' as "CourseName",
    c.payload->>'Branch' as "Branch",
    coalesce((c.payload->>'SortIndex')::integer,0) as "CourseSortIndex",
    e->>'TermName' as "TermName",
    e->>'SectionName' as "SectionName",
    coalesce((e->>'Status')::integer,-1) as "EnrollmentStatus",
    case when (e->>'RollNo') ~ '^-?[0-9]+$' then (e->>'RollNo')::integer else 0 end as "RollNo",
    s.payload->>'DOB' as "DOB",
    e->>'DOA' as "DOA",
    coalesce(s.payload->'Tags','[]'::jsonb) as "Tags",
    concat_ws(',',
        nullif(s.payload->>'Email',''),
        nullif(s.payload->'Father'->>'Email',''),
        nullif(s.payload->'Mother'->>'Email',''),
        nullif(s.payload->'Guardian'->>'Email','')) as "Emails",
    string_to_array(
        concat_ws(',',
            nullif(s.payload->>'Email',''),
            nullif(s.payload->'Father'->>'Email',''),
            nullif(s.payload->'Mother'->>'Email',''),
            nullif(s.payload->'Guardian'->>'Email','')), ',') as "EmailList",
    concat_ws(',',
        nullif(s.payload->>'Mobile',''),
        nullif(s.payload->'Father'->>'Mobile',''),
        nullif(s.payload->'Mother'->>'Mobile',''),
        nullif(s.payload->'Guardian'->>'Mobile','')) as "Mobiles",
    concat_ws(' ', s.payload->'Father'->>'FirstName',
                   s.payload->'Father'->>'MiddleName',
                   s.payload->'Father'->>'LastName') as "FatherName",
    concat_ws(' ', s.payload->'Mother'->>'FirstName',
                   s.payload->'Mother'->>'MiddleName',
                   s.payload->'Mother'->>'LastName') as "MotherName",
    s.payload->'Father'->>'PAN' as "FatherPAN",
    s.payload->'Mother'->>'PAN' as "MotherPAN",
    concat_ws('.', lower(s.payload->>'FirstName'), lower(s.payload->>'LastName'), left(s.id,4))
        || '@campustrack.net' as "VirtualId",
    i.owner_id as "OrgId",
    s.payload->>'UserId' as "UserId",
    e->>'AdmissionNo' as "AdmissionNo",
    s.payload as "Payload"
from students s
cross join lateral jsonb_array_elements(coalesce(s.payload->'Enrollments','[]'::jsonb)) e
left join courses c on c.id = e->>'CourseId'
left join institutes i on i.id = e->>'InstId';

create or replace view campus.v_student_coursewise_count as
select
    e->>'InstId' as "InstId",
    e->>'CourseId' as "CourseId",
    c.payload->>'Name' as "CourseName",
    e->>'TermName' as "TermName",
    e->>'SectionName' as "SectionName",
    sum(case when (s.payload->>'Gender')::integer = 1 then 1 else 0 end)::integer as "Boys",
    sum(case when (s.payload->>'Gender')::integer = 0 then 1 else 0 end)::integer as "Girls"
from campus.documents s
cross join lateral jsonb_array_elements(coalesce(s.payload->'Enrollments','[]'::jsonb)) e
left join campus.documents c on c.id = e->>'CourseId'
where s.type_name in ('Repo.Resource.Student','Student')
  and coalesce((s.payload->>'Status')::integer,-1) = 1
  and coalesce((e->>'Status')::integer,-1) = 1
group by e->>'InstId',e->>'CourseId',c.payload->>'Name',e->>'TermName',e->>'SectionName';

create or replace view campus.v_inst_student_count as
select
    e->>'InstId' as "InstId",
    sum(case when (s.payload->>'Gender')::integer = 1 then 1 else 0 end)::integer as "StudentCountMale",
    sum(case when (s.payload->>'Gender')::integer = 0 then 1 else 0 end)::integer as "StudentCountFemale"
from campus.documents s
cross join lateral jsonb_array_elements(coalesce(s.payload->'Enrollments','[]'::jsonb)) e
where s.type_name in ('Repo.Resource.Student','Student')
  and coalesce((s.payload->>'Status')::integer,-1) = 1
  and coalesce((e->>'Status')::integer,-1) = 1
group by e->>'InstId';

create or replace view campus.v_attendance_event_index as
select
    d.id as "Id",
    d.payload->>'InstId' as "InstId",
    d.payload->>'CourseId' as "CourseId",
    d.payload->>'TermName' as "TermName",
    d.payload->>'SectionName' as "SectionName",
    d.payload->>'Date' as "Date",
    coalesce((d.payload->>'PeriodNo')::integer,0) as "PeriodNo",
    d.payload->>'SubjectName' as "SubjectName",
    coalesce((d.payload->>'IsOptionalSubject')::boolean,false) as "IsOptionalSubject",
    d.payload->>'StudentId' as "StudentId",
    d.payload->>'StaffId' as "StaffId",
    d.payload->>'Attendance' as "Attendance",
    d.payload->>'CreatedOn' as "CreatedOn",
    d.payload->>'CreatedBy' as "CreatedBy"
from campus.documents d
where d.type_name in ('Repo.Resource.AttendanceEvent','AttendanceEvent');

create or replace view campus.v_student_essentials_index as
select
    d.id as "Id",
    d.payload->>'StudentId' as "StudentId",
    coalesce(d.payload->'Items','[]'::jsonb) as "Items"
from campus.documents d
where d.type_name in ('Repo.Resource.StudentEssentials','StudentEssentials');

create or replace view campus.v_ping_index as
select
    d.id as "Id",
    d.payload->>'Service' as "Service",
    d.payload->>'Message' as "Message",
    coalesce(d.payload->>'At', d.created_on::text) as "At",
    d.owner_id as "OwnerId",
    d.parent_id as "ParentId",
    d.created_on as "CreatedOn",
    d.created_by as "CreatedBy",
    d.modified_on as "ModifiedOn",
    d.modified_by as "ModifiedBy"
from campus.documents d
where d.type_name in ('ct._Core.Repo.Resource.Ping','Ping');

-- Useful operational indexes for the view-backed read paths.
create index if not exists ix_documents_student_enrollments
    on campus.documents using gin ((payload->'Enrollments'));
create index if not exists ix_documents_attendance
    on campus.documents ((payload->>'InstId'), (payload->>'Date'));
