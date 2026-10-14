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
