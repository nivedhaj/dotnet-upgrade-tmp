namespace Ct.Accounting.Domain;

public enum LedgerType { Ledger = 1, Group = 2 }
public enum LedgerStatus { Active = 1, Disabled = 99 }
public enum NatureOfAccounts { Inherit = 0, Assets = 10, Liabilities = 20, Income = 30, Expenses = 40 }
public enum VoucherStatus { Active = 1, Disabled = 99 }
public enum VoucherType { Payment = 1, Receipt = 2, Journal = 3, Contra = 4, Sales = 5, Purchase = 6 }
public enum InventoryTxType { In = 1, Out = 2, Adjust = 3 }

public class LedgerAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Name { get; set; } = "";
    public Guid? GroupId { get; set; }
    public LedgerType Type { get; set; } = LedgerType.Ledger;
    public NatureOfAccounts Nature { get; set; } = NatureOfAccounts.Assets;
    public LedgerStatus Status { get; set; } = LedgerStatus.Active;
    public decimal OpeningBalance { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class VoucherLine
{
    public Guid LedgerId { get; set; }
    public string? LedgerName { get; set; }
    public decimal Amount { get; set; }
    public bool IsDebit { get; set; } // true = By/Debit, false = To/Credit
}

public class Voucher
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string VoucherNo { get; set; } = "";
    public string? RefNo { get; set; }
    public string? Description { get; set; }
    public VoucherType Type { get; set; } = VoucherType.Journal;
    public VoucherStatus Status { get; set; } = VoucherStatus.Active;
    public DateTime VoucherDate { get; set; } = DateTime.UtcNow;
    public List<VoucherLine> Lines { get; set; } = [];
    public decimal ByTotal { get; set; }
    public decimal ToTotal { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class InventoryItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Unit { get; set; }
    public decimal QtyOnHand { get; set; }
    public decimal UnitCost { get; set; }
    public string? Location { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class InventoryJournal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public Guid? ItemId { get; set; }
    public string? Sku { get; set; }
    public InventoryTxType TxType { get; set; }
    public decimal Qty { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Ref { get; set; }
    public DateTime TxDate { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}

public class SmsCreditTx
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public int Credits { get; set; }
    public string? Description { get; set; }
    public DateTime TxDate { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}
