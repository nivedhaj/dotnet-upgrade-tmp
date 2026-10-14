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
