using Ct.Scheduler.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Scheduler.Controllers;

[ApiController]
[Route("api/scheduler/calendar")]
[Route("api/Calendar")]
public sealed class CalendarController(CalendarService calendar, CalendarQry qry) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await calendar.GetAsync(ct));

    [HttpPost("addCalendarRule")]
    public async Task<IActionResult> AddCalendarRule([FromBody] AddRuleRequest request, CancellationToken ct)
        => Ok(await calendar.AddCalendarRuleAsync(request, ct));

    [HttpPost("addCalendar")]
    public async Task<IActionResult> AddCalendar([FromBody] AddEventRequest request, CancellationToken ct)
        => Ok(await calendar.AddCalendarAsync(request, ct));

    [HttpPost("removeCalendarRule")]
    public async Task<IActionResult> RemoveCalendarRule([FromBody] SchedIdBody body, CancellationToken ct)
        => await calendar.RemoveCalendarRuleAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("generateCalendar")]
    public async Task<IActionResult> GenerateCalendar([FromBody] GenerateCalendarRequest request, CancellationToken ct)
        => Ok(await calendar.GenerateCalendarAsync(request, ct));

    [HttpPost("removeOccurrence")]
    public async Task<IActionResult> RemoveOccurrence([FromBody] RemoveOccurrenceBody body, CancellationToken ct)
        => await calendar.RemoveOccurrenceAsync(body.EventId, body.Date, ct) ? Ok() : NotFound();

    [HttpGet("getCalendarEvents")]
    public async Task<IActionResult> GetCalendarEvents([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await calendar.GetCalendarEventsAsync(from, to, ct));

    [HttpGet("upcomingEvents")]
    public async Task<IActionResult> GetUpcomingEvents([FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await calendar.GetUpcomingEventsAsync(days, ct));

    [HttpGet("widget/upcomingEvents")]
    public async Task<IActionResult> GetUpcomingEventsData(CancellationToken ct)
        => Ok(await calendar.GetUpcomingEventsWidgetAsync(ct));

    [HttpGet("holidayDates")]
    public async Task<IActionResult> GetHolidayDates(CancellationToken ct)
        => Ok(await qry.GetHolidayDates(ct));

    /// <summary>Alias matching original CalendarQry.UpcomingEventsWidgetData</summary>
    [HttpGet("widget/upcomingEventsData")]
    public async Task<IActionResult> UpcomingEventsWidgetData(CancellationToken ct)
        => Ok(await qry.UpcomingEventsWidgetData(ct));
}

public sealed class SchedIdBody { public required Guid Id { get; set; } }
public sealed class RemoveOccurrenceBody { public required Guid EventId { get; set; } public required DateTime Date { get; set; } }
