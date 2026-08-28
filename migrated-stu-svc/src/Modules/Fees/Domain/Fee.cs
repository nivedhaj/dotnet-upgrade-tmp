namespace Ct.Modules.Fees.Domain;

public sealed class Fee
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Amount { get; set; }
}

public sealed class FeeTransaction
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string TxNo { get; set; } = "";
    public DateTime TxDate { get; set; }
    public decimal Amount { get; set; }
    public string? Status { get; set; }
    public string InstallmentsPaid { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Ct.Modules.Academics.Domain.Student? Student { get; set; }
}

public sealed class StudentFeeSummary
{
    public Guid StudentPk { get; set; }
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public string? CourseName { get; set; }
    public string? TermName { get; set; }
    public string? SectionName { get; set; }
    public Guid? FeeId { get; set; }
    public string? FeeName { get; set; }
    public decimal? FeeAmount { get; set; }
    public Guid? FeeTransactionId { get; set; }
    public string? TxNo { get; set; }
    public DateTime? TxDate { get; set; }
    public decimal? PaidAmount { get; set; }
    public string? TransactionStatus { get; set; }
}
