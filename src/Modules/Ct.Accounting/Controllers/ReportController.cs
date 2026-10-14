using Ct.Accounting.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Accounting.Controllers;

[ApiController]
[Route("api/accounting/report")]
[Route("api/Report")]
public sealed class ReportController(AccountingReportService reports) : ControllerBase
{
    [HttpPost("smsCreditsTx")]
    public async Task<IActionResult> SMSCreditsTx(CancellationToken ct)
        => Ok(await reports.SmsCreditsTxReportAsync(ct));

    [HttpPost("datewiseExpense")]
    public async Task<IActionResult> GetDatewiseExpenseReport([FromBody] AccDateRangeBody? body, CancellationToken ct)
        => Ok(await reports.DatewiseExpenseAsync(body?.From, body?.To, ct));

    [HttpPost("categorywiseExpense")]
    public async Task<IActionResult> GetCategorywiseExpenseReport(CancellationToken ct)
        => Ok(await reports.CategorywiseExpenseAsync(ct));

    [HttpPost("yearwiseExpense")]
    public async Task<IActionResult> GetYearwiseExpenseReport(CancellationToken ct)
        => Ok(await reports.YearwiseExpenseAsync(ct));

    [HttpGet("tallyXML")]
    public async Task<IActionResult> GetTallyXMLReport(CancellationToken ct)
        => Ok(await reports.TallyXmlAsync(ct));

    [HttpGet("tallyPrimeXML")]
    public async Task<IActionResult> GetTallyPrimeXMLReport(CancellationToken ct)
        => Ok(await reports.TallyXmlAsync(ct)); // same export shape for Prime
}

public sealed class AccDateRangeBody { public DateTime? From { get; set; } public DateTime? To { get; set; } }
