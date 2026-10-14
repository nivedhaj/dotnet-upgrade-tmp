using Ct.Scheduler.Domain;
using Ct.Scheduler.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Scheduler.Application;

public sealed class CalendarService(SchedulerDbContext db, ISchedulerUserContext user)
{
    public async Task<object> GetAsync(CancellationToken ct = default)
    {
        var rules = await db.Rules.AsNoTracking()
            .Where(r => r.OwnerId == user.OwnerId && r.Status == CalendarRuleStatus.Active)
            .ToListAsync(ct);
        var events = await db.Events.AsNoTracking()
            .Where(e => e.OwnerId == user.OwnerId)
            .OrderByDescending(e => e.CreatedOn)
            .Take(500)
            .ToListAsync(ct);
        return new
        {
            rules = rules.Select(MapRule),
            events = events.Select(MapEvent)
        };
    }

    public async Task<object> AddCalendarRuleAsync(AddRuleRequest req, CancellationToken ct = default)
    {
        var rule = new CalendarRule
        {
            OwnerId = user.OwnerId,
            Title = req.Title.Trim(),
            CronExpression = req.CronExpression,
            Category = ParseCategory(req.Category),
            Weight = req.Weight,
            Duration = req.Duration,
            TopicId = req.TopicId,
            UserId = req.UserId ?? user.UserId,
            CreateMeetingLink = req.CreateMeetingLink,
            WeeklyDays = req.WeeklyDays,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        db.Rules.Add(rule);
        await db.SaveChangesAsync(ct);
        return MapRule(rule);
    }

    public async Task<object> AddCalendarAsync(AddEventRequest req, CancellationToken ct = default)
    {
        var ev = new CalendarEvent
        {
            OwnerId = user.OwnerId,
            InstId = req.InstId ?? user.InstId,
            EventName = req.EventName.Trim(),
            Category = ParseCategory(req.Category),
            Priority = req.Priority,
            Audience = req.Audience ?? [],
            ConductedBy = req.ConductedBy ?? [],
            Occurrences = (req.Dates ?? [req.Date ?? DateTime.UtcNow.Date])
                .Select(d => new EventOccurrence { EventDate = d.Date, Duration = req.Duration, Status = EventOccurrenceStatus.Active })
                .ToList(),
            RuleId = req.RuleId,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        db.Events.Add(ev);
        await db.SaveChangesAsync(ct);
        return MapEvent(ev);
    }

    public async Task<bool> RemoveCalendarRuleAsync(Guid id, CancellationToken ct = default)
    {
        var rule = await db.Rules.FirstOrDefaultAsync(r => r.Id == id && r.OwnerId == user.OwnerId, ct);
        if (rule is null) return false;
        rule.Status = CalendarRuleStatus.Disabled;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<object> GenerateCalendarAsync(GenerateCalendarRequest req, CancellationToken ct = default)
    {
        var from = req.From.Date;
        var to = req.To.Date;
        var weekly = req.WeeklyHolidays ?? [DayOfWeek.Sunday];
        var created = 0;

        // Weekly holidays from rules or request
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            if (weekly.Contains(d.DayOfWeek))
            {
                db.Events.Add(new CalendarEvent
                {
                    OwnerId = user.OwnerId,
                    InstId = req.InstId ?? user.InstId,
                    EventName = $"Weekly holiday ({d.DayOfWeek})",
                    Category = CalendarEventCategory.WeeklyHoliday,
                    Occurrences = [new EventOccurrence { EventDate = d, Status = EventOccurrenceStatus.Active }],
                    CreatedBy = user.UserId,
                    CreatedOn = DateTime.UtcNow
                });
                created++;
            }
        }

        // Expand active weekly rules by day-of-week
        var rules = await db.Rules.AsNoTracking()
            .Where(r => r.OwnerId == user.OwnerId && r.Status == CalendarRuleStatus.Active)
            .ToListAsync(ct);
        foreach (var rule in rules)
        {
            if (rule.WeeklyDays is { Count: > 0 })
            {
                for (var d = from; d <= to; d = d.AddDays(1))
                {
                    if (rule.WeeklyDays.Contains(d.DayOfWeek))
                    {
                        db.Events.Add(new CalendarEvent
                        {
                            OwnerId = user.OwnerId,
                            InstId = req.InstId ?? user.InstId,
                            EventName = rule.Title,
                            Category = rule.Category,
                            RuleId = rule.Id,
                            Occurrences =
                            [
                                new EventOccurrence
                                {
                                    EventDate = d,
                                    Duration = (int)rule.Duration,
                                    Status = EventOccurrenceStatus.Active
                                }
                            ],
                            CreatedBy = user.UserId,
                            CreatedOn = DateTime.UtcNow
                        });
                        created++;
                    }
                }
            }
        }

        await db.SaveChangesAsync(ct);
        return new { created, from, to };
    }

    public async Task<bool> RemoveOccurrenceAsync(Guid eventId, DateTime date, CancellationToken ct = default)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.Id == eventId && e.OwnerId == user.OwnerId, ct);
        if (ev is null) return false;
        foreach (var o in ev.Occurrences.Where(o => o.EventDate.Date == date.Date))
            o.Status = EventOccurrenceStatus.Deleted;
        // If all deleted, soft-remove event list
        ev.Occurrences = ev.Occurrences.ToList();
        ev.ModifiedBy = user.UserId;
        ev.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<object>> GetCalendarEventsAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var events = await db.Events.AsNoTracking().Where(e => e.OwnerId == user.OwnerId).ToListAsync(ct);
        var rows = new List<object>();
        foreach (var e in events)
        {
            foreach (var o in e.Occurrences.Where(o => o.Status == EventOccurrenceStatus.Active))
            {
                if (from is not null && o.EventDate.Date < from.Value.Date) continue;
                if (to is not null && o.EventDate.Date > to.Value.Date) continue;
                rows.Add(new
                {
                    eventId = e.Id,
                    e.EventName,
                    Category = e.Category.ToString(),
                    o.EventDate,
                    o.Duration,
                    e.Priority,
                    e.Audience
                });
            }
        }
        return rows.OrderBy(r => ((dynamic)r).EventDate).ToList();
    }

    public async Task<IReadOnlyList<object>> GetUpcomingEventsAsync(int days = 30, CancellationToken ct = default)
    {
        var until = DateTime.UtcNow.Date.AddDays(days);
        return await GetCalendarEventsAsync(DateTime.UtcNow.Date, until, ct);
    }

    public async Task<object> GetUpcomingEventsWidgetAsync(CancellationToken ct = default)
    {
        var upcoming = await GetUpcomingEventsAsync(14, ct);
        return new { count = upcoming.Count, events = upcoming.Take(10) };
    }

    public async Task<IReadOnlyList<DateTime>> GetHolidayDatesAsync(CancellationToken ct = default)
    {
        var events = await db.Events.AsNoTracking()
            .Where(e => e.OwnerId == user.OwnerId &&
                        (e.Category == CalendarEventCategory.Holiday || e.Category == CalendarEventCategory.WeeklyHoliday))
            .ToListAsync(ct);
        return events.SelectMany(e => e.Occurrences)
            .Where(o => o.Status == EventOccurrenceStatus.Active)
            .Select(o => o.EventDate.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();
    }

    private static CalendarEventCategory ParseCategory(string? c)
        => Enum.TryParse<CalendarEventCategory>(c, true, out var v) ? v : CalendarEventCategory.Event;

    private static object MapRule(CalendarRule r) => new
    {
        r.Id, r.Title, r.CronExpression, Status = r.Status.ToString(), Category = r.Category.ToString(),
        r.Weight, r.Duration, r.WeeklyDays, r.CreateMeetingLink, r.CreatedOn
    };

    private static object MapEvent(CalendarEvent e) => new
    {
        e.Id, e.InstId, e.EventName, Category = e.Category.ToString(), e.Priority,
        e.Audience, e.ConductedBy,
        Occurrences = e.Occurrences.Where(o => o.Status == EventOccurrenceStatus.Active)
            .Select(o => new { o.EventDate, o.Duration }),
        e.RuleId, e.CreatedOn
    };
}

public sealed class AddRuleRequest
{
    public required string Title { get; set; }
    public string? CronExpression { get; set; }
    public string? Category { get; set; }
    public int Weight { get; set; }
    public decimal Duration { get; set; }
    public string? TopicId { get; set; }
    public string? UserId { get; set; }
    public bool CreateMeetingLink { get; set; }
    public List<DayOfWeek>? WeeklyDays { get; set; }
}

public sealed class AddEventRequest
{
    public required string EventName { get; set; }
    public string? Category { get; set; }
    public string? InstId { get; set; }
    public int Priority { get; set; }
    public List<string>? Audience { get; set; }
    public List<string>? ConductedBy { get; set; }
    public DateTime? Date { get; set; }
    public List<DateTime>? Dates { get; set; }
    public int Duration { get; set; }
    public Guid? RuleId { get; set; }
}

public sealed class GenerateCalendarRequest
{
    public required DateTime From { get; set; }
    public required DateTime To { get; set; }
    public string? InstId { get; set; }
    public List<DayOfWeek>? WeeklyHolidays { get; set; }
}

public sealed class SchedulerService(CalendarService calendar)
{
    /// <summary>Library due date = start + N working days skipping holidays.</summary>
    public async Task<object> GetDueDateAsync(DateTime? start, int workingDays = 14, CancellationToken ct = default)
    {
        var from = (start ?? DateTime.UtcNow).Date;
        var holidays = (await calendar.GetHolidayDatesAsync(ct)).ToHashSet();
        var remaining = workingDays <= 0 ? 14 : workingDays;
        var d = from;
        while (remaining > 0)
        {
            d = d.AddDays(1);
            if (d.DayOfWeek is DayOfWeek.Sunday) continue;
            if (holidays.Contains(d)) continue;
            remaining--;
        }
        return new { start = from, dueDate = d, workingDays };
    }

    public Task<IReadOnlyList<DateTime>> GetHolidaysAsync(CancellationToken ct = default)
        => calendar.GetHolidayDatesAsync(ct);

    public async Task<object> GetNextWorkingDaysAsync(DateTime? start, int count = 5, CancellationToken ct = default)
    {
        var from = (start ?? DateTime.UtcNow).Date;
        var holidays = (await calendar.GetHolidayDatesAsync(ct)).ToHashSet();
        var days = new List<DateTime>();
        var d = from;
        while (days.Count < Math.Clamp(count, 1, 60))
        {
            d = d.AddDays(1);
            if (d.DayOfWeek is DayOfWeek.Sunday) continue;
            if (holidays.Contains(d)) continue;
            days.Add(d);
        }
        return new { start = from, workingDays = days };
    }
}

public sealed class SchedulerReportService(CalendarService calendar)
{
    public Task<object> CalendarReportAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
        => calendar.GetCalendarEventsAsync(from, to, ct).ContinueWith(t => (object)t.Result, ct);

    public async Task<object> HolidayListAsync(CancellationToken ct = default)
    {
        var dates = await calendar.GetHolidayDatesAsync(ct);
        return new { count = dates.Count, holidays = dates };
    }

    public async Task<object> UpcomingEventsReportAsync(int days = 30, CancellationToken ct = default)
        => await calendar.GetUpcomingEventsAsync(days, ct);
}
