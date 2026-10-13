namespace Ct.Gr.Domain;

public enum ReceiptStatus { Active = 1, Cancelled = 99 }
public enum PaymentMode { Cash = 10, Cheque = 20, DD = 30, Netbanking = 40, UPI = 50 }
public enum ReceiptType { Unknown = 0, Regular = 10, Donation = 20 }
public enum CustomerType { Unknown = 0, Regular = 10, Student = 20 }

public class Receipt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Number { get; set; } = "";
    public string InstId { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public Customer Customer { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public string? ReceivedBy { get; set; }
    public PaymentMode PaymentMode { get; set; } = PaymentMode.Cash;
    public FinancialInstrument? FinancialInstrument { get; set; }
    public ReceiptStatus Status { get; set; } = ReceiptStatus.Active;
    public ReceiptType ReceiptType { get; set; } = ReceiptType.Regular;
    public ReceiptMeta Meta { get; set; } = new();
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public bool Printed { get; set; }
    public DateTime? PrintedOn { get; set; }
    /// <summary>Cached print HTML (original receipt.HTML).</summary>
    public string? Html { get; set; }
    public bool Audited { get; set; }
    public DateTime? AuditedOn { get; set; }
    public string? AuditedBy { get; set; }
    public bool RegulatoryCompliant { get; set; }
    public string? RegulatoryNotes { get; set; }

    public void RecalcTotal() => TotalAmount = OrderItems.Sum(i => i.Amount);

    public int NumberSequence()
    {
        if (string.IsNullOrEmpty(Number)) return 0;
        var arr = Number.Split('/');
        return arr.Length == 2 && int.TryParse(arr[0], out var n) ? n : 0;
    }
}

public class OrderItem
{
    public string Particular { get; set; } = "";
    public Dictionary<string, string> Extras { get; set; } = new();
    public decimal Amount { get; set; }
}

public class Customer
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? Contact { get; set; }
    public string? PAN { get; set; }
    public string? StudyingIn { get; set; }
    public string? Section { get; set; }
    public CustomerType CustomerType { get; set; } = CustomerType.Regular;
}

public class FinancialInstrument
{
    public string Number { get; set; } = "000000";
    public DateTime? DrawnOn { get; set; }
    public string? BankName { get; set; }
    public string? BankBranch { get; set; }
}

public class ReceiptMeta
{
    public bool Include80G { get; set; }
}
