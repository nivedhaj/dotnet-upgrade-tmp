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
