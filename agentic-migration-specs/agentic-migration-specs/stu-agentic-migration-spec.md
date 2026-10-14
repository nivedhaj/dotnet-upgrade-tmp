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
