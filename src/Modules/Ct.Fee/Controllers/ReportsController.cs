using Ct.Fee.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Fee.Controllers;

/// <summary>
/// Report endpoints — full PostgreSQL implementations via ReportService
/// (replaces original RavenDB ReportController + ReportService).
/// </summary>
[ApiController]
[Route("api/fee/reports")]
public sealed class ReportsController(ReportService reports, FeeService fees) : ControllerBase
{
    [HttpPost("feeStructure")]
    public async Task<ActionResult<IReadOnlyList<FeeStructureRowDto>>> FeeStructure(CancellationToken ct)
        => Ok(await reports.GetFeeStructureAsync(ct));

    [HttpPost("coursewiseFeeStructure")]
    public async Task<ActionResult<IReadOnlyList<CoursewiseFeeStructureRowDto>>> CoursewiseFeeStructure(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetCoursewiseFeeStructureAsync(filter?.CourseId, ct));

    [HttpPost("feesExpected")]
    public async Task<ActionResult<IReadOnlyList<ExpectedFeeLineDto>>> FeesExpected(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetFeesExpectedAsync(filter, ct));

    [HttpPost("datewiseFeeCollection")]
    public async Task<ActionResult<DatewiseCollectionReportDto>> DatewiseFeeCollection(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
    {
        var from = filter?.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var to = filter?.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await reports.GetDatewiseFeeCollectionAsync(from, to, ct));
    }

    [HttpPost("datewiseFeeCollectionGrouped")]
    public async Task<ActionResult<GroupedDatewiseCollectionDto>> DatewiseFeeCollectionGrouped(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
    {
        var from = filter?.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var to = filter?.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await reports.GetDatewiseFeeCollectionGroupedAsync(from, to, ct));
    }

    [HttpPost("feesDefaultersVersion1")]
    public async Task<ActionResult<DefaultersReportDto>> FeesDefaultersV1(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetFeesDefaultersAsync(filter?.CourseId, filter?.ToDate, version: 1, showOpeningBalance: true, ct));

    [HttpPost("feesDefaultersVersion2")]
    public async Task<ActionResult<DefaultersReportDto>> FeesDefaultersV2(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetFeesDefaultersAsync(filter?.CourseId, filter?.ToDate, version: 2, showOpeningBalance: true, ct));

    [HttpGet("defaultersAsJson")]
    public async Task<ActionResult<DefaultersReportDto>> DefaultersAsJson(CancellationToken ct)
        => Ok(await reports.GetFeesDefaultersAsync(null, null, version: 2, showOpeningBalance: true, ct));

    [HttpPost("receiptTransaction")]
    public async Task<ActionResult<IReadOnlyList<ReceiptTransactionRowDto>>> ReceiptTransaction(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetReceiptTransactionsAsync(filter, ct));

    [HttpPost("receiptTransactionCSV")]
    public async Task<ActionResult<CsvReportDto>> ReceiptTransactionCsv(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetReceiptTransactionsCsvAsync(filter, ct));

    [HttpPost("studentwiseTransaction")]
    public async Task<ActionResult<StudentwiseTransactionReportDto>> StudentwiseTransaction(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(filter?.StudentId))
            return BadRequest("StudentId is required.");
        return Ok(await reports.GetStudentwiseTransactionsAsync(filter.StudentId, ct));
    }

    [HttpPost("courseStudentwiseTransaction")]
    public async Task<ActionResult<IReadOnlyList<StudentwiseTransactionReportDto>>> CourseStudentwiseTransaction(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetCourseStudentwiseTransactionsAsync(filter?.CourseId, ct));

    [HttpPost("adjustmentTransactions")]
    public async Task<ActionResult<IReadOnlyList<AdjustmentTransactionRowDto>>> AdjustmentTransactions(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetAdjustmentTransactionsAsync(filter?.CourseId, ct));

    [HttpPost("feesPaid")]
    public async Task<ActionResult<FeesPaidReportDto>> FeesPaid(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
    {
        var from = filter?.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var to = filter?.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await reports.GetFeesPaidAsync(from, to, filter?.CourseId, null, ct));
    }

    [HttpPost("feesPaidCSV")]
    public async Task<ActionResult<CsvReportDto>> FeesPaidCsv(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
    {
        var from = filter?.FromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));
        var to = filter?.ToDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return Ok(await reports.GetFeesPaidCsvAsync(from, to, filter?.CourseId, null, ct));
    }

    [HttpPost("breakUps")]
    public async Task<ActionResult<IReadOnlyList<BreakupRowDto>>> BreakUps(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetBreakUpsAsync(filter, ct));

    [HttpPost("fineCollected")]
    public async Task<ActionResult<IReadOnlyList<FineCollectedRowDto>>> FineCollected(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetFineCollectedAsync(filter, ct));

    [HttpPost("latePayment")]
    public async Task<ActionResult<IReadOnlyList<LatePaymentRowDto>>> LatePayment(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetLatePaymentAsync(filter, ct));

    [HttpPost("timeLine")]
    public async Task<ActionResult<IReadOnlyList<TimelinePointDto>>> TimeLine(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetTimeLineAsync(filter, ct));

    [HttpPost("adjustmentBalance")]
    public async Task<ActionResult<IReadOnlyList<AdjustmentBalanceRowDto>>> AdjustmentBalance(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetAdjustmentBalanceAsync(filter?.CourseId, ct));

    [HttpPost("advanceBalance")]
    public async Task<ActionResult<IReadOnlyList<AdvanceBalanceRowDto>>> AdvanceBalance(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
    {
        Guid[]? feeIds = null;
        if (!string.IsNullOrWhiteSpace(filter?.FeeId) && Guid.TryParse(filter.FeeId, out var fid))
            feeIds = [fid];
        return Ok(await reports.GetAdvanceBalanceAsync(feeIds, ct));
    }

    [HttpPost("TxHistory")]
    public async Task<ActionResult<IReadOnlyList<TxHistoryRowDto>>> TxHistory(CancellationToken ct)
        => Ok(await reports.GetTxHistoryAsync(ct));

    [HttpPost("feesPaidAfterAcademicYear")]
    public async Task<ActionResult<IReadOnlyList<FeesPaidAfterAyRowDto>>> FeesPaidAfterAcademicYear(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
    {
        int? year = null;
        if (!string.IsNullOrWhiteSpace(filter?.AcademicYear) && int.TryParse(filter.AcademicYear, out var y))
            year = y;
        return Ok(await reports.GetFeesPaidAfterAcademicYearAsync(year, ct));
    }

    [HttpPost("feesPaidByTCStudents")]
    public async Task<ActionResult<IReadOnlyList<FeesPaidByTcRowDto>>> FeesPaidByTcStudents(CancellationToken ct)
        => Ok(await reports.GetFeesPaidByTcStudentsAsync(ct));

    [HttpPost("coursewiseFeeBreakupAsJson")]
    public async Task<ActionResult<CoursewiseWidgetDto>> CoursewiseFeeBreakupAsJson(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetCoursewiseFeeBreakupAsync(filter, ct));

    [HttpPost("coursewiseFeeCollectionAsJson")]
    public async Task<ActionResult<CoursewiseWidgetDto>> CoursewiseFeeCollectionAsJson(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetCoursewiseFeeCollectionWidgetAsync(filter, ct));

    [HttpPost("coursewiseFeeStructureAsJson")]
    public async Task<ActionResult<CoursewiseWidgetDto>> CoursewiseFeeStructureAsJson(CancellationToken ct)
        => Ok(await reports.GetCoursewiseFeeBreakupAsync(null, ct));

    [HttpPost("paymentModeBreakup")]
    public async Task<ActionResult<IReadOnlyList<PaymentModeBreakupRowDto>>> PaymentModeBreakup(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetPaymentModeBreakupAsync(filter, ct));

    [HttpPost("printFeeReceipt")]
    public async Task<ActionResult<FeeTxHtmlDto>> PrintFeeReceipt(
        [FromBody] EmailFeeReceiptRequest request, CancellationToken ct)
    {
        var html = await reports.GetPrintFeeReceiptAsync(request.TxId, ct);
        return html is null ? NotFound() : Ok(html);
    }

    [HttpGet("feesPayable")]
    public async Task<ActionResult<IReadOnlyList<FeeToPayDto>>> FeesPayable(
        [FromQuery] string studentId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(studentId))
            return BadRequest("studentId is required.");
        return Ok(await reports.GetFeesPayableAsync(studentId, ct));
    }

    [HttpPost("feeReceivedInOtherAccount")]
    public async Task<ActionResult<IReadOnlyList<FeeRioAccountRowDto>>> FeeReceivedInOtherAccount(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetFeeReceivedInOtherAccountAsync(filter, ct));

    [HttpPost("concession")]
    public async Task<ActionResult<IReadOnlyList<ConcessionRowDto>>> Concession(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetConcessionAsync(filter, ct));

    [HttpPost("refund")]
    public async Task<ActionResult<IReadOnlyList<RefundReportRowDto>>> RefundReport(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetRefundReportAsync(filter, ct));

    [HttpPost("suspenseAccountTxn")]
    public async Task<ActionResult<IReadOnlyList<SuspenseReportRowDto>>> SuspenseAccountTxn(
        [FromBody] ReportFilterRequest? filter, CancellationToken ct)
        => Ok(await reports.GetSuspenseAccountTxnReportAsync(filter, ct));

    // Lightweight summaries (also on FeeService)
    [HttpGet("collection-datewise")]
    public async Task<ActionResult<IReadOnlyList<FeeCollectionSummary>>> Datewise(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
        => Ok(await fees.GetDatewiseCollectionAsync(from, to, ct));

    [HttpGet("student/{studentId}/summary")]
    public async Task<ActionResult<StudentFeeSummary>> StudentSummary(string studentId, CancellationToken ct)
        => Ok(await fees.GetStudentSummaryAsync(studentId, ct));
}
