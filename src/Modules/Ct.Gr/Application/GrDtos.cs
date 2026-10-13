using Ct.Gr.Domain;

namespace Ct.Gr.Application;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public sealed class ReceiptDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public string InstId { get; set; } = "";
    public DateTime Date { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerContact { get; set; }
    public string? CustomerPan { get; set; }
    public string CustomerType { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string? ReceivedBy { get; set; }
    public string PaymentMode { get; set; } = "";
    public string Status { get; set; } = "";
    public string ReceiptType { get; set; } = "";
    public bool Printed { get; set; }
    public bool Include80G { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = [];
    public FinancialInstrumentDto? FinancialInstrument { get; set; }
    public string? Html { get; set; }
    public bool Audited { get; set; }
    public bool RegulatoryCompliant { get; set; }
}

public sealed class OrderItemDto
{
    public string Particular { get; set; } = "";
    public decimal Amount { get; set; }
    public Dictionary<string, string>? Extras { get; set; }
}

public sealed class FinancialInstrumentDto
{
    public string? Number { get; set; }
    public DateTime? DrawnOn { get; set; }
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
}

public sealed class CreateReceiptRequest
{
    public DateTime? Date { get; set; }
    public string? Number { get; set; }
    public CustomerDto? Customer { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = [];
    public string PaymentMode { get; set; } = "Cash";
    public FinancialInstrumentDto? FinancialInstrument { get; set; }
    public string ReceiptType { get; set; } = "Regular";
    public string? ReceivedBy { get; set; }
    public bool Include80G { get; set; }
}

public sealed class CustomerDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Contact { get; set; }
    public string? PAN { get; set; }
    public string? StudyingIn { get; set; }
    public string? Section { get; set; }
    public string CustomerType { get; set; } = "Regular";
}

public sealed class UpdateReceiptRequest
{
    public required Guid Id { get; set; }
    public DateTime? Date { get; set; }
    public CustomerDto? Customer { get; set; }
    public string? ReceivedBy { get; set; }
    public string? ReceiptType { get; set; }
    public bool? Include80G { get; set; }
}

public sealed class UpdatePaymentDetailsRequest
{
    public required Guid Id { get; set; }
    public string PaymentMode { get; set; } = "Cash";
    public FinancialInstrumentDto? FinancialInstrument { get; set; }
}

public sealed class UpdateOrderItemRequest
{
    public required Guid Id { get; set; }
    public int ItemIndex { get; set; }
    public string? Particular { get; set; }
    public decimal? Amount { get; set; }
    public Dictionary<string, string>? Extras { get; set; }
}

public sealed class IdRequest
{
    public required Guid Id { get; set; }
}

public sealed class ReportFilterRequest
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Status { get; set; }
    public string? ReceiptType { get; set; }
    public string? PaymentMode { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class ReceiptSummaryDto
{
    public long Count { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal CancelledAmount { get; set; }
    public long CancelledCount { get; set; }
}

public sealed class MonthwiseCollectionDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Amount { get; set; }
    public long Count { get; set; }
}

public sealed class RevenueShareRowDto
{
    public string Particular { get; set; } = "";
    public decimal Amount { get; set; }
    public long Count { get; set; }
}

public sealed class ReceiptPrintDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public DateTime Date { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerPan { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentMode { get; set; } = "";
    public string? ReceivedBy { get; set; }
    public bool Include80G { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = [];
    public string Html { get; set; } = "";
}

public sealed class RegulatoryComplianceRequest
{
    public required Guid Id { get; set; }
    public bool Compliant { get; set; }
    public string? Notes { get; set; }
}

public sealed class MonthlyReceiptGroupDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public long Count { get; set; }
    public List<ReceiptDto> Receipts { get; set; } = [];
}

public sealed class RevenueByClassDto
{
    public string ClassName { get; set; } = "";
    public string? Section { get; set; }
    public decimal Amount { get; set; }
    public long Count { get; set; }
}

public sealed class RevenueByPersonDto
{
    public string? Name { get; set; }
    public string? Contact { get; set; }
    public string? Pan { get; set; }
    public decimal Amount { get; set; }
    public long Count { get; set; }
}

public sealed class InstituteBranding
{
    public string Name { get; set; } = "Institute";
    public string ShortName { get; set; } = "INST";
    public string? LogoUrl { get; set; }
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? EightyGText { get; set; }
}
