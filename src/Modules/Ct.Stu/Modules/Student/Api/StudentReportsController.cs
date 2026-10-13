using Ct.Modules.Admissions.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Modules.Admissions.Api;

/// <summary>Port of original ct.stu ReportController endpoints to Postgres queries.</summary>
[ApiController]
[Route("api/stu/report")]
[Route("api/student/report")]
public sealed class StudentReportsController(IStudentQueryService queries) : ControllerBase
{
    [HttpPost("sectionwise")]
    [HttpGet("sectionwise")]
    public async Task<IActionResult> Sectionwise([FromQuery] string? instId, CancellationToken ct)
        => Ok(await queries.GetSectionwiseAsync(instId, ct));

    [HttpPost("coursewise")]
    [HttpGet("coursewise")]
    public async Task<IActionResult> Coursewise([FromQuery] string? instId, CancellationToken ct)
    {
        var sectionwise = await queries.GetSectionwiseAsync(instId, ct);
        var coursewise = sectionwise.GroupBy(s => new { s.CourseId, s.CourseName })
            .Select(g => new { g.Key.CourseId, g.Key.CourseName, Count = g.Sum(x => x.Count), Sections = g.ToList() });
        return Ok(coursewise);
    }

    [HttpPost("newAdmissions")]
    [HttpGet("newAdmissions")]
    public async Task<IActionResult> NewAdmissions([FromQuery] string? instId, [FromQuery] DateOnly? from, CancellationToken ct)
    {
        var all = await queries.GetAllAsync(instId, ct);
        var cutoff = from ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));
        var filtered = all.Where(s => s.Doa is not null && s.Doa >= cutoff).ToList();
        return Ok(filtered);
    }

    [HttpGet("absentees")]
    [HttpPost("absentees")]
    public IActionResult Absentees()
        => Ok(new { message = "Absentees report requires attendance events integration." });
}
