using Ct.Staff.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Staff.Controllers;

[ApiController]
[Route("api/staff/report")]
[Route("api/Report")]
public sealed class ReportController(StaffReportService reports) : ControllerBase
{
    [HttpPost("getStaffTodayTimetableReport")]
    public async Task<IActionResult> GetStaffTodayTimetableReport([FromBody] StaffIdBody? body, CancellationToken ct)
        => Ok(await reports.TodayTimetableAsync(body?.Id, ct));

    [HttpPost("getStaffList")]
    public async Task<IActionResult> GetStaffListReport(CancellationToken ct)
        => Ok(await reports.StaffListAsync(ct));

    [HttpPost("viewPayslip")]
    public async Task<IActionResult> GetPayslip([FromBody] SalaryBody body, CancellationToken ct)
    {
        var p = await reports.PayslipAsync(body.Id, body.SalaryId == 0 ? null : body.SalaryId, ct);
        return p is null ? NotFound() : Ok(p);
    }
}
