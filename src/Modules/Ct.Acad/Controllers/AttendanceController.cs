using Ct.Acad.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/attendance")]
[Route("api/Attendance")]
public sealed class AttendanceController(AttendanceService attendance) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] PostAttendanceRequest request, CancellationToken ct)
        => Ok(await attendance.PostAsync(request, ct));

    [HttpGet("absentees")]
    public async Task<IActionResult> Absentees([FromQuery] DateTime? on, [FromQuery] string? courseId, CancellationToken ct)
        => Ok(await attendance.GetAbsenteesAsync(on, courseId, ct));

    [HttpGet("absenteeList")]
    public async Task<IActionResult> GetAbsenteesOn([FromQuery] DateTime? on, [FromQuery] string? courseId, CancellationToken ct)
        => Ok(await attendance.GetAbsenteesAsync(on, courseId, ct));

    [HttpPost("applyEvents")]
    public async Task<IActionResult> ApplyEvents([FromBody] List<PostAttendanceRequest> events, CancellationToken ct)
    {
        var results = new List<object>();
        foreach (var e in events ?? [])
            results.Add(await attendance.PostAsync(e, ct));
        return Ok(new { count = results.Count, results });
    }
}
