using Ct.Accounting.Domain;
using Ct.Accounting.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Accounting.Application;

public sealed class LedgerService(AccountingDbContext db, IAccountingUserContext user)
{
    public async Task<object> CreateAsync(string name, NatureOfAccounts nature, Guid? groupId, decimal opening, CancellationToken ct = default)
    {
        var a = new LedgerAccount
        {
            OwnerId = user.OwnerId,
            Name = name.Trim(),
            Nature = nature,
            GroupId = groupId,
            OpeningBalance = opening,
            CreatedBy = user.UserId
        };
        db.Ledgers.Add(a);
        await db.SaveChangesAsync(ct);
        return Map(a);
    }

    public async Task<object> AssetsAsync(CreateLedgerRequest req, CancellationToken ct = default)
        => await CreateAsync(req.Name, NatureOfAccounts.Assets, req.GroupId, req.OpeningBalance, ct);

    public async Task<object> LiabilitiesAsync(CreateLedgerRequest req, CancellationToken ct = default)
        => await CreateAsync(req.Name, NatureOfAccounts.Liabilities, req.GroupId, req.OpeningBalance, ct);

    public async Task<object> ChartOfAccountsAsync(CancellationToken ct = default)
    {
        var list = await db.Ledgers.AsNoTracking()
            .Where(l => l.OwnerId == user.OwnerId && l.Status == LedgerStatus.Active)
            .OrderBy(l => l.Nature).ThenBy(l => l.Name)
            .ToListAsync(ct);
        return list.GroupBy(l => l.Nature.ToString())
            .Select(g => new { nature = g.Key, accounts = g.Select(Map) });
    }

    public async Task<IReadOnlyList<object>> GetVoucherTargetAccountsAsync(CancellationToken ct = default)
        => await db.Ledgers.AsNoTracking()
            .Where(l => l.OwnerId == user.OwnerId && l.Status == LedgerStatus.Active
                        && (l.Nature == NatureOfAccounts.Expenses || l.Nature == NatureOfAccounts.Assets))
            .Select(l => Map(l)).ToListAsync(ct);

    public async Task<IReadOnlyList<object>> GetVoucherSourceAccountsAsync(CancellationToken ct = default)
        => await db.Ledgers.AsNoTracking()
            .Where(l => l.OwnerId == user.OwnerId && l.Status == LedgerStatus.Active
                        && (l.Nature == NatureOfAccounts.Income || l.Nature == NatureOfAccounts.Liabilities || l.Nature == NatureOfAccounts.Assets))
            .Select(l => Map(l)).ToListAsync(ct);

    private static object Map(LedgerAccount a) => new
    {
        a.Id, a.Name, Type = a.Type.ToString(), Nature = a.Nature.ToString(),
        Status = a.Status.ToString(), a.GroupId, a.OpeningBalance, a.CreatedOn
    };
}

public sealed class CreateLedgerRequest
{
    public required string Name { get; set; }
    public Guid? GroupId { get; set; }
    public decimal OpeningBalance { get; set; }
}

public sealed class VoucherService(AccountingDbContext db, IAccountingUserContext user)
{
    public async Task<object> CreateAsync(CreateVoucherRequest req, CancellationToken ct = default)
    {
        var lines = req.Lines ?? [];
        var by = lines.Where(l => l.IsDebit).Sum(l => l.Amount);
        var to = lines.Where(l => !l.IsDebit).Sum(l => l.Amount);
        if (lines.Count > 0 && by != to)
            throw new InvalidOperationException($"Voucher not balanced: By={by} To={to}");

        var v = new Voucher
        {
            OwnerId = user.OwnerId,
            VoucherNo = string.IsNullOrWhiteSpace(req.VoucherNo)
                ? $"V-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : req.VoucherNo.Trim(),
            RefNo = req.RefNo,
            Description = req.Description,
            Type = Enum.TryParse<VoucherType>(req.Type, true, out var t) ? t : VoucherType.Journal,
            VoucherDate = req.VoucherDate ?? DateTime.UtcNow,
            Lines = lines,
            ByTotal = by,
            ToTotal = to,
            CreatedBy = user.UserId
        };
        db.Vouchers.Add(v);
        await db.SaveChangesAsync(ct);
        return Map(v);
    }

    public async Task<IReadOnlyList<object>> GetAllAsync(CancellationToken ct = default)
        => await db.Vouchers.AsNoTracking()
            .Where(v => v.OwnerId == user.OwnerId && v.Status == VoucherStatus.Active)
            .OrderByDescending(v => v.VoucherDate)
            .Select(v => Map(v))
            .ToListAsync(ct);

    public async Task<bool> EnableAsync(Guid id, CancellationToken ct = default)
    {
        var v = await Load(id, ct); if (v is null) return false;
        v.Status = VoucherStatus.Active; await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> DisableAsync(Guid id, CancellationToken ct = default)
    {
        var v = await Load(id, ct); if (v is null) return false;
        v.Status = VoucherStatus.Disabled; await db.SaveChangesAsync(ct); return true;
    }

    private async Task<Voucher?> Load(Guid id, CancellationToken ct)
        => await db.Vouchers.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);

    private static object Map(Voucher v) => new
    {
        v.Id, v.VoucherNo, v.RefNo, v.Description, Type = v.Type.ToString(),
        Status = v.Status.ToString(), v.VoucherDate, v.Lines, v.ByTotal, v.ToTotal, v.CreatedOn
    };
}

public sealed class CreateVoucherRequest
{
    public string? VoucherNo { get; set; }
    public string? RefNo { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public DateTime? VoucherDate { get; set; }
    public List<VoucherLine>? Lines { get; set; }
}

public sealed class InventoryService(AccountingDbContext db, IAccountingUserContext user)
{
    public async Task<object> CreateItemAsync(CreateInventoryItemRequest req, CancellationToken ct = default)
    {
        var item = new InventoryItem
        {
            OwnerId = user.OwnerId,
            Sku = req.Sku?.Trim() ?? Guid.NewGuid().ToString("N")[..8],
            Name = req.Name.Trim(),
            Unit = req.Unit,
            QtyOnHand = req.Qty,
            UnitCost = req.UnitCost,
            Location = req.Location
        };
        db.InventoryItems.Add(item);
        await db.SaveChangesAsync(ct);
        return Map(item);
    }

    public async Task<object> JournalAsync(InventoryJournalRequest req, CancellationToken ct = default)
    {
        InventoryItem? item = null;
        if (req.ItemId is not null)
            item = await db.InventoryItems.FirstOrDefaultAsync(i => i.Id == req.ItemId && i.OwnerId == user.OwnerId, ct);
        else if (!string.IsNullOrWhiteSpace(req.Sku))
            item = await db.InventoryItems.FirstOrDefaultAsync(i => i.Sku == req.Sku && i.OwnerId == user.OwnerId, ct);

        var txType = Enum.TryParse<InventoryTxType>(req.TxType, true, out var t) ? t : InventoryTxType.In;
        var j = new InventoryJournal
        {
            OwnerId = user.OwnerId,
            ItemId = item?.Id,
            Sku = item?.Sku ?? req.Sku,
            TxType = txType,
            Qty = req.Qty,
            UnitCost = req.UnitCost,
            Ref = req.Ref,
            TxDate = req.TxDate ?? DateTime.UtcNow,
            CreatedBy = user.UserId
        };
        if (item is not null)
        {
            if (txType == InventoryTxType.In) item.QtyOnHand += req.Qty;
            else if (txType == InventoryTxType.Out) item.QtyOnHand -= req.Qty;
            else item.QtyOnHand = req.Qty; // adjust absolute
            if (req.UnitCost is not null) item.UnitCost = req.UnitCost.Value;
        }
        db.InventoryJournals.Add(j);
        await db.SaveChangesAsync(ct);
        return new { journal = j, item = item is null ? null : Map(item) };
    }

    public async Task<object> PurchaseAsync(List<InventoryJournalRequest> items, CancellationToken ct = default)
    {
        var results = new List<object>();
        foreach (var i in items ?? [])
        {
            i.TxType = "In";
            results.Add(await JournalAsync(i, ct));
        }
        return new { count = results.Count, results };
    }

    public async Task<object> SalesAsync(List<InventoryJournalRequest> items, CancellationToken ct = default)
    {
        var results = new List<object>();
        foreach (var i in items ?? [])
        {
            i.TxType = "Out";
            results.Add(await JournalAsync(i, ct));
        }
        return new { count = results.Count, results };
    }

    private static object Map(InventoryItem i) => new
    {
        i.Id, i.Sku, i.Name, i.Unit, i.QtyOnHand, i.UnitCost, i.Location, i.CreatedOn
    };
}

public sealed class CreateInventoryItemRequest
{
    public string? Sku { get; set; }
    public required string Name { get; set; }
    public string? Unit { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitCost { get; set; }
    public string? Location { get; set; }
}

public sealed class InventoryJournalRequest
{
    public Guid? ItemId { get; set; }
    public string? Sku { get; set; }
    public string? TxType { get; set; }
    public decimal Qty { get; set; }
    public decimal? UnitCost { get; set; }
    public string? Ref { get; set; }
    public DateTime? TxDate { get; set; }
}

public sealed class SmsCreditService(AccountingDbContext db, IAccountingUserContext user)
{
    public async Task<object> AddCreditsAsync(int credits, string? description, CancellationToken ct = default)
    {
        var tx = new SmsCreditTx
        {
            OwnerId = user.OwnerId,
            Credits = credits,
            Description = description,
            CreatedBy = user.UserId
        };
        db.SmsCreditTxs.Add(tx);
        await db.SaveChangesAsync(ct);
        return tx;
    }

    public async Task<IReadOnlyList<object>> GetTxAsync(CancellationToken ct = default)
        => await db.SmsCreditTxs.AsNoTracking()
            .Where(t => t.OwnerId == user.OwnerId)
            .OrderByDescending(t => t.TxDate)
            .Select(t => (object)new { t.Id, t.Credits, t.Description, t.TxDate })
            .ToListAsync(ct);
}

public sealed class AccountingReportService(AccountingDbContext db, IAccountingUserContext user)
{
    public async Task<object> DatewiseExpenseAsync(DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var q = db.Vouchers.AsNoTracking().Where(v => v.OwnerId == user.OwnerId && v.Status == VoucherStatus.Active);
        if (from is not null) q = q.Where(v => v.VoucherDate >= from);
        if (to is not null) q = q.Where(v => v.VoucherDate <= to);
        var list = await q.ToListAsync(ct);
        // expense lines = debit to expense nature - approximate via description or all debit lines
        var rows = list.SelectMany(v => v.Lines.Where(l => l.IsDebit).Select(l => new { v.VoucherDate.Date, l.Amount, v.VoucherNo }))
            .GroupBy(x => x.Date)
            .Select(g => new { date = g.Key, total = g.Sum(x => x.Amount), count = g.Count() })
            .OrderBy(x => x.date);
        return rows;
    }

    public async Task<object> CategorywiseExpenseAsync(CancellationToken ct = default)
    {
        var list = await db.Vouchers.AsNoTracking()
            .Where(v => v.OwnerId == user.OwnerId && v.Status == VoucherStatus.Active)
            .ToListAsync(ct);
        var rows = list.SelectMany(v => v.Lines.Where(l => l.IsDebit)
                .Select(l => new { name = l.LedgerName ?? l.LedgerId.ToString(), l.Amount }))
            .GroupBy(x => x.name)
            .Select(g => new { category = g.Key, total = g.Sum(x => x.Amount) });
        return rows;
    }

    public async Task<object> YearwiseExpenseAsync(CancellationToken ct = default)
    {
        var list = await db.Vouchers.AsNoTracking()
            .Where(v => v.OwnerId == user.OwnerId && v.Status == VoucherStatus.Active)
            .ToListAsync(ct);
        return list.SelectMany(v => v.Lines.Where(l => l.IsDebit).Select(l => new { v.VoucherDate.Year, l.Amount }))
            .GroupBy(x => x.Year)
            .Select(g => new { year = g.Key, total = g.Sum(x => x.Amount) })
            .OrderBy(x => x.year);
    }

    public async Task<object> SmsCreditsTxReportAsync(CancellationToken ct = default)
        => await db.SmsCreditTxs.AsNoTracking()
            .Where(t => t.OwnerId == user.OwnerId)
            .OrderByDescending(t => t.TxDate)
            .ToListAsync(ct);

    public async Task<object> TallyXmlAsync(CancellationToken ct = default)
    {
        var vouchers = await db.Vouchers.AsNoTracking()
            .Where(v => v.OwnerId == user.OwnerId && v.Status == VoucherStatus.Active)
            .OrderBy(v => v.VoucherDate)
            .Take(500)
            .ToListAsync(ct);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\"?>");
        sb.AppendLine("<ENVELOPE><BODY><IMPORTDATA><REQUESTDATA>");
        foreach (var v in vouchers)
        {
            sb.AppendLine($"<TALLYMESSAGE><VOUCHER VCHTYPE=\"{v.Type}\" ACTION=\"Create\">");
            sb.AppendLine($"<DATE>{v.VoucherDate:yyyyMMdd}</DATE><VOUCHERNUMBER>{System.Security.SecurityElement.Escape(v.VoucherNo)}</VOUCHERNUMBER>");
            sb.AppendLine($"<NARRATION>{System.Security.SecurityElement.Escape(v.Description ?? "")}</NARRATION>");
            foreach (var line in v.Lines)
            {
                var tag = line.IsDebit ? "DEBIT" : "CREDIT";
                sb.AppendLine($"<ALLLEDGERENTRIES.LIST><LEDGERNAME>{System.Security.SecurityElement.Escape(line.LedgerName ?? line.LedgerId.ToString())}</LEDGERNAME>");
                sb.AppendLine($"<ISDEEMEDPOSITIVE>{(line.IsDebit ? "Yes" : "No")}</ISDEEMEDPOSITIVE><AMOUNT>{line.Amount}</AMOUNT></ALLLEDGERENTRIES.LIST>");
            }
            sb.AppendLine("</VOUCHER></TALLYMESSAGE>");
        }
        sb.AppendLine("</REQUESTDATA></IMPORTDATA></BODY></ENVELOPE>");
        return new { xml = sb.ToString(), count = vouchers.Count };
    }
}

// Cmd/Qry facades
public sealed class AccountingCmd(LedgerService ledger, VoucherService voucher, InventoryService inv, SmsCreditService sms)
{
    public Task<object> Assets(CreateLedgerRequest r, CancellationToken ct = default) => ledger.AssetsAsync(r, ct);
    public Task<object> Liabilities(CreateLedgerRequest r, CancellationToken ct = default) => ledger.LiabilitiesAsync(r, ct);
    public Task<object> NewVoucher(CreateVoucherRequest r, CancellationToken ct = default) => voucher.CreateAsync(r, ct);
    public Task<bool> EnableVoucher(Guid id, CancellationToken ct = default) => voucher.EnableAsync(id, ct);
    public Task<bool> DisableVoucher(Guid id, CancellationToken ct = default) => voucher.DisableAsync(id, ct);
    public Task<object> InventoryJournal(InventoryJournalRequest r, CancellationToken ct = default) => inv.JournalAsync(r, ct);
    public Task<object> Purchase(List<InventoryJournalRequest> items, CancellationToken ct = default) => inv.PurchaseAsync(items, ct);
    public Task<object> Sales(List<InventoryJournalRequest> items, CancellationToken ct = default) => inv.SalesAsync(items, ct);
    public Task<object> AddSmsCredits(int credits, string? desc, CancellationToken ct = default) => sms.AddCreditsAsync(credits, desc, ct);
}

public sealed class AccountingQry(LedgerService ledger, VoucherService voucher, SmsCreditService sms)
{
    public Task<object> ChartOfAccounts(CancellationToken ct = default) => ledger.ChartOfAccountsAsync(ct);
    public Task<IReadOnlyList<object>> GetVoucherTargetAccounts(CancellationToken ct = default) => ledger.GetVoucherTargetAccountsAsync(ct);
    public Task<IReadOnlyList<object>> GetVoucherSourceAccounts(CancellationToken ct = default) => ledger.GetVoucherSourceAccountsAsync(ct);
    public Task<IReadOnlyList<object>> GetVouchers(CancellationToken ct = default) => voucher.GetAllAsync(ct);
    public Task<IReadOnlyList<object>> GetSmsTx(CancellationToken ct = default) => sms.GetTxAsync(ct);
}
