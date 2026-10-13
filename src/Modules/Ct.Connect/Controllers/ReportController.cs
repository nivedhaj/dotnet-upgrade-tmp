using Ct.Connect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Connect.Controllers;

[ApiController]
[Route("api/connect/report")]
[Route("api/Connect/Report")]
public sealed class ReportController(ConnectQry qry) : ControllerBase
{
    [HttpPost("SMSUsageReport")]
    [HttpGet("SMSUsageReport")]
    public async Task<IActionResult> SMSUsageReport(CancellationToken ct)
        => Ok(await qry.GetSmsUsageReportAsync(ct));

    [HttpPost("invalidMobiles")]
    [HttpGet("invalidMobiles")]
    public async Task<IActionResult> GetInvalidMobilesReport(CancellationToken ct)
        => Ok(await qry.GetInvalidMobilesReportAsync(ct));

    [HttpPost("invalidEmails")]
    [HttpGet("invalidEmails")]
    public async Task<IActionResult> GetInvalidEmailsReport(CancellationToken ct)
        => Ok(await qry.GetInvalidEmailsReportAsync(ct));
}
