using Ct.Acad.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/ttplanner")]
[Route("api/TTPlanner")]
public sealed class TTPlannerController(TtService tt) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? courseId, CancellationToken ct)
        => Ok(await tt.GetAsync(courseId, ct));

    [HttpPost("rule")]
    public async Task<IActionResult> Rule([FromBody] TtRulesRequest request, CancellationToken ct)
        => Ok(await tt.SetRulesAsync(request, ct));

    [HttpPost("run")]
    public async Task<IActionResult> Run([FromBody] TtGenerateRequest request, CancellationToken ct)
        => Ok(await tt.GenerateAsync(request, ct));

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateTT([FromBody] TtGenerateRequest request, CancellationToken ct)
        => Ok(await tt.GenerateAsync(request, ct));

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel([FromBody] IdRequest request, CancellationToken ct)
        => await tt.CancelAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("report/timetable")]
    public async Task<IActionResult> PrintTimetable([FromBody] IdRequest request, CancellationToken ct)
    {
        var r = await tt.PrintAsync(request.Id, ct);
        return r is null ? NotFound() : Ok(r);
    }
}
