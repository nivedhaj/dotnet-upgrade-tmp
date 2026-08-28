using Ct.Modules.Fees.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Modules.Fees.Api;

[ApiController, Route("api/stu/fees")]
public sealed class FeeController(IFeeQueries queries) : ControllerBase
{
    [HttpGet("student/{studentId:guid}/summary")]
    public async Task<IActionResult> StudentSummary(Guid studentId, CancellationToken ct) =>
        Ok(await queries.GetFeeSummaryAsync(studentId, ct));

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct) =>
        Ok(await queries.GetFeeSummaryAsync(null, ct));
}
