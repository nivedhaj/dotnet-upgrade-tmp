using Ct.Acad.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/calendar")]
[Route("api/Calendar")]
public sealed class CalendarController(CalendarService calendar, CalendarExtrasService extras) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await calendar.GetAsync(from, to, ct));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CalendarCreateRequest request, CancellationToken ct)
        => Ok(await calendar.CreateAsync(request.Title, request.Date, request.Type, request.IsHoliday, request.Description, ct));

    [HttpGet("holidays")]
    public async Task<IActionResult> Holidays(CancellationToken ct) => Ok(await calendar.HolidaysAsync(ct));

    [HttpGet("upcoming")]
    public async Task<IActionResult> Upcoming([FromQuery] int days = 30, CancellationToken ct = default)
        => Ok(await calendar.UpcomingAsync(days, ct));

    [HttpPost("getCalendarEvents")]
    public async Task<IActionResult> GetCalendarEvents([FromBody] CalendarRangeRequest? request, CancellationToken ct)
        => Ok(await extras.GetCalendarEventsAsync(request?.From, request?.To, ct));

    [HttpPost("generateCalendar")]
    public async Task<IActionResult> GenerateCalendar([FromBody] CalendarGenerateRequest request, CancellationToken ct)
        => Ok(await extras.GenerateCalendarAsync(request.From, request.To, request.WeeklyHolidays, ct));

    [HttpPost("removeCalendarRules")]
    public async Task<IActionResult> RemoveCalendarRules(CancellationToken ct)
        => Ok(await extras.RemoveCalendarRulesAsync(ct));

    [HttpPost("removeOccurrence")]
    public async Task<IActionResult> RemoveOccurrence([FromBody] IdRequest request, CancellationToken ct)
        => await extras.RemoveOccurrenceAsync(request.Id, ct) ? Ok() : NotFound();
}

public sealed class CalendarCreateRequest
{
    public required string Title { get; set; }
    public required DateTime Date { get; set; }
    public string? Type { get; set; }
    public bool IsHoliday { get; set; }
    public string? Description { get; set; }
}
public sealed class CalendarRangeRequest { public DateTime? From { get; set; } public DateTime? To { get; set; } }
public sealed class CalendarGenerateRequest
{
    public required DateTime From { get; set; }
    public required DateTime To { get; set; }
    public List<DayOfWeek>? WeeklyHolidays { get; set; }
}
