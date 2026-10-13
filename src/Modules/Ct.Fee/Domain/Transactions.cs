namespace Ct.Fee.Domain;

public sealed class FeeTransaction : AuditableEntity
{
    public string TxNo { get; set; } = "";
    public DateTimeOffset TxDate { get; set; }
    public string StudentId { get; set; } = "";
    public string? PaymentMode { get; set; }
    public bool IsOpeningBalanceAdjusted { get; set; }
    public string? RefNo { get; set; }
    public string? ReceivedInAccount { get; set; }
    public decimal Amount { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public List<FeeTransactionInstallment> InstallmentsPaid { get; set; } = [];
    public List<FeeTransactionFine> FinesPaid { get; set; } = [];
    public List<FeeTransactionDiscount> Discounts { get; set; } = [];
    public List<FeeTransactionAdjustment> Adjustments { get; set; } = [];
}

public sealed class FeeTransactionInstallment : AuditableEntity
{
    public Guid FeeTransactionId { get; set; }
    public Guid? FeeDefinitionId { get; set; }
    public int InstallmentNo { get; set; }
    public decimal Amount { get; set; }
    public FeeTransaction? FeeTransaction { get; set; }
}

public sealed class FeeTransactionFine : AuditableEntity
{
    public Guid FeeTransactionId { get; set; }
    public Guid? FeeDefinitionId { get; set; }
    public int FineNo { get; set; }
    public decimal Amount { get; set; }
    public FeeTransaction? FeeTransaction { get; set; }
}

public sealed class FeeTransactionDiscount : AuditableEntity
{
    public Guid FeeTransactionId { get; set; }
    public string? Reason { get; set; }
    public decimal Amount { get; set; }
    public FeeTransaction? FeeTransaction { get; set; }
}

public sealed class FeeTransactionAdjustment : AuditableEntity
{
    public Guid FeeTransactionId { get; set; }
    public string? Reason { get; set; }
    public decimal Amount { get; set; }
    public FeeTransaction? FeeTransaction { get; set; }
}

public sealed class Refund : AuditableEntity
{
    public string StudentId { get; set; } = "";
    public Guid? FeeTransactionId { get; set; }
    public DateTimeOffset RefundDate { get; set; }
    public decimal Amount { get; set; }
    public RefundStatus Status { get; set; } = RefundStatus.Pending;
    public string? Reason { get; set; }
    public string? Reference { get; set; }
    public FeeTransaction? FeeTransaction { get; set; }
}

public sealed class SuspenseAccountTransaction : AuditableEntity
{
    public DateTimeOffset TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string? PaymentMode { get; set; }
    public string? Particulars { get; set; }
    public SuspenseStatus Status { get; set; } = SuspenseStatus.Unidentified;
    public List<SuspenseAccountTransactionAllocation> Allocations { get; set; } = [];
}

public sealed class SuspenseAccountTransactionAllocation : AuditableEntity
{
    public Guid SuspenseAccountTransactionId { get; set; }
    public Guid FeeTransactionId { get; set; }
    public decimal AllocatedAmount { get; set; }
    public DateTimeOffset AllocatedOn { get; set; }
    public string AllocatedBy { get; set; } = "";
    public SuspenseAccountTransaction? SuspenseAccountTransaction { get; set; }
    public FeeTransaction? FeeTransaction { get; set; }
}
