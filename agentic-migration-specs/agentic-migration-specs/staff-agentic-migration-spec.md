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
