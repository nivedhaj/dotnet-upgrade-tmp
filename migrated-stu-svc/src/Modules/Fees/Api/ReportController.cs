using Ct.Modules.Fees.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Modules.Fees.Api;

[ApiController, Route("api/stu/[controller]")]
public sealed class ReportController(IFeeQueries queries) : ControllerBase
{
    [HttpGet("student-fee-summary")]
    public async Task<IActionResult> StudentFeeSummary([FromQuery] Guid? studentId, CancellationToken ct) =>
        Ok(await queries.GetFeeSummaryAsync(studentId, ct));
}
