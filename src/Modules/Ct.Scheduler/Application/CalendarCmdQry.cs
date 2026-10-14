using Ct.Scheduler.Domain;

namespace Ct.Scheduler.Application;

/// <summary>Facade matching original CalendarCmd surface.</summary>
public sealed class CalendarCmd(CalendarService calendar)
{
    public Task<object> AddCalendarRule(string title, string? cronExpression, string? eventCategory, string? id = null, CancellationToken ct = default)
        => calendar.AddCalendarRuleAsync(new AddRuleRequest
        {
            Title = title,
            CronExpression = cronExpression,
            Category = eventCategory
        }, ct);

    public Task<object> AddCalendar(
        string title,
        string? cronExpression,
        string? eventCategory,
        decimal duration,
        string? topicId,
        string? userId = null,
        CancellationToken ct = default)
        => calendar.AddCalendarAsync(new AddEventRequest
        {
            EventName = title,
            Category = eventCategory,
            Duration = (int)duration,
            RuleId = Guid.TryParse(topicId, out var rid) ? rid : null
        }, ct);

    public Task<bool> RemoveCalendarRule(Guid id, CancellationToken ct = default)
        => calendar.RemoveCalendarRuleAsync(id, ct);

    public Task<bool> RemoveOccurrence(Guid eventId, DateTime date, CancellationToken ct = default)
        => calendar.RemoveOccurrenceAsync(eventId, date, ct);

    public Task<object> GenerateCalendar(DateTime from, DateTime to, string? instId = null, List<DayOfWeek>? weeklyHolidays = null, CancellationToken ct = default)
        => calendar.GenerateCalendarAsync(new GenerateCalendarRequest
        {
            From = from,
            To = to,
            InstId = instId,
            WeeklyHolidays = weeklyHolidays
        }, ct);
}

/// <summary>Facade matching original CalendarQry surface.</summary>
public sealed class CalendarQry(CalendarService calendar)
{
    public Task<object> Get(CancellationToken ct = default) => calendar.GetAsync(ct);

    public Task<IReadOnlyList<DateTime>> GetHolidayDates(CancellationToken ct = default)
        => calendar.GetHolidayDatesAsync(ct);

    public Task<IReadOnlyList<object>> GetCalendarEvents(DateTime? from = null, DateTime? to = null, CancellationToken ct = default)
        => calendar.GetCalendarEventsAsync(from, to, ct);

    public Task<IReadOnlyList<object>> GetUpcomingEvents(int days = 30, CancellationToken ct = default)
        => calendar.GetUpcomingEventsAsync(days, ct);

    public Task<object> UpcomingEventsWidgetData(CancellationToken ct = default)
        => calendar.GetUpcomingEventsWidgetAsync(ct);
}

/// <summary>Report models + simple HTML (replaces Github StringTemplate).</summary>
public sealed class SchedulerReportCmd(CalendarService calendar, SchedulerReportService reports)
{
    public async Task<object> GetCalendarModel(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var events = await calendar.GetCalendarEventsAsync(from, to, ct);
        return new { title = "Calendar", from, to, events };
    }

    public async Task<object> GetCalendar(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var model = await GetCalendarModel(from, to, ct);
        var events = await calendar.GetCalendarEventsAsync(from, to, ct);
        var rows = string.Join("", events.Select(e =>
        {
            dynamic d = e;
            return $"<tr><td>{d.EventDate:yyyy-MM-dd}</td><td>{System.Net.WebUtility.HtmlEncode((string)d.EventName)}</td><td>{d.Category}</td></tr>";
        }));
        var html = $"<html><body><h2>Calendar</h2><table border='1' cellpadding='4'><tr><th>Date</th><th>Event</th><th>Category</th></tr>{rows}</table></body></html>";
        return new { html, model };
    }

    public async Task<object> GetHolidayListModel(CancellationToken ct = default)
    {
        var dates = await calendar.GetHolidayDatesAsync(ct);
        return new { title = "Holiday List", holidays = dates };
    }

    public async Task<object> GetHolidayList(CancellationToken ct = default)
    {
        var dates = await calendar.GetHolidayDatesAsync(ct);
        var rows = string.Join("", dates.Select(d => $"<tr><td>{d:yyyy-MM-dd}</td><td>{d:dddd}</td></tr>"));
        var html = $"<html><body><h2>Holiday List</h2><table border='1' cellpadding='4'><tr><th>Date</th><th>Day</th></tr>{rows}</table></body></html>";
        return new { html, holidays = dates };
    }

    public async Task<object> GetUpcomingEventsModel(int days = 30, CancellationToken ct = default)
    {
        var events = await calendar.GetUpcomingEventsAsync(days, ct);
        return new { title = "Upcoming Events", days, events };
    }

    public async Task<object> GetUpcomingEventsHtml(int days = 30, CancellationToken ct = default)
    {
        var events = await calendar.GetUpcomingEventsAsync(days, ct);
        var rows = string.Join("", events.Select(e =>
        {
            dynamic d = e;
            return $"<tr><td>{d.EventDate:yyyy-MM-dd}</td><td>{System.Net.WebUtility.HtmlEncode((string)d.EventName)}</td></tr>";
        }));
        var html = $"<html><body><h2>Upcoming Events</h2><table border='1' cellpadding='4'><tr><th>Date</th><th>Event</th></tr>{rows}</table></body></html>";
        return new { html, events };
    }
}
