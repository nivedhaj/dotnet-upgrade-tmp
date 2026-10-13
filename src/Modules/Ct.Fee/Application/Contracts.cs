using Ct.Fee.Domain;

namespace Ct.Fee.Application;

// ---- Fee definition ----
public sealed record FeeInstallmentDto(int InstallmentNo, string? Description, decimal Amount, DateOnly? DueDate);
public sealed record FeeFineDto(int FineNo, string? Description, decimal Amount, int? OverdueByDays);
public sealed record FeeDefinitionRequest(
    string Name,
    string? DisplayText,
    decimal Amount,
    string[] Tags,
    bool CollectStudentWise,
    bool SpansAcademicYears,
    IReadOnlyList<FeeInstallmentDto> Installments,
    IReadOnlyList<FeeFineDto> Fines,
    IReadOnlyList<string> StudentList,
    IReadOnlyList<string> CourseList);
public sealed record FeeDefinitionUpdateRequest(
    Guid Id,
    string Name,
    string? DisplayText,
    decimal Amount,
    string[] Tags,
    bool CollectStudentWise,
    bool SpansAcademicYears,
    IReadOnlyList<FeeInstallmentDto> Installments,
    IReadOnlyList<FeeFineDto> Fines,
    IReadOnlyList<string> StudentList,
    IReadOnlyList<string> CourseList);
public sealed record FeeDefinitionDto(
    Guid Id,
    Guid OwnerId,
    string Name,
    string NameLower,
    string? DisplayText,
    decimal Amount,
    string[] Tags,
    bool CollectStudentWise,
    bool IsTxDone,
    FeeStatus Status,
    bool SpansAcademicYears,
    IReadOnlyList<FeeInstallmentDto> Installments,
    IReadOnlyList<FeeFineDto> Fines,
    IReadOnlyList<string> StudentList,
    IReadOnlyList<string> CourseList);
public sealed record FeeInstallmentFlattenedDto(
    Guid FeeId,
    string FeeName,
    int InstallmentNo,
    string? Description,
    decimal Amount,
    DateOnly? DueDate,
    IReadOnlyList<string> CourseList);
public sealed record FeeStudentCheckRequest(Guid FeeId, string StudentId);
public sealed record CanRemoveStudentResult(bool CanRemove, string? Reason);

// ---- Transactions ----
public sealed record TxLine(Guid? FeeId, int No, decimal Amount);
public sealed record DiscountLine(string? Reason, decimal Amount);
public sealed record AdjustmentLine(string? Reason, decimal Amount);
public sealed record FeeTransactionRequest(
    string TxNo,
    DateTimeOffset TxDate,
    string StudentId,
    string? PaymentMode,
    bool IsOpeningBalanceAdjusted,
    string? RefNo,
    string? ReceivedInAccount,
    string? PaidBy,
    IReadOnlyList<TxLine> InstallmentsPaid,
    IReadOnlyList<TxLine> FinesPaid,
    IReadOnlyList<DiscountLine> Discounts,
    IReadOnlyList<AdjustmentLine> Adjustments);
public sealed record FeeTransactionUpdateRequest(Guid Id, DateTimeOffset TxDate, string? RefNo, string? PaidBy);
public sealed record FeeTransactionDto(
    Guid Id,
    string TxNo,
    DateTimeOffset TxDate,
    string StudentId,
    string? PaymentMode,
    bool IsOpeningBalanceAdjusted,
    string? RefNo,
    string? ReceivedInAccount,
    string? PaidBy,
    decimal Amount,
    TransactionStatus Status);
public sealed record FeeTxHtmlDto(Guid Id, string TxNo, string Html);
public sealed record StudentFeeDetailDto(
    string StudentId,
    IReadOnlyList<FeeTransactionDto> Transactions,
    IReadOnlyList<FeeToPayDto> FeesToPay);
public sealed record FeeToPayDto(
    Guid FeeId,
    string FeeName,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal Balance,
    IReadOnlyList<FeeInstallmentDto> PendingInstallments,
    IReadOnlyList<FeeFineDto> PendingFines);
public sealed record FeeCollectionAccountDto(string AccountName, string? AccountCode);

// ---- Refunds ----
public sealed record RefundRequest(
    string StudentId,
    Guid? FeeTransactionId,
    DateTimeOffset RefundDate,
    decimal Amount,
    string? Reason,
    string? Reference);
public sealed record RefundDto(
    Guid Id,
    string StudentId,
    Guid? FeeTransactionId,
    DateTimeOffset RefundDate,
    decimal Amount,
    RefundStatus Status,
    string? Reason,
    string? Reference);

// ---- Suspense ----
public sealed record SuspenseTransactionRequest(
    DateTimeOffset TransactionDate,
    decimal Amount,
    string? PaymentMode,
    string? Particulars,
    string? Reference);
public sealed record SuspenseAllocationRequest(Guid FeeTransactionId, decimal AllocatedAmount);
public sealed record SuspenseTransactionDto(
    Guid Id,
    DateTimeOffset TransactionDate,
    decimal Amount,
    string? PaymentMode,
    string? Particulars,
    string? Reference,
    SuspenseStatus Status,
    decimal AllocatedAmount);

// ---- Reports / summaries ----
public sealed record FeeCollectionSummary(DateOnly Date, decimal CollectedAmount, long TransactionCount);
public sealed record StudentFeeSummary(string StudentId, decimal ChargedAmount, decimal PaidAmount, decimal Balance);
public sealed record ReportFilterRequest(
    DateOnly? FromDate,
    DateOnly? ToDate,
    string? CourseId,
    string? StudentId,
    string? FeeId,
    string? AcademicYear,
    string? PaymentMode,
    bool? IncludeInactive);
public sealed record ReportResultDto(string ReportName, object? Data, string? Message = null);
public sealed record EmailFeeReceiptRequest(Guid TxId, string? StudentId, PrintOptionsDto? PrintOptions);
public sealed record PrintOptionsDto(bool IncludeHeader = true, bool IncludeFooter = true, string? Template = null);

// ---- Missing Cmd/Qry ports ----
public sealed record DateRangeDto(DateOnly From, DateOnly To, string RangeType);
public sealed record FeeExpectedCollectedDto(
    Guid FeeId,
    string FeeName,
    string? DisplayText,
    DateOnly PeriodFrom,
    DateOnly PeriodTo,
    decimal ExpectedAmount,
    decimal CollectedAmount,
    decimal Balance);
public sealed record StudentClosingBalanceDto(
    string StudentId,
    decimal ExpectedTotal,
    decimal PaidTotal,
    decimal AdjustmentTotal,
    decimal PendingTotal,
    decimal ClosingBalance,
    IReadOnlyList<DefaulterInstallmentDto> PendingInstallments);
public sealed record FeeInstallmentExpectedPaidDto(
    Guid FeeId,
    string FeeName,
    int InstallmentNo,
    string? Description,
    decimal ExpectedAmount,
    decimal PaidAmount,
    decimal Balance,
    DateOnly? DueDate,
    bool IsPaid);
public sealed record StudentFeesExpectedPaidDto(
    string StudentId,
    IReadOnlyList<FeeInstallmentExpectedPaidDto> Lines,
    decimal ExpectedTotal,
    decimal PaidTotal,
    decimal PendingTotal,
    decimal ExcessPaid);
public sealed record AssignStudentRequest(Guid FeeId, string StudentId);
public sealed record TransactResultDto(Guid Id, string TxNo, decimal Amount, string? Message);
