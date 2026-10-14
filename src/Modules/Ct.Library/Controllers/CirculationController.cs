using Ct.Library.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Library.Controllers;

[ApiController]
[Route("api/library/circulation")]
[Route("api/Circulation")]
public sealed class CirculationController(CirculationService circ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await circ.GetActiveAsync(ct));

    [HttpPost("issue")]
    public async Task<IActionResult> Issue([FromBody] IssueBody body, CancellationToken ct)
    {
        try { return Ok(await circ.IssueAsync(body.MaterialId, body.MemberId, body.Days, ct)); }
        catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("return")]
    public async Task<IActionResult> Return([FromBody] MaterialIdBody body, CancellationToken ct)
    {
        try { return Ok(await circ.ReturnAsync(body.MaterialId, ct)); }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
        { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("reissue")]
    public async Task<IActionResult> Reissue([FromBody] ReissueBody body, CancellationToken ct)
    {
        try { return Ok(await circ.ReissueAsync(body.MaterialId, body.Days, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet("widget/monthwiseCheckouts")]
    public async Task<IActionResult> GetMonthwiseCheckoutsWidgetData(CancellationToken ct)
        => Ok(await circ.MonthwiseCheckoutsAsync(ct));
}

public sealed class IssueBody { public required Guid MaterialId { get; set; } public required Guid MemberId { get; set; } public int Days { get; set; } = 14; }
public sealed class MaterialIdBody { public required Guid MaterialId { get; set; } }
public sealed class ReissueBody { public required Guid MaterialId { get; set; } public int Days { get; set; } = 14; }
