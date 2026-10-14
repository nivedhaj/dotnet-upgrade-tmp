using Ct.Scheduler.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Scheduler.Controllers;

[ApiController]
[Route("api/scheduler/report")]
[Route("api/Report")]
public sealed class ReportController(SchedulerReportService reports, SchedulerReportCmd reportCmd) : ControllerBase
{
    [HttpPost("calendar")]
    public async Task<IActionResult> GetCalendarReport([FromBody] DateRangeBody? body, CancellationToken ct)
        => Ok(await reports.CalendarReportAsync(body?.From, body?.To, ct));

    [HttpPost("holidayList")]
    public async Task<IActionResult> GetHolidayListReport(CancellationToken ct)
        => Ok(await reports.HolidayListAsync(ct));

    [HttpPost("upcomingEvents")]
    public async Task<IActionResult> GetUpcomingEventsReport([FromBody] DaysBody? body, CancellationToken ct)
        => Ok(await reports.UpcomingEventsReportAsync(body?.Days ?? 30, ct));

    [HttpPost("calendar/model")]
    [HttpGet("calendar/model")]
    public async Task<IActionResult> GetCalendarModel([FromBody] DateRangeBody? body, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
        => Ok(await reportCmd.GetCalendarModel(body?.From ?? from, body?.To ?? to, ct));

    [HttpPost("calendar/html")]
    public async Task<IActionResult> GetCalendarHtml([FromBody] DateRangeBody? body, CancellationToken ct)
        => Ok(await reportCmd.GetCalendar(body?.From, body?.To, ct));

    [HttpPost("holidayList/model")]
    [HttpGet("holidayList/model")]
    public async Task<IActionResult> GetHolidayListModel(CancellationToken ct)
        => Ok(await reportCmd.GetHolidayListModel(ct));

    [HttpPost("holidayList/html")]
    public async Task<IActionResult> GetHolidayListHtml(CancellationToken ct)
        => Ok(await reportCmd.GetHolidayList(ct));

    [HttpPost("upcomingEvents/model")]
    public async Task<IActionResult> GetUpcomingEventsModel([FromBody] DaysBody? body, CancellationToken ct)
        => Ok(await reportCmd.GetUpcomingEventsModel(body?.Days ?? 30, ct));

    [HttpPost("upcomingEvents/html")]
    public async Task<IActionResult> GetUpcomingEventsHtml([FromBody] DaysBody? body, CancellationToken ct)
        => Ok(await reportCmd.GetUpcomingEventsHtml(body?.Days ?? 30, ct));
}

public sealed class DateRangeBody { public DateTime? From { get; set; } public DateTime? To { get; set; } }
public sealed class DaysBody { public int Days { get; set; } = 30; }
