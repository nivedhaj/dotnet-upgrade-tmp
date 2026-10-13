using Ct.Gr.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Gr.Controllers;

/// <summary>Port of original api/gr/Report.</summary>
[ApiController]
[Route("api/gr/report")]
[Route("api/gr/Report")]
public sealed class ReportController(ReceiptService receipts) : ControllerBase
{
    [HttpPost("getReceiptReports")]
    public async Task<IActionResult> GetReceiptReports([FromBody] ReportFilterRequest filter, CancellationToken ct)
        => Ok(await receipts.GetReceiptReportsAsync(filter, ct));

    [HttpPost("getReceiptRevenueSharingReport")]
    public async Task<IActionResult> GetReceiptRevenueSharingReport([FromBody] ReportFilterRequest filter, CancellationToken ct)
        => Ok(await receipts.GetRevenueSharingAsync(filter, ct));

    [HttpPost("getReceiptsSummary")]
    public async Task<IActionResult> GetReceiptsSummary([FromBody] ReportFilterRequest filter, CancellationToken ct)
        => Ok(await receipts.GetSummaryAsync(filter, ct));

    [HttpPost("getReceiptConsolidatedReport")]
    public async Task<IActionResult> GetReceiptConsolidatedReport([FromBody] ReportFilterRequest filter, CancellationToken ct)
        => Ok(await receipts.GetConsolidatedReportAsync(filter, ct));

    [HttpPost("printReceipt")]
    public async Task<IActionResult> PrintReceipt([FromBody] IdRequest request, CancellationToken ct)
    {
        var print = await receipts.PrintAsync(request.Id, ct);
        return print is null ? NotFound() : Ok(print);
    }

    [HttpPost("getReceiptReportGrouped")]
    public async Task<IActionResult> GetReceiptReportGrouped([FromBody] ReportFilterRequest filter, CancellationToken ct)
        => Ok(await receipts.GetReceiptReportGroupedAsync(filter, ct));

    [HttpPost("getReceiptRevenueSharingByClass")]
    public async Task<IActionResult> GetReceiptRevenueSharingByClass([FromBody] ReportFilterRequest filter, CancellationToken ct)
        => Ok(await receipts.GetRevenueByClassAsync(filter, ct));

    [HttpPost("getReceiptRevenueSharingByPerson")]
    public async Task<IActionResult> GetReceiptRevenueSharingByPerson([FromBody] ReportFilterRequest filter, CancellationToken ct)
        => Ok(await receipts.GetRevenueByPersonAsync(filter, ct));

    [HttpPost("exportCsv")]
    public async Task<IActionResult> ExportCsv([FromBody] ReportFilterRequest filter, CancellationToken ct)
    {
        var csv = await receipts.ExportCsvAsync(filter, ct);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "receipts-report.csv");
    }
}
