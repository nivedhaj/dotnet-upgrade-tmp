using Ct.Gr.Domain;
using Ct.Gr.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Gr.Application;

/// <summary>Postgres port of ReceiptCmd + ReceiptQry + ReceiptService.</summary>
public sealed class ReceiptService(
    GrDbContext db,
    IGrUserContext user,
    IReceiptPrintService printer,
    IGrPolicy policy,
    IGrEventPublisher events)
{
    public async Task<PagedResult<ReceiptDto>> GetAsync(
        DateTime? from, DateTime? to, string? status, int page, int pageSize, string sort = "desc", CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var q = db.Receipts.AsNoTracking().Where(r => r.OwnerId == user.OwnerId);
        if (from is not null) q = q.Where(r => r.Date >= from);
        if (to is not null) q = q.Where(r => r.Date <= to);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ReceiptStatus>(status, true, out var st))
            q = q.Where(r => r.Status == st);

        var total = await q.CountAsync(ct);
        q = sort.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? q.OrderBy(r => r.Date).ThenBy(r => r.Number)
            : q.OrderByDescending(r => r.Date).ThenByDescending(r => r.Number);

        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<ReceiptDto>(items.Select(Map).ToList(), total, page, pageSize);
    }

    public async Task<ReceiptDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await db.Receipts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return r is null ? null : Map(r);
    }

    public async Task<ReceiptDto> CreateAsync(CreateReceiptRequest req, CancellationToken ct = default)
    {
        var receipt = new Receipt
        {
            OwnerId = user.OwnerId,
            InstId = user.OwnerId,
            Date = (req.Date ?? DateTime.UtcNow).Date,
            ReceivedBy = req.ReceivedBy ?? user.UserId,
            PaymentMode = ParseEnum(req.PaymentMode, PaymentMode.Cash),
            ReceiptType = ParseEnum(req.ReceiptType, ReceiptType.Regular),
            Status = ReceiptStatus.Active,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow,
            Meta = new ReceiptMeta { Include80G = req.Include80G }
        };

        if (req.Customer is not null)
        {
            receipt.Customer = new Customer
            {
                Name = req.Customer.Name,
                Address = req.Customer.Address,
                Contact = req.Customer.Contact,
                PAN = req.Customer.PAN,
                StudyingIn = req.Customer.StudyingIn,
                Section = req.Customer.Section,
                CustomerType = ParseEnum(req.Customer.CustomerType, CustomerType.Regular)
            };
        }

        receipt.OrderItems = (req.OrderItems ?? []).Select(i => new OrderItem
        {
            Particular = i.Particular,
            Amount = i.Amount,
            Extras = i.Extras ?? new()
        }).ToList();
        receipt.RecalcTotal();

        if (req.FinancialInstrument is not null && receipt.PaymentMode != PaymentMode.Cash)
        {
            receipt.FinancialInstrument = new FinancialInstrument
            {
                Number = req.FinancialInstrument.Number ?? "000000",
                DrawnOn = req.FinancialInstrument.DrawnOn,
                BankName = req.FinancialInstrument.BankName,
                BankBranch = req.FinancialInstrument.BankBranch
            };
        }

        receipt.Number = string.IsNullOrWhiteSpace(req.Number)
            ? await NextNumberAsync(ct)
            : req.Number.Trim();

        if (!req.Include80G && policy.DefaultInclude80G(req.ReceiptType))
            receipt.Meta.Include80G = true;

        receipt.Html = printer.Render(receipt, policy.GetBranding());
        db.Receipts.Add(receipt);
        await db.SaveChangesAsync(ct);
        await events.PublishReceiptCreatedAsync(receipt.Id, receipt.Number, ct);
        return Map(receipt);
    }

    public async Task<ReceiptDto?> UpdateReceiptAsync(UpdateReceiptRequest req, CancellationToken ct = default)
    {
        var r = await LoadTrackedAsync(req.Id, ct);
        if (r is null) return null;
        if (req.Date is not null) r.Date = req.Date.Value.Date;
        if (req.ReceivedBy is not null) r.ReceivedBy = req.ReceivedBy;
        if (req.ReceiptType is not null) r.ReceiptType = ParseEnum(req.ReceiptType, r.ReceiptType);
        if (req.Include80G is not null) r.Meta.Include80G = req.Include80G.Value;
        if (req.Customer is not null)
        {
            r.Customer.Name = req.Customer.Name ?? r.Customer.Name;
            r.Customer.Address = req.Customer.Address ?? r.Customer.Address;
            r.Customer.Contact = req.Customer.Contact ?? r.Customer.Contact;
            r.Customer.PAN = req.Customer.PAN ?? r.Customer.PAN;
            r.Customer.StudyingIn = req.Customer.StudyingIn ?? r.Customer.StudyingIn;
            r.Customer.Section = req.Customer.Section ?? r.Customer.Section;
            if (!string.IsNullOrWhiteSpace(req.Customer.CustomerType))
                r.Customer.CustomerType = ParseEnum(req.Customer.CustomerType, r.Customer.CustomerType);
        }
        Stamp(r);
        RefreshHtml(r);
        await db.SaveChangesAsync(ct);
        await NotifyUpdatedAsync(r, "update", ct);
        return Map(r);
    }

    public async Task<ReceiptDto?> UpdatePaymentDetailsAsync(UpdatePaymentDetailsRequest req, CancellationToken ct = default)
    {
        var r = await LoadTrackedAsync(req.Id, ct);
        if (r is null) return null;
        r.PaymentMode = ParseEnum(req.PaymentMode, r.PaymentMode);
        if (req.FinancialInstrument is not null)
        {
            r.FinancialInstrument ??= new FinancialInstrument();
            r.FinancialInstrument.Number = req.FinancialInstrument.Number ?? r.FinancialInstrument.Number;
            r.FinancialInstrument.DrawnOn = req.FinancialInstrument.DrawnOn ?? r.FinancialInstrument.DrawnOn;
            r.FinancialInstrument.BankName = req.FinancialInstrument.BankName ?? r.FinancialInstrument.BankName;
            r.FinancialInstrument.BankBranch = req.FinancialInstrument.BankBranch ?? r.FinancialInstrument.BankBranch;
        }
        Stamp(r);
        RefreshHtml(r);
        await db.SaveChangesAsync(ct);
        await NotifyUpdatedAsync(r, "update", ct);
        return Map(r);
    }

    public async Task<ReceiptDto?> UpdateOrderItemParticularAsync(UpdateOrderItemRequest req, CancellationToken ct = default)
    {
        var r = await LoadTrackedAsync(req.Id, ct);
        if (r is null || req.ItemIndex < 0 || req.ItemIndex >= r.OrderItems.Count) return null;
        if (req.Particular is not null) r.OrderItems[req.ItemIndex].Particular = req.Particular;
        // force EF jsonb change detection
        r.OrderItems = r.OrderItems.ToList();
        Stamp(r);
        RefreshHtml(r);
        await db.SaveChangesAsync(ct);
        await NotifyUpdatedAsync(r, "update", ct);
        return Map(r);
    }

    public async Task<ReceiptDto?> UpdateOrderItemAmountAsync(UpdateOrderItemRequest req, CancellationToken ct = default)
    {
        var r = await LoadTrackedAsync(req.Id, ct);
        if (r is null || req.ItemIndex < 0 || req.ItemIndex >= r.OrderItems.Count) return null;
        if (req.Amount is not null) r.OrderItems[req.ItemIndex].Amount = req.Amount.Value;
        r.OrderItems = r.OrderItems.ToList();
        r.RecalcTotal();
        Stamp(r);
        RefreshHtml(r);
        await db.SaveChangesAsync(ct);
        await NotifyUpdatedAsync(r, "update", ct);
        return Map(r);
    }

    public async Task<ReceiptDto?> UpdateOrderItemExtrasAsync(UpdateOrderItemRequest req, CancellationToken ct = default)
    {
        var r = await LoadTrackedAsync(req.Id, ct);
        if (r is null || req.ItemIndex < 0 || req.ItemIndex >= r.OrderItems.Count) return null;
        if (req.Extras is not null) r.OrderItems[req.ItemIndex].Extras = req.Extras;
        r.OrderItems = r.OrderItems.ToList();
        Stamp(r);
        RefreshHtml(r);
        await db.SaveChangesAsync(ct);
        await NotifyUpdatedAsync(r, "update", ct);
        return Map(r);
    }

    public async Task<bool> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var r = await LoadTrackedAsync(id, ct);
        if (r is null) return false;
        r.Status = ReceiptStatus.Cancelled;
        Stamp(r);
        RefreshHtml(r);
        await db.SaveChangesAsync(ct);
        await NotifyUpdatedAsync(r, "cancel", ct);
        return true;
    }

    public async Task<bool> ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var r = await LoadTrackedAsync(id, ct);
        if (r is null) return false;
        r.Status = ReceiptStatus.Active;
        Stamp(r);
        RefreshHtml(r);
        await db.SaveChangesAsync(ct);
        await NotifyUpdatedAsync(r, "activate", ct);
        return true;
    }

    public async Task<ReceiptPrintDto?> PrintAsync(Guid id, CancellationToken ct = default)
    {
        var r = await LoadTrackedAsync(id, ct);
        if (r is null) return null;
        r.Printed = true;
        r.PrintedOn = DateTime.UtcNow;
        Stamp(r);
        RefreshHtml(r);
        await db.SaveChangesAsync(ct);
        await events.PublishReceiptPrintedAsync(r.Id, r.Number, ct);
        return ToPrint(r);
    }

    public async Task<bool> AuditAsync(Guid id, CancellationToken ct = default)
    {
        var r = await LoadTrackedAsync(id, ct);
        if (r is null) return false;
        r.Audited = true;
        r.AuditedOn = DateTime.UtcNow;
        r.AuditedBy = user.UserId;
        Stamp(r);
        RefreshHtml(r);
        await db.SaveChangesAsync(ct);
        await NotifyUpdatedAsync(r, "audit", ct);
        return true;
    }

    public async Task<bool> SetRegulatoryComplianceAsync(Guid id, bool compliant, string? notes, CancellationToken ct = default)
    {
        var r = await LoadTrackedAsync(id, ct);
        if (r is null) return false;
        r.RegulatoryCompliant = compliant;
        r.RegulatoryNotes = notes;
        Stamp(r);
        RefreshHtml(r);
        await db.SaveChangesAsync(ct);
        await NotifyUpdatedAsync(r, "regulatory", ct);
        return true;
    }

    public async Task<IReadOnlyList<DateTime>> GetDatesAsync(CancellationToken ct = default)
    {
        return await db.Receipts.AsNoTracking()
            .Where(r => r.OwnerId == user.OwnerId)
            .Select(r => r.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .Take(365)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MonthwiseCollectionDto>> GetMonthwiseCollectionAsync(CancellationToken ct = default)
    {
        var rows = await db.Receipts.AsNoTracking()
            .Where(r => r.OwnerId == user.OwnerId && r.Status == ReceiptStatus.Active)
            .GroupBy(r => new { r.Date.Year, r.Date.Month })
            .Select(g => new MonthwiseCollectionDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Amount = g.Sum(x => x.TotalAmount),
                Count = g.LongCount()
            })
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .ToListAsync(ct);
        return rows;
    }

    public async Task<PagedResult<ReceiptDto>> GetReceiptReportsAsync(ReportFilterRequest filter, CancellationToken ct = default)
        => await GetAsync(filter.From, filter.To, filter.Status, filter.Page, filter.PageSize, "desc", ct);

    public async Task<ReceiptSummaryDto> GetSummaryAsync(ReportFilterRequest filter, CancellationToken ct = default)
    {
        var q = Filter(filter);
        var active = q.Where(r => r.Status == ReceiptStatus.Active);
        var cancelled = q.Where(r => r.Status == ReceiptStatus.Cancelled);
        return new ReceiptSummaryDto
        {
            Count = await active.LongCountAsync(ct),
            TotalAmount = await active.SumAsync(r => (decimal?)r.TotalAmount, ct) ?? 0,
            CancelledCount = await cancelled.LongCountAsync(ct),
            CancelledAmount = await cancelled.SumAsync(r => (decimal?)r.TotalAmount, ct) ?? 0
        };
    }

    public async Task<IReadOnlyList<RevenueShareRowDto>> GetRevenueSharingAsync(ReportFilterRequest filter, CancellationToken ct = default)
    {
        var receipts = await Filter(filter).Where(r => r.Status == ReceiptStatus.Active).ToListAsync(ct);
        return receipts.SelectMany(r => r.OrderItems)
            .GroupBy(i => i.Particular ?? "")
            .Select(g => new RevenueShareRowDto
            {
                Particular = g.Key,
                Amount = g.Sum(x => x.Amount),
                Count = g.LongCount()
            })
            .OrderByDescending(x => x.Amount)
            .ToList();
    }

    public async Task<object> GetConsolidatedReportAsync(ReportFilterRequest filter, CancellationToken ct = default)
    {
        var summary = await GetSummaryAsync(filter, ct);
        var byMode = await Filter(filter).Where(r => r.Status == ReceiptStatus.Active)
            .GroupBy(r => r.PaymentMode)
            .Select(g => new { Mode = g.Key.ToString(), Count = g.LongCount(), Amount = g.Sum(x => x.TotalAmount) })
            .ToListAsync(ct);
        var byType = await Filter(filter).Where(r => r.Status == ReceiptStatus.Active)
            .GroupBy(r => r.ReceiptType)
            .Select(g => new { Type = g.Key.ToString(), Count = g.LongCount(), Amount = g.Sum(x => x.TotalAmount) })
            .ToListAsync(ct);
        return new { summary, byPaymentMode = byMode, byReceiptType = byType };
    }


    public async Task<IReadOnlyList<MonthlyReceiptGroupDto>> GetReceiptReportGroupedAsync(ReportFilterRequest filter, CancellationToken ct = default)
    {
        var list = await Filter(filter).Where(r => r.Status == ReceiptStatus.Active)
            .OrderBy(r => r.Date).ToListAsync(ct);
        return list.GroupBy(r => new { r.Date.Year, r.Date.Month })
            .Select(g => new MonthlyReceiptGroupDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                TotalAmount = g.Sum(x => x.TotalAmount),
                Count = g.LongCount(),
                Receipts = g.Select(Map).ToList()
            }).OrderByDescending(x => x.Year).ThenByDescending(x => x.Month).ToList();
    }

    public async Task<IReadOnlyList<RevenueByClassDto>> GetRevenueByClassAsync(ReportFilterRequest filter, CancellationToken ct = default)
    {
        var list = await Filter(filter).Where(r => r.Status == ReceiptStatus.Active).ToListAsync(ct);
        return list.GroupBy(r => new { ClassName = r.Customer.StudyingIn ?? "Unknown", Section = r.Customer.Section })
            .Select(g => new RevenueByClassDto
            {
                ClassName = g.Key.ClassName,
                Section = g.Key.Section,
                Amount = g.Sum(x => x.TotalAmount),
                Count = g.LongCount()
            }).OrderByDescending(x => x.Amount).ToList();
    }

    public async Task<IReadOnlyList<RevenueByPersonDto>> GetRevenueByPersonAsync(ReportFilterRequest filter, CancellationToken ct = default)
    {
        var list = await Filter(filter).Where(r => r.Status == ReceiptStatus.Active).ToListAsync(ct);
        return list.GroupBy(r => new { r.Customer.Name, r.Customer.Contact, r.Customer.PAN })
            .Select(g => new RevenueByPersonDto
            {
                Name = g.Key.Name,
                Contact = g.Key.Contact,
                Pan = g.Key.PAN,
                Amount = g.Sum(x => x.TotalAmount),
                Count = g.LongCount()
            }).OrderByDescending(x => x.Amount).ToList();
    }

    public async Task<string> ExportCsvAsync(ReportFilterRequest filter, CancellationToken ct = default)
    {
        var page = await GetReceiptReportsAsync(new ReportFilterRequest
        {
            From = filter.From, To = filter.To, Status = filter.Status,
            ReceiptType = filter.ReceiptType, PaymentMode = filter.PaymentMode,
            Page = 1, PageSize = 10000
        }, ct);
        return GrCsv.FromReceipts(page.Items);
    }

    private IQueryable<Receipt> Filter(ReportFilterRequest filter)
    {
        var q = db.Receipts.AsNoTracking().Where(r => r.OwnerId == user.OwnerId);
        if (filter.From is not null) q = q.Where(r => r.Date >= filter.From);
        if (filter.To is not null) q = q.Where(r => r.Date <= filter.To);
        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<ReceiptStatus>(filter.Status, true, out var st))
            q = q.Where(r => r.Status == st);
        if (!string.IsNullOrWhiteSpace(filter.ReceiptType) && Enum.TryParse<ReceiptType>(filter.ReceiptType, true, out var rt))
            q = q.Where(r => r.ReceiptType == rt);
        if (!string.IsNullOrWhiteSpace(filter.PaymentMode) && Enum.TryParse<PaymentMode>(filter.PaymentMode, true, out var pm))
            q = q.Where(r => r.PaymentMode == pm);
        return q;
    }

    private async Task<string> NextNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = "";
        var max = await db.Receipts.AsNoTracking()
            .Where(r => r.OwnerId == user.OwnerId && r.Number.Contains("/"))
            .Select(r => r.Number)
            .ToListAsync(ct);
        var seq = max.Select(n =>
        {
            var parts = n.Split('/');
            return parts.Length >= 1 && int.TryParse(parts[0], out var v) ? v : 0;
        }).DefaultIfEmpty(0).Max() + 1;
        return policy.FormatReceiptNumber(seq, DateTime.UtcNow);
    }

    private async Task<Receipt?> LoadTrackedAsync(Guid id, CancellationToken ct) =>
        await db.Receipts.FirstOrDefaultAsync(r => r.Id == id && r.OwnerId == user.OwnerId, ct);

    private void Stamp(Receipt r)
    {
        r.ModifiedBy = user.UserId;
        r.ModifiedOn = DateTime.UtcNow;
    }

    private void RefreshHtml(Receipt r)
    {
        r.Html = printer.Render(r, policy.GetBranding());
    }

    private async Task NotifyUpdatedAsync(Receipt r, string action, CancellationToken ct)
    {
        try { await events.PublishReceiptUpdatedAsync(r.Id, r.Number, action, ct); }
        catch { /* non-fatal */ }
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct =>
        Enum.TryParse<TEnum>(value, true, out var v) ? v : fallback;

    private static ReceiptDto Map(Receipt r) => new()
    {
        Id = r.Id,
        Number = r.Number,
        InstId = r.InstId,
        Date = r.Date,
        CustomerName = r.Customer.Name,
        CustomerContact = r.Customer.Contact,
        CustomerPan = r.Customer.PAN,
        CustomerType = r.Customer.CustomerType.ToString(),
        TotalAmount = r.TotalAmount,
        ReceivedBy = r.ReceivedBy,
        PaymentMode = r.PaymentMode.ToString(),
        Status = r.Status.ToString(),
        ReceiptType = r.ReceiptType.ToString(),
        Printed = r.Printed,
        Include80G = r.Meta.Include80G,
        OrderItems = r.OrderItems.Select(i => new OrderItemDto
        {
            Particular = i.Particular,
            Amount = i.Amount,
            Extras = i.Extras
        }).ToList(),
        FinancialInstrument = r.FinancialInstrument is null ? null : new FinancialInstrumentDto
        {
            Number = r.FinancialInstrument.Number,
            DrawnOn = r.FinancialInstrument.DrawnOn,
            BankName = r.FinancialInstrument.BankName,
            BankBranch = r.FinancialInstrument.BankBranch
        },
        Html = r.Html,
        Audited = r.Audited,
        RegulatoryCompliant = r.RegulatoryCompliant
    };

    private ReceiptPrintDto ToPrint(Receipt r)
    {
        var html = string.IsNullOrWhiteSpace(r.Html) ? printer.Render(r, policy.GetBranding()) : r.Html!;
        return new ReceiptPrintDto
        {
            Id = r.Id,
            Number = r.Number,
            Date = r.Date,
            CustomerName = r.Customer.Name,
            CustomerAddress = r.Customer.Address,
            CustomerPan = r.Customer.PAN,
            TotalAmount = r.TotalAmount,
            PaymentMode = r.PaymentMode.ToString(),
            ReceivedBy = r.ReceivedBy,
            Include80G = r.Meta.Include80G,
            OrderItems = r.OrderItems.Select(i => new OrderItemDto { Particular = i.Particular, Amount = i.Amount, Extras = i.Extras }).ToList(),
            Html = html
        };
    }
}
