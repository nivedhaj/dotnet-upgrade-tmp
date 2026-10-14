using Ct.Library.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Library.Controllers;

[ApiController]
[Route("api/library/report")]
[Route("api/Report")]
public sealed class ReportController(LibraryReportService reports) : ControllerBase
{
    [HttpPost("summary")]
    public async Task<IActionResult> SummaryReport(CancellationToken ct) => Ok(await reports.SummaryAsync(ct));

    [HttpPost("valuation")]
    public async Task<IActionResult> ValuationReport(CancellationToken ct) => Ok(await reports.ValuationAsync(ct));

    [HttpPost("overDue")]
    public async Task<IActionResult> OverDue(CancellationToken ct) => Ok(await reports.OverDueAsync(ct));

    [HttpPost("lateFees")]
    public async Task<IActionResult> LateFeesReport(CancellationToken ct) => Ok(await reports.LateFeesAsync(ct));

    [HttpPost("materialsUnderCirculation")]
    public async Task<IActionResult> MaterialsUnderCirculation(CancellationToken ct) => Ok(await reports.UnderCirculationAsync(ct));

    [HttpPost("materialsUnderMaintenance")]
    public async Task<IActionResult> MaterialsUnderMaintenance(CancellationToken ct) => Ok(await reports.UnderMaintenanceAsync(ct));

    [HttpPost("materialsDueForMaintenance")]
    public async Task<IActionResult> MaterialsDueForMaintenance(CancellationToken ct) => Ok(await reports.DueForMaintenanceAsync(ct));

    [HttpPost("lostMaterials")]
    public async Task<IActionResult> LostMaterials(CancellationToken ct) => Ok(await reports.LostAsync(ct));

    [HttpPost("referenceMaterials")]
    public async Task<IActionResult> ReferenceMaterials(CancellationToken ct) => Ok(await reports.ReferenceAsync(ct));

    [HttpPost("booksWithoutISBN")]
    public async Task<IActionResult> BooksWithoutISBN(CancellationToken ct) => Ok(await reports.WithoutIsbnAsync(ct));

    [HttpPost("booksWithoutCoverPhoto")]
    public async Task<IActionResult> BooksWithoutCoverPhoto(CancellationToken ct) => Ok(await reports.WithoutCoverAsync(ct));

    [HttpPost("booksWithoutSynopsis")]
    public async Task<IActionResult> BooksWithoutSynopsis(CancellationToken ct) => Ok(await reports.WithoutSynopsisAsync(ct));

    [HttpPost("lastMonitored")]
    public async Task<IActionResult> LastMonitored(CancellationToken ct) => Ok(await reports.LastMonitoredAsync(ct));

    [HttpPost("unverifiedStock")]
    public async Task<IActionResult> UnverifiedStock(CancellationToken ct) => Ok(await reports.UnverifiedStockAsync(ct));

    [HttpPost("members")]
    public async Task<IActionResult> LibraryMembers(CancellationToken ct) => Ok(await reports.MembersAsync(ct));

    [HttpPost("ageAnalysis")]
    public async Task<IActionResult> AgeAnalysisReport(CancellationToken ct) => Ok(await reports.AgeAnalysisAsync(ct));
}
