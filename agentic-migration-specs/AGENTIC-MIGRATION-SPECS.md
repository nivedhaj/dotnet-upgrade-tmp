# Agentic Migration Specs (self-contained)

Each `*-agentic-migration-spec.md` includes:

1. Platform target (.NET 10 / C# 14 / EF Postgres)
2. Full endpoint inventory
3. Cmd/Qry signatures
4. Domain properties & enums
5. **Business rules (authoritative)** — cross-cutting + per-method signals from legacy Cmd/Handlers

**No legacy zip is required** if the agent follows these files + `00-AGENT-PROTOCOL.md`.

| File | Module |
|---|---|
| [fee-agentic-migration-spec.md](./fee-agentic-migration-spec.md) | Fee |
| [stu-agentic-migration-spec.md](./stu-agentic-migration-spec.md) | Student |
| [staff-agentic-migration-spec.md](./staff-agentic-migration-spec.md) | Staff |
| [scheduler-agentic-migration-spec.md](./scheduler-agentic-migration-spec.md) | Scheduler |
| [asset-agentic-migration-spec.md](./asset-agentic-migration-spec.md) | Asset |
| [accounting-agentic-migration-spec.md](./accounting-agentic-migration-spec.md) | Accounting |
| [admission-agentic-migration-spec.md](./admission-agentic-migration-spec.md) | Admission |
| [library-agentic-migration-spec.md](./library-agentic-migration-spec.md) | Library |
| [connect-agentic-migration-spec.md](./connect-agentic-migration-spec.md) | Connect |
| [gr-agentic-migration-spec.md](./gr-agentic-migration-spec.md) | Gr |
| [arte-agentic-migration-spec.md](./arte-agentic-migration-spec.md) | Arte |
| [acad-agentic-migration-spec.md](./acad-agentic-migration-spec.md) | Acad |

---

# CT Modular Migration — Agentic Protocol

## Goal
Migrate each legacy RavenDB (or event-sourced) service into a **.NET 10 / C# 14** modular project under `CtModularApp`, using **EF Core + PostgreSQL** only. No RavenDB. No Marten unless the product owner explicitly reintroduces event sourcing.

## Target stack
| Item | Value |
|---|---|
| Runtime | .NET 10 |
| Language | C# 14 (`nullable` enable, primary constructors OK) |
| Host | `Ct.Host` ASP.NET Core |
| Shared | `Ct.Shared` (headers, extensions) |
| Persistence | `Microsoft.EntityFrameworkCore` + `Npgsql.EntityFrameworkCore.PostgreSQL` 10.x |
| Events (optional) | RabbitMQ JSON publish — not Raven subscriptions |
| Auth context | Headers `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` |

## Module layout (mandatory)
```
Ct.{Module}/
  Ct.{Module}.csproj
  {Module}Module.cs          // Add{Module}Module + Map{Module}Endpoints
  Domain/                    // POCOs, enums
  Infrastructure/            // *DbContext, jsonb converters
  Application/               // *Service, *Cmd, *Qry facades
  Controllers/               // mirror legacy controller names where practical
```

## Extraction order (from old code)
1. **Controllers** → list every `[HttpGet|Post|Put|Delete]` route + action name. **Do not skip.**
2. **Repo/Db/Command (*Cmd.cs)** → write methods → `*Service` / `*Cmd` facade.
3. **Repo/Db/Query (*Qry.cs)** → read methods → `*Service` / `*Qry` facade.
4. **Repo/Resource** (or CQRS Aggregate) → domain entities + enums.
5. **Indexes** (`*_IX.cs`) → EF indexes or report projections (replace Raven map/reduce).
6. **Service/** (ReportService, RabbitMQ enqueue) → ReportService JSON/HTML stubs; `I*EventPublisher` for jobs.
7. **Startup/Program** → register `Add{Module}Module` + `AddApplicationPart` in `Ct.Host`.

## RavenDB → PostgreSQL mapping rules
| Raven pattern | Postgres / EF pattern |
|---|---|
| `IDocumentSession` Load/Query/Store | `DbContext` + `DbSet<T>` |
| Document id string | `Guid Id` (or keep string only if external refs require it) |
| Nested lists/dicts on document | `jsonb` columns + `HasConversion` + `ValueComparer` |
| Map/reduce index | LINQ group-by report methods or SQL views later |
| `session.Advanced.DocumentStore` | Connection string `Postgres` / `DefaultConnection` |
| Multi-tenant via `OwnerId` / `InstId` | Always filter `OwnerId == user.OwnerId` |
| Event sourcing stream (Asset/Library/Accounting legacy) | **Current-state tables** unless owner asks for event store |
| Rabbit `ct_xt` / `ct_x` jobs | Optional `I*EventPublisher` with same routing keys |

## Controller parity checklist
- [ ] Every legacy action method name exists on modular controllers (case-insensitive match OK for C#).
- [ ] Routes prefer legacy path segments (`disable`, `setTags`, …).
- Dual routes allowed: `api/{module}/...` and legacy `api/{Controller}`.
- Commented-out legacy endpoints may be restored as optional extras; document them.

## Anti-goals
- Do not leave Marten/Raven packages in csproj.
- Do not claim 1:1 print-engine HTML (Github StringTemplate) unless templates are provided.
- Do not skip Cmd/Qry methods that have no HTTP surface — expose as service methods and/or extra routes.

## Verification
```text
Compare: count(legacy Http* actions) vs count(modular Http* actions)
Target: 100% method-name coverage for active (non-commented) endpoints
```

## Deliverable per module
1. Spec file in `migration-specs/` (this folder).
2. `Ct.{Module}` project wired into Host.
3. Updated `CtModularApp-updated.zip`.


## Business rules section (mandatory in every module spec)

Each module spec contains a **"Business rules (authoritative)"** section derived from legacy Cmd/Qry/Handler bodies.

**Agent instructions:**
1. Endpoint tables define **names and routes only**.
2. **Business rules** define algorithms, preconditions, status changes, and side effects.
3. If a method has a rules entry, implement **exactly those constraints** (adapt Raven `session` calls to EF `SaveChangesAsync`).
4. If a method has **no** rules entry: OwnerId-scoped CRUD only; list it under `RULES NEEDED` in your summary.
5. Never invent fee math, seat counts, stock limits, or voucher balance rules beyond what is written.
6. Replace `session.Store` / `eventStore.Save` with EF; replace `Enque*` with optional `IEventPublisher` when a side effect is listed.


---

# Detailed Migration Spec: Acad

**Legacy:** `ct.acad` → **Target:** `Ct.Acad`  
**HTTP actions to implement:** 175  
**Notes:** Courses, exams, attendance, LMS — largest surface

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### AnonController.cs
- Route prefix: `api/acad/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/questions` | `GetQuestionsForAnon` |  |
| Get | `/random` | `GetRandomQuestions` |  |
| Post | `/random` | `SubmitAnswer` | [FromBody] RandomQuestionsAnswerDto dto |
| Get | `/assessment/tags` | `GetTags` |  |
| Get | `/assessment/publishedAssessments` | `GetPublishedAssessments` |  |
| Get | `/assessment/assessmentById` | `GetAssessmentById` | [FromQuery] string id |

### ArtefactController.cs
- Route prefix: `api/acad/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/StudentAttendanceTemplate` | `StudentAttendanceTemplate` | [FromBody]JObject jObj |

### AssessmentController.cs
- Route prefix: `api/acad/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `CreateAssessment` | [FromBody] CreateAssessmentDto dto |
| Get | `/` | `GetAssessments` |  |
| Get | `/assessmentById` | `GetAssessmentById` | [FromQuery] string id |
| Get | `/publishedAssessments` | `GetPublishedAssessments` |  |
| Post | `/update` | `UpdateAssessment` | [FromBody] UpdateAssessmentDto dto |
| Post | `/updateSubject` | `UpdateAssessmentSubject` | [FromBody] UpdateAssessmentSubjectDto dto |
| Post | `/updateDuration` | `UpdateAssessmentDuration` | [FromBody] UpdateAssessmentDurationDto dto |
| Post | `/updateTotalMarks` | `UpdateAssessmentTotalMarks` | [FromBody] UpdateAssessmentTotalMarksDto dto |
| Post | `/publish` | `PublishAssessment` | [FromQuery] string id |
| Post | `/unpublish` | `UnPublishAssessment` | [FromQuery] string id |
| Post | `/archive` | `ArchiveAssessment` | [FromQuery] string id |
| Post | `/unarchive` | `UnarchiveAssessment` | [FromQuery] string id |
| Post | `/submit` | `SubmitAssessment` | [FromBody] AssessmentSubmitDto dto |
| Get | `/submissions` | `GetAssessmentSubmissions` | [FromQuery] string id |
| Post | `/evaluate` | `EvaluateAssessment` | [FromBody] AssessmentEvaluationDto dto |
| Get | `/studentSumbissions` | `StudentSubmissions` | [FromQuery] string studentId |
| Post | `/refresh` | `Refresh` | [FromBody] AssessmentRefreshDto dto |
| Get | `/submissionsCount` | `GetSubmissionsCount` |  |
| Get | `/getAssessmentByQuestionIds` | `GetAssessmentsByQuestionIds` | [FromQuery] string ids |
| Post | `/disableTag` | `DisableTag` | [FromQuery] string id |
| Post | `/enableTag` | `EnableTag` | [FromQuery] string id |
| Post | `/updateTag` | `UpdateTag` | [FromBody] UpdateQATagDto dto |
| Post | `/tag` | `CreateTag` | [FromBody] CreateQATagDto dto |
| Get | `/tags` | `GetTags` |  |

### AttendanceController.cs
- Route prefix: `api/acad/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `Post` | [FromBody]string Id |
| Get | `/absentees` | `Absentees` | [FromQuery]string InstId, string CourseId, string TermName, string SecName, int Period, DateTime Date |
| Get | `/absenteeList` | `GetAbsenteesOn` |  |
| Post | `/applyEvents` | `ApplyEvents` | [FromBody]List<EventLog> events |

### CalendarController.cs
- Route prefix: `api/academics/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/addCalendarRule` | `Post` | [FromBody]string Id |
| Post | `/getCalendarEvents` | `GetCalendarEvents` | [FromBody]string Id |
| Post | `/generateCalendar` | `GenerateCalendar` | [FromBody]string Id |
| Post | `/removeCalendarRules` | `RemoveCalendarRules` | [FromBody]string Id |
| Post | `/removeOccurrence` | `RemoveOccurrence` | [FromBody]string Id |

### CourseController.cs
- Route prefix: `api/acad/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] bool isDisabled = false |
| Get | `/getFilters` | `GetFilters` |  |
| Post | `/` | `Post` | [FromBody] CreateCourseDto json |
| Post | `/updateCourse` | `UpdateCourse` | [FromBody] UpdateCourseDto json |
| Post | `/enable` | `Enable` | [FromQuery] string Id |
| Post | `/disable` | `Disable` | [FromQuery] string Id |
| Post | `/cache` | `Cache` | [FromQuery] bool reset = false |
| Post | `/newTerm` | `NewTerm` | [FromBody] NewTermDto json |
| Post | `/updateTerm` | `UpdateTerm` | [FromBody] UpdateTermDto json |
| Post | `/renameTerm` | `RenameTerm` | [FromBody] RenameTermDto json |
| Post | `/removeTerm` | `RemoveTerm` | [FromQuery] string Id, string termName |
| Post | `/addSection` | `NewSection` | [FromBody] NewSectionDto json |
| Post | `/updateSection` | `RenameSection` | [FromBody] UpdateSectionDto json |
| Post | `/removeSection` | `RemoveSection` | [FromQuery] string id, string termName, string secName |
| Post | `/addSubject` | `NewSubject` | [FromBody] NewSubjectDto json |
| Post | `/removeSubject` | `RemoveSubject` | [FromBody] RemoveSubjectDto json |
| Post | `/addClassTeacher` | `SetClassTeacher` | [FromBody] ClassTeacherDto json |
| Post | `/clone` | `Clone` | [FromBody] CloneCourseDto json |
| Post | `/addOptionalSubject` | `AddOptionalSubject` | [FromBody] string Id |
| Post | `/assignRollNumberToStudent` | `AssignRollNumberToStudent` | [FromBody] string Id |
| Post | `/assignStudents` | `AssignStudents` | [FromBody] string Id |
| Get | `/getStudents` | `GetStudents` | [FromBody] string Id |
| Get | `/getTimeTablePeriodwise` | `GetTimeTablePeriodwise` | [FromQuery] string id, [FromQuery] string termName, [FromQuery] string secName |
| Post | `/makeSubjectOptional` | `MakeSubjectOptional` | [FromBody] string Id |
| Post | `/makeSubjectRegular` | `MakeSubjectRegular` | [FromBody] string Id |
| Post | `/marksCardTemplate` | `MarksCardTemplate` | [FromBody] string Id |
| Post | `/saveTimeTablePeriodWise` | `SaveTimeTablePeriodWise` | string id, [FromBody] JObject jObj |
| Post | `/updateOptionalSubject` | `UpdateOptionalSubject` | [FromBody] string Id |
| Post | `/forInstitute` | `ForInstitute` | [FromBody] string Id |
| Post | `/setExamSubjectOrder` | `SetExamSubjectOrder` | [FromQuery] string id, [FromBody] List<string> examSubjectOrder |
| Get | `/getDto` | `GetDto` |  |
| Get | `/getDtoWithData` | `GetDtoWithData` | [FromQuery] string format = "json" |
| Get | `/facet` | `CourseFacet` |  |
| Post | `/setRank` | `CourseRank` | [FromBody] CourseRankDto json |

### ExamController.cs
- Route prefix: `api/acad/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string id = "", string status = "" |
| Post | `/` | `Post` | [FromBody] JObject jObj |
| Post | `/createExam` | `CreateExam` | [FromBody] JObject jObj |
| Post | `/renameExam` | `RenameExam` | [FromQuery] string id, [FromQuery] string name, [FromQuery] string newName |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Post | `/lockExam` | `LockExam` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/unlockExam` | `UnlockExam` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/SetTotalMaxMarks` | `SetTotalMaxMarks` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/setMaxMarks` | `SetMaxMarks` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/setMinMarks` | `SetMinMarks` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/setScheduledOn` | `SetScheduledOn` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/setConductedOn` | `SetConductedOn` | [FromQuery] string id, [FromBody] JObject jObj |
| Get | `/getExamContents` | `GetExamContents` | [FromQuery] string id |
| Post | `/setDaysWorked` | `SetDaysWorked` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/setDaysPresent` | `SetDaysPresent` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/setEvaluatedOn` | `SetEvaluatedOn` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/subjectGrading` | `SubjectGrading` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/RemoveChildSubject` | `RemoveChildSubject` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/AddChildSubject` | `AddChildSubject` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/ForStudent` | `ForStudent` | [FromBody] JObject jObj |
| Post | `/CopyExamContents` | `CopyExamContents` | [FromQuery] string id, [FromBody] List<CourseInExamDTO> courses |
| Get | `/getStudentsToEvaluate` | `GetStudentsToEvaluate` | [FromQuery] string id |
| Post | `/attendance` | `Attendance` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/evaluate` | `Evaluate` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/daysPresent` | `DaysPresent` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/remarks` | `ExamRemarks` | [FromQuery] string id, [FromBody] JObject jObj |
| Post | `/ApplyEvents` | `ApplyEvents` | [FromQuery] string id, [FromBody] List<EventLog> events |
| Post | `/applyEventsAsync` | `ApplyEventsAsync` | [FromQuery] string id, [FromBody] List<EventLog> events |
| Get | `/GetExamsCoursewise` | `GetExamsCoursewise` |  |
| Get | `/GetGradingDetails` | `GetGradingDetails` |  |
| Get | `/facet` | `Facet` |  |
| Post | `/enqueMarksCardJob` | `EnqueMarksCardJob` | [FromBody] EnqueMarksCardDto json |
| Post | `/generateMarksCard` | `GenerateMarksCard` | [FromBody] ReqMarksCardDto json |
| Post | `/getMarksCard` | `GetMarksCard` | [FromBody] ReqMarksCardDto json |
| Post | `/enqueEmailMarksCardJob` | `EnqueMarksCardJob` | [FromBody] EnqueMarksCardEmailDto json |
| Get | `/getExams` | `GetExams` | [FromQuery] string status = "" |
| Get | `/getStudents` | `GetStudents` | [FromQuery] string examId |
| Get | `/GetStudentScores` | `GetStudentScores` | [FromQuery] string studentId |
| Post | `/widget/studentPerformance` | `GetStudentPerformanceWidgetData` | [FromBody] StudentPerfDto dto |
| Post | `/setResultDate` | `SetResultDate` | [FromBody] JObject jObj |

### GradingController.cs
- Route prefix: `api/acad/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/getGradingDetails` | `GetGradingDetails` |  |
| Post | `/enableGrading` | `EnableGrading` | [FromQuery]string id |
| Post | `/disableGrading` | `DisableGrading` | [FromQuery]string id |
| Post | `/setGradingRules` | `SetGradingRules` | [FromBody]JObject jObj |

### LMSController.cs
- Route prefix: `api/acad/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/tag` | `CreateTag` | [FromBody] CreateQATagDto dto |
| Post | `/imageTag` | `CreateImageTag` | [FromBody] CreateQATagDto dto |
| Post | `/question` | `CreateQuestion` | [FromBody] CreateQuestionDto json |
| Get | `/tags` | `GetTags` |  |
| Get | `/imageTags` | `GetImageTags` |  |
| Get | `/questions` | `GetQuestions` |  |
| Post | `/updateTag` | `UpdateTag` | [FromBody] UpdateQATagDto dto |
| Post | `/updateQuestion` | `UpdateQuestion` | [FromBody] UpdateQuestionDto dto |
| Post | `/disableTag` | `DisableTag` | [FromQuery] string id |
| Post | `/enableTag` | `EnableTag` | [FromQuery] string id |
| Post | `/disableQuestion` | `DisableQuestion` | [FromQuery] string id |
| Post | `/enableQuestion` | `EnableQuestion` | [FromQuery] string id |
| Post | `/publish` | `PublishQuestions` | [FromBody] List<string> ids |
| Post | `/unpublish` | `UnpublishQuestions` | [FromBody] List<string> ids |
| Post | `/updateQuestionsTags` | `UpdateQuestionsTags` | [FromBody] List<UpdateQuestionTagsDto> json |
| Post | `/archive` | `ArchiveQuestion` | [FromQuery] string id |
| Post | `/unarchive` | `UnArchiveQuestion` | [FromQuery] string id |
| Get | `/random` | `GetRandomQuestions` |  |
| Post | `/random` | `SubmitAnswer` | [FromBody] RandomQuestionsAnswerDto dto |
| Post | `/remodelQuestion` | `RemodelQuestion` | [FromBody] RemodelQuestion dto |

### ReportController.cs
- Route prefix: `api/acad/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/getCourseReport` | `GetCourseListReport` | [FromBody] JObject jObj |
| Post | `/todaysEvents` | `TodaysEvents` | [FromBody] JObject jObj |
| Post | `/allAPlus` | `AllAPlus` | [FromBody] JObject jObj |
| Post | `/holidayList` | `HolidayList` | [FromBody] JObject jObj |
| Post | `/upcomingEvents` | `UpcomingEvents` | [FromBody] JObject jObj |
| Post | `/consolidatedMarks` | `ConsolidatedMarks` | [FromBody] JObject jObj |
| Post | `/rankList` | `RankList` | [FromBody] JObject jObj |
| Post | `/resultAtAGlance` | `ResultAtAGlance` | [FromBody] JObject jObj |
| Post | `/subjectTopScorers` | `SubjectTopScorers` | [FromBody] JObject jObj |
| Post | `/topTen` | `TopTen` | [FromBody] JObject jObj |
| Post | `/examAttendance` | `ExamAttendance` | [FromBody] JObject jObj |
| Post | `/coursewiseResultAnalysis` | `CoursewiseResultAnalysis` | [FromBody] JObject jObj |
| Post | `/coursewiseExamwisePerformanceAsJson` | `CoursewiseResultAnalysisAsJson` |  |
| Post | `/consolidatedReport` | `ConsolidatedReport` | [FromBody] JObject jObj |
| Post | `/examwiseFailures` | `ExamwiseFailures` | [FromBody] JObject jObj |
| Post | `/examList` | `ExamList` | [FromBody] JObject jObj |
| Post | `/examListAsJson` | `ExamListAsJson` |  |
| Post | `/fullMarks` | `FullMarks` | [FromBody] JObject jObj |
| Post | `/gradewiseStatistics` | `GradewiseStatistics` | [FromBody] JObject jObj |
| Post | `/resultAnalysis` | `ResultAnalysis` | [FromBody] JObject jObj |
| Post | `/resultAtAGlanceSummary` | `ResultAtAGlanceSummary` | [FromBody] JObject jObj |
| Post | `/subjectAverage` | `SubjectAverage` | [FromBody] JObject jObj |
| Post | `/studentPerformance` | `StudentPerformance` | [FromBody] JObject jObj |
| Post | `/studentPerformanceCSV` | `StudentPerformanceCSV` | [FromBody] JObject jObj |
| Post | `/failures` | `Failures` | [FromBody] JObject jObj |
| Post | `/GetMarksCard` | `GetMarksCard` | [FromBody] ReqMarksCardDto json |
| Post | `/hallTicket` | `HallTicket` | [FromBody] HallTicketDto dto |
| Post | `/noDueCertificate` | `NoDueCertificate` | [FromBody] NoDueCertificateDto dto |
| Post | `/assessment` | `Assessment` | [FromQuery] string id |
| Get | `/gradewiseStatisticsAsJson` | `GradewiseStatisticsAsJson` |  |

### TTPlannerController.cs
- Route prefix: `api/acad/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/rule` | `Rule` | [FromBody]string Id |
| Post | `/run` | `Run` | [FromBody] JObject jObj |
| Post | `/generate` | `GenerateTT` | [FromBody] JObject jObj |
| Post | `/cancel` | `Cancel` | [FromBody] JObject jObj |
| Get | `/` | `Get` | [FromQuery]string Id |
| Post | `/report/timetable` | `PrintTimetable` | [FromBody]JObject jObj |

## Command layer (*Cmd) — implement all
### AssessmentCmd.cs
- `Assessment CreateAssessment(string id, string subject, string subjectCode, string description, int duration, decimal totalMarks, List<AssessmentSection> sections, bool multipleAttempts = false, List<string> tags = null, string i)`
- `Assessment UpdateAssessment(string id, string subject, string subjectCode, string description, decimal totalMarks, int duration, List<AssessmentSection> sections, bool multipleAttempts = false, List<string> tags = null)`
- `Assessment UpdateSubject(string id, string subject)`
- `Assessment UpdateDuration(string id, TimeSpan duration)`
- `Assessment UpdateDescription(string id, string description)`
- `Assessment UpdateTotalMarks(string id, decimal totalMarks)`
- `void PublishAssessment(string id)`
- `void UnPublishAssessment(string id)`
- `void ArchiveAssessment(string id)`
- `void UnarchiveAssessment(string id)`
- `AssessmentSubmission SubmitAssessment(string studentId, string assessmentId, List<QuestionsAnswered> questionsAnswered, string instId = "")`
- `AssessmentSubmission EvaluateAssessment(string id, string evaluatedBy, List<QuestionsAnswered> questionsAnswered, decimal marksObtained)`
- `Assessment RefreshAssessment(string assessmentId, List<string> questionIds)`
- `AssessmentTag CreateTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
- `AssessmentTag UpdateTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
- `void DisableTag(string id)`
- `void EnableTag(string id)`

### AttendanceCmd.cs
- `void ApplyEvents(List<EventLog> events)`

### CourseCmd.cs
- `void Disable(string id)`
- `void Enable(string id)`
- `void SetCourseRank(string id, int rank)`
- `IEnumerable<object> GetTimeTablePeriodWise(string id, string termName, string secName)`
- `void SaveTimeTablePeriodWise(string id, string termName, string secName, string sTT)`

### ExamCmd.cs
- `Exam Create(string name, Dictionary<string, string> courses)`

### GradingCmd.cs

### LMSCmd.cs
- `QATag CreateTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
- `ImageTag CreateImageTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
- `void CreateQuestion(List<CreateQuestion> questions, string instId = "")`
- `Question UpdateQuestion(string id, string question, List<string> tags, Dictionary<string, string> meta, AnswerEnum answerType, List<Option> options, List<Hint> hints, decimal defaultWeightage, DifficultyEnum difficulty, stri)`
- `QATag UpdateTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
- `void DisableTag(string id)`
- `void EnableTag(string id)`
- `void DisableQuestion(string id)`
- `void EnableQuestion(string id)`
- `void PublishQuestions(List<string> ids)`
- `void UnpublishQuestions(List<string> ids)`
- `void UpdateQuestionTagsDto(List<UpdateQuestionTagsDto> questions)`
- `void ArchiveQuestion(string id)`
- `void UnArchiveQuestion(string id)`
- `List<RandomQuestionResult> SubmitAnswer(string userId, string userEmail, List<RandomQuestionsAnswer> answersSubmitted)`
- `QuestionDto RemodelQuestion(string question, List<string> tags, Dictionary<string, string> meta, AnswerEnum answerType, List<Option> options, List<Hint> hints, decimal defaultWeightage, DifficultyEnum difficulty, string keywords)`

### TTPlannerCmd.cs

## Query layer (*Qry) — implement all
### AssessmentQry.cs
- `AssessmentDto getAssessmentDto(Assessment assessment, List<AssessmentTag> assessmentTags, string inst_short_name)`
- `PublishedAssessmentDto getPublishedAssessmentDto(Assessment assessment, List<AssessmentTag> assessmentTags, string inst_short_name)`

### AttendanceQry.cs
- `dynamic GetAbsenteesOn(string instId = "", DateTime? date = null, int periodNumber = 0)`
- `AttendanceDto GetAttendance(string instId, string courseId, string termName, string secName, int periodNumber, DateTime date)`

### CourseQry.cs

### ExamQry.cs
- `Exam GetById(string id)`
- `List<StudentPerformanceWidget> GetStudentPerformanceWidget(string studentId, string courseId, string termName, string section)`
- `dynamic ExamPassWidget(string instId = "")`

### GradingQry.cs

### LMSQry.cs
- `List<RandomQuestion> GetRandomQuestions(string searchText = "", List<string> statuses = null, List<string> tags = null, string ownerId = "", string qual = "all")`

## Service layer — implement or fold into *Service
_None extracted._

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Assessment.cs
**Enums:**
- `AssessmentStatusEnum`: Unknown=0, Active=1, Disabled=99, WIP=40, Published=50, Archived=80
**Properties:**
- `string Id`
- `string Name`
- `bool Predefined`
- `string CSN`
- `Dictionary<string, string> Meta`
- `string Status`
- `string Id`
- `string QuestionText`
- `string HtmlText`
- `List<TagDto> Tags`
- `List<Option> Options`
- `Dictionary<string, string> Meta`
- `QuestionStatusEnum Status`
- `string AnswerType`
- `List<Hint> Hints`
- `string Instruction`
- `decimal DefaultWeightage`
- `string Id`
- `string QuestionText`
- `string HtmlText`
- `List<TagDto> Tags`
- `List<Option> Options`
- `Dictionary<string, string> Meta`
- `QuestionStatusEnum Status`
- `string AnswerType`
- `List<Hint> Hints`
- `string Instruction`
- `decimal DefaultWeightage`
- `List<SubQues> Questions`
- `decimal TotalMarks`
- `string Description`
- `string Subject`
- `string SubjectCode`
- `int Duration`
- `List<AssessmentSection> Sections`
- `AssessmentStatusEnum Status`
- `bool MultipleAttempts`
- `List<string> Tags`
- `string OwnerId`
- `string Instruction`
- `string InstructionHtml`
- `List<AssessmentQuestion> Questions`
- `string QuestionId`
- `List<int> Answers`
- `string AnswerText`
- `bool Correct`
- `DateTime EvaluatedOn`
- `string EvaluatedBy`
- `string QuestionId`
- `string QuestionText`
- `string HtmlText`
- `string AnswerType`
- `List<int> Answers`
- `string AnswerText`
- `List<Option> Options`
- `decimal DefaultWeightage`
- `bool Correct`
- `string EvaluatedBy`
- `string AssessmentId`
- `string StudentId`

### AttendanceEvent.cs
**Properties:**
- `string InstId`
- `string CourseId`
- `string TermName`
- `string SectionName`
- `DateTime Date`
- `int PeriodNo`
- `string SubjectName`
- `bool IsOptionalSubject`
- `string StudentId`
- `string StaffId`
- `string Attendance`

### Course.cs
**Enums:**
- `CourseStatusEnum`: Unknown=0, Active=1, Disabled=99
- `EduLevelEnum`: Unknown=-1, PreNursery=2, Nursery=5, School=10, UnderGraduate=20, Graduate=30, PostGraduate=40
**Properties:**
- `string Name`
- `string Branch`
- `string NameAndBranch`
- `EduLevelEnum EduLevel`
- `string EduLevelAsString`
- `string InstId`
- `string Affiliation`
- `CourseStatusEnum Status`
- `string StatusAsString`
- `List<Term> Terms`
- `List<string> ExamSubjectOrder`
- `int SortIndex`
- `int Rank`
- `int SeatsAvailable`
- `string Program`
- `string Name`
- `DateTime From`
- `DateTime To`
- `List<Section> Sections`
- `string Name`
- `List<Subject> Subjects`
- `List<StudentRoll> StudentRolls`
- `TimeTable TimeTable`
- `string ClassTeacherStaffId`
- `bool Optional`
- `List<StudentRoll> StudentRolls`
- `string StudentId`
- `string RollNo`

### Exam.cs
**Enums:**
- `ExamStatusEnum`: Unknown=0, Active=1, Scheduled=10, Conducted=20, Locked=90, Disabled=99
- `StudentExamStatus`: Present=10, Absent=20, Barred=30, NA=40
**Properties:**
- `string Name`
- `string InstId`
- `string CourseId`
- `string Term`
- `string Section`
- `List<ExamContent> ExamContents`
- `List<LockEntry> LockHistory`
- `List<Attendance> AttendanceList`
- `List<Remark> RemarksList`
- `ExamStatusEnum Status`
- `int DaysWorked`
- `decimal TotalMaxMarks`
- `int MergeIndex`
- `DateTime StartDate`
- `DateTime ResultDate`
- `string By`
- `DateTime On`
- `string Reason`
- `string Subject`
- `string ParentSubject`
- `decimal MaxMarks`
- `decimal MinMarks`
- `bool GradingOnly`
- `DateTime? ScheduledOn`
- `DateTime? ConductedOn`
- `List<EvalData> Evaluation`
- `string StudentId`
- `int DaysPresent`
- `string StudentId`
- `string Text`
- `string StudentId`
- `int DaysPresent`
- `DateTime? EvaluatedOn`
- `bool GradingOnly`
- `decimal MarksObtained`
- `string Grade`
- `StudentExamStatus Status`

### Grading.cs
**Enums:**
- `GradingStatusEnum`: Active=1, Disabled=99
**Properties:**
- `GradingStatusEnum Status`
- `List<GradingRule> GradingRules`
- `decimal Marks`
- `string Grade`
- `string Description`

### Period.cs
**Properties:**
- `int Number`
- `DayOfWeek DayOfWeek`
- `TimeSpan FromTime`
- `TimeSpan ToTime`
- `int Duration`
- `bool Break`
- `string BreakDescription`
- `List<StaffSubject> StaffSubjectList`
- `TeacherData TeacherData`
- `Subject Subject`
- `string StaffId`
- `bool IsSet`
- `string Remarks`
- `string StId`
- `string StaffFName`
- `string StaffMName`
- `string StaffLName`
- `string CourseId`
- `string CourseName`
- `string BranchName`
- `string TermName`
- `string SectionName`
- `string StaffFullName`

### Question.cs
**Enums:**
- `QuestionStatusEnum`: unknown=0, active=1, disabled=99, published=50, wip=40, archived=80
- `AnswerEnum`: Text=1, OneOf=2, ManyOf=3
- `DifficultyEnum`: low=10, medium=20, high=30
**Properties:**
- `int Id`
- `string OptionType`
- `string Value`
- `bool Answer`
- `string HtmlString`
- `string Value`
- `string HintType`
- `string QuestionText`
- `string PlainText`
- `string HtmlText`
- `List<QATag> Tags`
- `List<string> TagList`
- `List<Option> Options`
- `Dictionary<string, string> Meta`
- `QuestionStatusEnum Status`
- `AnswerEnum AnswerType`
- `List<Hint> Hints`
- `string Instruction`
- `decimal DefaultWeightage`
- `List<SubQuestion> Questions`
- `DifficultyEnum Difficulty`
- `List<string> Keywords`
- `string ISN`
- `string AnswerText`
- `string UserId`
- `string UserEmail`
- `List<RandomQuestionResult> QuestionsAnswered`

### Subject.cs
**Properties:**
- `string Name`
- `bool Optional`
- `List<StudentRoll> StudentRolls`
- `string StudentId`
- `string RollNo`

### Tag.cs
**Enums:**
- `TagStatusEnum`: Unknown=0, Active=1, Disabled=99
**Properties:**
- `string Name`
- `bool Predefined`
- `string CSN`
- `Dictionary<string, string> Meta`
- `TagStatusEnum Status`
- `string Name`
- `bool Predefined`
- `string CSN`
- `Dictionary<string, string> Meta`
- `TagStatusEnum Status`
- `string Name`
- `bool Predefined`
- `string CSN`
- `Dictionary<string, string> Meta`
- `TagStatusEnum Status`

### TimeTable.cs
**Properties:**
- `List<Period> Periods`
- `int Period`
- `List<TTCell> Cells`
- `string DOW`
- `Cell Cell`
- `int Number`
- `string DayOfWeek`
- `TimeSpan FromTime`
- `TimeSpan ToTime`
- `int? Duration`
- `bool Break`
- `string BreakDescription`
- `List<StaffSubject> StaffSubjectList`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **175** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Acad rules
- Exam lock: once locked, reject marks/content mutations unless explicit unlock exists.
- Copy exam contents: deep-copy structure to new exam id under same OwnerId.
- Attendance: upsert by course/date/student; do not double-count same session.
- Course disable excludes from default listings.


### Per-method signals from legacy code


### Source: `AssessmentCmd.cs`

#### `RefreshAssessment(string assessmentId, List<string> questionIds)`
**Guards / branches (from legacy):**
- if (assessment == null)
- if (questionIds == null || questionIds.Count == 0)
- if (_question == null)
- if (question != null)
- if (_question == null)
**Throws:** nameof(assessmentId; nameof(ques.Id; nameof(questionId
**Legacy body summary (adapt to EF):**
```
 var assessment = _session.Load<Assessment>(assessmentId); if (assessment == null) throw new ArgumentException(nameof(assessmentId)); if (questionIds == null || questionIds.Count == 0) { foreach (var section in assessment.Sections) { foreach (var ques in section.Questions) { var _question = _session.Load<Question>(ques.Id); if (_question == null) throw new ArgumentException(nameof(ques.Id)); ques.QuestionText = _question.QuestionText; ques.HtmlText = _question.HtmlText; } } } else { foreach (var
```

#### `CreateTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(name)
- if (name.Length >= 20)
- if (tagExists)
**Throws:** nameof(name; "Length of tag name exceeds 20 characters"; nameof(name
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name)); if (name.Length >= 20) throw new ArgumentException("Length of tag name exceeds 20 characters"); name = name.Trim().ToLower().Replace(" ", "_"); var tagExists = _session.Query<AssessmentTag>() .Where(o => o.Name == name) .Where(o => o.OwnerId == inst_id) .FirstOrDefault() != null; if (tagExists) throw new DuplicateObjectException(nameof(name)); var _tag = n
```

#### `CreateAssessment(string id, string subject, string subjectCode, string description, int duration, decimal totalMarks, List<AssessmentSection> sections, bool multipleAttempts = false, List<string> tags = null, string i)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(subject)
- if (ques.Questions.Count > 0)
**Throws:** nameof(subject
**Status assignments:** Status = AssessmentStatusEnum.WIP
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; if (string.IsNullOrEmpty(subject)) throw new ArgumentException(nameof(subject)); var _assessment = new Assessment() { Subject = subject, SubjectCode = subjectCode, Description = description, Duration = duration, Sections = sections, TotalMarks = totalMarks, Status = AssessmentStatusEnum.WIP, MultipleAttempts = multipleAttempts, OwnerId = inst_id, ModifiedOn = DateTime.Now, Tags = tags }; foreach (var section in _assessment.Sectio
```

#### `UpdateAssessment(string id, string subject, string subjectCode, string description, decimal totalMarks, int duration, List<AssessmentSection> sections, bool multipleAttempts = false, List<string> tags = null)`
**Guards / branches (from legacy):**
- if (_assessment == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _assessment = _session.Load<Assessment>(id); if (_assessment == null) throw new ObjectNotFoundException(); _assessment.Subject = subject; _assessment.SubjectCode = subjectCode; _assessment.Duration = duration; _assessment.Description = description; _assessment.Sections = sections; _assessment.TotalMarks = totalMarks; _assessment.MultipleAttempts = multipleAttempts; _assessment.Tags = tags; foreach (var section in _assessment.Sections) { foreach (var ques in section.Questions) { ques.Sanitiz
```

#### `UpdateSubject(string id, string subject)`
**Guards / branches (from legacy):**
- if (_assessment == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _assessment = _session.Load<Assessment>(id); if (_assessment == null) throw new ObjectNotFoundException(); _assessment.Subject = subject; _session.Store(_assessment); _session.SaveChanges(); return _assessment; 
```

#### `UpdateDuration(string id, TimeSpan duration)`
**Guards / branches (from legacy):**
- if (_assessment == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _assessment = _session.Load<Assessment>(id); if (_assessment == null) throw new ObjectNotFoundException(); // _assessment.Duration = duration; _session.Store(_assessment); _session.SaveChanges(); return _assessment; 
```

#### `UpdateDescription(string id, string description)`
**Guards / branches (from legacy):**
- if (_assessment == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _assessment = _session.Load<Assessment>(id); if (_assessment == null) throw new ObjectNotFoundException(); _assessment.Description = description; _session.Store(_assessment); _session.SaveChanges(); return _assessment; 
```

#### `UpdateTotalMarks(string id, decimal totalMarks)`
**Guards / branches (from legacy):**
- if (_assessment == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _assessment = _session.Load<Assessment>(id); if (_assessment == null) throw new ObjectNotFoundException(); _assessment.TotalMarks = totalMarks; _session.Store(_assessment); _session.SaveChanges(); return _assessment; 
```

#### `PublishAssessment(string id)`
**Guards / branches (from legacy):**
- if (_assessment == null)
**Throws:** nameof(id
**Status assignments:** Status = AssessmentStatusEnum.Published
**Legacy body summary (adapt to EF):**
```
 var _assessment = _session.Load<Assessment>(id); if (_assessment == null) throw new ArgumentException(nameof(id)); _assessment.Status = AssessmentStatusEnum.Published; _session.Store(_assessment); _session.SaveChanges(); 
```

#### `UnPublishAssessment(string id)`
**Guards / branches (from legacy):**
- if (_assessment == null)
**Throws:** nameof(id
**Status assignments:** Status = AssessmentStatusEnum.WIP
**Legacy body summary (adapt to EF):**
```
 var _assessment = _session.Load<Assessment>(id); if (_assessment == null) throw new ArgumentException(nameof(id)); _assessment.Status = AssessmentStatusEnum.WIP; _session.Store(_assessment); _session.SaveChanges(); 
```

#### `ArchiveAssessment(string id)`
**Guards / branches (from legacy):**
- if (_assessment == null)
**Throws:** nameof(id
**Status assignments:** Status = AssessmentStatusEnum.Archived
**Legacy body summary (adapt to EF):**
```
 var _assessment = _session.Load<Assessment>(id); if (_assessment == null) throw new ArgumentException(nameof(id)); _assessment.Status = AssessmentStatusEnum.Archived; _session.Store(_assessment); _session.SaveChanges(); 
```

#### `UnarchiveAssessment(string id)`
**Guards / branches (from legacy):**
- if (_assessment == null)
**Throws:** nameof(id
**Status assignments:** Status = AssessmentStatusEnum.Published
**Legacy body summary (adapt to EF):**
```
 var _assessment = _session.Load<Assessment>(id); if (_assessment == null) throw new ArgumentException(nameof(id)); _assessment.Status = AssessmentStatusEnum.Published; _session.Store(_assessment); _session.SaveChanges(); 
```

#### `SubmitAssessment(string studentId, string assessmentId, List<QuestionsAnswered> questionsAnswered, string instId = "")`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(studentId)
**Throws:** nameof(studentId
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; if (string.IsNullOrEmpty(studentId)) throw new ArgumentException(nameof(studentId)); var submission = new AssessmentSubmission() { StudentId = studentId, AssessmentId = assessmentId, QuestionsAnswered = questionsAnswered, OwnerId = inst_id }; submission.SetId(); _session.Store(submission); _session.SaveChanges(); return submission; 
```

#### `EvaluateAssessment(string id, string evaluatedBy, List<QuestionsAnswered> questionsAnswered, decimal marksObtained)`
**Guards / branches (from legacy):**
- if (submission == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var submission = _session.Load<AssessmentSubmission>(id); if (submission == null) throw new ArgumentException(nameof(id)); foreach (var question in questionsAnswered) { var q = submission.QuestionsAnswered.Find(o => o.QuestionId == question.QuestionId); q.Correct = question.Correct; q.EvaluatedBy = evaluatedBy; q.EvaluatedOn = DateTime.Now; } submission.MarksObtained = marksObtained; _session.Store(submission); _session.SaveChanges(); return submission; 
```

#### `UpdateTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<AssessmentTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Name = name; _tag.Predefined = predefined; _tag.Meta = meta; _session.Store(_tag); _session.SaveChanges(); return _tag; 
```

#### `DisableTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Disabled
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<AssessmentTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Disabled; _session.Store(_tag); _session.SaveChanges(); 
```

#### `EnableTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<AssessmentTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Active; _session.Store(_tag); _session.SaveChanges(); 
```


### Source: `AttendanceCmd.cs`

#### `ApplyEvents(List<EventLog> events)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst || events == null || events.Count == 0)
**Legacy body summary (adapt to EF):**
```
 if (!this._uc.BoundByInst || events == null || events.Count == 0) return; var att_events = get_dto_events(events); foreach (var att_event in att_events) { _session.Store(att_event); } _session.SaveChanges(); 
```


### Source: `CourseCmd.cs`

#### `SaveTimeTablePeriodWise(string id, string termName, string secName, string sTT)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByUser)
- if (course == null)
- if (string.IsNullOrEmpty(termName)
- if (string.IsNullOrEmpty(secName)
- if (cell.StaffSubjectList != null && cell.StaffSubjectList.Count > 0)
**Throws:** ; ; 
**Legacy body summary (adapt to EF):**
```
 if (!_uc.BoundByUser) { var course = _session.Load<Course>(id); if (course == null) throw new ObjectNotFoundException(); if (string.IsNullOrEmpty(termName)) throw new ArgumentNullException(); if (string.IsNullOrEmpty(secName)) throw new ArgumentNullException(); var ttPeriodWise = JsonConvert.DeserializeObject<IList<TTPeriodWise>>(sTT); TimeTable tt = new TimeTable(); foreach (var ttp in ttPeriodWise) { foreach (var ttCell in ttp.Cells) { var cell = ttCell.Cell; cell.Number = ttp.Period; int dow
```

#### `GetTimeTablePeriodWise(string id, string termName, string secName)`
**Guards / branches (from legacy):**
- if (course == null)
- if (string.IsNullOrEmpty(termName)
- if (string.IsNullOrEmpty(secName)
**Throws:** ; ; 
**Legacy body summary (adapt to EF):**
```
 var course = _session.Load<Course>(id); if (course == null) throw new ObjectNotFoundException(); if (string.IsNullOrEmpty(termName)) throw new ArgumentNullException(); if (string.IsNullOrEmpty(secName)) throw new ArgumentNullException(); var result = course.GetTimeTablePeriodWise(termName, secName); return result; 
```

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (_course == null || _course.InstId != inst_id)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; var _course = _session.Load<Course>(id); if (_course == null || _course.InstId != inst_id) throw new ArgumentException(nameof(id)); _course.Disable(); _session.Store(_course); _session.SaveChanges(); 
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (_course == null || _course.InstId != inst_id)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; var _course = _session.Load<Course>(id); if (_course == null || _course.InstId != inst_id) throw new ArgumentException(nameof(id)); _course.Enable(); _session.Store(_course); _session.SaveChanges(); 
```

#### `SetCourseRank(string id, int rank)`
**Guards / branches (from legacy):**
- if (_course == null || _course.InstId != inst_id)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; var _course = _session.Load<Course>(id); if (_course == null || _course.InstId != inst_id) throw new ArgumentException(nameof(id)); _course.Rank = rank; _session.Store(_course); _session.SaveChanges(); 
```


### Source: `ExamCmd.cs`

#### `Create(string name, Dictionary<string, string> courses)`
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 throw new NotImplementedException(); 
```


### Source: `GradingCmd.cs`


### Source: `LMSCmd.cs`

#### `CreateTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(name)
- if (name.Length >= 20)
- if (tagExists)
**Throws:** nameof(name; "Length of tag name exceeds 20 characters"; nameof(name
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name)); if (name.Length >= 20) throw new ArgumentException("Length of tag name exceeds 20 characters"); name = name.Trim().ToLower().Replace(" ", "_"); var tagExists = _session.Query<QATag>() .Where(o => o.Name == name) .Where(o => o.OwnerId == inst_id) .FirstOrDefault() != null; if (tagExists) throw new DuplicateObjectException(nameof(name)); var _tag = new QATag
```

#### `CreateImageTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(name)
- if (name.Length >= 20)
- if (tagExists)
**Throws:** nameof(name; "Length of tag name exceeds 20 characters"; nameof(name
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name)); if (name.Length >= 20) throw new ArgumentException("Length of tag name exceeds 20 characters"); name = name.Trim().ToLower().Replace(" ", "_"); var tagExists = _session.Query<ImageTag>() .Where(o => o.Name == name) .Where(o => o.OwnerId == inst_id) .FirstOrDefault() != null; if (tagExists) throw new DuplicateObjectException(nameof(name)); var _tag = new Im
```

#### `RemodelQuestion(string question, List<string> tags, Dictionary<string, string> meta, AnswerEnum answerType, List<Option> options, List<Hint> hints, decimal defaultWeightage, DifficultyEnum difficulty, string keywords)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(question)
- if (tags == null || tags.Count == 0)
- if (!string.IsNullOrEmpty(question)
- if (tag != null)
**Throws:** "Question text cannot be null"; "Tags cannot be null"
**Status assignments:** Status = status.Equals, Status = QuestionStatusEnum.wip, Status = tag.Status.ToString, Status = status.Equals
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; var inst_short_name = _uc.ShortName; var _question = new Question(); var subQues = new List<SubQuestion>(); if (string.IsNullOrEmpty(question)) throw new ArgumentNullException("Question text cannot be null"); if (tags == null || tags.Count == 0) throw new ArgumentNullException("Tags cannot be null"); if (!string.IsNullOrEmpty(question) && answerType != 0 && tags.Count != 0) { var index = 0; foreach (var sq in questions) { index++; subQues.Add(this.getSubQuestion(sq, in
```

#### `CreateQuestion(List<CreateQuestion> questions, string instId = "")`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(ques.QuestionText)
- if (ques.Tags == null || ques.Tags.Count == 0)
- if (!string.IsNullOrEmpty(ques.QuestionText)
**Throws:** "Question text cannot be null"; "Tags cannot be null"
**Status assignments:** Status = QuestionStatusEnum.wip, Status = QuestionStatusEnum.wip
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; var inst_short_name = _uc.ShortName; foreach (var ques in questions) { if (string.IsNullOrEmpty(ques.QuestionText)) throw new ArgumentNullException("Question text cannot be null"); if (ques.Tags == null || ques.Tags.Count == 0) throw new ArgumentNullException("Tags cannot be null"); if (!string.IsNullOrEmpty(ques.QuestionText) && ques.AnswerType != 0 && ques.Tags.Count != 0) { var subQues = new List<SubQuestion>(); var index = 0;
```

#### `UpdateQuestion(string id, string question, List<string> tags, Dictionary<string, string> meta, AnswerEnum answerType, List<Option> options, List<Hint> hints, decimal defaultWeightage, DifficultyEnum difficulty, stri)`
**Guards / branches (from legacy):**
- if (_question == null)
- if (tags == null || tags.Count == 0)
**Throws:** nameof(id; "Tags cannot be null"
**Status assignments:** Status = QuestionStatusEnum.wip
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; var inst_short_name = _uc.ShortName; var _question = _session.Load<Question>(id); if (_question == null) throw new ArgumentException(nameof(id)); if (tags == null || tags.Count == 0) throw new ArgumentNullException("Tags cannot be null"); var subQues = new List<SubQuestion>(); var index = 0; foreach (var sq in questions) { index++; subQues.Add(this.getSubQuestion(sq, inst_short_name, index)); } _question.QuestionText = question; _question.PlainText = plainText; _questi
```

#### `ArchiveQuestion(string id)`
**Guards / branches (from legacy):**
- if (_question == null)
- if (_question.Status != QuestionStatusEnum.published)
**Throws:** nameof(id; "Question is not published"
**Status assignments:** Status = QuestionStatusEnum.archived
**Legacy body summary (adapt to EF):**
```
 var _question = _session.Load<Question>(id); if (_question == null) throw new ArgumentException(nameof(id)); // Question has to be published to archive it if (_question.Status != QuestionStatusEnum.published) throw new ArgumentException("Question is not published"); _question.Status = QuestionStatusEnum.archived; _session.Store(_question); _session.SaveChanges(); 
```

#### `UpdateTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<QATag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Name = name; _tag.Predefined = predefined; _tag.Meta = meta; _session.Store(_tag); _session.SaveChanges(); return _tag; 
```

#### `DisableTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Disabled
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<QATag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Disabled; _session.Store(_tag); _session.SaveChanges(); 
```

#### `EnableTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<QATag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Active; _session.Store(_tag); _session.SaveChanges(); 
```

#### `DisableQuestion(string id)`
**Guards / branches (from legacy):**
- if (_question == null)
**Throws:** nameof(id
**Status assignments:** Status = QuestionStatusEnum.disabled
**Legacy body summary (adapt to EF):**
```
 var _question = _session.Load<Question>(id); if (_question == null) throw new ArgumentException(nameof(id)); _question.Status = QuestionStatusEnum.disabled; _session.Store(_question); _session.SaveChanges(); 
```

#### `EnableQuestion(string id)`
**Guards / branches (from legacy):**
- if (_question == null)
**Throws:** nameof(id
**Status assignments:** Status = QuestionStatusEnum.active
**Legacy body summary (adapt to EF):**
```
 var _question = _session.Load<Question>(id); if (_question == null) throw new ArgumentException(nameof(id)); _question.Status = QuestionStatusEnum.active; _session.Store(_question); _session.SaveChanges(); 
```

#### `PublishQuestions(List<string> ids)`
**Guards / branches (from legacy):**
- if (_question == null)
**Throws:** nameof(id
**Status assignments:** Status = QuestionStatusEnum.published, Status = QuestionStatusEnum.published
**Legacy body summary (adapt to EF):**
```
 foreach (var id in ids) { var _question = _session.Load<Question>(id); if (_question == null) throw new ArgumentException(nameof(id)); _question.Status = QuestionStatusEnum.published; foreach (var q in _question.Questions) { q.Status = QuestionStatusEnum.published; } _session.Store(_question); } _session.SaveChanges(); 
```

#### `UnpublishQuestions(List<string> ids)`
**Guards / branches (from legacy):**
- if (_question == null)
**Throws:** nameof(id
**Status assignments:** Status = QuestionStatusEnum.wip, Status = QuestionStatusEnum.wip
**Legacy body summary (adapt to EF):**
```
 foreach (var id in ids) { var _question = _session.Load<Question>(id); if (_question == null) throw new ArgumentException(nameof(id)); _question.Status = QuestionStatusEnum.wip; foreach (var q in _question.Questions) { q.Status = QuestionStatusEnum.wip; } _session.Store(_question); } _session.SaveChanges(); 
```

#### `UpdateQuestionTagsDto(List<UpdateQuestionTagsDto> questions)`
**Guards / branches (from legacy):**
- if (_question == null)
**Throws:** nameof(question.Id
**Legacy body summary (adapt to EF):**
```
 foreach (var question in questions) { var _question = _session.Load<Question>(question.Id); if (_question == null) throw new ArgumentException(nameof(question.Id)); _question.TagList = question.Tags; _session.Store(_question); } _session.SaveChanges(); 
```

#### `UnArchiveQuestion(string id)`
**Guards / branches (from legacy):**
- if (_question == null)
**Throws:** nameof(id
**Status assignments:** Status = QuestionStatusEnum.published
**Legacy body summary (adapt to EF):**
```
 var _question = _session.Load<Question>(id); if (_question == null) throw new ArgumentException(nameof(id)); _question.Status = QuestionStatusEnum.published; _session.Store(_question); _session.SaveChanges(); 
```

#### `SubmitAnswer(string userId, string userEmail, List<RandomQuestionsAnswer> answersSubmitted)`
**Guards / branches (from legacy):**
- if (question == null)
- if (question.Questions.Count > 0)
**Legacy body summary (adapt to EF):**
```
 var result = new List<RandomQuestionResult>(); foreach (var answer in answersSubmitted) { var question = _session.Load<Question>(answer.QuestionId); if (question == null) continue; if (question.Questions.Count > 0) { var result_list = new List<RandomQuestionResult>(); foreach (var ans in answer.Questions) { var subQ = question.Questions.Find(o => o.Id == ans.QuestionId); var answer_result = subQ.Evaluate(ans.Answers, ans.AnswerText); var dto = new RandomQuestionResult() { QuestionId = question.
```


### Source: `TTPlannerCmd.cs`


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Accounting

**Legacy:** `ct.accounting` → **Target:** `Ct.Accounting`  
**HTTP actions to implement:** 21  
**Notes:** Event-sourced ledgers/vouchers → EF; Tally XML export

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### InventoryController.cs
- Route prefix: `api/accounting/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `Post` | [FromBody] InventoryItemDto dto |

### JournalController.cs
- Route prefix: `api/accounting/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/inventory` | `InventoryJournal` | [FromBody] InventoryJournalDto dto |
| Post | `/purchase` | `PurchaseItems` | [FromBody] AccountingDto dto |
| Post | `/sales` | `SalesItems` | [FromBody] AccountingDto dto |
| Post | `/smsInventoryCredit` | `AddSMSCredits` | [FromBody] InventoryJournalDto dto |
| Get | `/smsTx` | `GetSMSTx` | [FromQuery] string instId |

### LedgerController.cs
- Route prefix: `api/accounting/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/assets` | `Assets` | [FromBody] LedgerAccountDto dto |
| Post | `/liabilities` | `Liabilities` | [FromBody] LedgerAccountDto dto |
| Post | `/chartOfAccounts` | `chartOfAccounts` |  |
| Get | `/voucherTargetAccounts` | `GetVoucherTargetAccounts` |  |
| Get | `/voucherSourceAccounts` | `GetVoucherSourceAccounts` |  |

### ReportController.cs
- Route prefix: `api/accounting/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/smsCreditsTx` | `SMSCreditsTx` | [FromBody] JObject jObj |
| Post | `/datewiseExpense` | `GetDatewiseExpenseReport` | [FromBody] JObject jObj |
| Post | `/categorywiseExpense` | `GetCategorywiseExpenseReport` | [FromBody] JObject jObj |
| Post | `/yearwiseExpense` | `GetYearwiseExpenseReport` | [FromBody] JObject jObj |
| Get | `/tallyXML` | `GetTallyXMLReport` | [FromQuery] string id |
| Get | `/tallyPrimeXML` | `GetTallyPrimeXMLReport` | [FromQuery] string id |

### VoucherController.cs
- Route prefix: `api/accounting/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/create` | `NewVoucher` | [FromBody] VoucherDto dto |
| Get | `/` | `Get` | [FromQuery] string id |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |

## Command layer (*Cmd) — implement all
_None extracted._

## Query layer (*Qry) — implement all
### AccountingQry.cs
- `List<SMSTx> GetSMSTx(string inst_id)`

### LedgerQry.cs
- `List<LedgerDto> GetVoucherTargetAccounts()`
- `List<LedgerDto> GetVoucherSourceAccounts()`

### VoucherQry.cs

## Service layer — implement or fold into *Service
### AccountingService.cs

### ReportService.cs

### VoucherService.cs
- `string GetNextVoucherNo()`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### AccountingJournal.cs
**Enums:**
- `AccountingJournalStatusEnum`: Active=1, Disabled=99
- `JournalEntryTypeEnum`: Dr=10, Cr=20
**Properties:**
- `string LedgerAccountId`
- `decimal Amount`
- `string Details`
- `string JournalType`
- `DateTime Date`
- `string Reference`
- `List<JournalEntry> Credits`
- `List<JournalEntry> Debits`
- `string Particulars`
- `AccountingJournalStatusEnum Status`
- `List<string> Tags`
- `string InventoryJournalId`
- `string OwnerId`
- `string Id`
- `string OwnerId`
- `string JournalType`
- `string Reference`
- `string Particulars`
- `DateTime Date`
- `List<JournalEntry> Credits`
- `List<JournalEntry> Debits`
- `string InventoryJournalId`
- `string Id`
- `string OwnerId`
- `string JournalType`
- `string Reference`
- `string Particulars`
- `DateTime Date`
- `List<JournalEntry> Credits`
- `List<JournalEntry> Debits`
- `string InventoryJournalId`
- `string Id`
- `string OwnerId`
- `string JournalType`
- `string Reference`
- `string Particulars`
- `DateTime Date`
- `List<JournalEntry> Credits`
- `List<JournalEntry> Debits`
- `string InventoryJournalId`
- `string Id`
- `string OwnerId`
- `string JournalType`
- `string Reference`
- `string Particulars`
- `DateTime Date`
- `List<JournalEntry> Credits`
- `List<JournalEntry> Debits`
- `string InventoryJournalId`
- `List<string> Tags`
- `string Id`
- `AccountingJournalStatusEnum Status`
- `string Id`
- `AccountingJournalStatusEnum Status`

### InventoryItem.cs
**Enums:**
- `InventoryItemStatusEnum`: Active=1, Disabled=99
- `InventoryTypeEnum`: Item=1, Group=2
**Properties:**
- `string Name`
- `string GroupId`
- `InventoryTypeEnum InventoryType`
- `string UOM`
- `List<string> Tags`
- `Dictionary<string, object> Attributes`
- `string OwnerId`
- `InventoryItemStatusEnum Status`
- `string Id`
- `string Name`
- `string GroupId`
- `InventoryTypeEnum InventoryType`
- `string UOM`
- `string OwnerId`
- `List<string> Tags`
- `Dictionary<string, object> Attributes`
- `string Id`
- `string Name`
- `string UOM`
- `string GroupId`
- `InventoryTypeEnum InventoryType`
- `List<string> Tags`
- `Dictionary<string, object> Attributes`
- `string OwnerId`
- `string Id`
- `InventoryItemStatusEnum Status`
- `string Id`
- `InventoryItemStatusEnum Status`

### LedgerAccount.cs
**Enums:**
- `LedgerOwnerTypeEnum`: Org=1, Inst=2
- `LedgerTypeEnum`: Ledger=1, Group=2
- `LedgerStatusEnum`: Active=1, Disabled=99
- `NatureOfAccountsEnum`: Inherit=0, Assets=10, Liabilities=20, Income=30, Expenses=40
**Properties:**
- `string Name`
- `string GroupId`
- `string OwnerId`
- `LedgerOwnerTypeEnum OwnerType`
- `LedgerTypeEnum LedgerType`
- `NatureOfAccountsEnum NatureOfAccounts`
- `LedgerStatusEnum Status`
- `string Id`
- `string Name`
- `string GroupId`
- `string OwnerId`
- `LedgerOwnerTypeEnum OwnerType`
- `LedgerTypeEnum LedgerType`
- `NatureOfAccountsEnum NatureOfAccounts`
- `string Id`
- `string Name`
- `string Id`
- `string Id`
- `string Id`
- `string GroupId`
- `NatureOfAccountsEnum NatureOfAccounts`

### Voucher.cs
**Enums:**
- `VoucherStatusEnum`: Active=1, Disabled=99
- `VoucherTypeEnum`: Expense=1
**Properties:**
- `string LedgerId`
- `decimal Amount`
- `Content Content`
- `bool Preview`
- `string Text`
- `string HtmlText`
- `string PlainText`
- `VoucherTypeEnum Type`
- `string VoucherNo`
- `string RefNo`
- `string Description`
- `DateTime Date`
- `List<LedgerItem> By`
- `List<LedgerItem> To`
- `decimal ByTotal`
- `decimal ToTotal`
- `Section Section`
- `List<string> Tags`
- `string OwnerId`
- `VoucherStatusEnum Status`
- `string Id`
- `string VoucherNo`
- `VoucherTypeEnum Type`
- `string RefNo`
- `string Description`
- `DateTime Date`
- `List<LedgerItem> By`
- `List<LedgerItem> To`
- `decimal ByTotal`
- `decimal ToTotal`
- `Section Section`
- `List<string> Tags`
- `string OwnerId`
- `string Id`
- `VoucherStatusEnum Status`
- `string Id`
- `VoucherStatusEnum Status`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **21** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Accounting rules
- **Voucher:** sum(debit lines) must equal sum(credit lines) or reject; store ByTotal/ToTotal.
- **Ledger natures:** Assets/Liabilities/Income/Expenses drive source vs target account lists for UI.
- **Inventory In:** increase QtyOnHand; **Out:** decrease; **Adjust:** set absolute qty when legacy did.
- **Tally XML:** export active vouchers as simplified ENVELOPE/TALLYMESSAGE structure (exact Tally schema fidelity optional).


### Per-method signals from legacy code

_No method-level extract; follow cross-cutting rules and endpoint semantics._


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Admission

**Legacy:** `ct.admission` → **Target:** `Ct.Admission`  
**HTTP actions to implement:** 43  
**Notes:** Applications, seat matrix, form templates, OTP

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### AdmissionController.cs
- Route prefix: `api/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/seatMatrix` | `SeatMatrix` | [FromBody] SeatMatrixDto dto |
| Post | `/seatMatrix/update` | `UpdateSeatMatrix` | [FromBody] UpdateSeatMatrixDto dto |
| Get | `/seatMatrix` | `GetSeatMatrix` |  |
| Get | `/seatsAvailable` | `GetSeatsAvailableForCourse` | string courseId |
| Get | `/applicationsCoursewise` | `ApplicationsCoursewise` |  |
| Get | `/` | `GetAdmissions` |  |

### AnonController.cs
- Route prefix: `api/admission/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/otp` | `OTP` | [FromBody] OTPDto json |

### ApplicationController.cs
- Route prefix: `api/admission/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `CreateApplication` | [FromBody] CreateApplicationDto json |
| Get | `/` | `GetApplications` |  |
| Get | `/getByEmail` | `GetApplicationByEmail` | [FromQuery] string userEmail |
| Post | `/updateIdentity` | `Identity` | [FromBody] IdentityDto json |
| Post | `/updateFatherDetails` | `FatherDetails` | [FromBody] ParentDetailsDto json |
| Post | `/updateMotherDetails` | `MotherDetails` | [FromBody] ParentDetailsDto json |
| Post | `/updateGuardianDetails` | `GuardianDetails` | [FromBody] ParentDetailsDto json |
| Post | `/updateAcademicHistory` | `UpdateAcademicDetails` | [FromBody] AcademicDetailsDto json |
| Post | `/updatePhotoUrl` | `UpdatePhotoURL` | [FromBody] UpdateUrlDto json |
| Post | `/updateAadharUrl` | `UpdateAadharURL` | [FromBody] UpdateUrlDto json |
| Post | `/updateHSCMarksCardUrl` | `UpdateHSCUrl` | [FromBody] UpdateUrlDto json |
| Post | `/updateSSCMarksCardUrl` | `UpdateSSCUrl` | [FromBody] UpdateUrlDto json |
| Post | `/casteCertificateUrl` | `UpdateCasteCertificateUrl` | [FromBody] UpdateUrlDto json |
| Post | `/leavingCertificateUrl` | `UpdateLeavingCertificateUrl` | [FromBody] UpdateUrlDto json |
| Post | `/domicileCertificateUrl` | `UpdateDomicileCertificateUrl` | [FromBody] UpdateUrlDto json |
| Post | `/transferCertificateUrl` | `UpdateTransferCertificateUrl` | [FromBody] UpdateUrlDto json |
| Post | `/birthCertificateUrl` | `UpdateBirthCertificateUrl` | [FromBody] UpdateUrlDto json |
| Post | `/admit` | `AdmitApplicant` | [FromBody] ApplicationsDto json |
| Get | `/admittedApplicants` | `GetAdmittedApplicants` | [FromBody] AdmittedStudentsDto json |
| Post | `/shortlist` | `Shortlist` | [FromBody] ApplicationsShortlistDto json |
| Post | `/select` | `Select` | [FromBody] ApplicationsDto json |
| Post | `/decline` | `Decline` | [FromBody] ApplicationsDto json |
| Post | `/reject` | `Reject` | [FromBody] ApplicationsDto json |
| Post | `/select/remove` | `RemoveFromSelection` | [FromBody] ApplicationIdDto json |
| Post | `/submit` | `Submit` | [FromBody] ApplicationIdDto json |

### ApplicationFormTemplateController.cs
- Route prefix: `api/admission/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `CreateApplicationFormTemplate` | [FromBody] AdmissionFormTemplateDto dto |
| Get | `/` | `GetApplicationFormTemplate` | [FromQuery] bool publishedOnly |
| Get | `/getById` | `GetApplicationFormTemplateById` | [FromQuery] string id |
| Post | `/update` | `UpdateApplicationFormTemplate` | [FromBody] UpdateAdmissionFormTemplateDto dto |
| Post | `/disable` | `DisableApplicationFormTemplate` | [FromQuery] string id |
| Post | `/enable` | `EnableApplicationFormTemplate` | [FromQuery] string id |
| Post | `/publish` | `PublishApplicationFormTemplate` | [FromQuery] string id |
| Post | `/unpublish` | `UnPublishApplicationFormTemplate` | [FromQuery] string id |
| Post | `/addShortlist` | `AddShortlist` | [FromBody] ShortlistDto json |
| Post | `/updateShortlist` | `UpdateShortlist` | [FromBody] ShortlistUpdateDto json |
| Get | `/shortlist` | `GetShortlist` | [FromQuery] string id, [FromQuery] string courseId |

## Command layer (*Cmd) — implement all
### AdmissionCmd.cs
- `SeatMatrix CreateMatrix(string courseId, string course, int totalSeats, List<SeatBreakUp> breakUp = null)`
- `SeatMatrix UpdateMatrix(string id, string courseId, string course, int totalSeats, List<SeatBreakUp> breakUp = null)`

### ApplicationCmd.cs
- `Application CreateApplication(string name, string gender, string email, string mobile, string templateId, string courseId, string course, string stream, string combination)`
- `Application UpdateIdentity(string id, DateTime dob, string residentialStatus, string category, string gender, ApplicantAddress address, string mobile)`
- `Application Submit(string id)`
- `void Admit(List<string> ids)`
- `Application UpdateFatherDetails(string id, string name, string email, string mobile)`
- `Application UpdateMotherDetails(string id, string name, string email, string mobile)`
- `Application UpdateGuardianDetails(string id, string name, string email, string mobile)`
- `Application UpdateAcademicDetails(string id, AcademicHistory hsc, AcademicHistory ssc)`
- `Application UpdateSSCDetails(string id, int yop, string board, decimal percentage)`
- `void UpdatePhotoUrl(string id, string url)`
- `void UpdateAadharUrl(string id, string url)`
- `void UpdateCasteCertificateUrl(string id, string url)`
- `void UpdateLeavingCertificateUrl(string id, string url)`
- `void UpdateDomicileCertificateUrl(string id, string url)`
- `void UpdateTransferCertificateUrl(string id, string url)`
- `void UpdateBirthCertificateUrl(string id, string url)`
- `void UpdateHSCMarksCardUrl(string id, string url)`
- `void UpdateSSCMarksCardUrl(string id, string url)`
- `void Shortlist(List<string> applicationIds, string applicationFormId, string courseId)`
- `void Select(List<string> applicationIds)`
- `void Decline(List<string> applicationIds)`
- `void Reject(List<string> applicationIds)`
- `void RemoveFromSelection(string id)`

### ApplicationFormTemplateCmd.cs

## Query layer (*Qry) — implement all
### AdmissionQry.cs
- `List<SeatMatrix> GetSeatMatrices()`
- `List<SeatsAvailableDto> GetSeatsAvailable(string courseId = "")`
- `List<AdmissionDto> getAdmissionDetails(string inst_id)`
- `List<ApplicationsCoursewise> GetApplicationsCoursewise()`
- `List<AdmissionDto> GetAdmissions(string searchText = "", List<string> categories = null)`

### ApplicationFormTemplateQry.cs
- `Shortlist GetShortlist(string id, string courseId)`

### ApplicationQry.cs
- `PagedResultDto<GetApplicationDto> GetApplications(string searchText = "", List<string> appliedFor = null, List<string> categories = null, List<decimal> hscPercentages = null, List<decimal> sscPercentages = null, List<string> statuses = null)`
- `List<Application> GetAdmittedApplications(int shortlistNo, string courseId)`
- `List<GetApplicationDto> GetApplicationByEmail(string email)`

## Service layer — implement or fold into *Service
_None extracted._

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Application.cs
**Enums:**
- `ApplicantCategoryEnum`: GM=40, OBC=50, SC=60, ST=70
- `ResidentialStatusEnum`: Indian=10, PIO_OCI=20, NRI=30
- `PaymentStatusEnum`: Pending=10, Pending=10, Paid=20, Paid=20
- `CourseLevelEnum`: PU=1, UG=2, PG=3
- `ApplicationStatusEnum`: WIP=10, Selected=15, Submitted=20, Declined=50, Shortlisted=25, Admitted=30, Rejected=35, OptedIn=40, OptedOut=45
**Properties:**
- `string Line1`
- `string Line2`
- `string City`
- `string State`
- `string Country`
- `string Pincode`
- `int YearOfPassing`
- `string Board`
- `decimal Percentage`
- `string Medium`
- `string Name`
- `string Email`
- `string Mobile`
- `CourseLevelEnum CourseLevel`
- `string CourseId`
- `string Course`
- `string Stream`
- `string Combination`
- `PaymentStatusEnum PaymentStatus`
- `string PaymentRef`
- `decimal AmountPaid`
- `string Name`
- `string Email`
- `string Mobile`
- `DateTime DOB`
- `ResidentialStatusEnum ResidentialStatus`
- `ApplicantCategoryEnum Category`
- `GenderEnum Gender`
- `ApplicantAddress Address`
- `AcademicHistory HSC`
- `AcademicHistory SSC`
- `ParentDetails FatherDetails`
- `ParentDetails MotherDetails`
- `ParentDetails GuardianDetails`
- `AppliedFor AppliedFor`
- `Payment Payment`
- `string PhotoURL`
- `string AadharURL`
- `string HSCMarksCardURL`
- `string SSCMarksCardURL`
- `string CasteCertificateURL`
- `string DomicileCertificateURL`
- `string BirthCertificateURL`
- `string TransferCertificateURL`
- `string LeavingCertificateURL`
- `string ApplicationFormTemplateId`
- `DateTime SubmittedOn`
- `int ApplicationNumber`
- `int ShortlistedIn`
- `DateTime DOA`
- `ApplicationStatusEnum ApplicationStatus`

### ApplicationFormTemplate.cs
**Enums:**
- `ApplicationFormTemplateStatusEnum`: Active=1, Disabled=99, Published=70
**Properties:**
- `string Title`
- `string Description`
- `AdmissionFormOptions Options`
- `DateTime StartDate`
- `DateTime EndDate`
- `ApplicationFormTemplateStatusEnum Status`
- `List<Shortlist> Shortlists`
- `UploadItem Uploads`
- `bool CollectParentDetails`
- `Dictionary<string, string> DisplayLabels`
- `decimal ApplicationFee`
- `AcademicDetails AcademicHistory`
- `List<string> ApplyFor`
- `AcademicDetail SSC`
- `AcademicDetail HSC`
- `string Board`
- `int YearOfPassing`
- `string Medium`
- `decimal Percentage`
- `bool Show`
- `bool Photo`
- `bool AadharCard`
- `bool SSCMarksheet`
- `bool HSCMarksheet`
- `bool CasteCertificate`
- `bool DomicileCertificate`
- `bool BirthCertificate`
- `bool TransferCertificate`
- `bool LeavingCertificate`
- `string Course`
- `string CourseId`
- `DateTime From`
- `DateTime To`
- `int ShortlistNo`

### SeatMatrix.cs
**Properties:**
- `string Category`
- `int Seats`
- `decimal Percentage`
- `string CourseId`
- `string Course`
- `int TotalSeats`
- `List<SeatBreakUp> BreakUp`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **43** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Admission rules
- **Application status flow:** WIP → Submitted → Shortlisted/Selected → Admitted | Declined | Rejected. Do not admit from Rejected.
- **AdmitApplicant:** set Status=Admitted, AdmittedOn=UtcNow; load SeatMatrix by CourseId; if missing or available seats ≤ 0 → 400; else AdmittedCount++.
- **Seat available:** available = TotalSeats - ReservedSeats - AdmittedCount.
- **Document URL updates:** merge into Documents dictionary by fixed keys (photo, aadhar, hsc, ssc, caste, leaving, domicile, transfer, birth).
- **OTP:** generate 6-digit code, store with expiry (~10 min); production would SMS via Connect — returning debug code in dev is OK.


### Per-method signals from legacy code


### Source: `AdmissionCmd`

#### `CreateMatrix(string courseId, string course, int totalSeats, List<SeatBreakUp> breakUp = null)`
**Guards / branches (from legacy):**
- if (!seatMatrixExists)
**Legacy body summary (adapt to EF):**
```
 var instId = _uc.InstId; var seatMatrixExists = _session .Query<SeatMatrix>() .Customize(x => x.WaitForNonStaleResults()) .Where(o => o.CourseId.Equals(courseId) && o.OwnerId.Equals(instId)).FirstOrDefault() != null; var sm = new SeatMatrix(); if (!seatMatrixExists) { sm.Course = course; sm.CourseId = courseId; sm.TotalSeats = totalSeats; sm.BreakUp = breakUp; sm.SetId(); sm.SetOwner(instId); } _session.Store(sm); _session.SaveChanges(); return sm; 
```

#### `UpdateMatrix(string id, string courseId, string course, int totalSeats, List<SeatBreakUp> breakUp = null)`
**Guards / branches (from legacy):**
- if (seatMatrix != null)
**Legacy body summary (adapt to EF):**
```
 var instId = _uc.InstId; var seatMatrix = _session .Query<SeatMatrix>() .Where(o => o.Id == id); var sm = new SeatMatrix(); if (seatMatrix != null) { sm.Course = course; sm.CourseId = courseId; sm.TotalSeats = totalSeats; sm.BreakUp = breakUp; } _session.Store(sm); _session.SaveChanges(); return sm; 
```


### Source: `ApplicationCmd`

#### `UpdateIdentity(string id, DateTime dob, string residentialStatus, string category, string gender, ApplicantAddress address, string mobile)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (application == null)
**Throws:** inst_id
**Status assignments:** Status = residentialStatus
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; // if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.DOB = dob; application.ResidentialStatus = residentialStatus == "20" ? ResidentialStatusEnum.PIO_OCI : residentialStatus == "30" ? ResidentialStatusEnum.NRI : ResidentialStatusEnum.Indian; application.Category = category == "70" ? ApplicantCategoryEnum.ST : category
```

#### `UpdateFatherDetails(string id, string name, string email, string mobile)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (application == null)
**Throws:** inst_id
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; // if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); var details = new ParentDetails(); details.Name = name; details.Email = email; details.Mobile = mobile; application.FatherDetails = details; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application
```

#### `UpdateMotherDetails(string id, string name, string email, string mobile)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (application == null)
**Throws:** inst_id
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; // if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); var details = new ParentDetails(); details.Name = name; details.Email = email; details.Mobile = mobile; application.MotherDetails = details; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application
```

#### `UpdateGuardianDetails(string id, string name, string email, string mobile)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (application == null)
**Throws:** inst_id
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; // if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); var details = new ParentDetails(); details.Name = name; details.Email = email; details.Mobile = mobile; application.GuardianDetails = details; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_applicati
```

#### `UpdateAcademicDetails(string id, AcademicHistory hsc, AcademicHistory ssc)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (application == null)
**Throws:** inst_id
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; // if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.HSC = hsc; application.SSC = ssc; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationUpdatedJob
```

#### `UpdateSSCDetails(string id, int yop, string board, decimal percentage)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (application == null)
**Throws:** inst_id
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; // if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); var details = new AcademicHistory(); details.YearOfPassing = yop; details.Board = board; details.Percentage = percentage; application.SSC = details; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_app
```

#### `Shortlist(List<string> applicationIds, string applicationFormId, string courseId)`
**Guards / branches (from legacy):**
- if (applicationFormTemplate == null)
- if (application == null)
**Throws:** "Application form template does not exist"
**Status assignments:** Status = ApplicationStatusEnum.Shortlisted
**Side effects / messaging:**
- `EnqueAdmApplicationStatusUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var applicationFormTemplate = _session.Load<ApplicationFormTemplate>(applicationFormId); if (applicationFormTemplate == null) throw new ArgumentNullException("Application form template does not exist"); var today = DateTime.Now; var shortlist = applicationFormTemplate.Shortlists.Where(o => o.CourseId == courseId) .Where(o => o.From <= today && o.To >= today) .FirstOrDefault(); foreach (var id in applicationIds) { var application = _session.Load<Application>(id); if (application == null) new Arg
```

#### `CreateApplication(string name, string gender, string email, string mobile, string templateId, string courseId, string course, string stream, string combination)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
**Throws:** inst_id
**Status assignments:** Status = ApplicationStatusEnum.WIP
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var application = new Application(); application.SetId(); application.Name = name; application.Email = email; application.Mobile = mobile; application.OwnerId = inst_id; application.Gender = gender == "Mr" ? GenderEnum.Male : GenderEnum.Female; var appliedFor = new AppliedFor(); // application.CourseLevel = courseLevel; appliedFor.CourseId = courseId; appliedFor.Course = course; appliedFor.Stream = stream; a
```

#### `Submit(string id)`
**Guards / branches (from legacy):**
- if (application == null)
**Status assignments:** Status = ApplicationStatusEnum.Submitted
**Side effects / messaging:**
- `EnqueAdmApplicationSubmittedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.ApplicationStatus = ApplicationStatusEnum.Submitted; application.SubmittedOn = DateTime.Now; application.ApplicationNumber = getNextApplicationNo(); _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_submitted", payload = { inst_id = _uc.InstId } }; _rabbitMQService.Enq
```

#### `Admit(List<string> ids)`
**Guards / branches (from legacy):**
- if (application == null)
**Status assignments:** Status = ApplicationStatusEnum.Admitted
**Side effects / messaging:**
- `EnqueAdmApplicationStatusUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 foreach (var id in ids) { var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.ApplicationStatus = ApplicationStatusEnum.Admitted; application.DOA = DateTime.Now; _session.Store(application); } _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_status_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationStatusUpdatedJob
```

#### `UpdatePhotoUrl(string id, string url)`
**Guards / branches (from legacy):**
- if (application == null)
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.PhotoURL = url; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationUpdatedJob(job); 
```

#### `UpdateAadharUrl(string id, string url)`
**Guards / branches (from legacy):**
- if (application == null)
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.AadharURL = url; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationUpdatedJob(job); 
```

#### `UpdateCasteCertificateUrl(string id, string url)`
**Guards / branches (from legacy):**
- if (application == null)
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.CasteCertificateURL = url; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationUpdatedJob(job); 
```

#### `UpdateLeavingCertificateUrl(string id, string url)`
**Guards / branches (from legacy):**
- if (application == null)
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.LeavingCertificateURL = url; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationUpdatedJob(job); 
```

#### `UpdateDomicileCertificateUrl(string id, string url)`
**Guards / branches (from legacy):**
- if (application == null)
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.DomicileCertificateURL = url; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationUpdatedJob(job); 
```

#### `UpdateTransferCertificateUrl(string id, string url)`
**Guards / branches (from legacy):**
- if (application == null)
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.TransferCertificateURL = url; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationUpdatedJob(job); 
```

#### `UpdateBirthCertificateUrl(string id, string url)`
**Guards / branches (from legacy):**
- if (application == null)
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.BirthCertificateURL = url; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationUpdatedJob(job); 
```

#### `UpdateHSCMarksCardUrl(string id, string url)`
**Guards / branches (from legacy):**
- if (application == null)
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.HSCMarksCardURL = url; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationUpdatedJob(job); 
```

#### `UpdateSSCMarksCardUrl(string id, string url)`
**Guards / branches (from legacy):**
- if (application == null)
**Side effects / messaging:**
- `EnqueAdmApplicationUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application is not found"); application.SSCMarksCardURL = url; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationUpdatedJob(job); 
```

#### `Select(List<string> applicationIds)`
**Guards / branches (from legacy):**
- if (application == null)
**Status assignments:** Status = ApplicationStatusEnum.Selected
**Side effects / messaging:**
- `EnqueAdmApplicationStatusUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 foreach (var id in applicationIds) { var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application"); application.ApplicationStatus = ApplicationStatusEnum.Selected; _session.Store(application); } _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_status_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationStatusUpdatedJob(job); 
```

#### `Decline(List<string> applicationIds)`
**Guards / branches (from legacy):**
- if (application == null)
**Status assignments:** Status = ApplicationStatusEnum.Declined
**Side effects / messaging:**
- `EnqueAdmApplicationStatusUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 foreach (var id in applicationIds) { var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application"); application.ApplicationStatus = ApplicationStatusEnum.Declined; _session.Store(application); } _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_status_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationStatusUpdatedJob(job); 
```

#### `Reject(List<string> applicationIds)`
**Guards / branches (from legacy):**
- if (application == null)
**Status assignments:** Status = ApplicationStatusEnum.Rejected
**Side effects / messaging:**
- `EnqueAdmApplicationStatusUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 foreach (var id in applicationIds) { var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application"); application.ApplicationStatus = ApplicationStatusEnum.Rejected; _session.Store(application); } _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_status_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationStatusUpdatedJob(job); 
```

#### `RemoveFromSelection(string id)`
**Guards / branches (from legacy):**
- if (application == null)
**Status assignments:** Status = ApplicationStatusEnum.Submitted
**Side effects / messaging:**
- `EnqueAdmApplicationStatusUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var application = _session.Load<Application>(id); if (application == null) new ArgumentNullException("Application"); application.ApplicationStatus = ApplicationStatusEnum.Submitted; _session.Store(application); _session.SaveChanges(); var job = new Job() { id = Guid.NewGuid().ToString(), title = "admission_application_status_updated", payload = { inst_id = _uc.InstId } }; _rabbitMQService.EnqueAdmApplicationStatusUpdatedJob(job); 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Arte

**Legacy:** `ct.arte` → **Target:** `Ct.Arte`  
**HTTP actions to implement:** 91  
**Notes:** Artefact metadata, publish, ETL/Github stubs

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### AnonController.cs
- Route prefix: `api/arte/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/getFiles` | `GetFilesFromUrl` | [FromQuery] string url |
| Get | `/contents` | `GetFiles` | [FromQuery] string url |
| Get | `/contentTags` | `GetContentTags` |  |
| Get | `/download` | `Download` | [FromQuery] string id |

### ArtefactController.cs
- Route prefix: `api/arte/[Controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string id |
| Post | `/updateTags` | `UpdateTags` | [FromBody] List<UpdateTagsDto> json |
| Get | `/dataTemplates` | `GetDataTemplates` |  |
| Get | `/dataTemplatesLive` | `GetDataTemplatesLive` | [FromQuery] string id |
| Get | `/dataTemplatesWorking` | `GetDataTemplatesWorking` | [FromQuery] string id |
| Get | `/getDataTemplatesByName` | `GetDataTemplatesByName` | [FromQuery] string name |
| Get | `/getOnQry` | `GetOnQry` | [FromQuery] string qry |
| Get | `/reports` | `GetReports` | [FromQuery] string id |
| Get | `/getTemplate` | `GetTemplate` | [FromQuery] string id |
| Get | `/getModel` | `GetModel` | [FromQuery] string id |
| Get | `/getUrl` | `GetUrl` | [FromQuery] string id |
| Get | `/marksCardTemplate` | `MarksCardTemplate` |  |
| Get | `/sysAdminPolicy` | `SysAdminPolicy` |  |
| Post | `/dataTemplatesSave` | `DataTemplatesSave` | [FromBody] ArtefactDto json |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/publishArtefact` | `PublishArte` | [FromQuery] string id, [FromQuery] string topicId |
| Post | `/publishArtefactToPublic` | `PublishArteToPublic` | [FromQuery] string id, [FromQuery] string topicId |
| Post | `/unpublishArtefact` | `UnpublishArte` | [FromQuery] string id |
| Post | `/unpublishPublicArtefact` | `UnpublishPublicArte` | [FromQuery] string id |
| Get | `/download` | `Download` | [FromQuery] string id |
| Get | `/pdfFileContent` | `PDFFileContent` | [FromQuery] string id |
| Get | `/getFileUrl` | `GetFileUrl` | [FromQuery] string path |
| Get | `/downloadWithTimestamp` | `DownloadWithTimestamp` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Post | `/extract` | `Extract` | [FromQuery] string id |
| Post | `/studentAttendanceTemplate` | `StudentAttendanceTemplate` | [FromBody] ArtefactDto json |
| Post | `/template` | `Template` | [FromBody] ArtefactDto json |
| Post | `/update` | `Update` | [FromBody] ArtefactDto json |
| Post | `/updateReport` | `UpdateReport` | [FromBody] ArtefactDto json |
| Post | `/upsert` | `Upsert` | List<IFormFile> files, string acadYear, string path |
| Post | `/uploadAndExtract` | `UploadAndExtract` | [FromBody] ArtefactDto json |
| Post | `/view` | `view` | [FromQuery] string id |
| Post | `/removeFile` | `RemoveFile` | [FromQuery] string FileName |
| Get | `/fileNames` | `GetFileNames` |  |
| Get | `/etlFileNames` | `GetEtlFileNames` | [FromQuery] string path |
| Get | `/file` | `GetFileContent` | [FromQuery] string name |
| Get | `/emailTemplate` | `GetEmailTemplate` |  |
| Get | `/emailTemplateWithoutHF` | `GetEmailTemplateWithoutHF` |  |
| Post | `/viewMarksCard` | `ViewMarksCard` | [FromBody] ViewMarkscardDto json |
| Post | `/publish` | `Publish` | List<IFormFile> files, string acadYear |
| Get | `/generateEtl` | `GenerateEtl` | [FromQuery] string id |
| Post | `/runEtl` | `RunEtl` | [FromQuery] string id |
| Post | `/executeAllEtlsInDir` | `ExecuteEtl` | [FromQuery] string id |
| Get | `/fileContent` | `FileContent` | [FromQuery] string id |
| Get | `/etlFileContent` | `EtlFileContent` | [FromQuery] string path |
| Post | `/applicantPhoto` | `UploadApplicantPhoto` | List<IFormFile> files, string path, List<string> tags = null, string provider = "aws" |
| Post | `/applicantAadhar` | `UploadApplicantAadhar` | List<IFormFile> files, string path, List<string> tags = null, string provider = "aws" |
| Post | `/uploadApplicantDocument` | `UploadApplicantDocument` | List<IFormFile> files, string path, List<string> tags = null, string provider = "aws" |
| Post | `/applicantHSCMarksCard` | `UploadApplicantHSCMarksCard` | List<IFormFile> files, string path, List<string> tags = null, string provider = "aws" |
| Post | `/applicantSSCMarksCard` | `UploadApplicantSSCMarksCard` | List<IFormFile> files, string path, List<string> tags = null, string provider = "aws" |
| Post | `/viewDocument` | `ViewDocument` | [FromBody] ApplicantDto json |
| Post | `/tag` | `CreateTag` | [FromBody] CreateTagDto dto |
| Post | `/contentTag` | `CreateContentTag` | [FromBody] CreateTagDto dto |
| Post | `/comment` | `AddComment` | [FromBody] CommentDto dto |
| Post | `/attribute` | `AddAttribute` | [FromBody] UpdateAttributesDto dto |
| Post | `/videoLink` | `VideoLink` | [FromBody] VideoLinkDto dto |
| Get | `/tags` | `GetTags` |  |
| Get | `/contentTags` | `GetContentTags` |  |
| Post | `/updateTag` | `UpdateTag` | [FromBody] UpdateTagDto dto |
| Post | `/updateContentTag` | `UpdateContentTag` | [FromBody] UpdateTagDto dto |
| Post | `/disableTag` | `DisableTag` | [FromQuery] string id |
| Post | `/enableTag` | `EnableTag` | [FromQuery] string id |
| Post | `/disableContentTag` | `DisableContentTag` | [FromQuery] string id |
| Post | `/enableContentTag` | `EnableContentTag` | [FromQuery] string id |
| Get | `/widget/artefactBandwidth` | `GetArtefactBandwidthWidgetData` |  |
| Get | `/widget/artefactCount` | `GetArtefactCountWidgetData` |  |
| Get | `/widget/sizeBuckets` | `GetWidgetSizeBucketsData` |  |
| Get | `/widget/artefactAuthorCount` | `GetWidgetArtefactAuthorCountData` |  |
| Get | `/widget/fileCount` | `GetWidgetFileCount` |  |
| Post | `/updatePublicUrl` | `UpdatePublicUrl` | [FromBody] UpdatePublicUrlDto dto |
| Post | `/removePublicUrl` | `RemovePublicUrl` | [FromBody] UpdatePublicUrlDto dto |
| Post | `/resetPublicUrl` | `ResetPublicUrl` | [FromBody] ResetPublicUrlDto dto |
| Post | `/upsertStudentDocument` | `UpsertStudentDocument` | List<IFormFile> files, string student_id |

### EtlController.cs
- Route prefix: `api/arte/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/generateInstEtl` | `CreateInstituteEtl` | [FromQuery] string path |
| Post | `/generateCourseEtl` | `CreateCourseEtl` | [FromQuery] string path |
| Post | `/generateStudentEtl` | `CreateStudentEtl` | [FromQuery] string path |
| Post | `/generateFeesEtl` | `CreateFeesEtl` | [FromQuery] string path |

### GithubController.cs
- Route prefix: `api/arte/[Controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/upsert` | `Upsert` | List<IFormFile> files, string path, string id = "" |
| Get | `/` | `Get` | [FromQuery] string id |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Get | `/download` | `Download` | [FromQuery] string id |
| Get | `/getFiles` | `GetFiles` | [FromQuery] string url |
| Post | `/updateDescription` | `UpdateDescription` | [FromBody] DescriptionDto dto |
| Get | `/filesWithContent` | `GetFilesWithContent` | [FromQuery] string url |
| Get | `/specsFile` | `GetSpecsFile` | [FromQuery] string filename |

### ReportController.cs
- Route prefix: `api/arte/[Controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/artefactList` | `GetArtefactsReport` | [FromBody] JObject jObj |

## Command layer (*Cmd) — implement all
### ArtefactCmd.cs
- `Artefact Create(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model, List<string> Tags)`
- `Artefact CreateEtlArtefact(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model)`
- `Artefact CreateWithStatusUpload(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model, List<string> Tags)`
- `Artefact PublishArtefact(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model)`
- `Artefact Update(string Id, string Title, string Description, Dictionary<string, string> MetaData, List<string> Tags)`
- `Artefact UpdateReport(string Id, string Title, string Description, Dictionary<string, string> MetaData, string Model)`
- `void Disable(string id)`
- `void Publish(string id, string topicId = "")`
- `void PublishToPublic(string id, string topicId = "")`
- `void Unpublish(string id)`
- `void UnpublishPublicArtefact(string id)`
- `string SetFileDownloadName(string id, long timestamp)`
- `void Enable(string id)`
- `void RunEtl(string url)`
- `void ExecuteEtls(string url)`
- `EtlData GenerateInstituteEtl(string file_name, string file_content)`
- `EtlData GenerateCoursesEtl(string file_name, string file_content)`
- `EtlData GenerateStudentsEtl(string file_name, string file_content)`
- `Artefact UpsertArtefact(string id, string title, string description, Dictionary<string, string> metaData, string mimeType, string fileName, double fileSize, string url, string sha1, string model, List<string> tags)`
- `void UpdateArtefactTags(List<UpdateTagsDto> artefacts)`
- `void UpdateDescription(string id, string description)`
- `ArtefactTag CreateTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
- `ContentTag CreateContentTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
- `ArtefactTag UpdateTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
- `ContentTag UpdateContentTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
- `void DisableTag(string id)`
- `void EnableTag(string id)`
- `void DisableContentTag(string id)`
- `void EnableContentTag(string id)`
- `void UpdateComment(string id, string by, string text, string html, long itemId)`
- `void UpdateAttribute(string id, List<AttributeDto> attributes)`
- `void UpdateVideoLink(string id, List<VLinkDto> links)`
- `void UpdatePublicUrl(string id, string publicUrl)`
- `void RemovePublicUrl(string id, string publicUrl)`
- `void ResetPublicUrl(string id)`

## Query layer (*Qry) — implement all
### ArtefactQry.cs
- `PagedResultDto<ArteDto> Get(string qual = "all")`
- `Artefact GetById(string id)`
- `IEnumerable<Artefact> GetReports()`
- `IEnumerable<Artefact> GetDataTemplates()`
- `IEnumerable<Artefact> GetByMimeType()`
- `IEnumerable<Artefact> GetByMetaData()`
- `bool FilenameExists(string filename)`
- `PagedResultDto<ArteDto> GetFilesFromUrl(string url, string searchText = "", List<string> tags = null, List<string> statuses = null, string qual = "all")`
- `List<ArteDto> GetAllFiles(string url, string searchText = "")`
- `Artefact GetByFilename(string filename)`
- `List<WidgetDto> GetBandwidthCountData()`
- `List<SizeBucketsDto> GetSizeBucketsData()`
- `List<FileCountDto> GetFileCountData()`

### TemplateQry.cs
- `string Render(string template_string, string model)`
- `static Template GetTemplateInstance(string template_string, string instanceName = "test")`

## Service layer — implement or fold into *Service
### ArtefactService.cs
- `PagedResultDto<ArteDto> GetFilesFromUrl(string url, string searchText = "", List<string> tags = null, List<string> statuses = null, string qual = "all")`
- `PagedResultDto<AnonArteDto> GetFiles(string url, string searchText = "", List<string> statuses = null, string qual = "any")`
- `Artefact GetById(string id)`
- `CreateInstituteDto GetInstitute(string id)`
- `byte[] DownloadFromPath(string url, string shortName)`
- `string GetFileContent(string url)`

### ReportService.cs
- `string GetArtefactsReport(PrintOptions options = null)`
- `List<ArtefactRptDto> GetArtefactsModel()`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Artefact.cs
**Enums:**
- `TagStatusEnum`: Unknown=0, Active=1, Disabled=99
- `ArtefactStatusEnum`: Unknown=0, Active=1, Disabled=99, Downloaded=90, Uploaded=80, Published=70, PublishedToPublic=75, Etl=60
- `VideoTypeEnum`: Record=10, Drive=20, Stream=30
**Properties:**
- `string Name`
- `bool Predefined`
- `string CSN`
- `Dictionary<string, string> Meta`
- `TagStatusEnum Status`
- `string Name`
- `bool Predefined`
- `string CSN`
- `Dictionary<string, string> Meta`
- `TagStatusEnum Status`
- `string Key`
- `dynamic Value`
- `long ItemId`
- `List<KeyValueAttr> Attributes`
- `VideoTypeEnum Type`
- `string Url`
- `long ItemId`
- `string By`
- `DateTime TimeStamp`
- `long ItemId`
- `string Text`
- `string Html`
- `string By`
- `DateTime On`
- `string Ref`
- `string Url`
- `string Title`
- `string Description`
- `Dictionary<string, string> MetaData`
- `List<string> Tags`
- `string MimeType`
- `string FileName`
- `double FileSize`
- `ArtefactStatusEnum Status`
- `string SHA1`
- `string Model`
- `string Template`
- `string Csv`
- `List<ChangeRef> ChangeSet`
- `List<Comment> Comments`
- `List<VideoLink> VideoLinks`
- `List<DataAttribute> DataAttributes`
- `DateTime PublishedOn`
- `List<string> PublicUrls`
- `List<string> Thumbnails`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **91** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Arte rules
- Publish/Unpublish toggle visibility; public/anon endpoints only return published artefacts.
- Tags/attributes replace or merge as per each Cmd (prefer replace when legacy Set*).
- ETL/Github routes may stub with same response shape if external tools absent.


### Per-method signals from legacy code


### Source: `ArtefactCmd.cs`

#### `CreateTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(name)
- if (name.Length >= 20)
- if (tagExists)
**Throws:** nameof(name; "Length of tag name exceeds 20 characters"; nameof(name
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name)); if (name.Length >= 20) throw new ArgumentException("Length of tag name exceeds 20 characters"); name = name.Trim().ToLower().Replace(" ", "_"); var tagExists = _session.Query<ArtefactTag>() .Where(o => o.Name == name) .Where(o => o.OwnerId == inst_id) .FirstOrDefault() != null; if (tagExists) throw new DuplicateObjectException(nameof(name)); var _tag = new
```

#### `CreateContentTag(string name, bool predefined = false, Dictionary<string, string> meta = null, string instId = "")`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(name)
- if (name.Length >= 20)
- if (tagExists)
**Throws:** nameof(name; "Length of tag name exceeds 20 characters"; nameof(name
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; if (string.IsNullOrEmpty(name)) throw new ArgumentException(nameof(name)); if (name.Length >= 20) throw new ArgumentException("Length of tag name exceeds 20 characters"); name = name.Trim().ToLower().Replace(" ", "_"); var tagExists = _session.Query<ContentTag>() .Where(o => o.Name == name) .Where(o => o.OwnerId == inst_id) .FirstOrDefault() != null; if (tagExists) throw new DuplicateObjectException(nameof(name)); var _tag = new 
```

#### `Publish(string id, string topicId = "")`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
- if (!string.IsNullOrEmpty(topicId)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.Published
**Side effects / messaging:**
- `EnquePublishToPublicJob(job)`
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.Published; artefact.PublishedOn = DateTime.Now; _session.Store(artefact); _session.SaveChanges(); if (!string.IsNullOrEmpty(topicId)) { var payload = new { artefact_id = artefact.Id, topic_id = topicId, topic = _uc.Topic.ToLower() }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id,
```

#### `PublishToPublic(string id, string topicId = "")`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
- if (!string.IsNullOrEmpty(topicId)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.PublishedToPublic
**Side effects / messaging:**
- `EnquePublishToPublicJob(job)`
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.PublishedToPublic; artefact.PublishedOn = DateTime.Now; _session.Store(artefact); _session.SaveChanges(); if (!string.IsNullOrEmpty(topicId)) { var payload = new { artefact_id = artefact.Id, topic_id = topicId, topic = _uc.Topic.ToLower() }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id =
```

#### `UnpublishPublicArtefact(string id)`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.Active
**Side effects / messaging:**
- `EnquePublishToPublicJob(job)`
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.Active; _session.Store(artefact); _session.SaveChanges(); var payload = new { artefact_id = artefact.Id, topic_id = string.Empty, topic = string.Empty }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "unpublish", topic = string.Empty, payload = payload, user_id = "", cli
```

#### `UpdateAttribute(string id, List<AttributeDto> attributes)`
**Guards / branches (from legacy):**
- if (_artefact == null)
- if (arteAttribute != null)
- if (attr != null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); foreach (var att in attributes) { var arteAttribute = _artefact.DataAttributes.Where(o => o.ItemId == att.ItemId).FirstOrDefault(); if (arteAttribute != null) { var attr = arteAttribute.Attributes.Where(o => o.Key == att.Key).FirstOrDefault(); if (attr != null) attr.Value = att.Value; } else { var attr = new DataAttribute(); attr.ItemId = att.ItemId; var attrDto = new KeyValueAttr(); att
```

#### `Create(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model, List<string> Tags)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(_artefact.Id)
- if (MetaData.ContainsKey("meeting_id")
**Status assignments:** Status = ArtefactStatusEnum.Active
**Side effects / messaging:**
- `EnqueUploadNotesJob(job)`
**Legacy body summary (adapt to EF):**
```
 var _artefact = new Artefact() { Id = Id, Title = Title, Description = Description, MetaData = MetaData, MimeType = MimeType, FileName = FileName, FileSize = FileSize, Url = Url, SHA1 = SHA1, Model = Model, Tags = Tags, Status = ArtefactStatusEnum.Active }; if (string.IsNullOrEmpty(_artefact.Id)) _artefact.SetId(); _artefact.SetOwner(_uc.InstId); _session.Store(_artefact); _session.SaveChanges(); if (MetaData.ContainsKey("meeting_id")) { var payload = new { meeting_id = MetaData["meeting_id"] }
```

#### `Update(string Id, string Title, string Description, Dictionary<string, string> MetaData, List<string> Tags)`
**Guards / branches (from legacy):**
- if (_artefact == null || string.IsNullOrEmpty(_artefact.Id)
- if (_uc.Persist)
**Throws:** nameof(_artefact
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(Id); if (_artefact == null || string.IsNullOrEmpty(_artefact.Id)) { throw new ArgumentException(nameof(_artefact)); } _artefact.Title = Title; _artefact.Description = Description; _artefact.MetaData = MetaData; _artefact.Tags = Tags; //persist if (_uc.Persist) { _session.Store(_artefact); _session.SaveChanges(); } return _artefact; 
```

#### `UpdatePublicUrl(string id, string publicUrl)`
**Guards / branches (from legacy):**
- if (_artefact == null)
- if (!string.IsNullOrEmpty(publicUrl)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); if (!string.IsNullOrEmpty(publicUrl)) _artefact.PublicUrls.Add(publicUrl); _artefact.PublicUrls = _artefact.PublicUrls.Distinct().ToList(); _session.Store(_artefact); _session.SaveChanges(); 
```

#### `RemovePublicUrl(string id, string publicUrl)`
**Guards / branches (from legacy):**
- if (_artefact == null)
- if (!string.IsNullOrEmpty(publicUrl)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); if (!string.IsNullOrEmpty(publicUrl)) _artefact.PublicUrls.Remove(publicUrl); _artefact.PublicUrls = _artefact.PublicUrls.Distinct().ToList(); _session.Store(_artefact); _session.SaveChanges(); 
```

#### `UpdateReport(string Id, string Title, string Description, Dictionary<string, string> MetaData, string Model)`
**Guards / branches (from legacy):**
- if (_artefact == null || string.IsNullOrEmpty(_artefact.Id)
**Throws:** nameof(_artefact
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(Id); if (_artefact == null || string.IsNullOrEmpty(_artefact.Id)) { throw new ArgumentException(nameof(_artefact)); } _artefact.Title = Title; _artefact.Description = Description; _artefact.MetaData = MetaData; _artefact.Model = Model; _session.Store(_artefact); _session.SaveChanges(); return _artefact; 
```

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.Disabled
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.Disabled; _session.Store(artefact); _session.SaveChanges(); 
```

#### `Unpublish(string id)`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.Active; _session.Store(artefact); _session.SaveChanges(); 
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (artefact == null || string.IsNullOrEmpty(artefact.Id)
**Throws:** nameof(artefact
**Status assignments:** Status = ArtefactStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null || string.IsNullOrEmpty(artefact.Id)) { throw new ArgumentException(nameof(artefact)); } artefact.Status = ArtefactStatusEnum.Active; _session.Store(artefact); _session.SaveChanges(); 
```

#### `GenerateInstituteEtl(string file_name, string file_content)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(line)
**Side effects / messaging:**
- `EnqueEtlScript(job)`
**Legacy body summary (adapt to EF):**
```
 string[] lines = file_content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); var institute = Helper.CsvToList<InstituteDto>(lines); var json_string = JsonConvert.SerializeObject(institute); var json_inst = JsonConvert.DeserializeObject<List<InstituteDto>>(json_string); var etl_inst_lines = _etlService.GetApiScript_CreateInstitute(json_inst); var sb = new StringBuilder(); foreach (var line in etl_inst_lines ?? new List<string>()) { if (string.IsNullOrEmpty(li
```

#### `GenerateCoursesEtl(string file_name, string file_content)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(line)
**Side effects / messaging:**
- `EnqueEtlScript(job)`
**Legacy body summary (adapt to EF):**
```
 string[] lines = file_content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); var courses = Helper.CsvToList<EtlCourseDto>(lines); var courses_string = JsonConvert.SerializeObject(courses); var course_list = JsonConvert.DeserializeObject<List<EtlCourseDto>>(courses_string); var etl_lines = _etlService.GetApiScript_CreateCourses(course_list); var sb = new StringBuilder(); foreach (var line in etl_lines ?? new List<string>()) { if (string.IsNullOrEmpty(line)) c
```

#### `GenerateStudentsEtl(string file_name, string file_content)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(line)
**Side effects / messaging:**
- `EnqueEtlScript(job)`
**Legacy body summary (adapt to EF):**
```
 string[] lines = file_content.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries); var students = Helper.CsvToList<EtlStudentDto>(lines); var students_string = JsonConvert.SerializeObject(students); var student_list = JsonConvert.DeserializeObject<List<EtlStudentDto>>(students_string); var etl_lines = _etlService.GetApiScript_CreateStudents(student_list); var sb = new StringBuilder(); foreach (var line in etl_lines ?? new List<string>()) { if (string.IsNullOrEmpty
```

#### `UpdateArtefactTags(List<UpdateTagsDto> artefacts)`
**Guards / branches (from legacy):**
- if (_artefact == null)
**Throws:** nameof(arte.Id
**Legacy body summary (adapt to EF):**
```
 foreach (var arte in artefacts) { var _artefact = _session.Load<Artefact>(arte.Id); if (_artefact == null) throw new ArgumentException(nameof(arte.Id)); _artefact.Tags = arte.Tags; _session.Store(_artefact); } _session.SaveChanges(); 
```

#### `UpdateDescription(string id, string description)`
**Guards / branches (from legacy):**
- if (artefact == null)
**Throws:** "Artefact not found"
**Legacy body summary (adapt to EF):**
```
 var artefact = _session.Load<Artefact>(id); if (artefact == null) throw new ArgumentNullException("Artefact not found"); artefact.Description = description; _session.Store(artefact); _session.SaveChanges(); 
```

#### `UpdateTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ArtefactTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Name = name; _tag.Predefined = predefined; _tag.Meta = meta; _session.Store(_tag); _session.SaveChanges(); return _tag; 
```

#### `UpdateContentTag(string id, string name, bool predefined, Dictionary<string, string> meta)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ContentTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Name = name; _tag.Predefined = predefined; _tag.Meta = meta; _session.Store(_tag); _session.SaveChanges(); return _tag; 
```

#### `DisableTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Disabled
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ArtefactTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Disabled; _session.Store(_tag); _session.SaveChanges(); 
```

#### `EnableTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ArtefactTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Active; _session.Store(_tag); _session.SaveChanges(); 
```

#### `DisableContentTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Disabled
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ContentTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Disabled; _session.Store(_tag); _session.SaveChanges(); 
```

#### `EnableContentTag(string id)`
**Guards / branches (from legacy):**
- if (_tag == null)
**Throws:** nameof(id
**Status assignments:** Status = TagStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var _tag = _session.Load<ContentTag>(id); if (_tag == null) throw new ArgumentException(nameof(id)); _tag.Status = TagStatusEnum.Active; _session.Store(_tag); _session.SaveChanges(); 
```

#### `UpdateComment(string id, string by, string text, string html, long itemId)`
**Guards / branches (from legacy):**
- if (_artefact == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); var cm = new Comment(); cm.By = by; cm.ItemId = itemId; cm.Html = html; cm.Text = text; cm.TimeStamp = DateTime.UtcNow; _artefact.Comments.Add(cm); _session.Store(_artefact); _session.SaveChanges(); 
```

#### `UpdateVideoLink(string id, List<VLinkDto> links)`
**Guards / branches (from legacy):**
- if (_artefact == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); foreach (var link in links) { var video = new VideoLink(); video.Type = link.Type.Equals("record") ? VideoTypeEnum.Record : link.Type.Equals("drive") ? VideoTypeEnum.Drive : VideoTypeEnum.Stream; video.Url = link.Url; video.ItemId = link.ItemId; _artefact.VideoLinks.Add(video); } _session.Store(_artefact); _session.SaveChanges(); 
```

#### `ResetPublicUrl(string id)`
**Guards / branches (from legacy):**
- if (_artefact == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _artefact = _session.Load<Artefact>(id); if (_artefact == null) throw new ArgumentException(nameof(id)); _artefact.PublicUrls.Clear(); _session.Store(_artefact); _session.SaveChanges(); 
```

#### `RunEtl(string url)`
**Side effects / messaging:**
- `EnqueEtlScript(job)`
**Legacy body summary (adapt to EF):**
```
 var url_arr = url.Split('?'); var arr = url_arr[0].Split('/'); var script_path = $"{arr[3]}/{arr[4]}"; var job = new Job() { title = "etl_run_script", client_id = _uc.InstId, payload = new { s3_path = script_path } }; _rabbitMQService.EnqueEtlScript(job); 
```

#### `ExecuteEtls(string url)`
**Side effects / messaging:**
- `EnqueEtlScript(job)`
**Legacy body summary (adapt to EF):**
```
 var script_path = url.Replace("//", "").Split(new char[] { '/' }, 2)[1]; // var arr = url_arr[0].Split('/'); // var script_path = $"{arr[3]}/{arr[4]}"; var job = new Job() { title = "run_etl_with_dir", client_id = string.Empty, payload = new { s3_jsonl_dir = script_path } }; _rabbitMQService.EnqueEtlScript(job); 
```

#### `UpsertArtefact(string id, string title, string description, Dictionary<string, string> metaData, string mimeType, string fileName, double fileSize, string url, string sha1, string model, List<string> tags)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(id)
- if (string.IsNullOrEmpty(artefact.Id)
**Status assignments:** Status = ArtefactStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var artefact = new Artefact(); if (string.IsNullOrEmpty(id)) { var changeRef = new ChangeRef(); changeRef.By = _uc.UserEmail; changeRef.On = DateTime.Now; artefact = new Artefact() { Id = id, Title = title, Description = description, MetaData = metaData, MimeType = mimeType, FileName = fileName, FileSize = fileSize, Url = url, SHA1 = sha1, Model = model, Tags = tags, Status = ArtefactStatusEnum.Active, ModifiedOn = DateTime.UtcNow }; if (string.IsNullOrEmpty(artefact.Id)) artefact.SetId(); arte
```

#### `CreateEtlArtefact(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(_artefact.Id)
**Status assignments:** Status = ArtefactStatusEnum.Etl
**Legacy body summary (adapt to EF):**
```
 var _artefact = new Artefact() { Id = Id, Title = Title, Description = Description, MetaData = MetaData, MimeType = MimeType, FileName = FileName, FileSize = FileSize, Url = Url, SHA1 = SHA1, Model = Model, Status = ArtefactStatusEnum.Etl }; if (string.IsNullOrEmpty(_artefact.Id)) _artefact.SetId(); _artefact.SetOwner(_uc.InstId); _session.Store(_artefact); _session.SaveChanges(); return _artefact; 
```

#### `CreateWithStatusUpload(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model, List<string> Tags)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(_artefact.Id)
**Status assignments:** Status = ArtefactStatusEnum.Uploaded
**Legacy body summary (adapt to EF):**
```
 var _artefact = new Artefact() { Id = Id, Title = Title, Description = Description, MetaData = MetaData, MimeType = MimeType, FileName = FileName, FileSize = FileSize, Url = Url, SHA1 = SHA1, Model = Model, Tags = Tags, Status = ArtefactStatusEnum.Uploaded }; if (string.IsNullOrEmpty(_artefact.Id)) _artefact.SetId(); _artefact.SetOwner(_uc.InstId); _session.Store(_artefact); _session.SaveChanges(); return _artefact; 
```

#### `PublishArtefact(string Id, string Title, string Description, Dictionary<string, string> MetaData, string MimeType, string FileName, double FileSize, string Url, string SHA1, string Model)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(_artefact.Id)
**Status assignments:** Status = ArtefactStatusEnum.Published
**Legacy body summary (adapt to EF):**
```
 var _artefact = new Artefact() { Id = Id, Title = Title, Description = Description, MetaData = MetaData, MimeType = MimeType, FileName = FileName, FileSize = FileSize, Url = Url, SHA1 = SHA1, Model = Model, Status = ArtefactStatusEnum.Published }; if (string.IsNullOrEmpty(_artefact.Id)) _artefact.SetId(); _artefact.SetOwner(_uc.InstId); _session.Store(_artefact); _session.SaveChanges(); return _artefact; 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Asset

**Legacy:** `ct.asset` → **Target:** `Ct.Asset`  
**HTTP actions to implement:** 14  
**Notes:** Event-sourced legacy → current-state EF; unique TrackingId

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### AssetController.cs
- Route prefix: `api/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `Post` | [FromBody] AssetDto json |
| Get | `/` | `Get` | [FromQuery] string id |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Post | `/setAttributes` | `SetAttributes` | [FromBody] UpdateAttributesDto json |
| Post | `/setTags` | `SetTags` | [FromBody] UpdateTagsDto json |
| Post | `/setValue` | `SetValue` | [FromBody] UpdateValueDto json |
| Post | `/relocate` | `Relocate` | [FromBody] AssetRelocateDto json |
| Post | `/updateTrackingId` | `UpdateTrackingId` | [FromBody] UpdateTrackingIdDto json |
| Post | `/maintain` | `Maintain` | [FromBody] AssetMaintainDto json |
| Post | `/setWarranty` | `SetWarranty` | [FromBody] AssetWarrantyDto json |
| Post | `/clone` | `Clone` | [FromBody] AssetCloneDto json |
| Post | `/` | `ImportArtefacts` | [FromBody]JObject jObj |

### ReportController.cs
- Route prefix: `api/asset/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/locationWiseAssets` | `GetLocationwiseAssetsReport` | [FromBody]JObject jObj |

## Command layer (*Cmd) — implement all
_None extracted._

## Query layer (*Qry) — implement all
### AssetQry.cs
- `AssetView GetAssetById(string id)`
- `bool IsDuplicateTrackingId(string trackingId)`

## Service layer — implement or fold into *Service
### ReportService.cs
- `string GetLocationwiseAssets(PrintOptions options = null, bool testData = false)`
- `string GetLocationwiseAssetsData(bool testData = false)`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Aggregate.cs
**Enums:**
- `AssetStatusEnum`: Active=1, Cleared=90, Disabled=99
**Properties:**
- `string Id`
- `string TrackingId`
- `string OwnerId`
- `string Location`
- `AssetStatusEnum Status`
- `string Id`
- `AssetStatusEnum Status`
- `string Id`
- `AssetStatusEnum Status`
- `string Id`
- `AssetStatusEnum Status`
- `string Id`
- `Dictionary<string, object> Attributes`
- `string Id`
- `Dictionary<string, object> Attributes`
- `string Id`
- `List<string> Tags`
- `string Id`
- `List<string> Tags`
- `string Id`
- `string OwnerId`
- `string NewTrackingId`
- `string Id`
- `string Location`
- `string Id`
- `decimal Value`
- `string Id`
- `string CloneId`
- `string TrackingId`
- `string Id`
- `Warranty Warranty`
- `string Id`
- `Maintenance Maintenance`
- `string TrackingId`
- `string OwnerId`
- `string Location`
- `Dictionary<string, object> Attributes`
- `List<string> Tags`
- `decimal Value`
- `Maintenance LastMaintenance`
- `Warranty CurrentWarranty`
- `AssetStatusEnum Status`
- `string By`
- `DateTime On`
- `string ActionTaken`
- `string CoveredBy`
- `DateTime From`
- `DateTime To`
- `string Details`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **14** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Asset rules
- **TrackingId** unique per OwnerId (case-insensitive check in legacy Qry).
- **Clone:** for each requested TrackingId, create new asset copying metadata; new Id; fail per-id on duplicate tracking.
- **Clear** sets Status=Cleared; **Disable** sets Disabled; **Enable** sets Active.
- **Maintain:** append maintenance record; set LastMaintenance; typically return to Active.
- No event-sourcing replay required — persist current state only.


### Per-method signals from legacy code


### Source: `AssetHandler`

#### `Handle(CloneAsset command)`
**Guards / branches (from legacy):**
- if (command == null || string.IsNullOrEmpty(command.Id)
- if (aggr != null && !string.IsNullOrEmpty(aggr.Id)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 if (command == null || string.IsNullOrEmpty(command.Id) || string.IsNullOrEmpty(command.CloneId)) throw new ArgumentNullException(); var aggr = eventStore.GetById<Asset>(command.Id); if (aggr != null && !string.IsNullOrEmpty(aggr.Id)) { var clone = aggr.Clone(command.CloneId, command.TrackingId); eventStore.Save(clone); } 
```

#### `Handle(CreateAsset command)`
**Guards / branches (from legacy):**
- if (asset != null && !string.IsNullOrEmpty(asset.Id)
**Legacy body summary (adapt to EF):**
```
 var asset = eventStore.GetById<Asset>(command.Id); if (asset != null && !string.IsNullOrEmpty(asset.Id)) return; //aggregate exists with the given id. var aggregate = new Asset(command.Id, command.TrackingId, command.OwnerId, command.Location); eventStore.Save(aggregate); 
```

#### `Handle(UpdateTrackingIdAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.UpdateTrackingId(command.NewTrackingId); eventStore.Save(aggr); 
```

#### `Handle(EnableAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.Enable(); eventStore.Save(aggr); 
```

#### `Handle(DisableAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.Disable(); eventStore.Save(aggr); 
```

#### `Handle(ClearAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.Clear(); eventStore.Save(aggr); 
```

#### `Handle(SetAssetAttributes command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.SetAttributes(command.Attributes); eventStore.Save(aggr); 
```

#### `Handle(ClearAssetAttributes command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.ClearAttributes(); eventStore.Save(aggr); 
```

#### `Handle(ClearAssetTags command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.ClearTags(); eventStore.Save(aggr); 
```

#### `Handle(MoveAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.Move(command.Location); eventStore.Save(aggr); 
```

#### `Handle(SetAssetValue command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.SetValue(command.Value); eventStore.Save(aggr); 
```

#### `Handle(MaintainAsset command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.Maintain(command.Maintenance); eventStore.Save(aggr); 
```

#### `Handle(SetWarranty command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Asset>(command.Id); aggr.SetWarranty(command.Warranty); eventStore.Save(aggr); 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Connect

**Legacy:** `ct.connect` → **Target:** `Ct.Connect`  
**HTTP actions to implement:** 46  
**Notes:** SMS/email templates, topics, workflows; Rabbit outbound

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### ConnectController.cs
- Route prefix: `api/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/SMSSent` | `SMSSent` |  |
| Get | `/EmailSent` | `EmailSent` |  |
| Post | `/` | `Post` | [FromBody] string id |
| Get | `/smsMessages` | `SMSMessages` | [FromQuery]bool activeOnly = false |
| Post | `/requestApproval` | `RequestApproval` | [FromBody]string message |
| Post | `/sms` | `SMS` | [FromBody] RecipientPostData json |
| Post | `/enable` | `Enable` | [FromQuery]string id |
| Post | `/disable` | `Disable` | [FromQuery]string id |
| Post | `/lookup` | `LookupContacts` | [FromBody] ContactLookupDto json |
| Get | `/smsDeliveryData` | `SMSDeliveryData` | [FromQuery] string smsId |
| Get | `/emailDeliveryData` | `EmailDeliveryData` | [FromQuery] string emailId |
| Post | `/email` | `Email` | [FromBody] CreateEmailDto json |
| Post | `/smsText` | `SMSText` | [FromBody] JObject jObj |

### FacetController.cs
- Route prefix: `api/facet/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/forStudent` | `ForStudent` |  |
| Post | `/` | `ApplyForStudents` | string id, [FromBody]JObject jObj |
| Get | `/` | `ForStaff` |  |
| Post | `/` | `ApplyForStaff` | string id, [FromBody]JObject jObj |
| Get | `/` | `ForParent` |  |
| Post | `/` | `ApplyForParents` | string id, [FromBody]JObject jObj |

### ReportController.cs
- Route prefix: `api/connect/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/SMSUsageReport` | `SMSUsageReport` | [FromBody]JObject jObj |
| Post | `/invalidMobiles` | `GetInvalidMobilesReport` | [FromBody]JObject jObj |
| Post | `/invalidEmails` | `GetInvalidEmailsReport` | [FromBody]JObject jObj |

### SMSTemplateController.cs
- Route prefix: `api/connect/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` |  |
| Post | `/` | `Post` | [FromBody] SmsTemplateCreateDto json |
| Post | `/enable` | `Enable` | [FromQuery]string id |
| Post | `/disable` | `Disable` | [FromQuery]string id |
| Post | `/approve` | `Approve` | [FromBody] SMSTemplateApproveDto json |
| Post | `/disapprove` | `Disapprove` | [FromBody] SMSTemplateDisapproveDto json |

### TopicController.cs
- Route prefix: `api/connect/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `Post` | [FromBody] CreateTopic dto |
| Post | `/mainTopic` | `CreateMainTopic` | [FromBody] CreateTopic dto |
| Post | `/updateMainTopic` | `UpdateMainTopic` | [FromBody] UpdateTopicDto dto |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/updateFriendlyName` | `UpdateFriendlyName` | [FromBody] UpdateFriendlyNameDto dto |
| Post | `/updateDescription` | `UpdateDescription` | [FromBody] UpdateDescriptionDto dto |
| Post | `/updateCategory` | `UpdateCategory` | [FromBody] UpdateCategoryDto dto |
| Post | `/updateRole` | `UpdateRole` | [FromBody] UpdateRoleDto dto |
| Post | `/updateAccess` | `UpdateAccess` | [FromBody] UpdateAccessDto dto |
| Post | `/updateSubscriptions` | `UpdateSubscriptions` | [FromBody] UpdateSubscriptionsDto dto |
| Get | `/` | `Topics` |  |
| Get | `/mainTopics` | `GetMainTopics` |  |
| Post | `/handle` | `UpdateHandle` | [FromBody] HandleDto dto |
| Get | `/handles` | `GetHandles` |  |
| Post | `/subscriptionAllowed` | `UpdateSubscriptionAllowed` | [FromBody] UpdateSubscriptionAllowedDto dto |

### WorkflowController.cs
- Route prefix: `api/connect/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/init` | `Init` | [FromQuery] string title |
| Post | `/` | `Post` | [FromBody] JObject jObj |

## Command layer (*Cmd) — implement all
### ConnectCmd.cs
- `dynamic Lookup(List<string> contacts)`
- `void RequestApproval(string message, string instId = "", bool approve = false)`
- `void Disable(string id)`
- `void Enable(string id)`

### SMSTemplateCmd.cs
- `void Disable(string id)`
- `void Enable(string id)`
- `SmsMessage Approve(string id, string reason)`
- `SmsMessage Disapprove(string id, string reason)`

### TopicCmd.cs
- `void CreateTopic(List<CreateTopicDto> topics)`
- `void CreateMainTopic(List<CreateTopicDto> topics)`
- `void UpdateMainTopic(UpdateTopicDto topic)`
- `void Disable(string id)`
- `void Enable(string id)`
- `void UpdateFriendlyName(string id, string friendlyName)`
- `void UpdateDescription(string id, string description)`
- `void UpdateCategory(string id, CategoryEnum category)`
- `void UpdateRole(string id, RoleEnum role)`
- `void UpdateAccess(string id, AccessEnum access)`
- `void UpdateSubscriptions(string id, dynamic subscriptions, bool canUnsubscribe, bool isSubscriptionAllowed)`
- `Topic UpdateHandle(string id, string handle)`
- `Topic UpdateSubscriptionAllowed(string id, bool isSubscriptionAllowed)`

## Query layer (*Qry) — implement all
### ConnectQry.cs
- `List<ContactLookupDto> GetContacts()`
- `List<ContactLookupDto> Lookup(List<string> contacts)`
- `List<SMS_Recipient> GetSMSRecipients(SMSRecipientsDto dto)`
- `object GetPushNotificationsForUser(string user_id)`
- `void PushNotifications(List<string> userIds, string title, string content)`
- `List<Email_Recipient> GetEmailRecipients(EmailRecipientsDto dto)`

### FacetQry.cs
- `List<SMS_Recipient> GetRecipientsFromFacets(List<Facet> facets)`
- `List<SMS_Recipient> ApplyForStudents(List<Facet> facets)`
- `List<Facet> GetForStudents()`
- `List<Facet> GetForStaff()`
- `List<SMS_Recipient> ApplyForStaff(List<Facet> facets)`
- `List<Facet> GetForParents()`
- `List<SMS_Recipient> ApplyForParents(List<Facet> facets)`
- `List<ExamFacet> GetExamFacets()`
- `AttendanceFacet GetAbsenteeFacets(DateTime? date = null)`
- `List<string> GetAbsenteesOn(string instId = "", DateTime? date = null, int periodNumber = 0)`
- `FeeDefaulterFacet GetFeeDefaultersFacet(DateTime? date = null)`
- `DefaultersDTO GetDefaultersBy(DateTime date)`

### SMSTemplateQry.cs

### TopicQry.cs
- `List<TopicDto> GetTopics()`
- `List<TopicDto> GetMainTopics()`
- `List<string> GetHandles()`

## Service layer — implement or fold into *Service
### EtlService.cs
- `string GetApiScript(string endPoint, object payload, string separator = "\u0009", string verb = "post")`
- `string GetTargetId(string base_id, string year)`
- `string GetS3PathApprovedSMSTemplates(string shortName, string targetYear, string env = "test")`
- `string GetEtlFileName(string shortName, string targetYear, string env = "test")`
- `string GetEtlFileName_G_SuiteUsers(string shortName, string targetYear, string env = "test")`
- `string GetEtlFileName_Results(string shortName, string targetYear, string env = "test")`

### MSAuthService.cs
- `Task AuthenticateRequestAsync(HttpRequestMessage request)`

### MSTeamService.cs
- `Task<IGraphServiceGroupsCollectionPage> GetClientGroups()`

### ReportService.cs
- `string GetSMSUsageReport(PrintOptions options = null)`
- `SMSUsageDto GetSMSUsageModel()`
- `string GetInvalidMobiles(PrintOptions options = null, bool testData = false)`
- `string GetInvalidMobilesData(bool testData = false)`
- `string GetInvalidEmails(PrintOptions options = null, bool testData = false)`
- `string GetInvalidEmailsData(bool testData = false)`

### SMSHelper.cs
- `static int SMSCreditsPerMessage(int messageLength)`

### WorkflowService.cs
- `Workflow For(string title)`
- `void Push(JObject jObj)`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Connect.cs

### Email.cs
**Properties:**
- `List<string> Recipients`
- `string Message`
- `string Type`
- `string From`
- `string Subject`
- `List<Attachment> Attachments`
- `Attachment Attachments`

### SmsMessage.cs
**Enums:**
- `SmsMessageStatusEnum`: Pending=0, Active=1, Disapproved=90, Disabled=99
**Properties:**
- `string Message`
- `SmsMessageStatusEnum Status`
- `string StatusAsString`
- `int Length`
- `int Credits`
- `string Reason`
- `string Id`
- `string InstId`
- `string Text`
- `string Id`
- `string Reason`
- `string Id`
- `string Reason`

### Student.cs
**Enums:**
- `StudentStatusEnum`: Unknown=-1, Active=1, Disabled=99
- `EnrollmentStatusEnum`: Unknown=-1, Active=1, Promoted=10, TC=98, Disabled=99
**Properties:**
- `string InstId`
- `string CourseId`
- `EnrollmentStatusEnum Status`
- `string Id`
- `string Name`
- `List<Enrollment> Enrollments`
- `StudentStatusEnum Status`
- `Person Father`
- `Person Mother`
- `Person Guardian`
- `string Email`
- `string Mobile`
- `string VirtualId`
- `string UserId`

### Topic.cs
**Enums:**
- `RoleEnum`: Admin=10, Member=20
- `CategoryEnum`: PrivateToInstitue=30, Public=40
- `AccessEnum`: Open=50, Restricted=60
- `TopicStatusEnum`: Unknown=0, Active=1, Disabled=99
**Properties:**
- `string MainTopicId`
- `string Name`
- `string FriendlyName`
- `string Description`
- `RoleEnum Role`
- `CategoryEnum Category`
- `AccessEnum Access`
- `dynamic Subscriptions`
- `TopicStatusEnum Status`
- `Dictionary<string, string> Meta`
- `List<string> Tags`
- `bool CanUnsubscribe`
- `bool CanPublish`
- `bool IsSubscriptionAllowed`
- `string Handle`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **46** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Connect rules
- SMS/Email send: validate template/recipients; enqueue job to Rabbit (or IEventPublisher) — do not block on gateway.
- Templates CRUD OwnerId-scoped; topics/workflows store definition JSON.


### Per-method signals from legacy code


### Source: `ConnectCmd.cs`

#### `RequestApproval(string message, string instId = "", bool approve = false)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (string.IsNullOrEmpty(inst_id)
- if (approve)
**Throws:** inst_id; 
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); if (string.IsNullOrEmpty(inst_id) || string.IsNullOrEmpty(message)) throw new ArgumentNullException(); var msg = new SmsMessage() { Message = message, CreatedBy = _uc.UserId, CreatedOn = DateTime.UtcNow }; msg.SetId(); msg.SetOwner(inst_id); if (approve) msg.Enable(); _session.Store(msg); _session.SaveChanges(); 
```

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (msg != null)
**Legacy body summary (adapt to EF):**
```
 var msg = _session.Load<SmsMessage>(id); if (msg != null) { msg.Disable(); _session.Store(msg); _session.SaveChanges(); } 
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (msg != null)
**Legacy body summary (adapt to EF):**
```
 var msg = _session.Load<SmsMessage>(id); if (msg != null) { msg.Enable(); _session.Store(msg); _session.SaveChanges(); } 
```

#### `Lookup(List<string> contacts)`
**Legacy body summary (adapt to EF):**
```
 return null; 
```


### Source: `SMSTemplateCmd.cs`

#### `Disapprove(string id, string reason)`
**Guards / branches (from legacy):**
- if (!_uc.UserEmail.EndsWith("bitloka.com")
- if (msg == null)
**Throws:** nameof(persona; nameof(msg
**Legacy body summary (adapt to EF):**
```
 var persona = _session.Load<PersonaContextDto>(_uc.CurrentPersonaId); if (!_uc.UserEmail.EndsWith("bitloka.com") || persona.Title != "Support") throw new NotInScopeException(nameof(persona)); var msg = _session.Load<SmsMessage>(id); if (msg == null) throw new ArgumentNullException(nameof(msg)); msg.Disapprove(reason); _session.Store(msg); _session.SaveChanges(); return msg; 
```

#### `Approve(string id, string reason)`
**Guards / branches (from legacy):**
- if (msg == null)
**Throws:** nameof(msg
**Legacy body summary (adapt to EF):**
```
 var msg = _session.Load<SmsMessage>(id); if (msg == null) throw new ArgumentNullException(nameof(msg)); msg.Approve(reason); _session.Store(msg); _session.SaveChanges(); return msg; 
```

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (msg != null)
**Legacy body summary (adapt to EF):**
```
 var msg = _session.Load<SmsMessage>(id); if (msg != null) { msg.Disable(); _session.Store(msg); _session.SaveChanges(); } 
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (msg != null)
**Legacy body summary (adapt to EF):**
```
 var msg = _session.Load<SmsMessage>(id); if (msg != null) { msg.Enable(); _session.Store(msg); _session.SaveChanges(); } 
```


### Source: `TopicCmd.cs`

#### `CreateTopic(List<CreateTopicDto> topics)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (topicExists)
- if (!topicExists)
**Throws:** inst_id; _tp.Name
**Status assignments:** Status = TopicStatusEnum.Active
**Side effects / messaging:**
- `EnqueTopicCreatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); foreach (var _tp in topics) { inst_id = string.IsNullOrEmpty(_tp.OwnerId) ? _uc.InstId : _tp.OwnerId; var topicExists = _session .Query<Topic>() .Customize(x => x.WaitForNonStaleResults()) .Where(o => o.Name == _tp.Name.ToLower()) .Where(o => o.MainTopicId == _tp.MainTopicId) .Where(o => o.OwnerId.Equals(inst_id)).FirstOrDefault() != null; if (topicExists) throw new DuplicateObjectException(_tp.Name); if (!t
```

#### `CreateMainTopic(List<CreateTopicDto> topics)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (topicExists)
- if (!topicExists)
**Throws:** inst_id; _tp.Name
**Status assignments:** Status = TopicStatusEnum.Active
**Side effects / messaging:**
- `EnqueTopicCreatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); foreach (var _tp in topics) { inst_id = string.IsNullOrEmpty(_tp.OwnerId) ? _uc.InstId : _tp.OwnerId; var topicExists = _session .Query<Topic>() .Customize(x => x.WaitForNonStaleResults()) .Where(o => o.Name == _tp.Name.ToLower()) .Where(o => o.MainTopicId == string.Empty) .Where(o => o.OwnerId.Equals(inst_id)).FirstOrDefault() != null; if (topicExists) throw new DuplicateObjectException(_tp.Name); if (!topi
```

#### `UpdateHandle(string id, string handle)`
**Guards / branches (from legacy):**
- if (topic == null)
- if (!handle.StartsWith(topic.Name.ToLower()
**Throws:** nameof(topic; "Incorrect handle name"
**Side effects / messaging:**
- `EnqueTopicUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var topic = _session.Load<Topic>(id); if (topic == null) throw new ObjectNotFoundException(nameof(topic)); if (!handle.StartsWith(topic.Name.ToLower())) throw new ArgumentException("Incorrect handle name"); topic.Handle = handle; _session.Store(topic); _session.SaveChanges(); var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "topic_updated", payload = payload }; _rabbitMQService.EnqueTopicUpdatedJob(job); return topic;
```

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (topic != null)
- if (!string.IsNullOrEmpty(topic.MainTopicId)
**Side effects / messaging:**
- `EnqueTopicStatusUpdatedJob(job)`
- `EnqueTopicStatusUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var topic = _session.Load<Topic>(id); if (topic != null) { topic.Disable(); _session.Store(topic); _session.SaveChanges(); if (!string.IsNullOrEmpty(topic.MainTopicId)) { var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "topic_status_updated", payload = payload }; _rabbitMQService.EnqueTopicStatusUpdatedJob(job); } else { var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (topic != null)
- if (!string.IsNullOrEmpty(topic.MainTopicId)
**Side effects / messaging:**
- `EnqueTopicStatusUpdatedJob(job)`
- `EnqueTopicStatusUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var topic = _session.Load<Topic>(id); if (topic != null) { topic.Enable(); _session.Store(topic); _session.SaveChanges(); if (!string.IsNullOrEmpty(topic.MainTopicId)) { var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "topic_status_updated", payload = payload }; _rabbitMQService.EnqueTopicStatusUpdatedJob(job); } else { var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new 
```

#### `UpdateMainTopic(UpdateTopicDto topic)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
**Throws:** inst_id
**Side effects / messaging:**
- `EnqueTopicCreatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var _topic = _session.Load<Topic>(topic.Id); _topic.Name = topic.Name.ToLower(); _topic.FriendlyName = topic.FriendlyName; _topic.Description = topic.Description; _topic.Category = topic.Category; _topic.Role = topic.Role; _topic.Access = topic.Access; _session.Store(_topic); _session.SaveChanges(); var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id =
```

#### `UpdateSubscriptionAllowed(string id, bool isSubscriptionAllowed)`
**Guards / branches (from legacy):**
- if (topic == null)
**Throws:** id
**Side effects / messaging:**
- `EnqueTopicUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var topic = _session.Load<Topic>(id); if (topic == null) throw new ObjectNotFoundException(id); topic.IsSubscriptionAllowed = isSubscriptionAllowed; _session.Store(topic); _session.SaveChanges(); var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "topic_updated", payload = payload }; _rabbitMQService.EnqueTopicUpdatedJob(job); return topic; 
```

#### `UpdateFriendlyName(string id, string friendlyName)`
**Guards / branches (from legacy):**
- if (topic != null)
**Side effects / messaging:**
- `EnqueTopicUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var topic = _session.Load<Topic>(id); if (topic != null) { topic.FriendlyName = friendlyName; _session.Store(topic); _session.SaveChanges(); var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "topic_updated", payload = payload }; _rabbitMQService.EnqueTopicUpdatedJob(job); } 
```

#### `UpdateDescription(string id, string description)`
**Guards / branches (from legacy):**
- if (topic != null)
**Side effects / messaging:**
- `EnqueTopicUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var topic = _session.Load<Topic>(id); if (topic != null) { topic.Description = description; _session.Store(topic); _session.SaveChanges(); var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "topic_updated", payload = payload }; _rabbitMQService.EnqueTopicUpdatedJob(job); } 
```

#### `UpdateCategory(string id, CategoryEnum category)`
**Guards / branches (from legacy):**
- if (topic != null)
**Side effects / messaging:**
- `EnqueTopicUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var topic = _session.Load<Topic>(id); if (topic != null) { topic.Category = category; _session.Store(topic); _session.SaveChanges(); var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "topic_updated", payload = payload }; _rabbitMQService.EnqueTopicUpdatedJob(job); } 
```

#### `UpdateRole(string id, RoleEnum role)`
**Guards / branches (from legacy):**
- if (topic != null)
**Side effects / messaging:**
- `EnqueTopicUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var topic = _session.Load<Topic>(id); if (topic != null) { topic.Role = role; _session.Store(topic); _session.SaveChanges(); var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "topic_updated", payload = payload }; _rabbitMQService.EnqueTopicUpdatedJob(job); } 
```

#### `UpdateAccess(string id, AccessEnum access)`
**Guards / branches (from legacy):**
- if (topic != null)
**Side effects / messaging:**
- `EnqueTopicUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var topic = _session.Load<Topic>(id); if (topic != null) { topic.Access = access; _session.Store(topic); _session.SaveChanges(); var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "topic_updated", payload = payload }; _rabbitMQService.EnqueTopicUpdatedJob(job); } 
```

#### `UpdateSubscriptions(string id, dynamic subscriptions, bool canUnsubscribe, bool isSubscriptionAllowed)`
**Guards / branches (from legacy):**
- if (topic != null)
**Side effects / messaging:**
- `EnqueTopicUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var topic = _session.Load<Topic>(id); if (topic != null) { topic.Subscriptions = subscriptions; topic.CanUnsubscribe = canUnsubscribe; topic.IsSubscriptionAllowed = isSubscriptionAllowed; _session.Store(topic); _session.SaveChanges(); var payload = new { inst_id = _uc.InstId }; var job_id = Guid.NewGuid().ToString(); var job = new Job() { id = job_id, title = "topic_updated", payload = payload }; _rabbitMQService.EnqueTopicUpdatedJob(job); } 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Fee

**Legacy:** `ct.fee` → **Target:** `Ct.Fee`  
**HTTP actions to implement:** 62  
**Notes:** Raven documents Fee/FeeTx; Rabbit fee.tx.done; indexes → reports

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### EmailController.cs
- Route prefix: `api/fee/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/feeReceipt` | `EmailFeeReceipt` | [FromBody] JObject jObj |

### FeeController.cs
- Route prefix: `api/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string id = "" |
| Get | `/getById` | `FeeGetById` | [FromQuery] string id |
| Get | `/feeInstFlattened` | `FeeInst` | [FromQuery] string courseId = "" |
| Post | `/` | `Post` | [FromBody] FeeCreateDto json |
| Post | `/updateFee` | `Update` | [FromBody] UpdateDto json |
| Post | `/canRemoveStudent` | `CanRemoveStudent` | [FromBody] FeeStudentDto json |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |

### FeeTxController.cs
- Route prefix: `api/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string id |
| Get | `/getTxHTML` | `GetTxHTML` | [FromQuery] string id |
| Post | `/transact` | `Transact` | [FromBody] FeeTx feeTx |
| Post | `/transact` | `Transact` | [FromBody] FeeTx feeTx |
| Post | `/disable` | `DisableTx` | [FromQuery] string id |
| Post | `/enable` | `EnableTx` | [FromQuery] string id |
| Get | `/studentTX` | `StudentTX` | [FromQuery] string id |
| Get | `/feesToBePaid` | `GetFeesToBePaid` | [FromQuery] string studentId |
| Post | `/updateTx` | `UpdateFeeTx` | [FromBody] UpdateFeeTxDto dto |
| Get | `/feeCollectionAccounts` | `GetFeeCollectionAccounts` |  |
| Post | `/suspenseAccountTx` | `CreateSuspenseAccountTx` | [FromBody] SuspenseAccountTxDto dto |
| Get | `/suspenseAccountTx` | `GetSuspenseAccountTx` | [FromQuery] string id |
| Post | `/allocateSuspenseAccountTx` | `AllocateSuspenseAccountTx` | [FromBody] FeeTx feeTx, [FromQuery] string suspenseAccountTxId |
| Post | `/disableSuspenseAccountTx` | `DisableSuspenseAccountTx` | [FromQuery] string id |
| Post | `/enableSuspenseAccountTx` | `EnableSuspenseAccountTx` | [FromQuery] string id |

### RefundController.cs
- Route prefix: `api/fee/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `Post` | [FromBody] RefundCreateDto json |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Get | `/` | `GetAll` |  |

### ReportController.cs
- Route prefix: `api/fee/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/feeStructure` | `FeeStructure` | [FromBody] JObject jObj |
| Post | `/coursewiseFeeStructure` | `CoursewiseFeeStructure` | [FromBody] JObject jObj |
| Post | `/feesExpected` | `FeesExpected` | [FromBody] JObject jObj |
| Post | `/datewiseFeeCollection` | `DatewiseFeeCollection` | [FromBody] JObject jObj |
| Post | `/datewiseFeeCollectionGrouped` | `DatewiseFeeCollectionGrouped` | [FromBody] JObject jObj |
| Post | `/feesDefaultersVersion1` | `FeesDefaultersList` | [FromBody] JObject jObj |
| Post | `/feesDefaultersVersion2` | `FeesDefaultersList_v2` | [FromBody] JObject jObj |
| Get | `/defaultersAsJson` | `DefaultersAsJson` |  |
| Post | `/receiptTransaction` | `ReceiptTransaction` | [FromBody] JObject jObj |
| Post | `/receiptTransactionCSV` | `ReceiptTransactionCSV` | [FromBody] JObject jObj |
| Post | `/studentwiseTransaction` | `StudentwiseTransaction` | [FromBody] JObject jObj |
| Post | `/courseStudentwiseTransaction` | `CourseStudentwiseTransaction` | [FromBody] JObject jObj |
| Post | `/adjustmentTransactions` | `AdjustmentTransactionsReport` | [FromBody] JObject jObj |
| Post | `/feesPaid` | `FeesPaid` | [FromBody] JObject jObj |
| Post | `/feesPaidCSV` | `FeesPaidCSV` | [FromBody] JObject jObj |
| Post | `/breakUps` | `GetBreakUpsReport` | [FromBody] JObject jObj |
| Post | `/fineCollected` | `GetFineCollectedReport` | [FromBody] JObject jObj |
| Post | `/latePayment` | `GetLatePaymentReport` | [FromBody] JObject jObj |
| Post | `/timeLine` | `GetTimeLineReport` | [FromBody] JObject jObj |
| Post | `/adjustmentBalance` | `AdjustmentBalanceReport` | [FromBody] JObject jObj |
| Post | `/advanceBalance` | `AdvanceBalanceReport` | [FromBody] JObject jObj |
| Post | `/TxHistory` | `TxHistory` |  |
| Post | `/feesPaidAfterAcademicYear` | `FeesPaidAfterAcademicYear` |  |
| Post | `/feesPaidByTCStudents` | `FeesPaidByTCStudents` |  |
| Post | `/coursewiseFeeBreakupAsJson` | `CoursewiseFeesBreakupAsJson` | [FromBody] JObject jObj |
| Post | `/coursewiseFeeCollectionAsJson` | `CoursewiseFeesCollectionAsJson` | [FromBody] JObject jObj |
| Post | `/coursewiseFeeStructureAsJson` | `CoursewiseFeesStructureAsJson` |  |
| Post | `/paymentModeBreakup` | `PaymentModeBreakup` | [FromBody] JObject jObj |
| Post | `/printFeeReceipt` | `PrintFeeReceipt` | [FromBody] JObject jObj |
| Get | `/feesPayable` | `FeesPayable` | [FromBody] JObject jObj |
| Post | `/feeReceivedInOtherAccount` | `FeeReceivedInOtherAccount` | [FromBody] JObject jObj |
| Post | `/concession` | `ConcessionReport` | [FromBody] JObject jObj |
| Post | `/refund` | `RefundReport` | [FromBody] JObject jObj |
| Post | `/suspenseAccountTxn` | `SuspenseAccountTxnReport` | [FromBody] JObject jObj |

## Command layer (*Cmd) — implement all
### FeeCmd.cs
- `void Disable(string id)`
- `void Enable(string id)`
- `void EnableTx(string id)`
- `void DisableTx(string id)`

### FeeTxCmd.cs
- `FeeTx UpdateFeeTx(string id, DateTime date, string refNo, string paidBy)`
- `SuspenseAccountTx CreateSuspenseAccountTx(SuspenseAccountTxDto dto)`
- `SuspenseAccountTx AllocateSuspenseAccountTx(string suspenseAccountTxId, string feeTxId)`
- `void DisableSuspenseAccountTx(string suspenseAccountTxId)`
- `void EnableSuspenseAccountTx(string suspenseAccountTxId)`

### RefundCmd.cs
- `object CreateRefund(string paymentMode, string studentId, decimal amount, DateTime date, string ownerId = null)`
- `void Disable(string id)`
- `void Enable(string id)`

## Query layer (*Qry) — implement all
### FeeQry.cs
- `RangeValue GetDates(string rangeType, int month, int year)`
- `List<FeeExpectedCollectedDto> FeesExpectedCollected(string instId = "", bool includeRTE = true)`

### FeeTxQry.cs
- `List<StudentClosingBalanceDto> GetClosingBalance(string instId = "", bool includeRTE = true)`
- `List<StuFeesExpectedPaidDto> GetStudentFeesExpectedPaid2(bool includeRTE = true)`
- `FeeTx GetFeeTxByRefNo(string refNo)`
- `List<string> GetFeeCollectionAccounts()`
- `SuspenseAccountTx GetSuspenseAccountTx(string id)`
- `List<SuspenseAccountTxnWithAllocationsDto> GetSuspenseAccountTx()`

### RefundQry.cs

## Service layer — implement or fold into *Service
### CsvService.cs
- `static ByteArrayContent GetByteArrayContent(string csv)`
- `static string Escape(string s)`
- `static string Unescape(string s)`
- `void Dispose()`

### EmailService.cs
- `void EmailFeeReceipt(string receiptString, string message, string studentId)`

### EtlService.cs
- `string GetApiScript(string endPoint, object payload, string separator = "\u0009", string verb = "post")`
- `string GetApiScriptJsonl(string endPoint, object payload, string separator = "\u0009", string verb = "post")`
- `string GetTargetId(string base_id, string year)`
- `string GetS3PathFees(string shortName, string targetYear, string env = "test")`
- `string GetEtlFileName(string shortName, string targetYear, string env = "test")`

### FeeService.cs
- `List<StuFeesExpectedPaidDto> GetStudentFeesExpectedPaid(List<StudentDetailDto> stuList, List<Fee> allFees, string courseId = null, List<FeeInstDto> feeInsts = null)`

### ReportService.cs
- `string GetBreakUps(PrintOptions options = null, bool testData = false)`
- `string GetBreakUpsData(bool testData = false)`
- `string GetFineCollected(PrintOptions options = null, bool testData = false)`
- `string GetFineCollectedData(bool testData = false)`
- `string GetLatePayment(PrintOptions options = null, bool testData = false)`
- `string GetLatePaymentData(bool testData = false)`
- `string GetTimeLine(PrintOptions options = null, bool testData = false)`
- `string GetTimeLineData(bool testData = false)`
- `CoursewiseFeeWidgetDto CoursewiseFeesBreakup(List<RangeValue> dateRanges, string rangeType)`
- `CoursewiseFeeCollectedWidgetDto CoursewiseFeeCollection(List<RangeValue> dateRanges, string rangeType)`
- `CoursewiseFeeWidgetDto CoursewiseFeesStructureBreakup()`

### StudentClientService.cs
- `Task UpdateStudentFeeReceivable_old(string studentId, string instId, string feeId, string feeName, string txId, decimal amount)`
- `Task UpdateStudentFeeReceivable(string studentId, string instId, string feeId, string feeName, string txId, decimal amount)`
- `Task UpdateFeeReceivableStatus(string studentId, string txId, string instId, string feeId, string feeName, decimal amount)`
- `Task UpdateStudentFeeReceivableForSpanningFee(string feeId, string feeName, List<string> studentIds, decimal amount, string instId)`
- `Task UpdateStudentFeeReceivableStatus(string feeId, List<string> studentIds, string instId, string status)`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Fee.cs
**Enums:**
- `FeeStatusEnum`: Unknown=0, Active=1, Disabled=99
**Properties:**
- `string Name`
- `string NameLower`
- `string DisplayText`
- `decimal Amount`
- `List<string> Tags`
- `bool CollectStudentWise`
- `List<string> StudentList`
- `List<string> CourseList`
- `List<Installment> Installments`
- `List<Fine> Fines`
- `bool IsTxDone`
- `FeeStatusEnum Status`
- `bool SpansAcademicYears`
- `int Id`
- `string Description`
- `Decimal Amount`
- `DateTime? DueDate`
- `int Id`
- `string Description`
- `Decimal Amount`
- `int OverDueByDays`
- `int Id`
- `Decimal Amount`
- `string Description`
- `string DueDate`
- `string FullName`
- `List<Sections> Sections`
- `bool Status`
- `string Name`
- `string FeeId`
- `int InstallmentId`
- `string Name`
- `decimal Amount`
- `DateTime? DueDate`
- `string FeeId`
- `string Name`
- `int InstallmentId`
- `decimal Amount`
- `DateTime Date`
- `string ReceiptNo`
- `string TxId`
- `int FineId`
- `string Name`
- `string FeeId`
- `string FeeName`
- `decimal Amount`
- `string StudentId`
- `List<FeesPaid> FeesPaid`
- `List<PendingFees> PendingFees`
- `List<FinesApplicable> FinesApplicable`
- `bool AllowDiscount`
- `string Id`
- `string Name`
- `string ShortName`
- `DateTime? AcademicYearFrom`
- `DateTime? AcademicYearTo`
- `string Id`
- `string Name`
- `string InstId`

### FeeTx.cs
**Enums:**
- `FeeTxStatusEnum`: Active=1, Disabled=99
**Properties:**
- `string TxNo`
- `DateTime TxDate`
- `string StudentId`
- `List<FeeTxDto> InstallmentsPaid`
- `List<FeeFineDto> FinesPaid`
- `List<FeeDiscountDto> Discounts`
- `FeeAdjustmentDto FeeAdjustment`
- `string PaymentMode`
- `bool IsFinePaid`
- `bool IsDiscountGiven`
- `bool HasFeeAdjustment`
- `bool IsOpeningBalanceAdjusted`
- `string RefNo`
- `string ReceivedInAccount`
- `decimal Amount`
- `FeeTxStatusEnum Status`
- `string PaidBy`
- `string ChequeNo`
- `string BankName`
- `DateTime ChequeDate`
- `string OnlineTxnRefNo`
- `bool IsFromSuspenseAccount`
- `string FeeId`
- `string FeeName`
- `string FeeDisplayText`
- `int InstallmentId`
- `string Description`
- `DateTime? DueDate`
- `decimal Amount`
- `string Id`
- `string Name`
- `string Description`
- `decimal Amount`
- `int FineId`
- `string Description`
- `decimal Amount`
- `string FeeId`
- `string FeeName`
- `string Description`
- `decimal Amount`
- `string Id`
- `string OwnerId`
- `string ParentId`
- `string TxNo`
- `DateTime TxDate`
- `string StudentId`
- `string StudentName`
- `string CourseId`
- `string CourseName`
- `string Section`
- `decimal Amount`
- `List<FeeTxDto> InstallmentsPaid`
- `List<FeeFineDto> FinesPaid`
- `List<FeeDiscountDto> Discounts`
- `FeeAdjustmentDto FeeAdjustment`
- `bool IsFinePaid`
- `bool IsDiscountGiven`
- `bool IsOpeningBalanceAdjusted`
- `FeeTxStatusEnum Status`
- `string RefNo`
- `string PaidBy`
- `string ReceivedInAccount`
- `string PaymentMode`
- `string OnlineTxnRefNo`
- `string ChequeNo`
- `DateTime ChequeDate`
- `string BankName`
- `string Id`
- `DateTime TxDate`
- `string RefNo`
- `string PaidBy`
- `string ReceivedInAccount`

### Refund.cs
**Enums:**
- `RefundStatusEnum`: Active=1, Disabled=99
**Properties:**
- `string PaymentMode`
- `string StudentId`
- `decimal Amount`
- `DateTime Date`
- `RefundStatusEnum Status`

### SuspenseAccountTx.cs
**Enums:**
- `SuspenseAccountTxStatusEnum`: Pending=10, PartiallyAllocated=20, Allocated=30, Active=1, Disabled=99
**Properties:**
- `DateTime TxDate`
- `decimal Amount`
- `string PaymentMode`
- `string OnlineTxnRefNo`
- `string PaymentParticulars`
- `SuspenseAccountTxStatusEnum Status`
- `List<Allocation> Allocations`
- `string FeeTxId`
- `string FeeTxNo`
- `decimal AllocatedAmount`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **62** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Fee rules
- Always scope by `OwnerId` from `X-Owner-Id` / inst header.
- **FeeTransaction uniqueness (Transact2-style):** reject duplicate payment for same student/fee/period/amount when legacy enforced uniqueness; keep transaction id stable.
- **Expected vs paid:** queries compute balances from fee definitions minus successful transactions (exclude cancelled/refunded as per status enums in domain).
- **Refunds:** only against existing successful TX; do not allow refund > paid remainder.
- **Events:** after successful fee TX, publish `fee.tx.done` (payload includes fee_tx_id) via Rabbit/optional publisher — do not require consumer in-module.
- Reports (defaulters, closing balance, sectionwise): filter Active students/fees; group by section/course as field names allow.


### Per-method signals from legacy code


### Source: `FeeCmd`

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (_fee == null)
- if (fee_tx != null)
- if (_fee.SpansAcademicYears)
**Throws:** inst_id; $"Cannot disable. Fee Tx already done for {_fee.Name}"
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var _fee = _session.Load<Fee>(id); if (_fee == null) return; var fee_tx = _session.Query<FeeTx_Index.Result, FeeTx_Index>() .Where(o => o.FeeId == _fee.Id) .OfType<Fee>() .FirstOrDefault(); if (fee_tx != null) throw new ArgumentException($"Cannot disable. Fee Tx already done for {_fee.Name}"); _fee?.Disable(); _session.Store(_fee); _session.SaveChanges(); if (_fee.SpansAcademicYears) UpdateStudentFeeReceivab
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (_fee == null)
- if (_fee.SpansAcademicYears)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var _fee = _session.Load<Fee>(id); if (_fee == null) return; _fee?.Enable(); _session.Store(_fee); _session.SaveChanges(); if (_fee.SpansAcademicYears) UpdateStudentFeeReceivableStatus(_fee, "Active"); 
```

#### `EnableTx(string id)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(id)
- if (_uc.Persist)
**Legacy body summary (adapt to EF):**
```
 if (string.IsNullOrEmpty(id)) return; var _tx = _session.Load<FeeTx>(id); _tx.Enable(); if (_uc.Persist) { _session.Store(_tx); _session.SaveChanges(); } 
```

#### `DisableTx(string id)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(id)
- if (_uc.Persist)
**Legacy body summary (adapt to EF):**
```
 if (string.IsNullOrEmpty(id)) return; var _tx = _session.Load<FeeTx>(id); _tx.Disable(); if (_uc.Persist) { _session.Store(_tx); _session.SaveChanges(); } 
```


### Source: `FeeTxCmd`

#### `AllocateSuspenseAccountTx(string suspenseAccountTxId, string feeTxId)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (suspenseAccountTx == null)
- if (feeTx == null)
- if (suspenseAccountTx.Allocations.Count == 0)
- if (feeTx.Amount == suspenseAccountTx.Amount)
- if (feeTx.Amount < suspenseAccountTx.Amount)
- if (feeTx.Amount <= 0 || feeTx.Amount > remainingAmount)
- if (feeTx.Amount == remainingAmount)
- if (feeTx.Amount < remainingAmount)
**Throws:** inst_id; "Suspense account transaction not found"; "Fee transaction not found"; "Allocation amount exceeds available amount"; "Invalid allocation amount"
**Status assignments:** Status = SuspenseAccountTxStatusEnum.Allocated, Status = SuspenseAccountTxStatusEnum.PartiallyAllocated, Status = SuspenseAccountTxStatusEnum.Allocated, Status = SuspenseAccountTxStatusEnum.PartiallyAllocated
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var suspenseAccountTx = _session.Load<SuspenseAccountTx>(suspenseAccountTxId); if (suspenseAccountTx == null) throw new ObjectNotFoundException("Suspense account transaction not found"); var feeTx = _session.Load<FeeTx>(feeTxId); if (feeTx == null) throw new ObjectNotFoundException("Fee transaction not found"); if(suspenseAccountTx.Allocations.Count == 0) { var allocation = new Allocation() { FeeTxId = feeTx
```

#### `DisableSuspenseAccountTx(string suspenseAccountTxId)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (suspenseAccountTx == null)
- if (suspenseAccountTx.Allocations != null && suspenseAccountTx.Allocations.Count > 0)
**Throws:** inst_id; "Suspense account transaction not found"; "Cannot disable a suspense account transaction that has allocations"
**Status assignments:** Status = SuspenseAccountTxStatusEnum.Disabled
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var suspenseAccountTx = _session.Load<SuspenseAccountTx>(suspenseAccountTxId); if (suspenseAccountTx == null) throw new ObjectNotFoundException("Suspense account transaction not found"); if(suspenseAccountTx.Allocations != null && suspenseAccountTx.Allocations.Count > 0) { throw new InvalidOperationException("Cannot disable a suspense account transaction that has allocations"); } suspenseAccountTx.Status = S
```

#### `EnableSuspenseAccountTx(string suspenseAccountTxId)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (suspenseAccountTx == null)
**Throws:** inst_id; "Suspense account transaction not found"
**Status assignments:** Status = SuspenseAccountTxStatusEnum.Pending
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var suspenseAccountTx = _session.Load<SuspenseAccountTx>(suspenseAccountTxId); if (suspenseAccountTx == null) throw new ObjectNotFoundException("Suspense account transaction not found"); suspenseAccountTx.Status = SuspenseAccountTxStatusEnum.Pending; _session.Store(suspenseAccountTx); _session.SaveChanges(); 
```

#### `UpdateFeeTx(string id, DateTime date, string refNo, string paidBy)`
**Guards / branches (from legacy):**
- if (tx == null)
**Throws:** "Transaction not found"
**Legacy body summary (adapt to EF):**
```
 var tx = _session.Load<FeeTx>(id); if (tx == null) throw new ObjectNotFoundException("Transaction not found"); else { tx.TxDate = date; tx.RefNo = refNo; tx.PaidBy = paidBy; tx.ReceivedInAccount = tx.ReceivedInAccount; _session.Store(tx); _session.SaveChanges(); } return tx; 
```

#### `CreateSuspenseAccountTx(SuspenseAccountTxDto dto)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
**Throws:** inst_id
**Status assignments:** Status = SuspenseAccountTxStatusEnum.Pending
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var suspenseAccountTx = new SuspenseAccountTx() { OwnerId = inst_id, TxDate = dto.TxDate, Amount = dto.Amount, PaymentMode = dto.PaymentMode, OnlineTxnRefNo = dto.OnlineTxnRefNo, PaymentParticulars = dto.PaymentParticulars, Status = SuspenseAccountTxStatusEnum.Pending }; suspenseAccountTx.SetId(); _session.Store(suspenseAccountTx); _session.SaveChanges(); return suspenseAccountTx; 
```


### Source: `RefundCmd`

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (_refund == null)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var _refund = _session.Load<Refund>(id); if (_refund == null) return; _refund?.Disable(); _session.Store(_refund); _session.SaveChanges(); 
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (_refund == null)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var _refund = _session.Load<Refund>(id); if (_refund == null) return; _refund?.Enable(); _session.Store(_refund); _session.SaveChanges(); 
```

#### `CreateRefund(string paymentMode, string studentId, decimal amount, DateTime date, string ownerId = null)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(paymentMode)
**Throws:** "Invalid refund data."
**Status assignments:** Status = RefundStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 if (string.IsNullOrEmpty(paymentMode) || string.IsNullOrEmpty(studentId) || amount <= 0) { throw new ArgumentException("Invalid refund data."); } var refund = new Refund() { PaymentMode = paymentMode, StudentId = studentId, Amount = amount, Date = date }; refund.SetId(); refund.SetOwner(ownerId ?? _uc.InstId); refund.Status = RefundStatusEnum.Active; _session.Store(refund); _session.SaveChanges(); return refund; 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Gr

**Legacy:** `ct.gr` → **Target:** `Ct.Gr`  
**HTTP actions to implement:** 19  
**Notes:** Receipts / 80G / numbering

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### GrController.cs
- Route prefix: `api/gr/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery]string Id = "" |
| Post | `/` | `Post` | [FromBody]CreateReceiptDto json |
| Post | `/update` | `UpdateReceipt` | [FromQuery]string id, [FromBody]JObject jObj |
| Post | `/updatePaymentDetails` | `UpdatePaymentDetails` | [FromQuery]string id, [FromBody]JObject jObj |
| Post | `/updateOrderItemParticular` | `UpdateOrderItemParticular` | [FromQuery]string id, [FromBody]JObject jObj |
| Post | `/updateOrderItemAmount` | `UpdateOrderItemAmount` | [FromQuery]string id, [FromBody]JObject jObj |
| Post | `/updateOrderItemExtras` | `UpdateOrderItemExtras` | [FromQuery]string id, [FromBody]JObject jObj |
| Post | `/cancel` | `CancelReceipt` | [FromQuery]string id |
| Post | `/activate` | `ActivateReceipt` | [FromQuery] string id |
| Post | `/printReceipt` | `PrintReceipt` | [FromQuery]string id, [FromBody]JObject jObj |
| Get | `/getDto` | `GetDto` |  |
| Get | `/getDtoWithData` | `GetDtoWithData` | [FromQuery]string format = "json" |
| Post | `/audit` | `Audit` | [FromBody]ReceiptAuditDto json |
| Get | `/widget/receiptCollection` | `ReceiptCollection` |  |

### ReportController.cs
- Route prefix: `api/gr/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/getReceiptReports` | `GetReceiptReports` | [FromBody]JObject jObj |
| Post | `/getReceiptRevenueSharingReport` | `GetReceiptRevenueSharingReport` | [FromBody]JObject jObj |
| Post | `/getReceiptsSummary` | `GetReceiptsSummary` | [FromBody]JObject jObj |
| Post | `/getReceiptConsolidatedReport` | `GetReceiptConsolidatedReport` | [FromBody]JObject jObj |
| Post | `/printReceipt` | `PrintReceipt` | [FromBody]JObject jObj |

## Command layer (*Cmd) — implement all
### ReceiptCmd.cs
- `Receipt Create(DateTime date, Customer customer, ReceiptTypeEnum receiptType, List<OrderItem> orderItems, PaymentModeEnum paymentMode, FinancialInstrument fi, bool revenueSharingEnabled, int revenueShare, string receivedBy, Meta meta, string id = "", string ownerId)`
- `string UpdateReceipt(string id, ReceiptTypeEnum receiptType, DateTime date, Customer customer, bool revenueSharingEnabled, int revenueShare, Meta meta, string refNo)`
- `string UpdatePaymentDetails(string id, PaymentModeEnum paymentMode, FinancialInstrument fi)`
- `string UpdateOrderItemParticular(string id, string oldParticular, string newParticular, decimal amount)`
- `string UpdateOrderItemAmount(string id, string particular, decimal oldAmount, decimal newAmount)`
- `Receipt Audit(string id, string createdBy, DateTime createdOn)`
- `string UpdateOrderItemExtras(string id, string particular, decimal amount, string extraFieldName, string extraFieldValue)`
- `void CancelReceipt(string id)`
- `void ActivateReceipt(string id)`
- `string PrintReceipt(string id, Meta meta)`

## Query layer (*Qry) — implement all
### ReceiptQry.cs
- `Receipt GetReceiptById(string id)`
- `RangeValue GetDates(string rangeType, int rangeValue)`
- `List<Receipt> GetDtoWithData()`
- `dynamic GetMonthwiseReceiptsCollectionWidgetData(string instId = "")`
- `void SetRegulatoryCompliance(string instituteCode, string registrationNumber)`

## Service layer — implement or fold into *Service
### CsvService.cs
- `static ByteArrayContent GetByteArrayContent(string csv)`
- `static string Escape(string s)`
- `static string Unescape(string s)`
- `void Dispose()`

### ReceiptService.cs
- `ReceiptDto GetDto()`
- `List<ReceiptDto> GetDtoWithData()`

### ReportService.cs
- `string GetReceiptReport(DateTime from, DateTime to, bool includeOrderItems, bool includeCustomerAddress, PrintOptions options = null)`
- `List<MonthlyReceipts> GetReceiptReportModel(ClientDto instDto, DateTime fromDate, DateTime toDate, bool includeOrderItems, bool includeCustomerAddress, CultureInfo indianCurrency)`
- `string GetReceiptRevenueSharingByClass(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
- `List<RevenueClassResult> GetReceiptRevenueSharingByClassModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate)`
- `string GetReceiptRevenueSharingByPerson(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
- `List<RevenuePersonResult> GetReceiptRevenueSharingByPersonModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate)`
- `string GetReceiptReportSummary(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
- `List<ReceiptSummaryTemplate> GetReceiptReportSummaryModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate, CultureInfo indianCurrency)`
- `string GetReceiptConsolidatedReport(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
- `OrgReceiptResult GetReceiptConsolidatedReportModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate, CultureInfo indianCurrency)`
- `string GetReceiptPrint(string receiptId, PrintOptions options = null)`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### DateHelper.cs

### Receipt.cs
**Enums:**
- `ReceiptStatusEnum`: Active=1, Cancelled=99
- `PaymentModeEnum`: Cash=10, Cheque=20, DD=30, Netbanking=40, UPI=50
- `ReceiptTypeEnum`: Unknown=0, Regular=10, Donation=20
- `CustomerTypeEnum`: Unknown=0, Regular=10, Student=20
**Properties:**
- `string Number`
- `string InstId`
- `DateTime Date`
- `Customer Customer`
- `List<OrderItem> OrderItems`
- `decimal TotalAmount`
- `string ReceivedBy`
- `PaymentModeEnum PaymentMode`
- `FinancialInstrument FinancialInstrument`
- `ReceiptStatusEnum Status`
- `ReceiptTypeEnum ReceiptType`
- `bool RevenueSharingEnabled`
- `int RevenueShare`
- `Meta Meta`
- `string HTML`
- `string RefNo`
- `string Particular`
- `Dictionary<string, string> Extras`
- `decimal Amount`
- `string Name`
- `string Address`
- `string Contact`
- `string PAN`
- `string StudyingIn`
- `string Section`
- `CustomerTypeEnum CustomerType`
- `string Number`
- `string NumberAsString`
- `DateTime DrawnOn`
- `string BankName`
- `string BankBranch`
- `string Number`
- `bool Include80G`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **19** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting GR (Receipt) rules
- Receipt numbers sequential per OwnerId (or per series if legacy had series field).
- Create receipt: persist lines, totals, optional 80G fields; publish receipt-created event if legacy did.
- Do not reuse numbers for voided receipts if legacy kept gaps — prefer monotonic sequence table/row.


### Per-method signals from legacy code


### Source: `CsvService.cs`

#### `Escape(string s)`
**Guards / branches (from legacy):**
- if (s.Contains(QUOTE)
- if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED)
**Legacy body summary (adapt to EF):**
```
 if (s.Contains(QUOTE)) s = s.Replace(QUOTE, ESCAPED_QUOTE); if (s.IndexOfAny(CHARACTERS_THAT_MUST_BE_QUOTED) > -1) s = QUOTE + s + QUOTE; return s; 
```

#### `Unescape(string s)`
**Guards / branches (from legacy):**
- if (s.StartsWith(QUOTE)
- if (s.Contains(ESCAPED_QUOTE)
**Legacy body summary (adapt to EF):**
```
 if (s.StartsWith(QUOTE) && s.EndsWith(QUOTE)) { s = s.Substring(1, s.Length - 2); if (s.Contains(ESCAPED_QUOTE)) s = s.Replace(ESCAPED_QUOTE, QUOTE); } return s; 
```

#### `Dispose()`
**Guards / branches (from legacy):**
- if (null != __reader)
**Legacy body summary (adapt to EF):**
```
 if (null != __reader) __reader.Dispose(); 
```

#### `GetByteArrayContent(string csv)`
**Legacy body summary (adapt to EF):**
```
 var stream = new MemoryStream(); StreamWriter writer = new StreamWriter(stream); writer.Write(csv); writer.Flush(); stream.Position = 0; var result = new ByteArrayContent(stream.ToArray()); return result; 
```


### Source: `ReceiptCmd.cs`

#### `Create(DateTime date, Customer customer, ReceiptTypeEnum receiptType, List<OrderItem> orderItems, PaymentModeEnum paymentMode, FinancialInstrument fi, bool revenueSharingEnabled, int revenueShare, string rec)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (_uc.IsETL()
- if (!ownerId.Equals(instId)
- if (customer == null || string.IsNullOrEmpty(customer.Name)
- if (orderItems == null || orderItems.Count == 0)
- if (paymentMode == PaymentModeEnum.Cheque || paymentMode == PaymentModeEnum.DD)
- if (string.IsNullOrEmpty(fi.BankName)
- if (inst_dto == null)
- if (string.IsNullOrEmpty(rcpt_number)
- if (dupRcptNum != null)
**Throws:** ; ; ; 
**Legacy body summary (adapt to EF):**
```
 var instId = _uc.InstId; // if (!this._uc.BoundByInst) return null; if (_uc.IsETL()) { if (!ownerId.Equals(instId)) return null; } if (customer == null || string.IsNullOrEmpty(customer.Name)) throw new ArgumentNullException(); if (orderItems == null || orderItems.Count == 0) throw new ArgumentNullException(); if (paymentMode == PaymentModeEnum.Cheque || paymentMode == PaymentModeEnum.DD) { if (string.IsNullOrEmpty(fi.BankName) || string.IsNullOrEmpty(fi.BankBranch)) throw new ArgumentNullExcept
```

#### `UpdateReceipt(string id, ReceiptTypeEnum receiptType, DateTime date, Customer customer, bool revenueSharingEnabled, int revenueShare, Meta meta, string refNo)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (receipt == null)
- if (string.IsNullOrEmpty(customer.Name)
- if (date == null)
- if (string.IsNullOrEmpty(customer.Address)
- if (string.IsNullOrEmpty(customer.Contact)
- if (receipt.Customer.CustomerType == CustomerTypeEnum.Student && !string.IsNullOrEmpty(customer.StudyingIn)
- if (revenueSharingEnabled)
- if (revenueShare > 0 && revenueShare < 100)
**Throws:** ; ; 
**Legacy body summary (adapt to EF):**
```
 var instId = _uc.InstId; if (!this._uc.BoundByInst) return null; var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); if (string.IsNullOrEmpty(customer.Name)) throw new ArgumentNullException(); if (date == null) throw new ArgumentNullException(); // update receipt date receipt.Date = date; receipt.ReceiptType = receiptType; //customer if (string.IsNullOrEmpty(customer.Address)) { customer.Address = null; } if (string.IsNullOrEmpty(customer.Contact)) 
```

#### `PrintReceipt(string id, Meta meta)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(id)
- if (receipt == null)
- if (inst_dto == null)
- if (org == null)
- if (string.IsNullOrEmpty(inst_header_dto.Line1)
- if (e.Key.Contains("Months")
- if (e.Key.Contains("ConductedBy")
- if (receipt.ReceiptType != ReceiptTypeEnum.Donation)
- if (string.IsNullOrEmpty(receipt.Customer.Address)
- if (string.IsNullOrEmpty(receipt.Customer.Contact)
**Throws:** ; 
**Legacy body summary (adapt to EF):**
```
 var instId = _uc.InstId; if (string.IsNullOrEmpty(id)) return null; var receipt = _session.Load<Receipt>(id); if (receipt == null) return null; var inst_dto = RedisCache.GetClientDto(instId); if (inst_dto == null) throw new ArgumentNullException(); //TBD: Get org details // var org = session.Load<Org>(institute.ParentId); // if (org == null) throw new ArgumentNullException(); var include80G = meta != null ? meta.Include80G : false; var prefix = inst_dto.ShortName + "_"; string headerTemplateFil
```

#### `UpdateOrderItemParticular(string id, string oldParticular, string newParticular, decimal amount)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (receipt == null)
- if (string.IsNullOrEmpty(oldParticular)
- if (orderItem == null)
**Throws:** ; ; "OrderItem not found"
**Legacy body summary (adapt to EF):**
```
 if (!this._uc.BoundByInst) return null; var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); if (string.IsNullOrEmpty(oldParticular) || string.IsNullOrEmpty(newParticular)) throw new ArgumentNullException(); //check if order item exists var orderItem = receipt.OrderItems.Where(o => o.Particular.Equals(oldParticular) && o.Amount.Equals(amount)).SingleOrDefault(); if (orderItem == null) throw new ObjectNotFoundException("OrderItem not found"); // updat
```

#### `UpdateOrderItemAmount(string id, string particular, decimal oldAmount, decimal newAmount)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (receipt == null)
- if (string.IsNullOrEmpty(particular)
- if (orderItem == null)
**Throws:** ; ; "OrderItem not found"
**Legacy body summary (adapt to EF):**
```
 if (!this._uc.BoundByInst) return null; var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); if (string.IsNullOrEmpty(particular)) throw new ArgumentNullException(); //check if order item exists var orderItem = receipt.OrderItems.Where(o => o.Particular.Equals(particular) && o.Amount.Equals(oldAmount)).SingleOrDefault(); if (orderItem == null) throw new ObjectNotFoundException("OrderItem not found"); // update orderItem orderItem.Amount = newAmount; 
```

#### `UpdatePaymentDetails(string id, PaymentModeEnum paymentMode, FinancialInstrument fi)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (paymentMode == PaymentModeEnum.Cheque || paymentMode == PaymentModeEnum.DD)
- if (string.IsNullOrEmpty(fi.BankName)
- if (receipt == null)
**Throws:** ; 
**Legacy body summary (adapt to EF):**
```
 if (!this._uc.BoundByInst) return null; if (paymentMode == PaymentModeEnum.Cheque || paymentMode == PaymentModeEnum.DD) { if (string.IsNullOrEmpty(fi.BankName) || string.IsNullOrEmpty(fi.BankBranch)) throw new ArgumentNullException(); } var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); receipt.PaymentMode = paymentMode; receipt.FinancialInstrument = fi; receipt.HTML = this.PrintReceipt(receipt.Id, receipt.Meta); _session.SaveChanges(); return rece
```

#### `UpdateOrderItemExtras(string id, string particular, decimal amount, string extraFieldName, string extraFieldValue)`
**Guards / branches (from legacy):**
- if (!this._uc.BoundByInst)
- if (receipt == null)
- if (orderItem == null)
- if (ext.Key == null)
**Throws:** ; "OrderItem not found"
**Legacy body summary (adapt to EF):**
```
 if (!this._uc.BoundByInst) return null; var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); //check if order item exists var orderItem = receipt.OrderItems.Where(o => o.Particular.Equals(particular) && o.Amount.Equals(amount)).SingleOrDefault(); if (orderItem == null) throw new ObjectNotFoundException("OrderItem not found"); // update orderItem var ext = orderItem.Extras.Where(e => e.Key.Equals(extraFieldName)).SingleOrDefault(); if (ext.Key == null
```

#### `Audit(string id, string createdBy, DateTime createdOn)`
**Guards / branches (from legacy):**
- if (string.IsNullOrEmpty(id)
- if (receipt == null)
**Throws:** ; 
**Legacy body summary (adapt to EF):**
```
 var instId = _uc.InstId; if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(); var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); receipt.CreatedOn = createdOn; receipt.CreatedBy = createdBy; _session.SaveChanges(); return receipt; 
```

#### `CancelReceipt(string id)`
**Guards / branches (from legacy):**
- if (this._uc.BoundByInst)
- if (receipt == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 if (this._uc.BoundByInst) { var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); //set status to Cancelled receipt.CancelReceipt(); receipt.ModifiedBy = this._uc.UserId; receipt.ModifiedOn = DateTime.UtcNow; _session.SaveChanges(); } 
```

#### `ActivateReceipt(string id)`
**Guards / branches (from legacy):**
- if (this._uc.BoundByInst)
- if (receipt == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 if (this._uc.BoundByInst) { var receipt = _session.Load<Receipt>(id); if (receipt == null) throw new ArgumentNullException(); //set status to Active receipt.ActivateReceipt(); receipt.ModifiedBy = this._uc.UserId; receipt.ModifiedOn = DateTime.UtcNow; _session.SaveChanges(); } 
```


### Source: `ReceiptService.cs`

#### `GetDto()`
**Legacy body summary (adapt to EF):**
```
 return new ReceiptDto(); 
```

#### `GetDtoWithData()`
**Legacy body summary (adapt to EF):**
```
 var receipts = _reciptQry.GetDtoWithData(); var recpt_list = new List<ReceiptDto>(); foreach (var recpt in receipts) { var dto = new ReceiptDto(); dto.id = recpt.Id; dto.amount = recpt.TotalAmount; dto.customer_address = recpt.Customer.Address; dto.customer_contact = recpt.Customer.Contact; dto.customer_name = recpt.Customer.Name; dto.customer_pan_number = recpt.Customer.PAN; dto.customer_section = recpt.Customer.Section; dto.customer_studying_in = recpt.Customer.StudyingIn; dto.date = recpt.Da
```


### Source: `ReportService.cs`

#### `GetReceiptReport(DateTime from, DateTime to, bool includeOrderItems, bool includeCustomerAddress, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (options == null)
- if (options.RecordsPerPage == 0)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 CultureInfo indianCurrency = new CultureInfo("hi-IN"); //context var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); //report header var inst_dto = RedisCache.GetClientDto(inst_id); //report options if (options == null) options = new PrintOptions() { RecordsPerPage = 2 }; if (options.RecordsPerPage == 0) options.RecordsPerPage = 2; //model var model = GetReceiptReportModel(inst_dto, from, to, includeOrderItems, includeCustomerAddress, indianCurrency); var dat
```

#### `GetReceiptConsolidatedReportModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate, CultureInfo indianCurrency)`
**Guards / branches (from legacy):**
- if (inst_ids != null && inst_ids.Count()
- if (inst != null)
- if (r == null || r.OrderItems == null || r.OrderItems.Count == 0)
- if (receiptChunkResult.Sum(x => x.ReceiptChunkCount)
- if (!instReceiptResult.Any(i => i.InstituteName.Contains(inst.Name)
**Status assignments:** Status = o, Status = r.Status
**Legacy body summary (adapt to EF):**
```
 var org = RedisCache.GetOrgDto(inst_dto.OwnerId); List<InstReceiptResult> instReceiptResult = new List<InstReceiptResult>(); var inst_ids = _session .Advanced.RawQuery<dynamic>($"from Institutes where OwnerId = '{org.Id}'") .Select(inst => (string)inst.Id) .ToList(); var tDate = toDate.AddDays(1).Date; if (inst_ids != null && inst_ids.Count() > 0) { foreach (var id in inst_ids) { var inst = RedisCache.GetClientDto(id); if (inst != null) { List<ReceiptChunkResult> receiptChunkResult = new List<R
```

#### `GetReceiptPrint(string receiptId, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (options == null)
- if (options.RecordsPerPage == 0)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 //context var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); //report header var inst_dto = RedisCache.GetClientDto(inst_id); //report options if (options == null) options = new PrintOptions() { RecordsPerPage = 1 }; //chunk size to be set if (options.RecordsPerPage == 0) options.RecordsPerPage = 1;//chunk size to be set //model var model = GetReceiptPrintModel(receiptId); //prepare report_data based on model & report options var title1 = $""; Tuple<string, 
```

#### `GetReceiptReportModel(ClientDto instDto, DateTime fromDate, DateTime toDate, bool includeOrderItems, bool includeCustomerAddress, CultureInfo indianCurrency)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (r == null || r.OrderItems == null || r.OrderItems.Count == 0)
**Throws:** inst_id
**Status assignments:** Status = o, Status = r.Status
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var monthlyReceipts = new List<MonthlyReceipts>(); fromDate = fromDate.Date < instDto.AcademicYearFrom.Date ? instDto.AcademicYearFrom : fromDate; toDate = toDate.Date > instDto.AcademicYearTo.Date ? instDto.AcademicYearTo.AddDays(1).Date : toDate; var receiptTemplate = new List<ReceiptTemplate>(); Expression<Func<Receipt_Details_Index.Result, bool>> filterByInst = o => o.InstId.Equals(_uc.InstId); Expressio
```

#### `GetReceiptRevenueSharingByClass(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (inst_dto == null)
- if (options == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ArgumentNullException(); if (options == null) options = new PrintOptions(); fromDate = fromDate.Date < inst_dto.AcademicYearFrom.Date ? inst_dto.AcademicYearFrom : fromDate; toDate = toDate.Date > inst_dto.AcademicYearTo.Date ? inst_dto.AcademicYearTo.AddDays(1).Date : toDate; //model var model = GetReceiptRevenueSharingByClassModel(inst_dto, fromDate, toDate); //data var data = GetReportD
```

#### `GetReceiptRevenueSharingByPerson(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (inst_dto == null)
- if (options == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ArgumentNullException(); if (options == null) options = new PrintOptions(); fromDate = fromDate.Date < inst_dto.AcademicYearFrom.Date ? inst_dto.AcademicYearFrom : fromDate; toDate = toDate.Date > inst_dto.AcademicYearTo.Date ? inst_dto.AcademicYearTo.AddDays(1).Date : toDate; //model var model = GetReceiptRevenueSharingByPersonModel(inst_dto, fromDate, toDate); //data var data = GetReport
```

#### `GetReceiptReportSummary(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (inst_dto == null)
- if (options == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 CultureInfo indianCurrency = new CultureInfo("hi-IN"); var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ArgumentNullException(); if (options == null) options = new PrintOptions(); var org = RedisCache.GetOrgDto(inst_dto.OwnerId); fromDate = fromDate.Date < inst_dto.AcademicYearFrom.Date ? inst_dto.AcademicYearFrom : fromDate; toDate = toDate.Date > inst_dto.AcademicYearTo.Date ? inst_dto.AcademicYearTo.AddDays(1).Date : toDate; //model v
```

#### `GetReceiptRevenueSharingByClassModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate)`
**Guards / branches (from legacy):**
- if (receiptQry != null && receiptQry.Count()
- if (r == null || r.OrderItems == null || r.OrderItems.Count == 0)
**Status assignments:** Status = o, Status = r.Status
**Legacy body summary (adapt to EF):**
```
 var revenueResult = new List<RevenueClassResult>(); Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_dto.Id); Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByReceiptStatus = o => o.Status == ReceiptStatusEnum.Active; Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByDateRange = o => (fromDate.Date <= o.Date) && (o.Date <= toDate.Date); var receiptQry = _session.Query<Receipt_Revenue_Share_Index.Result, Receip
```

#### `GetReceiptConsolidatedReport(DateTime fromDate, DateTime toDate, PrintOptions options = null)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByOrg)
- if (options == null)
**Legacy body summary (adapt to EF):**
```
 // if (!_uc.BoundByOrg) // return null; var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); CultureInfo indianCurrency = new CultureInfo("hi-IN"); if (options == null) options = new PrintOptions(); //model var model = GetReceiptConsolidatedReportModel(inst_dto, fromDate, toDate, indianCurrency); //data var data = GetReportData_ReceiptConsolidated(inst_dto, model, fromDate, toDate, indianCurrency, options); return data; 
```

#### `GetReceiptRevenueSharingByPersonModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate)`
**Guards / branches (from legacy):**
- if (r == null || r.OrderItems == null || r.OrderItems.Count == 0)
**Status assignments:** Status = o, Status = r.Status
**Legacy body summary (adapt to EF):**
```
 List<RevenuePersonResult> revenueResult = new List<RevenuePersonResult>(); Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_dto.Id); Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByReceiptStatus = o => o.Status == ReceiptStatusEnum.Active; Expression<Func<Receipt_Revenue_Share_Index.Result, bool>> filterByDateRange = o => (fromDate.Date <= o.Date) && (o.Date <= toDate.Date); var receiptQry = _session.Query<Receipt_Revenue_Sha
```

#### `GetReceiptReportSummaryModel(ClientDto inst_dto, DateTime fromDate, DateTime toDate, CultureInfo indianCurrency)`
**Status assignments:** Status = o
**Legacy body summary (adapt to EF):**
```
 var receiptSummary = new List<ReceiptSummaryTemplate>(); Expression<Func<Receipt_Details_Index.Result, bool>> filterByInst = o => o.InstId.Equals(inst_dto.Id); Expression<Func<Receipt_Details_Index.Result, bool>> filterByReceiptStatus = o => o.Status == ReceiptStatusEnum.Active; Expression<Func<Receipt_Details_Index.Result, bool>> filterByDateRange = o => (fromDate.Date < o.Date) && (o.Date < toDate.Date); var receiptQry = _session.Query<Receipt_Details_Index.Result, Receipt_Details_Index>() .W
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Library

**Legacy:** `ct.library` → **Target:** `Ct.Library`  
**HTTP actions to implement:** 50  
**Notes:** Material/Member/Circulation; remove any Marten

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### BarcodeController.cs
- Route prefix: `api/library/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/generate` | `Generate` | [FromBody]JObject jObj |

### CirculationController.cs
- Route prefix: `api/library/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string barcodeValue = "" |
| Post | `/issue` | `Issue` | [FromBody] MaterialIssueDto json |
| Post | `/return` | `Return` | [FromBody] MaterialReturnDto json |
| Post | `/reissue` | `Reissue` | [FromBody] MaterialReissueDto json |
| Get | `/widget/monthwiseCheckouts` | `GetMonthwiseCheckoutsWidgetData` |  |

### MaterialController.cs
- Route prefix: `api/library/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string id |
| Get | `/verificationMaterial` | `GetStockVerificationMaterial` | [FromQuery] string barcodeValue = "" |
| Post | `/` | `Post` | [FromBody] MaterialDto json |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/maintenance` | `Maintenance` | [FromQuery] string id |
| Post | `/returnMaintenanceMaterial` | `MaintenanceMaterialReturn` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Post | `/updateISBN` | `UpdateISBN` | [FromBody] UpdateISBNDto json |
| Post | `/clone` | `Clone` | [FromBody] MaterialCloneDto json |
| Post | `/relocate` | `Relocate` | [FromBody] MaterialRelocateDto json |
| Post | `/update` | `Update` | [FromBody] MaterialDto json |
| Post | `/setAttributes` | `SetAttributes` | [FromBody] UpdateAttributesDto json |
| Post | `/setTags` | `SetTags` | [FromBody] UpdateTagsDto json |
| Post | `/setValue` | `SetValue` | [FromBody] UpdateValueDto json |
| Post | `/verifyStock` | `VerifyStock` | [FromQuery] string id |
| Post | `/setPages` | `SetPages` | [FromBody] UpdatePagesDto json |
| Post | `/setOwnerId` | `SetOwnerId` | [FromBody] UpdateOwnerIdDto json |
| Get | `/getDtoWithData` | `GetDtoWithData` | [FromQuery] string format = Constants.FORMAT_CSV |
| Get | `/getDto` | `GetDtoWithoutData` |  |
| Get | `/widget/materialBreakup` | `GetMaterialBreakupWidgetData` |  |

### MemberController.cs
- Route prefix: `api/library/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` |  |
| Post | `/` | `Post` | [FromBody] MemberDto json |
| Post | `/disable` | `Disable` | [FromQuery]string id |
| Post | `/enable` | `Enable` | [FromQuery]string id |
| Post | `/setAttributes` | `SetAttributes` | [FromBody]UpdateMemberAttributesDto json |
| Post | `/setTags` | `SetTags` | [FromBody]UpdateMemberTagsDto json |
| Post | `/job` | `Job` |  |
| Post | `/upsert` | `Upsert` |  |

### ReportController.cs
- Route prefix: `api/library/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/ageAnalysis` | `AgeAnalysisReport` | [FromBody]JObject jObj |
| Post | `/booksWithoutCoverPhoto` | `BooksWithoutCoverPhoto` | [FromBody] JObject jObj |
| Post | `/booksWithoutISBN` | `BooksWithoutISBN` | [FromBody] JObject jObj |
| Post | `/booksWithoutSynopsis` | `BooksWithoutSynopsis` | [FromBody] JObject jObj |
| Post | `/lastMonitored` | `LastMonitored` | [FromBody] JObject jObj |
| Post | `/lateFees` | `LateFeesReport` | [FromBody] JObject jObj |
| Post | `/lostMaterials` | `LostMaterials` | [FromBody] JObject jObj |
| Post | `/materialsDueForMaintenance` | `MaterialsDueForMaintenance` | [FromBody] JObject jObj |
| Post | `/materialsUnderCirculation` | `MaterialsUnderCirculation` | [FromBody] JObject jObj |
| Post | `/materialsUnderMaintenance` | `MaterialsUnderMaintenance` | [FromBody] JObject jObj |
| Post | `/members` | `LibraryMembers` | [FromBody] JObject jObj |
| Post | `/overDue` | `OverDue` | [FromBody] JObject jObj |
| Post | `/referenceMaterials` | `ReferenceMaterials` | [FromBody] JObject jObj |
| Post | `/summary` | `SummaryReport` | [FromBody] JObject jObj |
| Post | `/valuation` | `ValuationReport` | [FromBody] JObject jObj |
| Post | `/unverifiedStock` | `UnverifiedStock` | [FromBody] JObject jObj |

## Command layer (*Cmd) — implement all
_None extracted._

## Query layer (*Qry) — implement all
### CirculationQry.cs
- `dynamic Get(string barcodeValue = "")`
- `dynamic GetMonthwiseCheckoutWidgetData(string instId = "")`

### MaterialQry.cs
- `MaterialView GetStockVerificationMaterial(string barcodeValue = "")`
- `dynamic GetMaterialBreakupWidgetData(string instId = "")`

### MemberQry.cs
- `IEnumerable<MemberView> Get()`

## Service layer — implement or fold into *Service
_None extracted._

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### Material.cs
**Enums:**
- `OwnerStatusEnum`
- `MaterialStatusEnum`: Active=1, Reserved=5, Issued=10, UnderMaintenance=20, OutOfCirculation=90, Disabled=99
**Properties:**
- `string TrackingId`
- `string ISBN`
- `string Title`
- `string Author`
- `string Publisher`
- `string OwnerId`
- `List<Owner> OwnerShip`
- `string Location`
- `Dictionary<string, object> Attributes`
- `List<string> Tags`
- `decimal Value`
- `DateTime TxDate`
- `DateTime DueOn`
- `MaterialStatusEnum Status`
- `long LastVerifiedOn`
- `string IssueTo`
- `int Pages`
- `string OwnerId`
- `DateTime? From`
- `DateTime? To`
- `OwnerStatusEnum Status`
- `string Id`
- `string TrackingId`
- `string ISBN`
- `string Title`
- `string Author`
- `string Publisher`
- `string OwnerId`
- `string Location`
- `MaterialStatusEnum Status`
- `string Id`
- `MaterialStatusEnum Status`
- `string Id`
- `MaterialStatusEnum Status`
- `string Id`
- `MaterialStatusEnum Status`
- `string Id`
- `MaterialStatusEnum Status`
- `string Id`
- `string OwnerId`
- `string TrackingId`
- `string MemberId`
- `string MembershipId`
- `string MemberType`
- `DateTime TxDate`
- `DateTime DueOn`
- `int DueIn`
- `MaterialStatusEnum Status`
- `string Id`
- `string OwnerId`
- `string TrackingId`
- `string MembershipId`
- `DateTime TxDate`
- `MaterialStatusEnum Status`
- `string Id`
- `string OwnerId`
- `string TrackingId`
- `DateTime TxDate`
- `DateTime DueOn`
- `string Id`
- `MaterialStatusEnum Status`
- `string Id`
- `Dictionary<string, object> Attributes`
- `string Id`
- `Dictionary<string, object> Attributes`
- `string Id`
- `List<string> Tags`
- `string Id`
- `List<string> Tags`
- `string Id`
- `string Location`
- `string Id`
- `string OwnerId`
- `string NewTrackingId`
- `string Title`
- `string Author`
- `string Publisher`
- `string Id`
- `decimal Value`
- `string Id`

### Member.cs
**Enums:**
- `MemberStatusEnum`: Active=1, Disabled=99
**Properties:**
- `string Name`
- `string MembershipId`
- `GenderEnum Gender`
- `Dictionary<string, object> Attributes`
- `List<string> Tags`
- `MemberStatusEnum Status`
- `string OwnerId`
- `string Id`
- `string Name`
- `string MembershipId`
- `MemberStatusEnum Status`
- `string OwnerId`
- `string Id`
- `MemberStatusEnum Status`
- `string Id`
- `MemberStatusEnum Status`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **50** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Library rules
- **Issue:** material must be Available (not Issued/Maintenance/Disabled/Lost); member Active; count of member open issues < MaxIssues; then create Circulation Issued, set material Issued + DueOn + CurrentMemberId.
- **Return:** find open circulation for material; set ReturnedOn; if ReturnedOn.Date > DueOn.Date, LateFee = daysLate * dailyRate (legacy used policy; use 1.0 per day if unspecified); material → Available; clear CurrentMemberId.
- **Reissue:** open circulation required; extend DueOn by N days; increment ReissueCount.
- **TrackingId** unique per OwnerId among non-disabled materials.


### Per-method signals from legacy code


### Source: `CirculationHandler`

#### `Handle(IssueMaterial command)`
**Guards / branches (from legacy):**
- if (!command.TxDate.HasValue)
- if (!command.DueOn.HasValue)
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.MaterialId); //if (!command.TxDate.HasValue) command.TxDate = DateTime.Now; // 15 Calendar Days are added if the DueOn is not provided.!-- The client has to provide the actual DueOn // if (!command.DueOn.HasValue) command.DueOn = DateTime.Now.AddDays(15); aggregate.Issue(command.TrackingId, command.MembershipId, command.MemberType, command.TxDate.Value, command.DueOn.Value); eventStore.Save(aggregate); 
```

#### `Handle(ReturnMaterial command)`
**Guards / branches (from legacy):**
- if (!command.TxDate.HasValue)
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.MaterialId); if (!command.TxDate.HasValue) command.TxDate = DateTime.Now; aggregate.Return(command.TrackingId, command.TxDate.Value); eventStore.Save(aggregate); 
```

#### `Handle(ReissueMaterial command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.MaterialId); aggregate.Reissue(command.TrackingId, command.TxDate.Value, command.DueOn); eventStore.Save(aggregate); 
```


### Source: `MaterialHandler`

#### `Handle(VerifyMaterialStock command)`
**Guards / branches (from legacy):**
- if (command == null || string.IsNullOrEmpty(command.Id)
- if (aggr != null && !string.IsNullOrEmpty(aggr.Id)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 if (command == null || string.IsNullOrEmpty(command.Id)) throw new ArgumentNullException(); var aggr = eventStore.GetById<Material>(command.Id); if (aggr != null && !string.IsNullOrEmpty(aggr.Id)) { aggr.VerifyStock(); eventStore.Save(aggr); } 
```

#### `Handle(CloneMaterial command)`
**Guards / branches (from legacy):**
- if (command == null || string.IsNullOrEmpty(command.Id)
- if (aggr != null && !string.IsNullOrEmpty(aggr.Id)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 if (command == null || string.IsNullOrEmpty(command.Id) || string.IsNullOrEmpty(command.CloneId)) throw new ArgumentNullException(); var aggr = eventStore.GetById<Material>(command.Id); if (aggr != null && !string.IsNullOrEmpty(aggr.Id)) { var clone = aggr.Clone(command.CloneId, command.TrackingId); eventStore.Save(clone); } 
```

#### `Handle(CreateMaterial command)`
**Guards / branches (from legacy):**
- if (aggr != null && !string.IsNullOrEmpty(aggr.Id)
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Material>(command.Id); if (aggr != null && !string.IsNullOrEmpty(aggr.Id)) return; //aggregate exists with the given id. var aggregate = new Material(command.Id, command.TrackingId, command.Title, command.OwnerId, command.Location, command.Author, command.Publisher, command.ISBN); aggregate.SetOwnerId(command.OwnerId); eventStore.Save(aggregate); 
```

#### `Handle(UpdateMaterial command)`
**Legacy body summary (adapt to EF):**
```
 var aggr = eventStore.GetById<Material>(command.Id); aggr.UpdateMaterial(command.NewTrackingId, command.Title, command.Author, command.Publisher); eventStore.Save(aggr); 
```

#### `Handle(EnableMaterial command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.Enable(); eventStore.Save(aggregate); 
```

#### `Handle(DisableMaterial command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.Disable(); eventStore.Save(aggregate); 
```

#### `Handle(MaterialMaintenance command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.Maintenance(); eventStore.Save(aggregate); 
```

#### `Handle(MaintenanceMaterialReturn command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.MaintenanceMaterialReturn(); eventStore.Save(aggregate); 
```

#### `Handle(TakeOutOfCirculation command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.TakeOutOfCirculation(); eventStore.Save(aggregate); 
```

#### `Handle(SetAttributes command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.SetAttributes(command.Attributes); eventStore.Save(aggregate); 
```

#### `Handle(ClearAttributes command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.ClearAttributes(); eventStore.Save(aggregate); 
```

#### `Handle(SetTags command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.SetTags(command.Tags); eventStore.Save(aggregate); 
```

#### `Handle(ClearTags command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.ClearTags(); eventStore.Save(aggregate); 
```

#### `Handle(SetOwnerId command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.SetOwnerId(command.OwnerId); eventStore.Save(aggregate); 
```

#### `Handle(MoveMaterial command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.Move(command.Location); eventStore.Save(aggregate); 
```

#### `Handle(SetValue command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.SetValue(command.Value); eventStore.Save(aggregate); 
```

#### `Handle(SetPages command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.SetPages(command.Pages); eventStore.Save(aggregate); 
```

#### `Handle(UpdateISBN command)`
**Legacy body summary (adapt to EF):**
```
 var aggregate = eventStore.GetById<Material>(command.Id); aggregate.UpdateISBN(command.ISBN); eventStore.Save(aggregate); 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Scheduler

**Legacy:** `ct.scheduler` → **Target:** `Ct.Scheduler`  
**HTTP actions to implement:** 13  
**Notes:** Calendar rules/events; library due-date working days

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### CalendarController.cs
- Route prefix: `api/scheduler/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` |  |
| Post | `/addCalendarRule` | `AddCalendarRule` | [FromBody] AddCalendarRuleDto json |
| Post | `/addCalendar` | `AddCalendar` | [FromBody] AddCalendarDto json |
| Post | `/removeCalendarRule` | `RemoveCalendarRule` | [FromBody] RemoveCalendarRuleDto json |
| Post | `/generateCalendar` | `GenerateCalendar` |  |
| Post | `/removeOccurrence` | `RemoveOccurrence` | [FromBody] RemoveOccurrenceDto json |
| Get | `/getCalendarEvents` | `GetCalendarEvents` |  |
| Get | `/upcomingEvents` | `GetUpcomingEvents` | [FromQuery] string eventType |
| Get | `/widget/upcomingEvents` | `GetUpcomingEventsData` |  |

### ReportController.cs
- Route prefix: `api/scheduler/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/calendar` | `GetCalendarReport` | [FromBody] JObject jObj |
| Post | `/holidayList` | `GetHolidayListReport` | [FromBody] JObject jObj |
| Post | `/upcomingEvents` | `GetUpcomingEventsReport` | [FromBody] JObject jObj |

### SchedulerController.cs
- Route prefix: `api/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/getLibraryDueDate` | `GetDueDate` | [FromQuery] DateTime? date |

## Command layer (*Cmd) — implement all
### CalendarCmd.cs
- `void AddCalendarRule(string title, string cronExpression, CalendarEventCategoryEnum eventCategory, string id = "")`
- `void AddCalendar(string title, string cronExpression, CalendarEventCategoryEnum eventCategory, decimal duration, string topicId, string userId, string id = "")`
- `void RemoveCalendarRule(string title, string id = "")`
- `void RemoveOccurrence(string title, DateTime date, string id = "")`
- `void GenerateCalendar(string id = "")`

## Query layer (*Qry) — implement all
### CalendarQry.cs
- `List<CalendarRule> Get()`
- `List<DateTime> GetHolidayDates()`
- `List<InstituteCalendar> GetCalendarEvents(string id = "")`
- `List<CalendarRuleDto> GetUpcomingEvents(string eventType = "", string instId = "")`
- `List<UpcomingEventsDto> UpcomingEventsWidgetData(string instId = "")`

## Service layer — implement or fold into *Service
### ReportService.cs
- `string GetCalendar(PrintOptions options = null)`
- `CalendarReportDto GetCalendarModel()`
- `string GetHolidayList(PrintOptions options = null)`
- `CalendarReportDto GetHolidayListModel()`
- `string GetUpcomingEvents(PrintOptions options = null)`
- `CalendarReportDto GetUpcomingEventsModel()`

### SchedulerService.cs
- `DateTime GetDueDate(DateTime? date = null)`
- `List<DateTime> GetHolidays(DateTime? acad_from = null, DateTime? acad_to = null)`
- `IEnumerable<DateTime> GetNextWorkingDays(int count, DateTime? fromDate = null, DateTime? toDate = null, List<DateTime> holidays = null)`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### CalendarRule.cs
**Enums:**
- `CalendarRuleStatusEnum`: Active=1, Disabled=99
- `CalendarEventCategoryEnum`: Event=10, Holiday=20, WeeklyHoliday=30, Exam=40
**Properties:**
- `string Title`
- `string CronExpression`
- `CalendarRuleStatusEnum CalendarRuleStatus`
- `CalendarEventCategoryEnum CalendarEventCategory`
- `int Weight`
- `decimal Duration`
- `string TopicId`
- `string UserId`
- `bool CreateMeetingLink`

### Cron.cs
**Properties:**
- `string CronExpression`
- `string CronMin`
- `string CronHour`
- `string CronDOM`
- `string CronMonth`
- `string CronDOW`
- `bool IsEveryDay`
- `bool IsAnyDay`
- `bool IsLastDayOfMonth`
- `bool IsDayList`
- `bool IsEveryMonth`
- `bool IsMonthList`
- `bool IsEveryDOW`
- `bool IsAnyDOW`
- `bool IsLastDOW`
- `bool IsNthDOW`
- `bool IsDOWList`

### IAcademicsModule.cs

### InstituteCalendar.cs
**Enums:**
- `EventStatusEnum`: Active=1, Deleted=99
**Properties:**
- `string InstId`
- `string EventName`
- `CalendarEventCategoryEnum EventCategory`
- `string EventCategoryAsString`
- `int Priority`
- `List<string> Audience`
- `List<string> ConductedBy`
- `List<Event> EventDates`
- `DateTime EventDate`
- `EventStatusEnum Status`
- `int Duration`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **13** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Scheduler rules
- **GenerateCalendar:** for each date in [From, To], create WeeklyHoliday events for configured weekend days; expand active rules with WeeklyDays into occurrences.
- **GetDueDate (library):** start from given date; add calendar days skipping Sunday and dates in holiday list until N working days counted; return due date.
- **RemoveOccurrence:** mark occurrence Status=Deleted (soft), do not hard-delete event document if other occurrences remain.
- **RemoveCalendarRule:** set rule Status=Disabled, do not delete.


### Per-method signals from legacy code


### Source: `CalendarCmd`

#### `RemoveOccurrence(string title, DateTime date, string id = "")`
**Guards / branches (from legacy):**
- if (_uc.BoundByUser)
- if (inst_dto == null)
- if (calendarRule != null)
- if (instituteCalendar != null)
- if (eventX != null)
- if (eventX.EventDate.Date > DateTime.UtcNow.Date)
**Throws:** ; 
**Side effects / messaging:**
- `EnqueueCalendarEventStatusUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (_uc.BoundByUser) throw new NotInInstituteContextException(); Expression<Func<InstituteCalendar, bool>> filterByInstId = o => o.InstId.Equals(_uc.InstId); Expression<Func<InstituteCalendar, bool>> eventNameMatches = o => o.EventName.Equals(title); Expression<Func<InstituteCalendar, bool>> eventDateMatches = o => o.EventDates.Any(e => e.EventDate.Date.Equals(date.Date)); var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ObjectNotFoundEx
```

#### `RemoveCalendarRule(string title, string id = "")`
**Guards / branches (from legacy):**
- if (_uc.BoundByUser)
- if (inst_dto == null)
- if (calendarRule != null)
- if (instituteCalendar != null)
- if (instituteCalendar.EventDates[i].EventDate > DateTime.UtcNow)
**Throws:** ; 
**Side effects / messaging:**
- `EnqueueCalendarEventStatusUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 if (_uc.BoundByUser) throw new NotInInstituteContextException(); var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ObjectNotFoundException(); Expression<Func<InstituteCalendar, bool>> filterByInstId = o => o.InstId.Equals(_uc.InstId); Expression<Func<InstituteCalendar, bool>> eventNameMatches = o => o.EventName.Equals(title); var qry = _session.Query<InstituteCalendar>().AsQueryable(); qry = qry.Where(filterByInstId); qry = qry.Where(even
```

#### `AddCalendarRule(string title, string cronExpression, CalendarEventCategoryEnum eventCategory, string id = "")`
**Guards / branches (from legacy):**
- if (!_uc.BoundByUser)
- if (inst_dto == null)
- if (string.IsNullOrEmpty(title)
- if (calendarRule == null)
**Throws:** ; 
**Side effects / messaging:**
- `EnqueueCalendarEventCreatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByUser) { var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ObjectNotFoundException(); if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(cronExpression)) throw new ArgumentNullException(); var calendarRule = _session.Query<CalendarRule>() .Where(c => c.Title.Equals(title)) .Where(c => c.OwnerId.Equals(inst_id)) .SingleOrDefault(); if (calendarRule == null) { calendarRule = new CalendarRule { Title = title }; calendarRu
```

#### `AddCalendar(string title, string cronExpression, CalendarEventCategoryEnum eventCategory, decimal duration, string topicId, string userId, string id = "")`
**Guards / branches (from legacy):**
- if (!_uc.BoundByUser)
- if (inst_dto == null)
- if (string.IsNullOrEmpty(title)
**Throws:** ; 
**Side effects / messaging:**
- `EnqueueCalendarEventCreatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByUser) { var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ObjectNotFoundException(); if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(cronExpression)) throw new ArgumentNullException(); var calendarRule = new CalendarRule { Title = title }; calendarRule.SetOwner(userId); calendarRule.SetParent(inst_id); calendarRule.SetId(id); calendarRule.CronExpression = cronExpression; calendarRule.CalendarEventCategory = eventCa
```

#### `GenerateCalendar(string id = "")`
**Guards / branches (from legacy):**
- if (!_uc.BoundByUser)
- if (inst_dto == null)
- if (calendarRules == null)
**Throws:** ; 
**Status assignments:** Status = EventStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 if (!_uc.BoundByUser) { var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); if (inst_dto == null) throw new ArgumentNullException(); var calendarRules = _session.Query<CalendarRule>() .Where(cr => cr.OwnerId == inst_id) .Where(cr => cr.CalendarRuleStatus == CalendarRuleStatusEnum.Active) .ToList(); if (calendarRules == null) throw new ArgumentNullException(); var instCalendarDocs = _session.Query<InstituteCalendar>() .Where(c => c.InstId.Equals(_uc.InstId)) .ToList(); for
```


### Source: `SchedulerService`

#### `GetNextWorkingDays(int count, DateTime? fromDate = null, DateTime? toDate = null, List<DateTime> holidays = null)`
**Guards / branches (from legacy):**
- if (i > count)
- if (dt > acad_to)
- if (holidays.Contains(dt)
**Legacy body summary (adapt to EF):**
```
 holidays = holidays ?? new List<DateTime>(); var inst_dto = RedisCache.GetClientDto(_uc.InstId); var acad_to = inst_dto.AcademicYearTo.Value; var from_date = fromDate.HasValue ? fromDate.Value : DateTime.Today; var to_date = toDate.HasValue ? toDate.Value.Date : acad_to.Date; var i = 0; for (var dt = from_date; dt <= to_date; dt = dt.AddDays(1)) { if (i > count) yield break; else if (dt > acad_to) yield break; else if (holidays.Contains(dt)) continue; else { i++; yield return dt; } } 
```

#### `GetHolidays(DateTime? acad_from = null, DateTime? acad_to = null)`
**Guards / branches (from legacy):**
- if (calendar_rules == null || calendar_rules.Count == 0)
**Legacy body summary (adapt to EF):**
```
 var inst_dto = RedisCache.GetClientDto(_uc.InstId); acad_from = acad_from.HasValue ? acad_from : inst_dto.AcademicYearFrom; acad_to = acad_to.HasValue ? acad_to : inst_dto.AcademicYearTo; var result = new List<DateTime>(); var calendar_rules = _policyEngine.CalendarRules.Where(o => o.CalendarEventCategory == CalendarEventCategoryEnum.Holiday).ToList(); if (calendar_rules == null || calendar_rules.Count == 0) return result; foreach (var cal_rule in calendar_rules) { var _cron = new Cron().Parse(
```

#### `GetDueDate(DateTime? date = null)`
**Legacy body summary (adapt to EF):**
```
 date = date == null ? DateTime.Today : date.Value.Date; var inst_id = _uc.InstId; var inst_dto = RedisCache.GetClientDto(inst_id); var academic_year_to_date = inst_dto.AcademicYearTo.Value; List<DateTime> holidays = GetHolidays(); int checkout_period = _policyEngine.LibraryCheckoutPeriod; var due_date = GetNextWorkingDays(checkout_period, date, holidays: holidays).Last(); return due_date; 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Staff

**Legacy:** `ct.staff` → **Target:** `Ct.Staff`  
**HTTP actions to implement:** 40  
**Notes:** Raven Staff with nested salaries/timetable; SalaryStructure

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### PingDBController.cs
- Route prefix: `api/staff/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` |  |
| Post | `/` | `Post` | [FromBody]Ping ping |

### ReportController.cs
- Route prefix: `api/staff/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/getStaffTodayTimetableReport` | `GetStaffTodayTimetableReport` | [FromBody] JObject jObj |
| Post | `/getStaffList` | `GetStaffListReport` | [FromBody] JObject jObj |
| Post | `/viewPayslip` | `GetPayslip` | [FromBody] JObject jObj |

### SalaryController.cs
- Route prefix: `api/staff/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/` | `CreateSalaryStructure` | [FromBody] SalaryStructureCreateDto json |
| Get | `/` | `Get` |  |
| Get | `/currentSalaryStructure` | `GetCurrentSalaryStructure` |  |

### StaffController.cs
- Route prefix: `api/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` |  |
| Get | `/getById` | `GetById` | [FromQuery] string id |
| Get | `/getByEmail` | `GetByEmail` | [FromQuery] string email |
| Post | `/` | `CreateStaff` | [FromBody] StaffCreateDto json |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Post | `/assignInstitutes` | `AssignInstitutes` | [FromBody] AssignInstDto json |
| Post | `/assignSubjects` | `AssignSubjects` | [FromBody] AssignSubDto json |
| Post | `/setAlias` | `SetAlias` | [FromQuery] string alias |
| Post | `/setClassTeacher` | `setClassTeacher` | [FromBody] SetClassTeacherDto json |
| Post | `/setPrimaryEmail` | `setPrimaryEmail` | [FromBody] SetPrimaryEmailDto json |
| Post | `/setPrimaryMobile` | `SetPrimaryMobile` | [FromBody] SetPrimaryMobileDto json |
| Post | `/timetable` | `Timetable` | [FromBody] TimeTableDto json |
| Get | `/timeTable` | `Timetable` | [FromQuery] string id, [FromQuery] DateTime dt, string range = "w" |
| Post | `/forInstitute` | `ForInstitute` | [FromBody] ForInstDto json |
| Post | `/update` | `UpdateStaff` | [FromBody] UpdateDto json |
| Get | `/facet` | `Facet` |  |
| Post | `/sms/staff` | `SMSStaff` | [FromBody] RecipientPostData json |
| Post | `/email/staff` | `EmailStaff` | [FromBody] CreateEmailDto json |
| Post | `/smsRecipients` | `SMSRecipients` | [FromBody] RecipientPostData json |
| Post | `/recipients` | `SubscriptionRecipients` | [FromBody] RecipientsDto dto |
| Post | `/updateStaffRefId` | `AssignStudentId` | [FromBody] UpdateStaffRefIdDto json |
| Post | `/updateGender` | `UpdateGender` | [FromBody] UpdateGenderDto json |
| Post | `/cache` | `Cache` | [FromQuery] bool reset = false |
| Post | `/employ` | `Employ` | [FromBody] EmploymentDto dto |
| Post | `/updateDesignation` | `UpdateDesignation` | [FromBody] UpdateDesignationDto json |
| Post | `/salaryAdd` | `SalaryAdd` | [FromBody] SalaryDto json |
| Post | `/salaryUpdate` | `SalaryUpdate` | [FromBody] SalaryUpdateDto json |
| Post | `/salarydelete` | `SalaryDelete` | [FromBody] SalaryDeleteDto json |
| Post | `/addPayslip` | `AddPayslip` | [FromBody] AddPayslipDto json |
| Post | `/verifyPayslip` | `VerifyPayslip` | [FromBody] AddPayslipDto json |
| Post | `/applyEvents` | `ApplyEvents` | [FromBody] List<EventLog> events |

## Command layer (*Cmd) — implement all
### SalaryCmd.cs
- `SalaryStructure Create(Dictionary<string, decimal> earnings, Dictionary<string, decimal> deductions, DateTime effectiveFrom)`

### StaffCmd.cs
- `void Disable(string id)`
- `void Enable(string id)`
- `Staff AssignInstitutes()`
- `Staff AssignSubjects(object subjects)`
- `Staff SetAlias(string alias)`
- `Staff setClassTeacher()`
- `Staff setPrimaryEmail(string id, string email)`
- `Staff SetPrimaryMobile(string id, string mobile)`
- `Staff ForInstitute()`
- `Staff Timetable()`
- `Staff UpdateStaff(string id, DateTime? DOB, DateTime DOJ, string name, GenderEnum gender, string refId, string email, string mobile)`
- `Staff UpdateStaffRefId(string id, string refId)`
- `Staff UpdateGender(string id, GenderEnum gender)`
- `void Employ(string id, string instId, string designation, DateTime? from, string refId)`
- `Staff AddSalary(string id, List<Item> earnings, List<Item> deductions, DateTime date)`
- `Staff UpdateSalary(string id, int salaryId, List<Item> earnings, List<Item> deductions, DateTime date)`
- `Staff DeleteSalary(string id, int salaryId)`
- `void UpdateDesignation(string id, string title, DateTime from, string remarks, StaffTypeEnum staffType)`
- `void AddPayslip(string id, DateTime date, List<Item> earnings, List<Item> deductions, string payment, int daysWorked, decimal amount, string bankAccountNumber, string ifsc, int salaryStructureId)`
- `void VerifyPayslip(string id, DateTime date)`

## Query layer (*Qry) — implement all
### SalaryQry.cs
- `List<SalaryStructure> Get()`
- `SalaryStructure GetCurrentSalaryStructure()`

### StaffQry.cs
- `List<StaffDto> Get()`
- `AttendanceDto GetAttendance(string instId = "", string staffId = "", DateTime? dateFrom = null, DateTime? dateTo = null, int sessionNumber = 0)`
- `Staff GetById(string id)`
- `IEnumerable<Staff> GetByEmail(string email)`
- `SubscriptionRecipientDto getRecipientDto(Staff stf)`
- `StaffTT GetTimetable(string id, DateTime date, string range = "w")`
- `dynamic GetStaffCountInstituteGenderwise(List<string> instituteIds = null)`
- `dynamic GetStaffCountInstitutewise(List<string> instituteIds = null)`

## Service layer — implement or fold into *Service
_None extracted._

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### AttendanceEvent.cs
**Properties:**
- `string Id`
- `string InstId`
- `DateTime Date`
- `int SessionNo`
- `string StaffId`
- `string Attendance`
- `DateTime CreatedOn`
- `string CreatedBy`

### Email.cs
**Properties:**
- `List<string> Recipients`
- `string Message`
- `string Type`
- `string From`
- `string Subject`
- `List<Attachment> Attachments`

### EventLog.cs
**Properties:**
- `string Event`
- `object TimeStamp`
- `string EventDataType`
- `string EventData`

### SalaryStructure.cs
**Properties:**
- `Dictionary<string, decimal> Earnings`
- `Dictionary<string, decimal> Deductions`
- `DateTime EffectiveFrom`

### Staff.cs
**Enums:**
- `StaffStatusEnum`: Unknown=-1, Active=1, Disabled=99
- `StaffTypeEnum`: Teaching=0, NonTeaching=1, Management=2
- `EmploymentStatusEnum`: Unknown=-1, Active=1, Disabled=99
- `CourseStatusEnum`: Unknown=0, Active=1, Disabled=99
- `EduLevelEnum`: Unknown=-1, PreNursery=2, Nursery=5, School=10, UnderGraduate=20, Graduate=30, PostGraduate=40
- `PaymentModeEnum`: Cash=0, Cheque=1, Online=2
- `PayslipStatusEnum`: Active=1, Verified=10
**Properties:**
- `string InstId`
- `DateTime? DOJ`
- `List<Designation> Designations`
- `StaffStatusEnum Status`
- `List<Employment> EmploymentHistory`
- `List<CourseSubject> CourseSubjectList`
- `string Alias`
- `ClassTeacher ClassTeacher`
- `string RefId`
- `string UserId`
- `List<Salary> Salaries`
- `List<Payslip> Payslips`
- `string Title`
- `DateTime From`
- `string Remarks`
- `string InstId`
- `StaffTypeEnum StaffType`
- `string Title`
- `DateTime From`
- `string Remarks`
- `string InstId`
- `string CourseId`
- `string CourseName`
- `string TermName`
- `string SecName`
- `List<string> SubjectsAssigned`
- `string InstId`
- `string Designation`
- `string RefId`
- `DateTime? From`
- `List<CourseSubject> CourseSubjectList`
- `EmploymentStatusEnum Status`
- `string CourseId`
- `string CourseName`
- `string Branch`
- `string Term`
- `string Section`
- `int AcadWeek`
- `DateTime StartOfWeek`
- `DayOfWeek StartDay`
- `IEnumerable<Object> TimeTable`
- `DateTime AcademicYearFrom`
- `DateTime AcademicYearTo`
- `string InstituteCode`
- `string RegistrationNumber`
- `bool ParentalAccessEnabled`
- `bool StaffAccessEnabled`
- `bool StudentAccessEnabled`
- `List<string> CourseOrder`
- `List<Period> Periods`
- `int Period`
- `List<TTCell> Cells`
- `string DOW`
- `Cell Cell`
- `int Number`
- `string DayOfWeek`
- `TimeSpan FromTime`
- `TimeSpan ToTime`
- `int? Duration`
- `bool Break`
- `string BreakDescription`
- `List<StaffSubject> StaffSubjectList`
- `int Number`
- `DayOfWeek DayOfWeek`
- `TimeSpan FromTime`
- `TimeSpan ToTime`
- `int Duration`
- `bool Break`
- `string BreakDescription`
- `List<StaffSubject> StaffSubjectList`
- `TeacherData TeacherData`
- `Subject Subject`
- `string StaffId`
- `bool IsSet`
- `string Remarks`
- `string StId`
- `string StaffFName`
- `string StaffMName`
- `string StaffLName`
- `string CourseId`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **40** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Staff rules
- **SalaryAdd / Payslip:** append SalaryRecord with new incremental Id; distinguish IsPayslip flag.
- **Employ:** append EmploymentRecord Active; ensure InstId in InstituteIds.
- **Disable/Enable:** Status Disabled vs Active; exclude Disabled from default lists.
- **Recipients:** filter by StaffType Teaching/NonTeaching/Management and optional id/code lists (* = all of type).


### Per-method signals from legacy code


### Source: `SalaryCmd`

#### `Create(Dictionary<string, decimal> earnings, Dictionary<string, decimal> deductions, DateTime effectiveFrom)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); var salaryStructure = new SalaryStructure(); salaryStructure.SetId(string.Empty); salaryStructure.SetOwner(inst_id); salaryStructure.Earnings = earnings; salaryStructure.Deductions = deductions; salaryStructure.EffectiveFrom = effectiveFrom; _session.Store(salaryStructure); _session.SaveChanges(); return salaryStructure; 
```


### Source: `StaffCmd`

#### `UpdateSalary(string id, int salaryId, List<Item> earnings, List<Item> deductions, DateTime date)`
**Guards / branches (from legacy):**
- if (staff == null)
- if (salary == null)
**Throws:** "Staff"; "Salary info not found"
**Legacy body summary (adapt to EF):**
```
 var staff = _session.Load<Staff>(id); if (staff == null) throw new ArgumentNullException("Staff"); var inst_id = _uc.InstId; var salary = staff.Salaries.FirstOrDefault(o => o.Id == salaryId); if (salary == null) throw new ArgumentNullException("Salary info not found"); salary.Earnings = earnings; salary.Deductions = deductions; salary.EffectiveDate = date; _session.Store(staff); _session.SaveChanges(); staff.Salaries = staff.Salaries.OrderByDescending(o => o.EffectiveDate).ToList(); return staf
```

#### `DeleteSalary(string id, int salaryId)`
**Guards / branches (from legacy):**
- if (staff == null)
- if (salary == null)
**Throws:** "Staff"; "Salary info not found"
**Legacy body summary (adapt to EF):**
```
 var staff = _session.Load<Staff>(id); if (staff == null) throw new ArgumentNullException("Staff"); var inst_id = _uc.InstId; var salary = staff.Salaries.FirstOrDefault(o => o.Id == salaryId && o.InstId == inst_id); if (salary == null) throw new ArgumentNullException("Salary info not found"); staff.Salaries.Remove(salary); _session.Store(staff); _session.SaveChanges(); staff.Salaries = staff.Salaries.OrderByDescending(o => o.EffectiveDate).ToList(); return staff; 
```

#### `VerifyPayslip(string id, DateTime date)`
**Guards / branches (from legacy):**
- if (staff == null)
- if (payslip == null)
**Throws:** "Staff"; 
**Status assignments:** Status = PayslipStatusEnum.Verified
**Legacy body summary (adapt to EF):**
```
 var staff = _session.Load<Staff>(id); if (staff == null) throw new ArgumentNullException("Staff"); var inst_id = _uc.InstId; var payslip = staff.Payslips.Where(o => o.Date == date && o.InstId == inst_id).FirstOrDefault(); if (payslip == null) throw new ObjectNotFoundException(); payslip.PayslipStatus = PayslipStatusEnum.Verified; _session.Store(staff); _session.SaveChanges(); 
```

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (string.IsNullOrEmpty(id)
- if (_staff == null)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); if (string.IsNullOrEmpty(id)) return; var _staff = _session.Load<Staff>(id); if (_staff == null) return; _staff?.Disable(); //TBD: Check if this is required var empl = _staff.EmploymentHistory.FirstOrDefault(o => o.InstId == inst_id); empl?.Disable(); _session.Store(_staff); _session.SaveChanges(); 
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (string.IsNullOrEmpty(id)
- if (_staff == null)
**Throws:** inst_id
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); if (string.IsNullOrEmpty(id)) return; var _staff = _session.Load<Staff>(id); if (_staff == null) return; _staff?.Disable();//TBD: Check if this is required var empl = _staff.EmploymentHistory.FirstOrDefault(o => o.InstId == inst_id); empl?.Enable(); _session.Store(_staff); _session.SaveChanges(); 
```

#### `AddPayslip(string id, DateTime date, List<Item> earnings, List<Item> deductions, string payment, int daysWorked, decimal amount, string bankAccountNumber, string ifsc, int salaryStructureId)`
**Guards / branches (from legacy):**
- if (staff == null)
- if (_payslip == null)
**Throws:** "Staff"
**Status assignments:** Status = PayslipStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var staff = _session.Load<Staff>(id); if (staff == null) throw new ArgumentNullException("Staff"); var inst_id = _uc.InstId; var _payslip = staff.Payslips.Where(o => o.Date.Month == date.Month && o.Date.Year == date.Year && o.InstId == inst_id).FirstOrDefault(); if (_payslip == null) { var payslip = new Payslip(); payslip.Id = getNewPayslipId(staff.Payslips); payslip.Date = DateTime.SpecifyKind(date, DateTimeKind.Utc); payslip.Earnings = earnings; payslip.Deductions = deductions; payslip.Paymen
```

#### `UpdateStaffRefId(string id, string refId)`
**Guards / branches (from legacy):**
- if (_staff == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _staff = _session.Load<Staff>(id); if (_staff == null) { throw new ObjectNotFoundException(nameof(id)); } _staff.RefId = refId; _session.Store(_staff); _session.SaveChanges(); return _staff; 
```

#### `UpdateGender(string id, GenderEnum gender)`
**Guards / branches (from legacy):**
- if (_staff == null)
**Throws:** nameof(id
**Legacy body summary (adapt to EF):**
```
 var _staff = _session.Load<Staff>(id); if (_staff == null) { throw new ObjectNotFoundException(nameof(id)); } _staff.Gender = gender; _session.Store(_staff); _session.SaveChanges(); return _staff; 
```

#### `Employ(string id, string instId, string designation, DateTime? from, string refId)`
**Guards / branches (from legacy):**
- if (staff == null)
**Throws:** "Staff"
**Legacy body summary (adapt to EF):**
```
 var staff = _session.Load<Staff>(id); if (staff == null) throw new ArgumentNullException("Staff"); staff.EmployInInstitute(instId, designation, from, refId); _session.Store(staff); _session.SaveChanges(); 
```

#### `AddSalary(string id, List<Item> earnings, List<Item> deductions, DateTime date)`
**Guards / branches (from legacy):**
- if (staff == null)
**Throws:** "Staff"
**Legacy body summary (adapt to EF):**
```
 var staff = _session.Load<Staff>(id); if (staff == null) throw new ArgumentNullException("Staff"); var inst_id = _uc.InstId; staff.AddSalary(inst_id, earnings, deductions, date); _session.Store(staff); _session.SaveChanges(); staff.Salaries = staff.Salaries.OrderByDescending(o => o.EffectiveDate).ToList(); return staff; 
```

#### `UpdateDesignation(string id, string title, DateTime from, string remarks, StaffTypeEnum staffType)`
**Guards / branches (from legacy):**
- if (staff == null)
**Throws:** "Staff"
**Legacy body summary (adapt to EF):**
```
 var staff = _session.Load<Staff>(id); if (staff == null) throw new ArgumentNullException("Staff"); var inst_id = _uc.InstId; staff.AddDesignation(inst_id, title, from, remarks, staffType); _session.Store(staff); _session.SaveChanges(); 
```

#### `AssignInstitutes()`
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 throw new NotImplementedException(); 
```

#### `AssignSubjects(object subjects)`
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 throw new NotImplementedException(); 
```

#### `SetAlias(string alias)`
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 throw new NotImplementedException(); 
```

#### `setClassTeacher()`
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 throw new NotImplementedException(); 
```

#### `ForInstitute()`
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 throw new NotImplementedException(); 
```

#### `Timetable()`
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 throw new NotImplementedException(); 
```

#### `setPrimaryEmail(string id, string email)`
**Guards / branches (from legacy):**
- if (!string.IsNullOrEmpty(email)
**Legacy body summary (adapt to EF):**
```
 var _staff = _session.Load<Staff>(id); if (!string.IsNullOrEmpty(email)) _staff.SetPrimaryEMail(email); _session.Store(_staff); _session.SaveChanges(); return _staff; 
```

#### `SetPrimaryMobile(string id, string mobile)`
**Guards / branches (from legacy):**
- if (!string.IsNullOrEmpty(mobile)
**Legacy body summary (adapt to EF):**
```
 var _staff = _session.Load<Staff>(id); if (!string.IsNullOrEmpty(mobile)) _staff.SetPrimaryMobile(mobile); _session.Store(_staff); _session.SaveChanges(); return _staff; 
```

#### `UpdateStaff(string id, DateTime? DOB, DateTime DOJ, string name, GenderEnum gender, string refId, string email, string mobile)`
**Legacy body summary (adapt to EF):**
```
 var _staff = _session.Load<Staff>(id); _staff.DOB = DOB; _staff.DOJ = DOJ; _staff.Name = name; _staff.Gender = gender; _staff.Email = email; _staff.Mobile = mobile; _staff.RefId = refId; _session.Store(_staff); _session.SaveChanges(); return _staff; 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.


---

# Detailed Migration Spec: Student

**Legacy:** `ct.stu` → **Target:** `Ct.Stu`  
**HTTP actions to implement:** 105  
**Notes:** Raven Student/Person; used by Fee directory

## Target platform (mandatory)
- **.NET 10**, **C# 14**, nullable enable
- **EF Core 10** + **Npgsql** PostgreSQL only (no RavenDB, no Marten)
- Module project: `Ct.{Name}/` with Domain, Infrastructure (`*DbContext`), Application (`*Service`, `*Cmd`, `*Qry`), Controllers
- Register `Add{Name}Module` + `AddApplicationPart` in `Ct.Host`
- Tenant headers: `X-Owner-Id`, `X-User-Id`, `X-Inst-Id` → always filter by OwnerId
- Nested collections → `jsonb` + ValueComparer
- Dual routes: `api/{module}/...` and legacy `api/{ControllerName}`

## Implementation order for agent
1. Create domain entities + enums from Domain resources section.
2. Create DbContext with tables + jsonb conversions + OwnerId indexes.
3. Implement Application services covering every Cmd + Qry method.
4. Implement Controllers with **exact** action method names and routes from the table.
5. Wire module DI; register in Host.
6. Verify method-name coverage 100% against the endpoint table.

## Endpoints (complete inventory from legacy controllers)

### ReportController.cs
- Route prefix: `api/stu/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Post | `/getMarksCard` | `GetMarksCardReport` | [FromBody] JObject jObj |
| Post | `/renderTemp` | `RenderTempReport` | [FromBody] JObject jObj |
| Post | `/viewFeeReceipt` | `ViewFeeReceiptReport` | [FromBody] JObject jObj |
| Post | `/viewTC` | `ViewTCReport` | [FromBody] JObject jObj |
| Post | `/certificate` | `CertificateReport` | [FromBody] CertificateDto dto |
| Post | `/getMarksForAllStudentsInExam` | `GetMarksForAllStudentsInExamReport` | [FromBody] JObject jObj |
| Post | `/getStudentTodayTimetableReport` | `GetStudentTodayTimetableReport` | [FromBody] JObject jObj |
| Post | `/hallTicket` | `HallTicketReport` | [FromBody] HallTicketDto dto |
| Post | `/studentDetails` | `StudentDetailsReport` | [FromBody] JObject jObj |
| Get | `/studentDetailsCSV` | `GetStudentDetailsCSV` | [FromBody] List<string> courseIds |
| Post | `/sectionwiseBreakUp` | `SectionwiseBreakUp` | [FromBody] JObject jObj |
| Post | `/basicSectionwiseBreakUp` | `BasicSectionwiseBreakUp` | [FromBody] JObject jObj |
| Post | `/newAdmissions` | `NewAdmissions` | [FromBody] JObject jObj |
| Post | `/coursewiseBreakUp` | `CoursewiseBreakUp` | [FromBody] JObject jObj |
| Post | `/coursewiseBreakUpAsJson` | `CoursewiseBreakUpAsJson` |  |
| Post | `/absentees` | `AbsenteesReport` | [FromBody] JObject jObj |
| Post | `/studentParentDetails` | `StudentParentDetailsReport` | [FromBody] JObject jObj |
| Post | `/studentParentDetailsCSV` | `StudentParentDetailsCSV` | [FromBody] JObject jObj |
| Post | `/tcStudents` | `TCStudentsReport` | [FromBody] JObject jObj |
| Post | `/iep` | `IEPReport` | [FromBody] JObject jObj |

### StudentController.cs
- Route prefix: `api/stu/[controller]`
| HTTP | Route | Method | Parameters |
|---|---|---|---|
| Get | `/` | `Get` | [FromQuery] string id = "", [FromQuery] bool activeStudentsOnly = false, [FromQuery] bool todaysAbsenteesOnly = false, [FromQuery] bool tcStudentsOnly = false, [FromQuery] int per = 0 |
| Get | `/all` | `All` | [FromQuery] bool includeRTE = false |
| Post | `/` | `Post` | [FromBody] CreateStudentDto json |
| Post | `/` | `UploadPhoto` | [FromQuery] string fileName |
| Post | `/enable` | `Enable` | [FromQuery] string id |
| Post | `/disable` | `Disable` | [FromQuery] string id |
| Post | `/tc` | `TC` | [FromQuery] string id |
| Post | `/AssignStudentId` | `AssignStudentId` | [FromBody] AssignStudentIdDto json |
| Post | `/promote` | `Promote` | [FromBody] StuPromotionDto dto |
| Post | `/setHomeAddress` | `SetHomeAddress` | [FromBody] SetHomeAddressStudentDto json |
| Post | `/setHomePhone` | `SetHomePhone` | [FromBody] HomePhoneDto json |
| Post | `/setPrimaryEmail` | `SetPrimaryEmail` | [FromBody] SetPrimaryEmailDto json |
| Post | `/setPrimaryMobile` | `SetPrimaryMobile` | [FromBody] SetPrimaryMobileDto json |
| Post | `/updateTags` | `SetTags` | [FromBody] UpdateTagsDto json |
| Post | `/updateCourse` | `UpdateCourse` | [FromBody] UpdateCourseDto json |
| Post | `/updateGender` | `UpdateGender` | [FromBody] UpdateGenderDto json |
| Post | `/timetable` | `TimeTable` | [FromBody] TimeTableDto json |
| Post | `/updateFatherDetails` | `UpdateFatherDetails` | [FromBody] UpdateFatherDetailsStudentDto json |
| Post | `/updateMotherDetails` | `UpdateMotherDetails` | [FromBody] UpdateMotherDetailsStudentDto json |
| Post | `/updateFatherEmail` | `UpdateFatherEmail` | [FromBody] UpdateFatherDetailsStudentDto json |
| Post | `/updateFatherMobile` | `UpdateFatherMobile` | [FromQuery] string id, [FromQuery] string mobile |
| Post | `/updateMotherEmail` | `UpdateMotherEmail` | [FromQuery] string id, [FromQuery] string email |
| Post | `/updateMotherMobile` | `UpdateMotherMobile` | [FromQuery] string id, [FromQuery] string mobile |
| Post | `/updateGuardianDetails` | `UpdateGuardianDetails` | [FromBody] UpdateGuardianDetailsStudentDto json |
| Post | `/updateGuardianMobile` | `UpdateGuardianMobile` | [FromQuery] string id, [FromQuery] string mobile |
| Post | `/updateGuardianEmail` | `UpdateGuardianEmail` | [FromQuery] string id, [FromQuery] string email |
| Post | `/update` | `Update` | [FromBody] UpdateStudentDto json |
| Post | `/getFilters` | `GetFilters` | [FromBody] GetFiltersStudentDto json |
| Post | `/setRollNo` | `SetRollNo` | [FromBody] SetRollNoDto json |
| Post | `/cache` | `Cache` | [FromQuery] bool reset = false |
| Post | `/applyEvents` | `ApplyEvents` | [FromBody] List<EventLog> events |
| Post | `/sms/absentees` | `SmsAbsentees` | [FromBody] JObject jObj |
| Get | `/facet` | `Facet` | [FromQuery] bool includeStudents = true |
| Post | `/sms/parent` | `SMSParentStudents` | [FromBody] RecipientPostData json |
| Post | `/email/parent` | `EmailParentStudents` | [FromBody] CreateEmailDto json |
| Post | `/smsRecipients` | `SMSRecipients` | [FromBody] RecipientPostData json |
| Post | `/recipients` | `SubscriptionRecipients` | [FromBody] RecipientPostData json |
| Post | `/getContacts` | `GetContacts` | [FromBody] JObject jObj |
| Post | `/getContactsFromFacet` | `GetContactsFromFacet` | [FromBody] RecipientsFacet Recipients |
| Get | `/getDto` | `GetDto` |  |
| Get | `/getDtoWithData` | `GetDtoWithData` | [FromQuery] string format = "json" |
| Get | `/getStudentsForExamFacets` | `GetStudentsForExamFacets` | [FromQuery] string courseId |
| Get | `/absenteesRecipientCount` | `GetAbsenteesSMSRecipientCount` | [FromQuery] int perNumber |
| Post | `/flatData` | `GetStudentsFlatData` | [FromBody] JObject jObj, [FromQuery] bool snakeCase = false |
| Get | `/profile` | `Profile` | [FromQuery] string userEmail, [FromQuery] string id |
| Post | `/updateDomicile` | `UpdateDomicile` | [FromBody] UpdateDomicileDto dto |
| Post | `/disableFatherMobile` | `DisableFatherMobile` | [FromQuery] string id |
| Post | `/disableMotherMobile` | `DisableMotherMobile` | [FromQuery] string id |
| Post | `/disableGuardianMobile` | `DisableGuardianMobile` | [FromQuery] string id |
| Post | `/enableFatherMobile` | `EnableFatherMobile` | [FromQuery] string id |
| Post | `/enableMotherMobile` | `EnableMotherMobile` | [FromQuery] string id |
| Post | `/enableGuardianMobile` | `EnableGuardianMobile` | [FromQuery] string id |
| Post | `/disableFatherEmail` | `DisableFatherEmail` | [FromQuery] string id |
| Post | `/disableMotherEmail` | `DisableMotherEmail` | [FromQuery] string id |
| Post | `/disableGuardianEmail` | `DisableGuardianEmail` | [FromQuery] string id |
| Post | `/enableFatherEmail` | `EnableFatherEmail` | [FromQuery] string id |
| Post | `/enableMotherEmail` | `EnableMotherEmail` | [FromQuery] string id |
| Post | `/enableGuardianEmail` | `EnableGuardianEmail` | [FromQuery] string id |
| Post | `/feesReceivable` | `UpdateFeesReceivable` | [FromBody] FeesReceivableDto dto |
| Post | `/feesReceivable` | `UpdateFeesReceivable` | [FromBody] FeesReceivableDto dto |
| Post | `/feesReceivableUpdate` | `UpdateFeesReceivableStatus` | [FromBody] FeesReceivableDto dto |
| Post | `/feesReceivableForSpanningFee` | `UpdateFeesReceivableForSpanningFee` | [FromBody] FeesReceivableForSpanningFeeDto dto |
| Post | `/feesReceivableStatusUpdate` | `UpdateFeesReceivableStatus` | [FromBody] FeesReceivableStatusBeforeTxDto dto |
| Post | `/openingBalance` | `UpdateOpeningBalance` | [FromBody] OpeningBalanceDto dto |
| Get | `/widget/genderBreakup` | `GetGenderBreakupData` |  |
| Get | `/getStudentsByTag` | `GetStudentsByTag` | [FromQuery] string tags |
| Post | `/iep` | `AddIEP` | [FromBody] CreateIEPDto dto |
| Get | `/iep` | `GetIEPDetails` | [FromQuery] string id |
| Get | `/goalsPerformance` | `GetGoalsPerformance` | [FromQuery] string studentId, [FromQuery] string term |
| Post | `/addStudentEssentials` | `AddStudentEssentials` | [FromBody] CreateItemRecordDto dto |
| Post | `/updateItem` | `UpdateItem` | [FromBody] UpdateItemDto dto |
| Post | `/disableItem` | `DisableItem` | [FromBody] DisableItemDto dto |
| Post | `/enableItem` | `EnableEssentialItem` | [FromBody] DisableItemDto dto |
| Get | `/essentials` | `GetStudentEssentials` | [FromQuery] string studentId |
| Post | `/medicalDiagnosisReport` | `MedicalDiagnosisReport` | [FromBody] DocumentDto json |
| Post | `/disabilityReport` | `DisabilityReport` | [FromBody] DocumentDto json |
| Post | `/birthCertificate` | `BirthCertificate` | [FromBody] DocumentDto json |
| Post | `/identificationProof` | `IdentificationProof` | [FromBody] DocumentDto json |
| Post | `/addressProof` | `AddressProof` | [FromBody] DocumentDto json |
| Post | `/therapyReport` | `TherapyReport` | [FromBody] DocumentDto json |
| Post | `/psycologicalAssessmentReport` | `PsycologicalAssessmentReport` | [FromBody] DocumentDto json |
| Get | `/baselineDataForm` | `GetBaselineDataForm` |  |
| Post | `/createBaselineData` | `CreateBaselineDataForm` | [FromBody] CreateBaselineDataDto dto |
| Get | `/baselineData` | `GetBaselineData` | [FromQuery] string id |
| Post | `/photo` | `Photo` | [FromBody] DocumentDto json |

## Command layer (*Cmd) — implement all
### StudentCmd.cs
- `Student Create(string aadharNo, string admissionNumber, string applicationNumber, string courseName, string courseId, DateTime doa, DateTime? dob, string email, GenderEnum? gender, string mobile, string name, string secName, string studentId, string termName, strin)`
- `Student UploadPhoto(string fileName)`
- `void Enable(string id)`
- `void Disable(string id)`
- `void Promote(string id, string baseInstId, string baseCourseId, string baseTerm, string baseSection, string targetInstId, string targetCourseId, string targetTerm, string targetSection)`
- `Student SetHomeAddress(string id, string city, string country, string line1, string line2, string pinCode, string state)`
- `Student SetHomePhone(string id, string homePhone)`
- `Student SetPrimaryEmail(string id, string email)`
- `Student SetPrimaryMobile(string id, string mobile)`
- `Student SetTags(string id, List<string> tags)`
- `Student TC(DateTime appliedOn, Dictionary<string, string> attributes, string character, DateTime issuedOn, string name, string remarks, string tcNo)`
- `Student TimeTable()`
- `Student UpdateFatherDetails(string id, string email, string mobile, string name, string pan)`
- `Student UpdateMotherDetails(string id, string email, string mobile, string name, string pan)`
- `Student UpdateFatherEmail(string id, string email)`
- `Student UpdateMotherMobile(string id, string mobile)`
- `Student UpdateFatherMobile(string id, string mobile)`
- `Student UpdateMotherEmail(string id, string email)`
- `Student UpdateGuardianDetails(string id, string email, GenderEnum gender, string mobile, string name)`
- `SMS SmsAbsentees(string instId = "", DateTime? date = null, int periodNumber = 0, string smsRefId = "")`
- `Student UpdateGuardianMobile(string id, string mobile)`
- `Student UpdateGuardianEmail(string id, string email)`
- `Student Update(string aadharNo, string id, string admissionNumber, string applicationNumber, DateTime? doa, DateTime DOB, GenderEnum gender, string name)`
- `Student GetFilters()`
- `Student SetRollNo(string id, string rollNo)`
- `Student UpdateCourse(string id, string courseName, string courseId, string secName, string branch = "")`
- `Student UpdateGender(string id, GenderEnum gender)`
- `string GetNextStudentId(DateTime doa, string courseName)`
- `string get_predicate(List<List<string>> recipients)`
- `List<string> GetAbsenteeIds(string instId = "", DateTime? date = null, int periodNumber = 0)`
- `List<string> GetAbsenteeMobiles(string instId = "", DateTime? date = null, int periodNumber = 0)`
- `Student UpdateDomicile(string id, string caste, string country, string motherTongue, string state)`
- `Student DisableFatherMobile(string id)`
- `Student DisableMotherMobile(string id)`
- `Student DisableGuardianMobile(string id)`
- `Student EnableFatherMobile(string id)`
- `Student EnableMotherMobile(string id)`
- `Student EnableGuardianMobile(string id)`
- `Student DisableFatherEmail(string id)`
- `Student DisableMotherEmail(string id)`
- `Student DisableGuardianEmail(string id)`
- `Student EnableFatherEmail(string id)`
- `Student EnableMotherEmail(string id)`
- `Student EnableGuardianEmail(string id)`
- `Student UpdateFeesReceivable(string id, string instId, string feeId, string feeName, string txId, decimal amount)`
- `Student UpdateFeesReceivableStatus(string id, string txId, string instId, string feeId, string feeName, decimal amount)`
- `void UpdateFeesReceivableForSpanningFee(string instId, string feeId, string feeName, decimal amount, List<string> studentIds)`
- `void UpdateFeesReceivableStatusBeforeTx(string instId, string feeId, string status, List<string> studentIds)`
- `Student UpdateOpeningBalance(string id, string instId, decimal amount)`
- `Student AddIEP(string studentId, List<IEPTerm> terms)`
- `StudentEssentials AddStudentEssentials(string studentId, string name, DateTime date, decimal amount)`
- `StudentEssentials UpdateItemList(string studentId, string name, DateTime date, decimal amount)`
- `StudentEssentials UpdateItem(string id, int itemId, string name, DateTime date, decimal amount)`
- `StudentEssentials DisableItem(string id, int itemId)`
- `StudentEssentials EnableItem(string id, int itemId)`
- `void SetMedicalDiagnosisReportPath(string id, string documentPath)`
- `void SetDisabilityReportPath(string id, string documentPath)`
- `void SetBirthCertificatePath(string id, string documentPath)`
- `void SetIdentificationProofPath(string id, string documentPath)`
- `void SetAddressProofPath(string id, string documentPath)`
- `void SetTherapyReportPath(string id, string documentPath)`
- `void SetPsycologicalAssessmentReportPath(string id, string documentPath)`
- `void CreateBaselineDataCollectionForm(string name, DateTime dob, string age, DateTime DOOb, string language, string therapistName, GenderEnum gender, BaselinePTOT pTOT, BaselineSpeechLanguage speechLanguage, bool consultedPTOT = false, bool consultedSpeechLanguage = false)`
- `void SetPhotoPath(string id, string documentPath)`

## Query layer (*Qry) — implement all
### StudentQry.cs
- `Dictionary<int, List<string>> GetAbsenteesOn(string instId = "", DateTime? date = null, int periodNumber = 0)`
- `dynamic GetStudentsForExamFacets(string courseId)`
- `List<StudentFlatDto> GetFlatStudentData(List<string> studentIds = null)`
- `dynamic GetAttendanceWidgetData(string instId = "")`
- `dynamic GetStudentCountInstituteGenderwise(List<string> instituteIds = null)`
- `dynamic GetStudentCountInstitutewise(List<string> instituteIds = null)`
- `string get_predicate(List<List<string>> recipients)`
- `List<IEPTerm> GetIEPDetails(string id)`
- `bool StudentEssentialsExists(string studentId)`
- `StudentEssentialsDto GetStudentEssentials(string studentId)`
- `StudentEssentialsDto GetStudentEssentialsDto(StudentEssentials json)`
- `BaselineDataForm GetBaselineDataCollectionForm()`
- `List<BaselineData> GetBaselineDataCollection()`
- `BaselineData GetBaselineDataById(string id)`

## Service layer — implement or fold into *Service
### CsvService.cs
- `static ByteArrayContent GetByteArrayContent(string csv)`
- `static string Escape(string s)`
- `static string Unescape(string s)`
- `void Dispose()`

### EtlService.cs
- `string GetApiScript(string endPoint, object payload, string separator = "\u0009", string verb = "post")`
- `string GetApiScriptJsonl(string endPoint, object payload, string separator = "\u0009", string verb = "post")`
- `string GetTargetId(string base_id, string year)`
- `string GetS3PathStudents(string shortName, string targetYear, string env = "test")`
- `string GetEtlFileName(string shortName, string targetYear, string env = "test")`
- `string GetEtlFileName_NewStudents(string shortName, string targetYear, string courseName, string env = "test")`
- `string GetEtlFileName_DemoStudents(string short_name, string acad_start_year)`
- `string GetEtlFileName_Opening_balance(string shortName, string targetYear, string env = "test")`
- `int[] Shuffle(int n)`

### ReportService.cs
- `string GetMarksCard(PrintOptions options = null, bool testData = false)`
- `string GetMarksCardData(bool testData = false)`
- `string RenderTemp(PrintOptions options = null, bool testData = false)`
- `string RenderTempData(bool testData = false)`
- `string ViewFeeReceipt(PrintOptions options = null, bool testData = false)`
- `string ViewFeeReceiptData(bool testData = false)`
- `string ViewTC(PrintOptions options = null, bool testData = false)`
- `string ViewTCData(bool testData = false)`
- `string Certificate(PrintOptions options = null, bool testData = false)`
- `string CertificateData(bool testData = false)`
- `string GetMarksForAllStudentsInExam(PrintOptions options = null, bool testData = false)`
- `string GetMarksForAllStudentsInExamData(bool testData = false)`
- `string GetStudentTodayTimetable(PrintOptions options = null, bool testData = false)`
- `string GetStudentTodayTimetableData(bool testData = false)`
- `SPD_CSV GetStudentParentDetailsAsCSV(string courseId = "", string section = "")`
- `string CalculateAge(DateTime dob)`

### StudentService.cs
- `StudentDto GetDto()`
- `List<StudentDto> GetDtoWithData()`

## Domain resources (from legacy Resource/Aggregate)
Implement as EF entities. Nested lists/dicts → jsonb unless volume requires tables.
### AttendanceEvent.cs
**Properties:**
- `string Id`
- `string InstId`
- `string CourseId`
- `string TermName`
- `string SectionName`
- `DateTime Date`
- `int PeriodNo`
- `string SubjectName`
- `bool IsOptionalSubject`
- `string StudentId`
- `string StaffId`
- `string Attendance`
- `DateTime CreatedOn`
- `string CreatedBy`

### BaselineData.cs
**Properties:**
- `BaselinePTOT BaselinePTOT`
- `BaselineSpeechLanguage BaselineSpeechLanguage`
- `BaselineSpecialEducation BaselineSpecialEducation`
- `string Name`
- `DateTime DOB`
- `string AgeAsOnDOOb`
- `GenderEnum Gender`
- `string TherapistName`
- `DateTime DateOfObservation`
- `string Language`
- `BaselinePTOT BaselinePTOT`
- `BaselineSpeechLanguage BaselineSpeechLanguage`
- `BaselineSpecialEducation BaselineSpecialEducation`
- `bool ConsultedForPTOT`
- `bool ConsultedForSpeechLanguage`
- `bool ConsultedForSpecialEducation`
- `List<BaselineItem> ActivityFocusTopic`
- `List<BaselineItem> Speech`
- `List<BaselineItem> Condition`
- `List<BaselineItem> EyeContact`
- `List<BaselineItem> SittingTolerance`
- `List<BaselineItem> ActivityCompliance`
- `List<BaselineItem> ReceptiveLanguage`
- `List<BaselineItem> ExpressiveLanguage`
- `List<BaselineItem> SocialCommunication`
- `List<BaselineItem> PragmaticsSkills`
- `List<BaselineItem> Intelligibility`
- `List<BaselineItem> Behavior`
- `List<BaselineItem> Literacy`
- `List<BaselineItem> Recommendation`
- `List<BaselineItem> Condition`
- `List<BaselineItem> Speech`
- `List<BaselineItem> NameCallResponse`
- `List<BaselineItem> ActivityFocusAttention`
- `List<BaselineItem> EyeContact`
- `List<BaselineItem> SittingTolerance`
- `List<BaselineItem> InstructionFollowing`
- `List<BaselineItem> BodyBuilt`
- `string Posture`
- `List<BaselineItem> MuscleTone`
- `string MuscleTightness`
- `string Grasp`
- `string Precision`
- `List<BaselineItem> Proprioception`
- `List<BaselineItem> StaticBalance`
- `List<BaselineItem> DynamicBalance`
- `string Coordination`
- `string Gait`
- `string SensoryProfile`
- `List<BaselineItem> Behavior`
- `List<BaselineItem> Condition`
- `List<BaselineItem> Speech`
- `List<BaselineItem> ActivityFocusAttention`
- `List<BaselineItem> EyeContact`
- `List<BaselineItem> SittingTolerance`
- `List<BaselineItem> InstructionsComprehension`
- `List<BaselineItem> ActivityCompliance`
- `List<BaselineItem> Behavior`
- `List<BaselineItem> BehaviorsHinderingLearning`
- `string Writing`
- `string Literacy`
- `string NumberConcept`
- `string CommunicationConversationalSkill`
- `string Cognition`
- `string EVS`
- `GroupSession GroupSession`
- `BasicLifeSkills BasicLifeSkillsObservations`
- `string AdditionalComments`
- `string Appearance`
- `string GruopSittingBehavior`
- `string BehaviorObserved`
- `string FollowingInstructions`
- `string WaitingForTurn`
- `string ParticipationInActivity`
- `string EatingHabits`
- `string ToiletTraining`
- `string KnowingBelongings`
- `string DressingSkills`

### Course.cs
**Enums:**
- `CourseStatusEnum`: Unknown=0, Active=1, Disabled=99
- `EduLevelEnum`: Unknown=-1, PreNursery=2, Nursery=5, School=10, UnderGraduate=20, Graduate=30, PostGraduate=40
**Properties:**
- `string Name`
- `string Branch`
- `string NameAndBranch`
- `EduLevelEnum EduLevel`
- `string EduLevelAsString`
- `string InstId`
- `string Affiliation`
- `CourseStatusEnum Status`
- `string StatusAsString`
- `List<Term> Terms`
- `List<string> ExamSubjectOrder`
- `int SortIndex`
- `int Rank`
- `string Name`
- `DateTime From`
- `DateTime To`
- `List<Section> Sections`
- `string Name`
- `List<Subject> Subjects`
- `List<StudentRoll> StudentRolls`
- `bool Optional`
- `List<StudentRoll> StudentRolls`
- `string StudentId`
- `string RollNo`

### Email.cs
**Properties:**
- `List<string> Recipients`
- `string Message`
- `string Type`
- `string From`
- `string Subject`
- `List<Attachment> Attachments`

### EventLog.cs
**Properties:**
- `string Event`
- `object TimeStamp`
- `string EventDataType`
- `string EventData`

### IEP.cs
**Properties:**
- `List<IEPTerm> Terms`
- `string Title`
- `DateTime From`
- `DateTime To`
- `int WorkingDays`
- `int DaysPresent`
- `List<Domain> Domains`
- `string InstId`
- `string Title`
- `string Educators`
- `string Performance`
- `string Goals`
- `List<GoalsPerformance> GoalsPerformance`
- `string Evaluations`
- `string Remarks`
- `int Id`
- `string Goal`
- `decimal Performance`

### Student.cs
**Enums:**
- `StudentStatusEnum`: Unknown=-1, Active=1, Disabled=99
- `EnrollmentStatusEnum`: Unknown=-1, Active=1, Promoted=10, TC=98, Disabled=99
- `TxStatusEnum`: Unknown=-1, Active=1, Disabled=99
**Properties:**
- `List<Enrollment> Enrollments`
- `string StudentId`
- `string Attendance`
- `string Category`
- `StudentStatusEnum Status`
- `Person Father`
- `Person Mother`
- `Person Guardian`
- `string AadharNumber`
- `string UDID`
- `Domicile Domicile`
- `string UserId`
- `List<FeesReceivable> FeesReceivable`
- `IndividualEducationPlan IEP`
- `Documents Documents`
- `string PhotoUrl`
- `bool HasValidContact`
- `bool HasValidParentalContact`
- `string EmailCSV`
- `string MobileCSV`
- `string Title`
- `string Particulars`
- `DateTime AsOn`
- `string Caste`
- `string Country`
- `string MotherTongue`
- `string State`
- `string InstId`
- `string CourseId`
- `string CourseName`
- `string TermName`
- `string SectionName`
- `string Branch`
- `string RollNo`
- `int RollNoAsInt`
- `string StudentId`
- `DateTime DOA`
- `string ApplicationNo`
- `string AdmissionNo`
- `List<string> OptionalSubjects`
- `string TCNo`
- `DateTime? TCAppliedOn`
- `DateTime? TCIssuedOn`
- `string Character`
- `string Conduct`
- `string Remarks`
- `Dictionary<string, object> Attributes`
- `EnrollmentStatusEnum Status`
- `string TCNo`
- `DateTime? AppliedOn`
- `DateTime? IssuedOn`
- `string Character`
- `string Conduct`
- `string Remarks`
- `Dictionary<string, object> Attributes`
- `string InstId`
- `string FeeId`
- `string FeeName`
- `string TxId`
- `TxStatusEnum TxStatus`
- `decimal Amount`
- `string InstId`
- `string FeeId`
- `string FeeName`
- `decimal Amount`
- `string DiagnosisReport`
- `string DisabilityCertificate`
- `string BirthCerificate`
- `string IdentificationProof`
- `string AddressProof`
- `string TherapyReport`
- `string PsycologicalAssessmentReport`

### StudentEssentials.cs
**Enums:**
- `ItemStatusEnum`: Active=1, Disable=99, Unknown=0
**Properties:**
- `string StudentId`
- `List<Item> Items`
- `int Id`
- `string Name`
- `DateTime Date`
- `decimal Amount`
- `string InstId`
- `ItemStatusEnum Status`

## EF mapping rules for this module
- Primary key: `Guid Id` (generate on create).
- Always set `OwnerId` from user context on insert.
- Soft status enums rather than physical delete when legacy used Disable.
- Report endpoints: return structured JSON; optional simple HTML wrapper.
- External SMS/email: stub or publish to Connect/Rabbit; do not hard-require gateways.

## Acceptance checklist
- [ ] All **105** HTTP actions present (method names match table)
- [ ] All listed Cmd/Qry methods available on services/facades
- [ ] No Raven/Marten package references
- [ ] Host registration complete
- [ ] OwnerId isolation on every query


---

## Business rules (authoritative)

Derived from legacy Command/Handler method bodies. **Implement these constraints in EF services.**


### Cross-cutting Student rules
- Scope all operations by OwnerId/Inst.
- Enable/Disable toggle status; default lists exclude disabled.
- Contact/identity updates merge into nested person/contact structures (jsonb).
- Reports aggregate by course/section/category using stored fields only.


### Per-method signals from legacy code


### Source: `StudentCmd.cs`

#### `Update(string aadharNo, string id, string admissionNumber, string applicationNumber, DateTime? doa, DateTime DOB, GenderEnum gender, string name)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (id == null)
- if (_student == null)
- if (enrol == null)
- if (!string.IsNullOrEmpty(_student.VirtualId)
**Throws:** nameof(id; nameof(id; nameof(_student; nameof(enrol
**Side effects / messaging:**
- `EnqueStudentUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 if (!_uc.BoundByInst) throw new NotInScopeException(nameof(id)); if (id == null) throw new ArgumentNullException(nameof(id)); var _student = _session.Load<Student>(id); if (_student == null) throw new ArgumentNullException(nameof(_student)); _student.DOB = DOB; _student.Name = name; _student.Gender = gender; _student.AadharNumber = aadharNo; _student.AssignApplicationNo(_uc.InstId, applicationNumber); _student.AssignAdmissionNo(_uc.InstId, admissionNumber); var enrol = _student.Enrollments.Firs
```

#### `Promote(string id, string baseInstId, string baseCourseId, string baseTerm, string baseSection, string targetInstId, string targetCourseId, string targetTerm, string targetSection)`
**Guards / branches (from legacy):**
- if (student == null)
- if (baseCourse == null || targetCourse == null)
- if (!baseCourse.InstId.Equals(baseInstId)
**Throws:** "Student"; "Incorrect base/target course"; "Mismatch base/target course and inst"
**Side effects / messaging:**
- `EnqueStudentUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var student = _session.Load<Student>(id); if (student == null) throw new ArgumentNullException("Student"); var baseInstCourses = RedisCache.GetCourseDto(baseInstId); var baseCourse = baseInstCourses.Find(c => c.Id.Equals(baseCourseId)); var targetInstCourses = RedisCache.GetCourseDto(targetInstId); var targetCourse = targetInstCourses.Find(c => c.Id.Equals(targetCourseId)); if (baseCourse == null || targetCourse == null) throw new ArgumentException("Incorrect base/target course"); if (!baseCour
```

#### `Enable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (string.IsNullOrEmpty(id)
- if (_student == null)
- if (enrol != null)
**Throws:** inst_id
**Side effects / messaging:**
- `EnqueFWStudentStatusUpdatedJob(job_for_fee_worker)`
- `EnqueStudentUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); if (string.IsNullOrEmpty(id)) return; var _student = _session.Load<Student>(id); if (_student == null) return; var enrol = _student.Enrollments.FirstOrDefault(o => o.InstId == inst_id); if (enrol != null) enrol.Enable(); _session.Store(_student); _session.SaveChanges(); var fw_job_id = Guid.NewGuid().ToString(); var job_for_fee_worker = new Job() { id = fw_job_id, title = "student_status_updated", token = _u
```

#### `Disable(string id)`
**Guards / branches (from legacy):**
- if (!_uc.BoundByInst)
- if (string.IsNullOrEmpty(id)
- if (_student == null)
- if (enrol != null)
**Throws:** inst_id
**Side effects / messaging:**
- `EnqueFWStudentStatusUpdatedJob(job_for_fee_worker)`
- `EnqueStudentUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; if (!_uc.BoundByInst) throw new NotInScopeException(inst_id); if (string.IsNullOrEmpty(id)) return; var _student = _session.Load<Student>(id); if (_student == null) return; var enrol = _student.Enrollments.FirstOrDefault(o => o.InstId == inst_id); if (enrol != null) enrol.Disable(); _session.Store(_student); _session.SaveChanges(); var fw_job_id = Guid.NewGuid().ToString(); var job_for_fee_worker = new Job() { id = fw_job_id, title = "student_status_updated", token = _
```

#### `Create(string aadharNo, string admissionNumber, string applicationNumber, string courseName, string courseId, DateTime doa, DateTime? dob, string email, GenderEnum? gender, string mobile, string name, string)`
**Guards / branches (from legacy):**
- if (_uc.BoundByUser)
- if (!string.IsNullOrEmpty(id)
- if (student == null)
- if (_uc.Persist)
**Status assignments:** Status = AddressStatus.Active
**Side effects / messaging:**
- `EnqueStudentCreatedJob(job)`
- `EnqueStudentCreatedFeeWorkerJob(job_for_fee_worker)`
**Legacy body summary (adapt to EF):**
```
 var inst_id = string.IsNullOrEmpty(instId) ? _uc.InstId : instId; //ToDo: validate input if (_uc.BoundByUser) return null; Student student = null; if (!string.IsNullOrEmpty(id)) { student = _session.Load<Student>(id); } if (student == null) student = new Student(Person.Create(name, dob, gender)); // student.Attendance = "Present"; student.SetId(id); student.Enable(); student.SetPrimaryEMail(email); student.SetPrimaryMobile(mobile); var next_stu_id = string.IsNullOrEmpty(studentId) ? GetNextStud
```

#### `DisableFatherMobile(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (mobile == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Disabled
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var mobile = _student.Father.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault(); if (mobile == null) throw new ObjectNotFoundException(); mobile.Status = ContactStatus.Disabled; _student.Father.Mobile = string.Empty; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `DisableMotherMobile(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (mobile == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Disabled
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var mobile = _student.Mother.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault(); if (mobile == null) throw new ObjectNotFoundException(); mobile.Status = ContactStatus.Disabled; _student.Mother.Mobile = string.Empty; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `DisableGuardianMobile(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (mobile == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Disabled
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var mobile = _student.Guardian.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault(); if (mobile == null) throw new ObjectNotFoundException(); mobile.Status = ContactStatus.Disabled; _student.Guardian.Mobile = string.Empty; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `EnableFatherMobile(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (mobile == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Active
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var mobile = _student.Father.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault(); if (mobile == null) throw new ObjectNotFoundException(); mobile.Status = ContactStatus.Active; _student.Father.Mobile = mobile.Info; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `EnableMotherMobile(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (mobile == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Active
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var mobile = _student.Mother.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault(); if (mobile == null) throw new ObjectNotFoundException(); mobile.Status = ContactStatus.Active; _student.Mother.Mobile = mobile.Info; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `EnableGuardianMobile(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (mobile == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Active
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var mobile = _student.Guardian.Contacts.Where(o => o.ContactType == ContactTypeEnum.Mobile).FirstOrDefault(); if (mobile == null) throw new ObjectNotFoundException(); mobile.Status = ContactStatus.Active; _student.Guardian.Mobile = mobile.Info; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `DisableFatherEmail(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (email == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Disabled
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var email = _student.Father.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault(); if (email == null) throw new ObjectNotFoundException(); email.Status = ContactStatus.Disabled; _student.Father.Email = string.Empty; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `DisableMotherEmail(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (email == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Disabled
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var email = _student.Mother.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault(); if (email == null) throw new ObjectNotFoundException(); email.Status = ContactStatus.Disabled; _student.Mother.Email = string.Empty; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `DisableGuardianEmail(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (email == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Disabled
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var email = _student.Guardian.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault(); if (email == null) throw new ObjectNotFoundException(); email.Status = ContactStatus.Disabled; _student.Guardian.Email = string.Empty; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `EnableFatherEmail(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (email == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Active
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var email = _student.Father.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault(); if (email == null) throw new ObjectNotFoundException(); email.Status = ContactStatus.Active; _student.Father.Email = email.Info; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `EnableMotherEmail(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (email == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Active
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var email = _student.Mother.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault(); if (email == null) throw new ObjectNotFoundException(); email.Status = ContactStatus.Active; _student.Mother.Email = email.Info; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `EnableGuardianEmail(string id)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (email == null)
**Throws:** ; 
**Status assignments:** Status = ContactStatus.Active
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var email = _student.Guardian.Contacts.Where(o => o.ContactType == ContactTypeEnum.Email).FirstOrDefault(); if (email == null) throw new ObjectNotFoundException(); email.Status = ContactStatus.Active; _student.Guardian.Email = email.Info; _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `UpdateFeesReceivableStatus(string id, string txId, string instId, string feeId, string feeName, decimal amount)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (feesReceivable == null)
**Throws:** ; 
**Status assignments:** Status = TxStatusEnum.Disabled, Status = TxStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var feesReceivable = _student.FeesReceivable.Where(o => o.TxId == txId).FirstOrDefault(); if (feesReceivable == null) throw new ObjectNotFoundException(); feesReceivable.TxStatus = TxStatusEnum.Disabled; var dto = new FeesReceivable() { InstId = instId, FeeId = feeId, FeeName = feeName, TxId = string.Empty, Amount = amount, TxStatus = TxStatusEnum.Active }; _student.FeesReceivable.Add(dto); _se
```

#### `UpdateItem(string id, int itemId, string name, DateTime date, decimal amount)`
**Guards / branches (from legacy):**
- if (essentials == null)
- if (item == null)
**Throws:** ; 
**Legacy body summary (adapt to EF):**
```
 var inst_id = _uc.InstId; var essentials = _session.Load<StudentEssentials>(id); if (essentials == null) throw new ObjectNotFoundException(); var item = essentials.Items.Where(o => o.Id.Equals(itemId)).FirstOrDefault(); if (item == null) throw new ArgumentNullException(); item.Name = name; item.Date = date; item.Amount = amount; _session.Store(essentials); _session.SaveChanges(); return essentials; 
```

#### `DisableItem(string id, int itemId)`
**Guards / branches (from legacy):**
- if (essentials == null)
- if (item == null)
**Throws:** ; 
**Status assignments:** Status = ItemStatusEnum.Disable
**Legacy body summary (adapt to EF):**
```
 var essentials = _session.Load<StudentEssentials>(id); if (essentials == null) throw new ObjectNotFoundException(); var item = essentials.Items.Where(o => o.Id.Equals(itemId)).FirstOrDefault(); if (item == null) throw new ArgumentNullException(); item.Status = ItemStatusEnum.Disable; _session.Store(essentials); _session.SaveChanges(); return essentials; 
```

#### `EnableItem(string id, int itemId)`
**Guards / branches (from legacy):**
- if (essentials == null)
- if (item == null)
**Throws:** ; 
**Status assignments:** Status = ItemStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var essentials = _session.Load<StudentEssentials>(id); if (essentials == null) throw new ObjectNotFoundException(); var item = essentials.Items.Where(o => o.Id.Equals(itemId)).FirstOrDefault(); if (item == null) throw new ArgumentNullException(); item.Status = ItemStatusEnum.Active; _session.Store(essentials); _session.SaveChanges(); return essentials; 
```

#### `UpdateFeesReceivable(string id, string instId, string feeId, string feeName, string txId, decimal amount)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (feesReceivable != null)
- if (feesReceivable == null)
**Throws:** 
**Status assignments:** Status = TxStatusEnum.Active
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var feesReceivable = _student.FeesReceivable.Where(o => o.TxId == txId).FirstOrDefault(); if (feesReceivable != null) return _student; if (feesReceivable == null) { var dto = new FeesReceivable() { InstId = instId, FeeId = feeId, FeeName = feeName, TxId = txId, Amount = amount, TxStatus = TxStatusEnum.Active }; _student.FeesReceivable.Add(dto); } _session.Store(_student); _session.SaveChanges()
```

#### `UpdateFatherDetails(string id, string email, string mobile, string name, string pan)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (_student.Father == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); if (_student.Father == null) { var father = new Person { Name = name, Gender = GenderEnum.Male }; father.SetId(); _student.Father = father; _session.Store(father); } else { _student.Father.ResetName(name); _student.Father.Gender = GenderEnum.Male; } _student.Father.SetPrimaryEMail(email); _student.Father.SetPrimaryMobile(mobile); _student.Father.PAN = pan; _session.Store(_student); _session.Sav
```

#### `UpdateMotherDetails(string id, string email, string mobile, string name, string pan)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (_student.Mother == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); if (_student.Mother == null) { var mother = new Person { Name = name, Gender = GenderEnum.Female }; mother.SetId(); _student.Mother = mother; _session.Store(mother); } else { _student.Mother.ResetName(name); _student.Mother.Gender = GenderEnum.Female; } _student.Mother.SetPrimaryEMail(email); _student.Mother.SetPrimaryMobile(mobile); _student.Mother.PAN = pan; _session.Store(_student); _session
```

#### `UpdateFatherEmail(string id, string email)`
**Guards / branches (from legacy):**
- if (_student == null || _student.Father == null)
- if (!string.IsNullOrEmpty(email)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null || _student.Father == null) throw new ObjectNotFoundException(); if (!string.IsNullOrEmpty(email)) _student.Father.SetPrimaryEMail(email); _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `UpdateMotherMobile(string id, string mobile)`
**Guards / branches (from legacy):**
- if (_student == null || _student.Mother == null)
- if (!string.IsNullOrEmpty(mobile)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null || _student.Mother == null) throw new ObjectNotFoundException(); if (!string.IsNullOrEmpty(mobile)) _student.Mother.SetPrimaryMobile(mobile); _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `UpdateFatherMobile(string id, string mobile)`
**Guards / branches (from legacy):**
- if (_student == null || _student.Father == null)
- if (!string.IsNullOrEmpty(mobile)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null || _student.Father == null) throw new ObjectNotFoundException(); if (!string.IsNullOrEmpty(mobile)) _student.Father.SetPrimaryMobile(mobile); _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `UpdateMotherEmail(string id, string email)`
**Guards / branches (from legacy):**
- if (_student == null || _student.Mother == null)
- if (!string.IsNullOrEmpty(email)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null || _student.Mother == null) throw new ObjectNotFoundException(); if (!string.IsNullOrEmpty(email)) _student.Mother.SetPrimaryEMail(email); _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `UpdateGuardianDetails(string id, string email, GenderEnum gender, string mobile, string name)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (_student.Guardian == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); if (_student.Guardian == null) { var guardian = new Person { Name = name, Gender = gender }; guardian.SetId(); _student.Guardian = guardian; _session.Store(guardian); } else { _student.Guardian.ResetName(name); _student.Guardian.Gender = gender; } _student.Guardian.SetPrimaryEMail(email); _student.Guardian.SetPrimaryMobile(mobile); _session.Store(_student); _session.SaveChanges(); return _stude
```

#### `UpdateGuardianMobile(string id, string mobile)`
**Guards / branches (from legacy):**
- if (_student == null || _student.Guardian == null)
- if (!string.IsNullOrEmpty(mobile)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null || _student.Guardian == null) throw new ObjectNotFoundException(); if (!string.IsNullOrEmpty(mobile)) _student.Guardian.SetPrimaryMobile(mobile); _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `UpdateGuardianEmail(string id, string email)`
**Guards / branches (from legacy):**
- if (_student == null || _student.Guardian == null)
- if (!string.IsNullOrEmpty(email)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null || _student.Guardian == null) throw new ObjectNotFoundException(); if (!string.IsNullOrEmpty(email)) _student.Guardian.SetPrimaryEMail(email); _session.Store(_student); _session.SaveChanges(); return _student; 
```

#### `UpdateCourse(string id, string courseName, string courseId, string secName, string branch = "")`
**Guards / branches (from legacy):**
- if (_student.Enrollments[i].InstId == _uc.InstId)
- if (!string.IsNullOrEmpty(_student.VirtualId)
**Side effects / messaging:**
- `EnqueStudentUpdatedJob(job)`
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); for (var i = 0; i < _student.Enrollments.Count; i++) { if (_student.Enrollments[i].InstId == _uc.InstId) { _student.Enrollments[i].CourseName = courseName; _student.Enrollments[i].SectionName = secName; _student.Enrollments[i].CourseId = courseId; _student.Enrollments[i].Branch = branch; } } _session.Store(_student); _session.SaveChanges(); //since the student course may have changed, delete the student profile. if (!string.IsNullOrEmpty(_student.Virtu
```

#### `GetAbsenteeIds(string instId = "", DateTime? date = null, int periodNumber = 0)`
**Guards / branches (from legacy):**
- if (instId == "")
- if (att == null)
- if (periodNumber != 0 && att.PeriodNo != periodNumber)
- if (sa.Att.Equals("absent", StringComparison.CurrentCultureIgnoreCase)
**Legacy body summary (adapt to EF):**
```
 if (instId == "") instId = _uc.InstId; var _dt = date.HasValue ? date.Value.Date : DateTime.Now.Date; var qry = _session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>() .Where(o => o.InstId.Equals(instId)) .Where(o => o.Date == _dt) .OfType<AttendanceEvent>(); var att_events = new List<AttendanceEvent>(); using (var enumerator = _session.Advanced.Stream(qry)) { while (enumerator.MoveNext()) { var att = enumerator.Current.Document; if (att == null) continue; if (periodNumber != 0 && att.P
```

#### `GetAbsenteeMobiles(string instId = "", DateTime? date = null, int periodNumber = 0)`
**Guards / branches (from legacy):**
- if (instId == "")
- if (att == null)
- if (periodNumber != 0 && att.PeriodNo != periodNumber)
- if (sa.Att.Equals("absent", StringComparison.CurrentCultureIgnoreCase)
**Legacy body summary (adapt to EF):**
```
 if (instId == "") instId = _uc.InstId; var _dt = date.HasValue ? date.Value.Date : DateTime.Now.Date; var qry = _session.Query<AttendanceEvent_IX.Result, AttendanceEvent_IX>() .Where(o => o.InstId.Equals(instId)) .Where(o => o.Date == _dt) .OfType<AttendanceEvent>(); var att_events = new List<AttendanceEvent>(); using (var enumerator = _session.Advanced.Stream(qry)) { while (enumerator.MoveNext()) { var att = enumerator.Current.Document; if (att == null) continue; if (periodNumber != 0 && att.P
```

#### `UpdateOpeningBalance(string id, string instId, decimal amount)`
**Guards / branches (from legacy):**
- if (_student == null)
- if (feesReceivable == null)
**Throws:** 
**Legacy body summary (adapt to EF):**
```
 var _student = _session.Load<Student>(id); if (_student == null) throw new ObjectNotFoundException(); var feesReceivable = _student.FeesReceivable.Where(o => o.InstId == instId).FirstOrDefault(); if (feesReceivable == null) { var dto = new OpeningBalance() { InstId = instId, Amount = amount }; // _student.FeesReceivable.Add(dto); } else feesReceivable.Amount = amount; _session.Store(_student); _session.SaveChanges(); return _student; 
```


### Agent implementation notes
- Map Raven `IDocumentSession` → `DbContext`; `Store`/`SaveChanges` → `SaveChangesAsync`.
- Map `eventStore.Append` / stream saves → single EF entity update (current state).
- Map `throw new ...Exception` → ASP.NET 400/404 with message.
- Preserve status enum numeric meanings when writing conversions.
