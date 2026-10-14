using Ct.Library.Domain;
using Ct.Library.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Library.Application;

public sealed class MaterialService(LibraryDbContext db, ILibraryUserContext user)
{
    public async Task<IReadOnlyList<object>> GetAllAsync(CancellationToken ct = default)
        => await db.Materials.AsNoTracking()
            .Where(m => m.OwnerId == user.OwnerId && m.Status != MaterialStatus.Disabled)
            .OrderBy(m => m.Title)
            .Select(m => Map(m))
            .ToListAsync(ct);

    public async Task<object?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var m = await db.Materials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return m is null ? null : Map(m);
    }

    public async Task<object> CreateAsync(CreateMaterialRequest req, CancellationToken ct = default)
    {
        var m = new Material
        {
            OwnerId = user.OwnerId,
            TrackingId = string.IsNullOrWhiteSpace(req.TrackingId) ? Guid.NewGuid().ToString("N")[..10].ToUpperInvariant() : req.TrackingId.Trim(),
            ISBN = req.ISBN,
            Title = req.Title.Trim(),
            Author = req.Author,
            Publisher = req.Publisher,
            Location = req.Location,
            Value = req.Value,
            Pages = req.Pages,
            Tags = req.Tags ?? [],
            Attributes = req.Attributes ?? new(),
            CoverUrl = req.CoverUrl,
            Synopsis = req.Synopsis,
            AcquiredOn = req.AcquiredOn ?? DateTime.UtcNow,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        if (req.IsReference) m.Status = MaterialStatus.Reference;
        db.Materials.Add(m);
        await db.SaveChangesAsync(ct);
        return Map(m);
    }

    public async Task<object?> UpdateAsync(Guid id, CreateMaterialRequest req, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return null;
        m.Title = req.Title.Trim();
        if (req.ISBN is not null) m.ISBN = req.ISBN;
        if (req.Author is not null) m.Author = req.Author;
        if (req.Publisher is not null) m.Publisher = req.Publisher;
        if (req.Location is not null) m.Location = req.Location;
        if (req.Value > 0) m.Value = req.Value;
        if (req.Pages > 0) m.Pages = req.Pages;
        if (req.Tags is not null) m.Tags = req.Tags;
        if (req.Attributes is not null) m.Attributes = req.Attributes;
        if (req.CoverUrl is not null) m.CoverUrl = req.CoverUrl;
        if (req.Synopsis is not null) m.Synopsis = req.Synopsis;
        Stamp(m); await db.SaveChangesAsync(ct); return Map(m);
    }

    public async Task<object?> CloneAsync(Guid id, CancellationToken ct = default)
    {
        var src = await db.Materials.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (src is null) return null;
        var copy = new Material
        {
            OwnerId = user.OwnerId,
            TrackingId = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
            ISBN = src.ISBN, Title = src.Title, Author = src.Author, Publisher = src.Publisher,
            Location = src.Location, Value = src.Value, Pages = src.Pages,
            Tags = src.Tags.ToList(), Attributes = new Dictionary<string, string>(src.Attributes),
            CoverUrl = src.CoverUrl, Synopsis = src.Synopsis,
            CreatedBy = user.UserId, CreatedOn = DateTime.UtcNow
        };
        db.Materials.Add(copy); await db.SaveChangesAsync(ct); return Map(copy);
    }

    public async Task<bool> SetStatusAsync(Guid id, MaterialStatus status, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.Status = status; Stamp(m); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UpdateIsbnAsync(Guid id, string isbn, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.ISBN = isbn; Stamp(m); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> RelocateAsync(Guid id, string location, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.Location = location; Stamp(m); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetAttributesAsync(Guid id, Dictionary<string, string> attrs, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.Attributes = attrs; Stamp(m); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetTagsAsync(Guid id, List<string> tags, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.Tags = tags; Stamp(m); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetValueAsync(Guid id, decimal value, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.Value = value; Stamp(m); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetPagesAsync(Guid id, int pages, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.Pages = pages; Stamp(m); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetOwnerIdAsync(Guid id, string ownerId, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.OwnerId = ownerId; Stamp(m); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> VerifyStockAsync(Guid id, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.LastVerifiedOn = DateTime.UtcNow; Stamp(m); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> MaintenanceAsync(Guid id, CancellationToken ct = default)
        => await SetStatusAsync(id, MaterialStatus.Maintenance, ct);

    public async Task<bool> MaintenanceReturnAsync(Guid id, CancellationToken ct = default)
        => await SetStatusAsync(id, MaterialStatus.Available, ct);

    public async Task<IReadOnlyList<object>> GetStockVerificationAsync(CancellationToken ct = default)
    {
        var list = await db.Materials.AsNoTracking()
            .Where(m => m.OwnerId == user.OwnerId && m.Status != MaterialStatus.Disabled)
            .ToListAsync(ct);
        return list.Select(m => new { m.Id, m.TrackingId, m.Title, m.LastVerifiedOn, verified = m.LastVerifiedOn is not null }).ToList<object>();
    }

    public async Task<object> WidgetBreakupAsync(CancellationToken ct = default)
    {
        var list = await db.Materials.AsNoTracking().Where(m => m.OwnerId == user.OwnerId && m.Status != MaterialStatus.Disabled).ToListAsync(ct);
        return new
        {
            total = list.Count,
            byStatus = list.GroupBy(m => m.Status.ToString()).Select(g => new { status = g.Key, count = g.Count() }),
            totalValue = list.Sum(m => m.Value)
        };
    }

    private async Task<Material?> Load(Guid id, CancellationToken ct)
        => await db.Materials.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);

    private void Stamp(Material m) { m.ModifiedBy = user.UserId; m.ModifiedOn = DateTime.UtcNow; }

    private static object Map(Material m) => new
    {
        m.Id, m.TrackingId, m.ISBN, m.Title, m.Author, m.Publisher, m.Location, m.Value, m.Pages,
        Status = m.Status.ToString(), m.Tags, m.Attributes, m.LastVerifiedOn, m.CoverUrl, m.Synopsis,
        m.TxDate, m.DueOn, m.CurrentMemberId, m.CreatedOn
    };
}

public sealed class CreateMaterialRequest
{
    public string? TrackingId { get; set; }
    public required string Title { get; set; }
    public string? ISBN { get; set; }
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public string? Location { get; set; }
    public decimal Value { get; set; }
    public int Pages { get; set; }
    public List<string>? Tags { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public string? CoverUrl { get; set; }
    public string? Synopsis { get; set; }
    public DateTime? AcquiredOn { get; set; }
    public bool IsReference { get; set; }
}

public sealed class MemberService(LibraryDbContext db, ILibraryUserContext user)
{
    public async Task<IReadOnlyList<object>> GetAllAsync(CancellationToken ct = default)
        => await db.Members.AsNoTracking()
            .Where(m => m.OwnerId == user.OwnerId && m.Status != MemberStatus.Disabled)
            .OrderBy(m => m.Name)
            .Select(m => Map(m))
            .ToListAsync(ct);

    public async Task<object> CreateAsync(CreateMemberRequest req, CancellationToken ct = default)
    {
        var m = new Member
        {
            OwnerId = user.OwnerId,
            Name = req.Name.Trim(),
            MemberCode = req.MemberCode,
            Email = req.Email,
            Mobile = req.Mobile,
            MemberType = req.MemberType,
            RefId = req.RefId,
            Tags = req.Tags ?? [],
            Attributes = req.Attributes ?? new(),
            MaxIssues = req.MaxIssues <= 0 ? 3 : req.MaxIssues,
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        db.Members.Add(m); await db.SaveChangesAsync(ct); return Map(m);
    }

    public async Task<object> UpsertAsync(CreateMemberRequest req, CancellationToken ct = default)
    {
        Member? existing = null;
        if (!string.IsNullOrWhiteSpace(req.RefId))
            existing = await db.Members.FirstOrDefaultAsync(x => x.OwnerId == user.OwnerId && x.RefId == req.RefId, ct);
        if (existing is null && !string.IsNullOrWhiteSpace(req.MemberCode))
            existing = await db.Members.FirstOrDefaultAsync(x => x.OwnerId == user.OwnerId && x.MemberCode == req.MemberCode, ct);
        if (existing is null) return await CreateAsync(req, ct);
        existing.Name = req.Name.Trim();
        if (req.Email is not null) existing.Email = req.Email;
        if (req.Mobile is not null) existing.Mobile = req.Mobile;
        if (req.Tags is not null) existing.Tags = req.Tags;
        if (req.Attributes is not null) existing.Attributes = req.Attributes;
        existing.ModifiedBy = user.UserId; existing.ModifiedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct); return Map(existing);
    }

    public async Task<bool> EnableAsync(Guid id, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.Status = MemberStatus.Active; m.ModifiedOn = DateTime.UtcNow; await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> DisableAsync(Guid id, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.Status = MemberStatus.Disabled; m.ModifiedOn = DateTime.UtcNow; await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetAttributesAsync(Guid id, Dictionary<string, string> attrs, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.Attributes = attrs; m.ModifiedOn = DateTime.UtcNow; await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetTagsAsync(Guid id, List<string> tags, CancellationToken ct = default)
    {
        var m = await Load(id, ct); if (m is null) return false;
        m.Tags = tags; m.ModifiedOn = DateTime.UtcNow; await db.SaveChangesAsync(ct); return true;
    }

    public Task<object> JobStubAsync() => Task.FromResult<object>(new { queued = true, note = "Member sync job placeholder" });

    private async Task<Member?> Load(Guid id, CancellationToken ct)
        => await db.Members.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);

    private static object Map(Member m) => new
    {
        m.Id, m.Name, m.MemberCode, m.Email, m.Mobile, m.MemberType, m.RefId,
        Status = m.Status.ToString(), m.Tags, m.Attributes, m.MaxIssues, m.CreatedOn
    };
}

public sealed class CreateMemberRequest
{
    public required string Name { get; set; }
    public string? MemberCode { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? MemberType { get; set; }
    public string? RefId { get; set; }
    public List<string>? Tags { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
    public int MaxIssues { get; set; } = 3;
}

public sealed class CirculationService(LibraryDbContext db, ILibraryUserContext user)
{
    public async Task<IReadOnlyList<object>> GetActiveAsync(CancellationToken ct = default)
        => await db.Circulations.AsNoTracking()
            .Where(c => c.OwnerId == user.OwnerId && c.Status == CirculationStatus.Issued)
            .OrderByDescending(c => c.IssuedOn)
            .Select(c => Map(c))
            .ToListAsync(ct);

    public async Task<object> IssueAsync(Guid materialId, Guid memberId, int days = 14, CancellationToken ct = default)
    {
        var mat = await db.Materials.FirstOrDefaultAsync(m => m.Id == materialId && m.OwnerId == user.OwnerId, ct)
            ?? throw new KeyNotFoundException("Material not found");
        if (mat.Status is MaterialStatus.Issued or MaterialStatus.Maintenance or MaterialStatus.Disabled)
            throw new InvalidOperationException($"Material is {mat.Status}");
        var mem = await db.Members.AsNoTracking().FirstOrDefaultAsync(m => m.Id == memberId && m.OwnerId == user.OwnerId, ct)
            ?? throw new KeyNotFoundException("Member not found");
        if (mem.Status == MemberStatus.Disabled) throw new InvalidOperationException("Member disabled");

        var active = await db.Circulations.CountAsync(c => c.MemberId == memberId && c.Status == CirculationStatus.Issued, ct);
        if (active >= mem.MaxIssues) throw new InvalidOperationException("Member issue limit reached");

        var circ = new Circulation
        {
            OwnerId = user.OwnerId,
            MaterialId = materialId,
            MemberId = memberId,
            TrackingId = mat.TrackingId,
            MemberName = mem.Name,
            IssuedOn = DateTime.UtcNow,
            DueOn = DateTime.UtcNow.AddDays(days <= 0 ? 14 : days),
            Status = CirculationStatus.Issued,
            CreatedBy = user.UserId
        };
        mat.Status = MaterialStatus.Issued;
        mat.CurrentMemberId = memberId;
        mat.TxDate = circ.IssuedOn;
        mat.DueOn = circ.DueOn;
        db.Circulations.Add(circ);
        await db.SaveChangesAsync(ct);
        return Map(circ);
    }

    public async Task<object> ReturnAsync(Guid materialId, CancellationToken ct = default)
    {
        var mat = await db.Materials.FirstOrDefaultAsync(m => m.Id == materialId && m.OwnerId == user.OwnerId, ct)
            ?? throw new KeyNotFoundException("Material not found");
        var circ = await db.Circulations.FirstOrDefaultAsync(c =>
            c.MaterialId == materialId && c.OwnerId == user.OwnerId && c.Status == CirculationStatus.Issued, ct)
            ?? throw new InvalidOperationException("No active issue");
        circ.ReturnedOn = DateTime.UtcNow;
        circ.Status = CirculationStatus.Returned;
        if (circ.ReturnedOn > circ.DueOn)
        {
            var daysLate = (circ.ReturnedOn.Value.Date - circ.DueOn.Date).Days;
            circ.LateFee = daysLate * 1m; // simple rule
            circ.Status = CirculationStatus.Returned;
        }
        mat.Status = MaterialStatus.Available;
        mat.CurrentMemberId = null;
        mat.DueOn = null;
        await db.SaveChangesAsync(ct);
        return Map(circ);
    }

    public async Task<object> ReissueAsync(Guid materialId, int days = 14, CancellationToken ct = default)
    {
        var circ = await db.Circulations.FirstOrDefaultAsync(c =>
            c.MaterialId == materialId && c.OwnerId == user.OwnerId && c.Status == CirculationStatus.Issued, ct)
            ?? throw new InvalidOperationException("No active issue");
        circ.DueOn = DateTime.UtcNow.AddDays(days <= 0 ? 14 : days);
        circ.ReissueCount++;
        var mat = await db.Materials.FirstOrDefaultAsync(m => m.Id == materialId, ct);
        if (mat is not null) mat.DueOn = circ.DueOn;
        await db.SaveChangesAsync(ct);
        return Map(circ);
    }

    public async Task<object> MonthwiseCheckoutsAsync(CancellationToken ct = default)
    {
        var list = await db.Circulations.AsNoTracking()
            .Where(c => c.OwnerId == user.OwnerId)
            .ToListAsync(ct);
        var groups = list.GroupBy(c => new { c.IssuedOn.Year, c.IssuedOn.Month })
            .Select(g => new { year = g.Key.Year, month = g.Key.Month, count = g.Count() })
            .OrderBy(x => x.year).ThenBy(x => x.month);
        return groups;
    }

    private static object Map(Circulation c) => new
    {
        c.Id, c.MaterialId, c.MemberId, c.TrackingId, c.MemberName,
        c.IssuedOn, c.DueOn, c.ReturnedOn, Status = c.Status.ToString(),
        c.LateFee, c.ReissueCount
    };
}

public sealed class LibraryReportService(LibraryDbContext db, ILibraryUserContext user)
{
    private IQueryable<Material> Mats => db.Materials.AsNoTracking().Where(m => m.OwnerId == user.OwnerId);
    private IQueryable<Circulation> Circs => db.Circulations.AsNoTracking().Where(c => c.OwnerId == user.OwnerId);

    public async Task<object> SummaryAsync(CancellationToken ct = default)
    {
        var mats = await Mats.Where(m => m.Status != MaterialStatus.Disabled).ToListAsync(ct);
        var members = await db.Members.AsNoTracking().CountAsync(m => m.OwnerId == user.OwnerId && m.Status == MemberStatus.Active, ct);
        var issued = mats.Count(m => m.Status == MaterialStatus.Issued);
        return new { materials = mats.Count, members, issued, available = mats.Count(m => m.Status == MaterialStatus.Available), totalValue = mats.Sum(m => m.Value) };
    }

    public async Task<object> ValuationAsync(CancellationToken ct = default)
        => new { total = await Mats.Where(m => m.Status != MaterialStatus.Disabled).SumAsync(m => m.Value, ct) };

    public async Task<object> OverDueAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var list = await Circs.Where(c => c.Status == CirculationStatus.Issued && c.DueOn < now).ToListAsync(ct);
        return list;
    }

    public async Task<object> LateFeesAsync(CancellationToken ct = default)
        => await Circs.Where(c => c.LateFee > 0).Select(c => new { c.Id, c.TrackingId, c.MemberName, c.LateFee, c.ReturnedOn }).ToListAsync(ct);

    public async Task<object> UnderCirculationAsync(CancellationToken ct = default)
        => await Circs.Where(c => c.Status == CirculationStatus.Issued).ToListAsync(ct);

    public async Task<object> UnderMaintenanceAsync(CancellationToken ct = default)
        => await Mats.Where(m => m.Status == MaterialStatus.Maintenance).Select(m => new { m.Id, m.TrackingId, m.Title }).ToListAsync(ct);

    public async Task<object> DueForMaintenanceAsync(CancellationToken ct = default)
        => await Mats.Where(m => m.Status == MaterialStatus.Available && m.AcquiredOn != null && m.AcquiredOn < DateTime.UtcNow.AddYears(-5))
            .Select(m => new { m.Id, m.TrackingId, m.Title, m.AcquiredOn }).ToListAsync(ct);

    public async Task<object> LostAsync(CancellationToken ct = default)
        => await Mats.Where(m => m.Status == MaterialStatus.Lost).Select(m => new { m.Id, m.TrackingId, m.Title }).ToListAsync(ct);

    public async Task<object> ReferenceAsync(CancellationToken ct = default)
        => await Mats.Where(m => m.Status == MaterialStatus.Reference).Select(m => new { m.Id, m.TrackingId, m.Title }).ToListAsync(ct);

    public async Task<object> WithoutIsbnAsync(CancellationToken ct = default)
        => await Mats.Where(m => m.Status != MaterialStatus.Disabled && (m.ISBN == null || m.ISBN == "")).Select(m => new { m.Id, m.TrackingId, m.Title }).ToListAsync(ct);

    public async Task<object> WithoutCoverAsync(CancellationToken ct = default)
        => await Mats.Where(m => m.Status != MaterialStatus.Disabled && (m.CoverUrl == null || m.CoverUrl == "")).Select(m => new { m.Id, m.TrackingId, m.Title }).ToListAsync(ct);

    public async Task<object> WithoutSynopsisAsync(CancellationToken ct = default)
        => await Mats.Where(m => m.Status != MaterialStatus.Disabled && (m.Synopsis == null || m.Synopsis == "")).Select(m => new { m.Id, m.TrackingId, m.Title }).ToListAsync(ct);

    public async Task<object> LastMonitoredAsync(CancellationToken ct = default)
        => await Mats.OrderByDescending(m => m.LastVerifiedOn).Take(100).Select(m => new { m.Id, m.TrackingId, m.Title, m.LastVerifiedOn }).ToListAsync(ct);

    public async Task<object> UnverifiedStockAsync(CancellationToken ct = default)
        => await Mats.Where(m => m.Status != MaterialStatus.Disabled && m.LastVerifiedOn == null).Select(m => new { m.Id, m.TrackingId, m.Title }).ToListAsync(ct);

    public async Task<object> MembersAsync(CancellationToken ct = default)
        => await db.Members.AsNoTracking().Where(m => m.OwnerId == user.OwnerId).Select(m => new { m.Id, m.Name, m.MemberCode, Status = m.Status.ToString() }).ToListAsync(ct);

    public async Task<object> AgeAnalysisAsync(CancellationToken ct = default)
    {
        var list = await Mats.Where(m => m.Status != MaterialStatus.Disabled).ToListAsync(ct);
        var now = DateTime.UtcNow;
        return list.GroupBy(m =>
        {
            if (m.AcquiredOn is null) return "unknown";
            var years = (now - m.AcquiredOn.Value).TotalDays / 365.0;
            if (years < 1) return "0-1y";
            if (years < 5) return "1-5y";
            if (years < 10) return "5-10y";
            return "10y+";
        }).Select(g => new { ageBand = g.Key, count = g.Count(), value = g.Sum(x => x.Value) });
    }
}

public sealed class BarcodeService
{
    public object Generate(string? trackingId, int count = 1)
    {
        var codes = new List<object>();
        for (var i = 0; i < Math.Clamp(count, 1, 100); i++)
        {
            var code = string.IsNullOrWhiteSpace(trackingId)
                ? Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
                : trackingId + (count > 1 ? $"-{i + 1}" : "");
            codes.Add(new { code, barcode = code });
        }
        return new { codes };
    }
}
