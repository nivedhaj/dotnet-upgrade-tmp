# Migration Matrix

Status key: **IMPLEMENTED** (code written) / **TESTED** (unit or integration
test exercises it) / **NOT TESTED** (written, unverified) / **NOT STARTED**.

Per spec rule 121 (do not fabricate verification): this build was produced in
a sandbox **without a .NET SDK or PostgreSQL instance available**. Nothing
below has been compiled or executed. Before relying on any of this, run:

```
dotnet restore
dotnet build
dotnet test
docker compose up --build
```

and update this matrix with real results.

## Phase A — Project & infrastructure
| Item | Status |
|---|---|
| Single `ct.csproj`, net10.0, C# 12 | IMPLEMENTED, NOT TESTED |
| Modular folder structure | IMPLEMENTED |
| `CtDbContext` + `IEntityTypeConfiguration<T>` per entity | IMPLEMENTED, NOT TESTED |
| Exception handling middleware | IMPLEMENTED, NOT TESTED |
| Health check endpoint | IMPLEMENTED, NOT TESTED |
| Swagger | IMPLEMENTED, NOT TESTED |
| Dockerfile / docker-compose | IMPLEMENTED, NOT TESTED (image never built) |

## Phase B/D — Domain model & EF Core mapping (Admissions)
| Item | Status |
|---|---|
| Person + Name normalization + VirtualId | IMPLEMENTED, **TESTED** (unit) |
| Student, StudentRelation, StudentDomicile | IMPLEMENTED, TESTED (partial — see Integration tests) |
| PersonContact/Address/Tag/Occupation | IMPLEMENTED, NOT TESTED |
| Course/Term/Section/Subject + business rules | IMPLEMENTED, **TESTED** (unit) |
| StudentEnrollment + Enrol/Assign/SetCourse/SetRollNo | IMPLEMENTED, **TESTED** (unit) |
| Promotion (clone + replace, transactional) | IMPLEMENTED, TESTED (unit domain logic; integration test uses InMemory provider, not a real transaction) |

## Phase C — PostgreSQL schema
| Item | Status |
|---|---|
| Schema + tables for Admissions vertical (persons, students, relations, contacts, addresses, tags, occupations, domicile, courses/terms/sections/subjects, enrollments, event_logs) | IMPLEMENTED, NOT TESTED (SQL never executed) |
| Attendance, Fees, Opening balances, IEP, Essentials, Documents, Baseline tables | **NOT STARTED** |
| Indexes (core + trigram) | IMPLEMENTED for the tables above, NOT TESTED |
| Views (`student_search_view`, `student_institute_summary_view`) | IMPLEMENTED, NOT TESTED |
| Additional recommended views (flat data, fee summary, attendance summary, gender breakup) | **NOT STARTED** |
| Triggers (`set_modified_on`) | IMPLEMENTED for persons/students only, NOT TESTED |

## Phase E — Students & enrollment (API)
Routes implemented against spec section 50:

`GET /all`, `GET /getDto`, `GET /getStudentsByTag`, `POST /flatData`,
`POST /create`, `POST /enable`, `POST /disable`, `POST /promote`,
`POST /setRollNo`, `POST /updateCourse`, `POST /setPrimaryEmail`,
`POST /setPrimaryMobile`, `POST /updateTags`.

Routes from section 50 **NOT yet implemented** (require modules not yet
built — see below): `POST /AssignStudentId`, `POST /update` (full field
update), `POST /getFilters`, `POST /cache`, `POST /applyEvents`,
`POST /setHomeAddress`, `POST /setHomePhone`, `POST /updateGender`,
`POST /updateDomicile`, `GET /profile`, all parent
(father/mother/guardian) update/enable/disable routes, all attendance/SMS
routes, all contact/recipient routes, `GET /facet`, `GET /getDtoWithData`,
`GET /getStudentsForExamFacets`, all fees routes, all IEP routes, all
student-essentials routes, all document routes, all baseline routes,
`POST /timetable`.

## Phase F–L — Not started
Contacts/parents beyond primary email/mobile, Academics (courses API
surface), Attendance module, Fees module, Examinations module, IEP,
Essentials, Documents, Baseline, Reports, RabbitMQ/Redis/S3/GitHub
integrations, policy engines, auth middleware beyond a minimal
`IUserContext` — **NOT STARTED**. These are large, independently-specified
subsystems (spec sections 20–27, 48–51, 79–85) and were deliberately not
stubbed out with fake implementations, per rule 8 ("do not invent new
business rules") and rule 121.

## Documented assumptions (spec rule 125)
1. **Enrollment status values**: exact legacy `EnrollmentStatus` values were
   not recoverable from this standalone spec. Implemented as
   `Unknown=-1, Active=1, Disabled=99` mirroring `StudentStatus`/`CourseStatus`,
   stored as plain `smallint` so unknown legacy values round-trip. Needs
   verification against the real legacy source if/when available.
2. **"Current" enrollment for FirstOrDefault projections**: `StudentDto`
   fields like `CourseId`/`RollNo`/`SectionName` are populated from
   `s.Enrollments.Select(...).FirstOrDefault()` with no explicit ordering,
   since a student may have concurrent enrollments across institutes and the
   spec does not define a single "current" one at the Student aggregate
   level. Callers needing an institute-scoped view should query
   `student_search_view` filtered by `inst_id`, or `GetEnrollment(instId)`.
   **This needs a product decision, not a guess** — flagged rather than
   silently resolved.
3. **`GetNextStudentId` algorithm** (spec section 41): the exact legacy
   sequence rule was not recoverable. Not implemented in this pass;
   `StudentId` is currently caller-supplied only. A database-backed sequence
   strategy should be designed against real legacy ID samples before
   implementing, per the spec's own instruction to document rather than
   invent semantics.
4. Father/Mother/Guardian `Person` records are **not** auto-created on
   `POST /create`. Spec section 35 step 6 says "Initialize father/mother/
   guardian" but section 124 rule 5 says parent records are Persons, not
   duplicated fields, and creating empty Person rows for parents that were
   never supplied contact info would invent data. Parent relations are
   created lazily by the (not-yet-implemented) `updateFatherDetails` /
   `updateMotherDetails` / `updateGuardianDetails` endpoints instead.
5. `student_enrollments` has no `modified_on` column in the delivered
   schema even though other audited tables do; the trigger script
   deliberately excludes it rather than adding an undocumented column.

## Definition of Done (spec section 120) — current state
Only the items below can be honestly checked off, and only once you've
actually run the commands in this repo yourself:

- [ ] .NET 10 / C# 12 — code targets it; **not compiled in this environment**
- [ ] Application builds — **not verified**
- [ ] Unit tests pass — **not executed**
- [ ] Integration tests pass — **not executed**
- [ ] Docker builds / PostgreSQL starts — **not attempted**
- [ ] Student CRUD / Enrollment / Promotion work — implemented, unverified
- [ ] Everything else in the checklist — not started

Do not mark any of these complete until you've run the real commands and
seen real output.
