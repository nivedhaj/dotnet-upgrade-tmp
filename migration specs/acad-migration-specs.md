# Agentic Migration Steps — ct.acad

## Target

Migrate `ct.acad` from .NET Core 3.1 + RavenDB to .NET 10 + C# 14 + PostgreSQL 17+ + EF Core 10 while preserving existing API contracts, reports, policy behavior, authentication context, tenant isolation, audit fields, LMS/exam/course workflows, and integrations.

This is not a rewrite. The agent must migrate behavior module by module and verify each module before removing RavenDB.

---

## Source Snapshot From `ct.acad.zip`

### Current Platform

- Target framework: `netcoreapp3.1`
- Startup style: `Program.cs` + `Startup.cs`
- DI: ASP.NET Core DI + Autofac assembly registration
- JSON: `Microsoft.AspNetCore.Mvc.NewtonsoftJson`
- Database: RavenDB via `RavenDB.Client 4.2.103`
- Logging: NLog + RabbitMQ target
- Integrations: AWS S3, RabbitMQ, Redis, Octokit/GitHub
- Swagger: Swashbuckle 5.4.1

### Controllers

- `AnonController`
- `ArtefactController`
- `AssessmentController`
- `AttendanceController`
- `CalendarController`
- `CourseController`
- `ExamController`
- `GradingController`
- `LMSController`
- `ReportController`
- `TTPlannerController`
- `_Core/Controllers/_MonitorController`

### Raven Resource/Documents

- Core: `Entity`, `Person`, `App`, `Img`, `Ping`
- Academic: `Course`, `Exam`, `Assessment`, `AssessmentSubmission`, `Question`, `RandomQuestionSubmission`, `QATag`, `ImageTag`, `AssessmentTag`, `Grading`, `AttendanceEvent`, `Markscard`, `EventLog`, `Subject`, `Period`, `TimeTable`, `TTPlanner`, `Calendar`

### Raven Indexes

- `Ping_Index`
- `Course_Structure_Index`
- `Course_Sections_Index`
- `Exam_Index`
- `Assessment_IX`
- `AssessmentQuestionIndex`
- `AttendanceEvent_IX`
- `Question_IX`

### High-Risk Files

- `Startup.cs`: Raven `IDocumentStore`, scoped `IDocumentSession`, audit hook `OnBeforeStore`
- `_Core/Security/UserContext.cs`: exposes `IDocumentSession` and `IAsyncDocumentSession`
- `Service/ReportService.cs`: very large report surface, heavy Raven queries and `Load<Course>` calls
- `Repo/Db/Command/ExamCmd.cs`: optimistic concurrency, async Raven session, many state-changing exam operations
- `Repo/Db/Command/CourseCmd.cs`: mutates nested course/term/section/subject structures
- `Repo/Db/Command/LMSCmd.cs`: question/tag workflows
- `Repo/Db/Query/*Qry.cs`: Raven index projections and streamed queries

---

## Agent Ground Rules

1. Do not remove RavenDB until inventory, schema, data migration, and regression tests are complete.
2. Do not change public route paths, HTTP verbs, query names, body shapes, or response property names unless explicitly documented.
3. Preserve `Newtonsoft.Json` if response compatibility depends on it.
4. Preserve tenant/institute filtering everywhere. Treat any missing `OwnerId`, `InstId`, `ClientId`, or persona filter as a critical defect.
5. Preserve report outputs byte-for-byte where reports generate CSV/HTML/PDF-like artifacts.
6. Preserve policy engine results before changing persistence.
7. Use `decimal`/`numeric` for marks where precision matters. Do not introduce floating point in calculations unless the legacy model already uses it and tests pin behavior.
8. Convert Raven `LoadDocument`/`Load<T>` behavior to explicit joins, foreign keys, batch loads, or projections.
9. Convert Raven `Advanced.Stream` to EF async streaming or paged batched reads.
10. Convert Raven optimistic concurrency in `ExamCmd` to EF concurrency tokens or explicit row-version columns.

---

## Phase 0 — Prepare Workspace

1. Extract `ct.acad.zip` into a clean repo folder.
2. Delete generated artifacts from source control consideration:
   - `bin/`
   - `obj/`
   - `*.dll`, `*.pdb`, generated runtime config files
3. Create migration working docs:
   - `migration/inventory.md`
   - `migration/dependency-map.md`
   - `migration/raven-inventory.md`
   - `migration/api-inventory.md`
   - `migration/document-table-map.md`
   - `migration/index-mapping.md`
   - `migration/report-inventory.md`
   - `migration/test-plan.md`
   - `migration/cutover-runbook.md`
4. Create a new branch: `migration/dotnet10-postgres`.
5. Run the old app if possible against a Raven test database and capture golden responses for all high-value endpoints.

Acceptance:

- Source tree is clean and repeatable.
- Golden response capture process exists before code changes.

---

## Phase 1 — Dependency Migration Plan

Create `migration/dependency-map.md` with every NuGet package from `ct.acad.csproj`.

| Current Package | Current Version | Migration Action |
|---|---:|---|
| `RavenDB.Client` | 4.2.103 | Remove only after EF migration is complete |
| `Microsoft.AspNetCore.Mvc.NewtonsoftJson` | 3.1.5 | Upgrade to .NET 10 compatible version; preserve serializer behavior |
| `Swashbuckle.AspNetCore` | 5.4.1 | Upgrade or replace with ASP.NET Core OpenAPI if contract diff is acceptable |
| `Microsoft.OpenApi` | 1.1.4 | Upgrade only if still required |
| `Autofac` | 5.2.0 | Prefer built-in DI unless keyed policy registration requires Autofac; otherwise update to .NET 10-compatible Autofac |
| `Autofac.Extensions.DependencyInjection` | 6.0.0 | Same as above |
| `JWT` | 7.2.1 | Audit usage; replace with `System.IdentityModel.Tokens.Jwt` if feasible without auth behavior change |
| `NLog`, `NLog.Web.AspNetCore` | 4.x | Upgrade to current .NET 10-compatible NLog packages |
| `RabbitMQ.Client` | 6.1.0 | Upgrade only after verifying NLog RMQ target and explicit service usage |
| `StackExchange.Redis` | 2.1.55 | Upgrade and preserve cache key formats |
| `Octokit` | 0.48.0 | Upgrade if `GithubService` is used in production |
| `AWSSDK.Core`, `AWSSDK.S3` | 3.3.x | Upgrade and preserve S3 bucket/key behavior |
| `StringTemplate4` | 4.0.9-beta1 | Audit report/template usage; replace only if output golden tests pass |
| `System.Web.Http` | 4.0.0 | Remove if unused; avoid legacy Web API dependency in .NET 10 |

Acceptance:

- Every package has a usage decision.
- RavenDB is the only package planned for mandatory removal.

---

## Phase 2 — Complete API Inventory

Scan controllers and create `migration/api-inventory.md`.

For each action record:

- Controller
- Route prefix
- HTTP verb/path
- Query parameters
- Body DTO or raw `JObject/string`
- Response DTO or file response
- Auth requirements
- User context fields used
- Reads/writes
- Side effects: Redis, RabbitMQ, S3, marks card generation, audit/event logs
- Legacy status codes and error body shape

Special attention:

- `CourseController` has many mutation endpoints for terms, sections, subjects, optional subjects, class teachers, roll numbers, timetables, DTO exports, facets, and ranks.
- `ExamController` contains high-risk workflows for exam creation, marks entry, lock/unlock, grading, events, marks cards, email jobs, widgets, and student scores.
- `ReportController` has the largest output surface. Capture golden outputs for each report route before refactor.
- `CalendarController` uses `/api/academics/[controller]`, not `/api/acad/[controller]`; preserve this route unless consumers approve a change.

Acceptance:

- No controller/action is missing from the inventory.
- At least one golden request/response exists for every public route family.

---

## Phase 3 — Raven Usage Inventory

Create `migration/raven-inventory.md` by scanning all `.cs` files for:

```text
Raven
IDocumentStore
IDocumentSession
IAsyncDocumentSession
DocumentStore
OpenSession
OpenAsyncSession
Store
SaveChanges
SaveChangesAsync
Delete
Load
LoadAsync
Query
RawQuery
LoadDocument
AbstractIndexCreationTask
AbstractMultiMapIndexCreationTask
Advanced.Stream
UseOptimisticConcurrency
WaitForNonStaleResults
```

Known files to inventory:

- `Startup.cs`
- `_Core/Controllers/_MonitorController.cs`
- `_Core/Security/UserContext.cs`
- `_Core/Repo/Db/Command/PingCmd.cs`
- `_Core/Repo/Db/Query/PingQry.cs`
- `Repo/Db/Command/AssessmentCmd.cs`
- `Repo/Db/Command/AttendanceCmd.cs`
- `Repo/Db/Command/CourseCmd.cs`
- `Repo/Db/Command/ExamCmd.cs`
- `Repo/Db/Command/GradingCmd.cs`
- `Repo/Db/Command/LMSCmd.cs`
- `Repo/Db/Command/TTPlannerCmd.cs`
- `Repo/Db/Query/AssessmentQry.cs`
- `Repo/Db/Query/AttendanceQry.cs`
- `Repo/Db/Query/CourseQry.cs`
- `Repo/Db/Query/ExamQry.cs`
- `Repo/Db/Query/GradingQry.cs`
- `Repo/Db/Query/LMSQry.cs`
- `Service/LMSService.cs`
- `Service/ReportService.cs`
- `Service/TTPlannerService.cs`
- all files in `Repo/Db/Index/`

For each usage capture:

- File/class/method
- Raven operation
- Document type
- Index used
- Tenant filter used
- Sort/paging/filter behavior
- Replacement EF query/command
- Test required
- Migration status

Acceptance:

- Raven inventory has every occurrence accounted for.
- `UserContext` Raven session exposure is flagged as a blocker.

---

## Phase 4 — Modernize Host Without Changing Behavior

1. Convert project to SDK-style .NET 10 settings:

```xml
<TargetFramework>net10.0</TargetFramework>
<LangVersion>14.0</LangVersion>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

2. Move to minimal `Program.cs` or keep `Startup` temporarily, but do not mix patterns confusingly.
3. Preserve middleware order:
   - CORS
   - exception middleware
   - auth middleware
   - routing/controllers
4. Preserve exposed CORS headers:
   - `Authorization`
   - `AccessToken`
   - `CT-Context`
   - `Content-Disposition`
5. Keep custom `HtmlOutputFormatter` if reports depend on HTML output.
6. Preserve NLog initialization and RabbitMQ target behavior.
7. Replace Autofac scanning only after confirming all `Cmd`, `Qry`, `Service`, and keyed policy engines are registered equivalently.

Acceptance:

- App starts on .NET 10 with Raven still connected.
- Existing monitor ping works.
- Existing auth middleware still populates user context.

---

## Phase 5 — User Context Decoupling

Current `UserContext` exposes Raven sessions:

- `IDocumentSession Session`
- `IAsyncDocumentSession AsyncSession`

Replace this with application-only context:

```csharp
public interface IUserContext
{
    string UserId { get; }
    string InstId { get; }
    string ClientId { get; }
    string PersonaId { get; }
    IReadOnlyCollection<string> Permissions { get; }
}
```

Create a separate persistence boundary:

```csharp
public sealed class AcadDbContext : DbContext { }
```

Steps:

1. Extract session-independent user fields from `UserContext`.
2. Move document loading of client/persona/app data into `UserContextProvider` or a new `IUserContextLoader` backed first by Raven, then EF.
3. Remove direct database access from `IUserContext`.
4. Keep `ExternalState["UserId"]` audit behavior until EF audit interceptor replaces it.

Acceptance:

- `IUserContext` has no Raven types.
- Controllers/services can get tenant/user data without database session access.

---

## Phase 6 — PostgreSQL Schema Design

Create `AcadDbContext` and table mappings. Use explicit relational tables for independently queried or updated nested data. Use JSONB only for blobs that are not filtered/joined independently.

### Core Tables

- `acad_app`
- `acad_person`
- `acad_img`
- `acad_ping`
- `acad_audit_event` or keep audit columns on each table

### Course Tables

- `acad_course`
- `acad_course_term`
- `acad_course_section`
- `acad_course_subject`
- `acad_course_optional_subject`
- `acad_course_student_roll`
- `acad_course_class_teacher`
- `acad_course_timetable_period`
- `acad_course_rank`

Course has deeply nested structures and many endpoints mutate terms, sections, subjects, optional subjects, roll numbers, class teachers, ranks, and timetables. Do not store the whole `Course` document as JSONB unless a nested object is never filtered, joined, reported, or updated independently.

### Exam Tables

- `acad_exam`
- `acad_exam_content`
- `acad_exam_subject`
- `acad_exam_student`
- `acad_exam_student_score`
- `acad_exam_attendance`
- `acad_exam_remark`
- `acad_exam_event_log`
- `acad_exam_lock_entry`
- `acad_marks_card`

`ExamCmd` has high-risk updates and optimistic concurrency. Add row-version/concurrency columns to exam aggregate roots and score tables.

### Assessment/LMS Tables

- `acad_question`
- `acad_question_option`
- `acad_question_hint`
- `acad_question_tag`
- `acad_qa_tag`
- `acad_image_tag`
- `acad_assessment_tag`
- `acad_assessment`
- `acad_assessment_section`
- `acad_assessment_question`
- `acad_assessment_submission`
- `acad_assessment_answer`
- `acad_random_question_submission`

`Question_IX` loads `QATag` documents and has reduce behavior. Model tags relationally and rewrite the index as explicit joins/grouping.

### Grading/Attendance/Planner Tables

- `acad_grading`
- `acad_grading_rule`
- `acad_attendance_event`
- `acad_period`
- `acad_timetable`
- `acad_timetable_cell`
- `acad_tt_planner`

Acceptance:

- `migration/document-table-map.md` documents every resource, nested collection, PK/FK, tenant column, JSONB decision, and index.
- EF model has tenant-scoped indexes for all primary queries.

---

## Phase 7 — Audit Migration

Legacy audit behavior is in `Startup.Store_OnBeforeStore`:

- If `Entity.Audited` is true, Raven hook skips audit mutation.
- If `CreatedBy` is empty, set `CreatedOn` and `CreatedBy`.
- Otherwise set `ModifiedOn` and `ModifiedBy`.
- User ID comes from Raven session `ExternalState["UserId"]`.

EF replacement:

1. Create an `AuditableEntity` base class matching existing fields.
2. Create `SaveChangesInterceptor` or override `SaveChanges/SaveChangesAsync`.
3. Preserve `Audited` skip semantics.
4. Preserve blank-user fallback behavior if legacy uses empty string.
5. Add regression tests for create, update, audited-skip, and missing user.

Acceptance:

- Audit fields match legacy behavior for Raven golden cases.

---

## Phase 8 — Index Migration

Create `migration/index-mapping.md`.

### `Course_Structure_Index`

- Purpose: course structure projections.
- Replacement: EF projection from `acad_course`, terms, sections, subjects.
- Add indexes by owner/institute, course status, term/section names.

### `Course_Sections_Index`

- Purpose: check course section assignment and section lookups.
- Replacement: `acad_course_section` queries and unique constraints.

### `Exam_Index`

- Has `LoadDocument<Course>(exam.CourseId)`.
- Replacement: join `acad_exam` to `acad_course`.
- Add indexes on owner, course id, term, section, subject, status, exam date.

### `Assessment_IX`

- Multi-map index over assessments and tag data.
- Uses `LoadDocument<AssessmentTag>(assessment.Tags)`.
- Replacement: `assessment` + `assessment_tag` join table + grouped projection.

### `AssessmentQuestionIndex`

- Replacement: direct joins from assessment sections/questions to question table.

### `AttendanceEvent_IX`

- Replacement: `acad_attendance_event` indexes by institute/course/term/section/date/period.

### `Question_IX`

- Has `LoadDocument<QATag>(question.TagList)` and reduce logic.
- Replacement: normalized `question_tag` table and grouped EF/SQL view if needed.

### `Ping_Index`

- Replace with health check/ping table or remove if only monitor diagnostics.

Acceptance:

- No Raven index is mechanically copied.
- Each index has an EF query, SQL equivalent, PostgreSQL indexes, and regression test.

---

## Phase 9 — Command Migration Order

Migrate write flows in this order, because later modules depend on earlier data.

### 9.1 Ping

- Migrate `PingCmd` and `PingQry` first.
- Use as smoke test for EF, audit, tenant, and health checks.

### 9.2 Course

Migrate `CourseCmd` fully:

- Create course
- Update course
- Enable/disable
- New/update/rename/remove term
- Add/update/remove section
- Add/remove subject
- Optional subject workflows
- Class teacher assignment
- Student roll assignment
- Clone course
- Time table period-wise save
- Rank/order updates

Tests:

- Course clone preserves nested structure.
- Removing term/section cascades exactly as legacy.
- Optional/regular subject transitions preserve existing status semantics.
- Cross-institute users cannot mutate another institute course.

### 9.3 Grading

- Migrate `GradingCmd` and `GradingQry`.
- Preserve active grading rule selection.
- Add uniqueness constraint for active grading per owner if legacy enforces it.

### 9.4 Exam

Migrate `ExamCmd` carefully:

- Exam creation from courses
- Exam update/status transitions
- Marks entry
- Marks lock/unlock
- Attendance updates
- Remarks
- Event application
- Async event application
- Grading application
- Marks card generation trigger
- Result date updates

Concurrency:

- Replace Raven `UseOptimisticConcurrency = true` with EF concurrency token on `acad_exam` and score/event rows.
- Add conflict tests with two contexts updating the same exam.

Transactions:

- Use database transactions for multi-exam or multi-student writes.
- Avoid saving inside tight loops unless legacy behavior requires partial commits; document any preserved partial-commit behavior.

### 9.5 Assessment

Migrate `AssessmentCmd`:

- Create/update assessment
- Subject/duration/total marks updates
- Publish/unpublish/archive/unarchive
- Submit assessment
- Evaluate assessment
- Refresh assessment
- Tag enable/disable/update/create

### 9.6 LMS

Migrate `LMSCmd`:

- QA tags and image tags
- Question create/update
- Enable/disable/archive/unarchive
- Publish/unpublish
- Bulk tag update
- Random questions and answer submission
- Remodel question

### 9.7 Attendance

- Migrate attendance event creation and apply-events.
- Preserve date/period/course/section semantics.

### 9.8 TT Planner and Calendar

- Migrate planner commands only after course/period/time-table schema is stable.
- Calendar files are partly empty/stubbed; preserve visible endpoints and document any no-op behavior.

Acceptance:

- Each command module has unit tests and at least one API regression test.
- Raven command classes are deleted only after EF equivalents pass tests.

---

## Phase 10 — Query Migration Order

For read-only queries use `AsNoTracking()` and DTO projections.

1. `PingQry`
2. `CourseQry`
3. `GradingQry`
4. `AttendanceQry`
5. `ExamQry`
6. `AssessmentQry`
7. `LMSQry`
8. `ReportService`
9. `LMSService`
10. `TTPlannerService`

Required replacements:

- Raven `Query<T>()` → `DbSet<T>.AsNoTracking()`
- Raven indexed query `Query<Result, Index>()` → EF projection or SQL view
- Raven `Load<T>(id)` inside loops → batched query or join
- Raven `Load<T>(ids)` → `Where(x => ids.Contains(x.Id))`
- Raven `Advanced.Stream(qry)` → `AsAsyncEnumerable()` or paged query

Acceptance:

- No migrated query performs an N+1 load for courses, exams, tags, or students.
- All queries include tenant filters.
- Query output golden tests pass.

---

## Phase 11 — Report Migration

`ReportService.cs` is the largest risk area and must be migrated after Course and Exam query parity exists.

Create `migration/report-inventory.md` for every `ReportController` route:

- `getCourseReport`
- `todaysEvents`
- `allAPlus`
- `holidayList`
- `upcomingEvents`
- `consolidatedMarks`
- `rankList`
- `resultAtAGlance`
- `subjectTopScorers`
- `topTen`
- `examAttendance`
- `coursewiseResultAnalysis`
- `coursewiseExamwisePerformanceAsJson`
- `consolidatedReport`
- `examwiseFailures`
- `examList`
- `examListAsJson`
- `fullMarks`
- `gradewiseStatistics`
- `resultAnalysis`
- `resultAtAGlanceSummary`
- `subjectAverage`
- `studentPerformance`
- `studentPerformanceCSV`
- `failures`
- `GetMarksCard`
- `hallTicket`
- `noDueCertificate`
- `assessment`
- `gradewiseStatisticsAsJson`

For each report:

1. Capture Raven golden output.
2. Identify source entities and indexes.
3. Rewrite as EF projection or SQL view.
4. Preserve ordering, rounding, absent/barred/NA handling, grade mapping, and file content type.
5. Add regression test comparing JSON/CSV/HTML payload to golden output.

Acceptance:

- Every report has a golden test.
- Large reports are paged/streamed without loading entire database into memory.

---

## Phase 12 — Policy Engine Preservation

Policy engines are not persistence code but affect academic calculations and report behavior.

Inventory:

- `DefaultPolicyEngine`
- `BITSCHPolicyEngine`
- `BTLCOLPolicyEngine`
- `BTLSCHPolicyEngine`
- `PPECKNPolicyEngine`
- `PPECPSPolicyEngine`
- `VVSGCSPolicyEngine`
- `VVSSPUPolicyEngine`

Steps:

1. Add unit tests for each policy engine using known exam/course/marks inputs.
2. Preserve keyed registration by `PolicyEngineEnum`.
3. If removing Autofac, implement keyed service resolution using .NET 10 keyed services or a provider dictionary.
4. Run report golden tests for each institute/persona variant represented by policy engines.

Acceptance:

- Policy results are unchanged before and after persistence migration.

---

## Phase 13 — Authentication, Authorization, and Tenant Isolation

Files:

- `_Core/Middleware/Auth.cs`
- `_Core/Security/UserContext.cs`
- `_Core/Security/UserContextDTO.cs`
- `_Core/Security/PersonaContextDto.cs`
- `_Core/Security/AppService.cs`

Steps:

1. Preserve token parsing and response headers.
2. Preserve `CT-Context`, `Authorization`, and `AccessToken` semantics.
3. Remove Raven session from auth context.
4. Add a global query-filter strategy for tenant tables where safe.
5. Add explicit tenant predicates where global filters do not apply.
6. Add cross-institute tests for Course, Exam, Assessment, Question, Grading, Attendance, Reports.

Acceptance:

- A user from institute A cannot read or mutate institute B data.
- Anonymous routes remain anonymous only where legacy allowed them.

---

## Phase 14 — EF Core Infrastructure

Create:

- `Infrastructure/AcadDbContext.cs`
- `Infrastructure/AuditSaveChangesInterceptor.cs`
- `Infrastructure/EntityConfigurations/*`
- `Infrastructure/DependencyInjection.cs`
- `Infrastructure/Migrations/*`

Register:

```csharp
services.AddDbContext<AcadDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("Postgres")));
```

Guid/string ID strategy:

- Legacy Raven IDs are strings. Preserve legacy IDs as string columns if external clients send them.
- Add surrogate UUID PKs only if every API remains compatible and a mapping table exists.
- Recommended: store `id text primary key` for migrated legacy documents and use relational child IDs for new child tables.

Acceptance:

- EF migrations create schema from empty PostgreSQL database.
- Legacy document IDs remain resolvable by API.

---

## Phase 15 — Data Migration Utility

Create a migration tool, preferably separate from the API:

- `tools/Ct.Acad.Migrator`

Inputs:

- Raven export JSON, or direct Raven connection in read-only mode
- PostgreSQL connection string
- tenant/client mapping configuration

Order:

1. Core/person/app/image data required by auth/context
2. Courses and nested structure
3. Grading rules
4. Questions and tags
5. Assessments and submissions
6. Exams and marks/events/attendance/remarks
7. Attendance events
8. Markscards
9. Time tables and planner data
10. Ping/diagnostic data only if needed

Utility requirements:

- Idempotent upsert by legacy Raven ID
- Per-document error capture
- Row counts by document type
- Checksum/totals for exams, assessments, marks, and reports
- Dry-run mode
- Resume support
- Migration summary JSON and Markdown output

Acceptance:

- Running migration twice does not duplicate data.
- Counts and totals reconcile against Raven.

---

## Phase 16 — Testing Strategy

Create projects:

- `ct.acad.UnitTests`
- `ct.acad.IntegrationTests`
- `ct.acad.ApiTests`
- `ct.acad.RegressionTests`

Minimum tests:

### Unit

- Course structure mutations
- Exam marks calculations
- Grading rules
- Policy engines
- Assessment scoring/evaluation
- LMS question status transitions
- Audit interceptor

### Integration

- EF schema migration applies to PostgreSQL
- Tenant filters
- Concurrency conflicts
- Multi-write transaction rollback
- Raven import utility idempotency

### API

- Every controller route from inventory
- Auth required/anonymous behavior
- Status code compatibility
- JSON property compatibility
- File/report content type compatibility

### Regression

- Golden JSON for courses, exams, LMS, assessments
- Golden CSV/HTML/report outputs
- Golden marks card/hall ticket/no-due certificate outputs

Acceptance:

- `dotnet test` passes locally and in Docker/CI.
- Regression tests fail on any route/body/output drift.

---

## Phase 17 — Docker and Local Dev

Add:

- `Dockerfile`
- `docker-compose.yml`
- `.dockerignore`
- PostgreSQL 17 service
- Optional Redis/RabbitMQ services if integration tests need them

Compose services:

- `api`
- `postgres`
- `redis`
- `rabbitmq`

Acceptance:

- `docker compose up --build` starts API and PostgreSQL.
- `/api/acad/_Monitor/ping` or replacement health endpoint returns OK.
- EF migrations apply automatically only if the deployment policy allows it; otherwise document manual migration command.

---

## Phase 18 — Raven Removal

Only after all tests pass:

1. Remove `RavenDB.Client` package.
2. Remove all `using Raven.*` statements.
3. Delete or archive `Repo/Db/Index/*` Raven index classes after mapping is complete.
4. Remove `IDocumentStore`, `IDocumentSession`, `IAsyncDocumentSession` registrations.
5. Remove Raven config from `appsettings*.json`.
6. Remove Raven certificate setup from startup.
7. Remove Raven-specific monitor endpoint `db/index` or replace with EF migration/health diagnostics.
8. Run `rg "Raven|IDocumentSession|IDocumentStore|IAsyncDocumentSession|AbstractIndex|LoadDocument"` and require zero production matches.

Acceptance:

- Production code contains no RavenDB dependency.
- Migration docs keep historical Raven index mappings.

---

## Phase 19 — Cutover Runbook

Create `migration/cutover-runbook.md`:

1. Freeze writes to Raven or enable dual-write only if explicitly implemented and tested.
2. Export Raven data.
3. Run migrator dry-run.
4. Run migrator live.
5. Compare counts:
   - courses
   - exams
   - exam scores
   - assessments
   - submissions
   - questions
   - tags
   - attendance events
   - markscards
6. Run report checksum suite.
7. Run API smoke tests.
8. Switch connection strings and deploy .NET 10 app.
9. Monitor logs, RabbitMQ, Redis, PostgreSQL connections, and report latency.
10. Keep Raven read-only backup through agreed rollback window.

Rollback:

- Restore previous app version.
- Re-enable Raven writes.
- Do not attempt reverse migration unless explicitly built and tested.

---

## Phase 20 — Definition of Done

The migration is complete only when:

- App targets `net10.0` and builds cleanly.
- RavenDB is absent from production code and package references.
- PostgreSQL is the production persistence store.
- EF Core migrations/schema are present.
- All API routes are inventoried and compatibility-tested.
- All Raven indexes have documented EF/SQL replacements.
- Reports have golden regression coverage.
- Tenant isolation tests pass.
- Audit behavior matches legacy semantics.
- Exam concurrency behavior is preserved.
- Migrator is idempotent and reconciles counts/totals.
- Docker Compose starts the app with PostgreSQL 17+.
- `dotnet test` passes.
- Cutover and rollback docs exist.

---

## Recommended Agent Execution Prompt

Use this prompt for the implementing agent:

```text
You are migrating ct.acad from .NET Core 3.1 + RavenDB to .NET 10 + EF Core 10 + PostgreSQL 17+. Follow migration/ct-acad-agentic-migration-steps.md exactly. Do not remove RavenDB until inventories, schema, data migration, and regression tests are complete. Preserve all routes, DTO JSON shapes, policy behavior, reports, tenant isolation, audit behavior, and integrations. Work module-by-module in this order: host modernization, user-context decoupling, EF infrastructure, Ping, Course, Grading, Exam, Assessment, LMS, Attendance, TTPlanner/Calendar, Reports, migrator, Raven removal, Docker, cutover docs. After each module, add tests and run build/test. Record every unavoidable API or output difference in migration/api-compatibility-notes.md.
```
