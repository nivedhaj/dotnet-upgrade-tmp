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
