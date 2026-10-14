using Ct.Scheduler.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Scheduler.Controllers;

[ApiController]
[Route("api/scheduler")]
[Route("api/Scheduler")]
public sealed class SchedulerController(SchedulerService scheduler) : ControllerBase
{
    [HttpGet("getLibraryDueDate")]
    public async Task<IActionResult> GetDueDate([FromQuery] DateTime? date, [FromQuery] int workingDays = 14, CancellationToken ct = default)
        => Ok(await scheduler.GetDueDateAsync(date, workingDays, ct));

    [HttpGet("holidays")]
    [HttpGet("getHolidays")]
    public async Task<IActionResult> GetHolidays([FromQuery] DateTime? acad_from, [FromQuery] DateTime? acad_to, CancellationToken ct)
        => Ok(await scheduler.GetHolidaysAsync(ct));

    [HttpGet("nextWorkingDays")]
    public async Task<IActionResult> GetNextWorkingDays([FromQuery] DateTime? date, [FromQuery] int count = 5, CancellationToken ct = default)
        => Ok(await scheduler.GetNextWorkingDaysAsync(date, count, ct));
}
