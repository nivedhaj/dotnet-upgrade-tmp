namespace Ct.Fee.Application;

// ---- Shared filter (already in Contracts; report-specific shapes below) ----

public sealed record FeeInstRef(Guid FeeId, int InstallmentNo);

// Fee structure
public sealed record FeeStructureRowDto(
    Guid FeeId,
    string FeeName,
    string? DisplayText,
    decimal TotalAmount,
    bool CollectStudentWise,
    bool SpansAcademicYears,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> CourseIds,
    IReadOnlyList<string> StudentIds,
    IReadOnlyList<FeeInstallmentDto> Installments,
    IReadOnlyList<FeeFineDto> Fines);

// Coursewise fee structure
public sealed record CoursewiseFeeStructureRowDto(
    string CourseId,
    Guid FeeId,
    string FeeName,
    int InstallmentNo,
    string? Description,
    decimal Amount,
    DateOnly? DueDate);

// Expected fees
public sealed record ExpectedFeeLineDto(
    Guid FeeId,
    string FeeName,
    string? DisplayText,
    decimal AmountPerStudent,
    int StudentCount,
    decimal ExpectedAmount,
    decimal CollectedAmount,
    decimal Balance,
    string? CourseId);

// Datewise collection
public sealed record DatewiseCollectionRowDto(
    DateOnly Date,
    Guid? FeeId,
    string FeeName,
    decimal Amount,
    long TransactionCount,
    string? PaymentMode);

public sealed record DatewiseCollectionReportDto(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<string> FeeColumns,
    IReadOnlyList<DatewiseCollectionRowDto> Rows,
    decimal GrandTotal);

public sealed record GroupedDatewiseCollectionDto(
    DateOnly From,
    DateOnly To,
    IReadOnlyList<DateOnly> Dates,
    IReadOnlyList<string> FeeNames,
    IReadOnlyList<GroupedDateRowDto> Rows,
    decimal GrandTotal);

public sealed record GroupedDateRowDto(
    DateOnly Date,
    IReadOnlyDictionary<string, decimal> AmountByFee,
    decimal DayTotal);

// Defaulters
public sealed record DefaulterInstallmentDto(
    Guid FeeId,
    string FeeName,
    int InstallmentNo,
    string? Description,
    DateOnly? DueDate,
    decimal Amount,
    decimal Paid,
    decimal Balance);

public sealed record DefaulterStudentDto(
    string StudentId,
    string? CourseId,
    decimal TotalDue,
    decimal TotalPaid,
    decimal Balance,
    IReadOnlyList<DefaulterInstallmentDto> Installments);

public sealed record DefaultersReportDto(
    string? CourseId,
    DateOnly AsOf,
    int Version,
    bool ShowOpeningBalance,
    IReadOnlyList<DefaulterStudentDto> Students,
    decimal GrandBalance);

// Receipt / studentwise transactions
public sealed record ReceiptTransactionRowDto(
    Guid TxId,
    string TxNo,
    DateTimeOffset TxDate,
    string StudentId,
    string? PaymentMode,
    string? RefNo,
    string? ReceivedInAccount,
    decimal Amount,
    string Status,
    bool IsOpeningBalanceAdjusted,
    IReadOnlyList<TxLineDetailDto> Installments,
    IReadOnlyList<TxLineDetailDto> Fines,
    IReadOnlyList<NamedAmountDto> Discounts,
    IReadOnlyList<NamedAmountDto> Adjustments);

public sealed record TxLineDetailDto(Guid? FeeId, int No, decimal Amount);
public sealed record NamedAmountDto(string? Reason, decimal Amount);

public sealed record StudentwiseTransactionReportDto(
    string StudentId,
    IReadOnlyList<ReceiptTransactionRowDto> Transactions,
    decimal TotalPaid,
    decimal TotalDiscount,
    decimal TotalAdjustment);

// Adjustment transactions
public sealed record AdjustmentTransactionRowDto(
    Guid TxId,
    string TxNo,
    DateTimeOffset TxDate,
    string StudentId,
    string? Reason,
    decimal Amount);

// Fees paid
public sealed record FeesPaidRowDto(
    string StudentId,
    string? CourseId,
    Guid FeeId,
    string FeeName,
    int InstallmentNo,
    decimal AmountDue,
    decimal AmountPaid,
    DateTimeOffset? PaidOn,
    string? PaymentMode,
    string? TxNo);

public sealed record FeesPaidReportDto(
    DateOnly From,
    DateOnly To,
    string? CourseId,
    IReadOnlyList<FeesPaidRowDto> Rows,
    decimal TotalPaid);

// Breakups / fine / late / timeline
public sealed record BreakupRowDto(string Category, string Label, decimal Amount, long Count);
public sealed record FineCollectedRowDto(
    Guid TxId,
    string TxNo,
    DateTimeOffset TxDate,
    string StudentId,
    Guid? FeeId,
    int FineNo,
    decimal Amount);
public sealed record LatePaymentRowDto(
    string StudentId,
    Guid FeeId,
    string FeeName,
    int InstallmentNo,
    DateOnly? DueDate,
    DateTimeOffset? PaidOn,
    int DaysLate,
    decimal Amount);
public sealed record TimelinePointDto(DateOnly Date, decimal Collected, decimal Expected, decimal CumulativeCollected);

// Adjustment / advance balance
public sealed record AdjustmentBalanceRowDto(
    string StudentId,
    decimal AdjustmentTotal,
    decimal PaidTotal,
    decimal Net);
public sealed record AdvanceBalanceRowDto(
    string StudentId,
    Guid? FeeId,
    string? FeeName,
    decimal Charged,
    decimal Paid,
    decimal Advance);

// Tx history
public sealed record TxHistoryRowDto(
    Guid TxId,
    string TxNo,
    DateTimeOffset TxDate,
    string StudentId,
    string? PaymentMode,
    decimal Amount,
    string Status,
    DateTimeOffset CreatedOn,
    string CreatedBy,
    DateTimeOffset ModifiedOn,
    string ModifiedBy);

// Fees paid after AY / by TC
public sealed record FeesPaidAfterAyRowDto(
    Guid TxId,
    string TxNo,
    DateTimeOffset TxDate,
    string StudentId,
    decimal Amount,
    int? AcademicYearFrom);
public sealed record FeesPaidByTcRowDto(
    string StudentId,
    Guid TxId,
    string TxNo,
    DateTimeOffset TxDate,
    decimal Amount,
    string Note);

// Widgets / JSON
public sealed record CoursewiseWidgetCellDto(string CourseId, string FeeName, decimal Amount);
public sealed record CoursewiseWidgetDto(
    IReadOnlyList<string> Courses,
    IReadOnlyList<string> Fees,
    IReadOnlyList<CoursewiseWidgetCellDto> Cells,
    decimal GrandTotal);
public sealed record PaymentModeBreakupRowDto(string PaymentMode, decimal Amount, long Count);
public sealed record ConcessionRowDto(
    Guid TxId,
    string TxNo,
    DateTimeOffset TxDate,
    string StudentId,
    string? Reason,
    decimal Amount);
public sealed record FeeRioAccountRowDto(
    Guid TxId,
    string TxNo,
    DateTimeOffset TxDate,
    string StudentId,
    string ReceivedInAccount,
    decimal Amount);
public sealed record RefundReportRowDto(
    Guid Id,
    string StudentId,
    Guid? FeeTransactionId,
    DateTimeOffset RefundDate,
    decimal Amount,
    string Status,
    string? Reason,
    string? Reference);
public sealed record SuspenseReportRowDto(
    Guid Id,
    DateTimeOffset TransactionDate,
    decimal Amount,
    decimal AllocatedAmount,
    decimal UnallocatedAmount,
    string Status,
    string? PaymentMode,
    string? Particulars);

// CSV wrapper
public sealed record CsvReportDto(string FileName, string CsvContent, object? SourceData);
