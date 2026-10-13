using Ct.Acad.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/grading")]
[Route("api/Grading")]
public sealed class GradingController(GradingService grading) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await grading.GetAllAsync(ct));

    [HttpGet("getGradingDetails")]
    [HttpGet("GetGradingDetails")]
    public async Task<IActionResult> GetGradingDetails([FromQuery] Guid? id, CancellationToken ct)
    {
        var all = await grading.GetAllAsync(ct);
        if (id is null) return Ok(all);
        var one = all.FirstOrDefault(g => g.Id == id);
        return one is null ? NotFound() : Ok(one);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateGradingRequest request, CancellationToken ct)
        => Ok(await grading.CreateAsync(request.Name, request.Rules ?? [], ct));

    [HttpPost("enable")]
    [HttpPost("enableGrading")]
    public async Task<IActionResult> EnableGrading([FromBody] IdRequest request, CancellationToken ct)
        => await grading.EnableAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("disable")]
    [HttpPost("disableGrading")]
    public async Task<IActionResult> DisableGrading([FromBody] IdRequest request, CancellationToken ct)
        => await grading.DisableAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("setGradingRules")]
    public async Task<IActionResult> SetGradingRules([FromBody] CreateGradingRequest request, CancellationToken ct)
    {
        // create/replace by name
        return Ok(await grading.CreateAsync(request.Name, request.Rules ?? [], ct));
    }

    [HttpGet("getGrade")]
    public async Task<IActionResult> GetGrade([FromQuery] Guid id, [FromQuery] decimal marks, [FromQuery] decimal totalMarks, CancellationToken ct)
        => Ok(new { grade = await grading.GetGradeAsync(id, marks, totalMarks, ct) });
}
