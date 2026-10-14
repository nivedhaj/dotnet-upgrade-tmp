using Ct.Asset.Domain;
using Ct.Asset.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Ct.Asset.Application;

public sealed class AssetService(AssetDbContext db, IAssetUserContext user)
{
    public async Task<IReadOnlyList<object>> GetAllAsync(CancellationToken ct = default)
        => await db.Assets.AsNoTracking()
            .Where(a => a.OwnerId == user.OwnerId && a.Status != AssetStatus.Disabled)
            .OrderBy(a => a.TrackingId)
            .Select(a => Map(a))
            .ToListAsync(ct);

    public async Task<object?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var a = await db.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        return a is null ? null : Map(a);
    }

    public async Task<object> CreateAsync(CreateAssetRequest req, CancellationToken ct = default)
    {
        var tracking = string.IsNullOrWhiteSpace(req.TrackingId)
            ? Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
            : req.TrackingId.Trim();
        if (await IsDuplicateTrackingIdAsync(tracking, ct))
            throw new InvalidOperationException($"TrackingId '{tracking}' already exists");

        var a = new AssetItem
        {
            OwnerId = user.OwnerId,
            TrackingId = tracking,
            Name = req.Name,
            Location = req.Location,
            Value = req.Value,
            Tags = req.Tags ?? [],
            Attributes = req.Attributes ?? new(),
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        db.Assets.Add(a);
        await db.SaveChangesAsync(ct);
        return Map(a);
    }

    public async Task<bool> EnableAsync(Guid id, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Status = AssetStatus.Active; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> DisableAsync(Guid id, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Status = AssetStatus.Disabled; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetAttributesAsync(Guid id, Dictionary<string, string> attrs, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Attributes = attrs; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetTagsAsync(Guid id, List<string> tags, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Tags = tags; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetValueAsync(Guid id, decimal value, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Value = value; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> RelocateAsync(Guid id, string location, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Location = location; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UpdateTrackingIdAsync(Guid id, string newTrackingId, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        if (await IsDuplicateTrackingIdAsync(newTrackingId, ct))
            throw new InvalidOperationException($"TrackingId '{newTrackingId}' already exists");
        a.TrackingId = newTrackingId.Trim(); Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> MaintainAsync(Guid id, string? actionTaken, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Maintenance.Add(new MaintenanceRecord
        {
            On = DateTime.UtcNow,
            By = user.UserId,
            ActionTaken = actionTaken
        });
        a.Maintenance = a.Maintenance.ToList();
        a.LastMaintenance = DateTime.UtcNow;
        a.Status = AssetStatus.Active;
        Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> SetWarrantyAsync(Guid id, string? coveredBy, DateTime? from, DateTime? to, string? details, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.CurrentWarranty = new WarrantyRecord
        {
            CoveredBy = coveredBy,
            From = from,
            To = to,
            Details = details
        };
        Stamp(a); await db.SaveChangesAsync(ct); return true;
    }


    public async Task<bool> ClearAttributesAsync(Guid id, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Attributes = new(); Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> ClearTagsAsync(Guid id, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Tags = []; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> ClearAsync(Guid id, CancellationToken ct = default)
    {
        var a = await Load(id, ct); if (a is null) return false;
        a.Status = AssetStatus.Cleared; Stamp(a); await db.SaveChangesAsync(ct); return true;
    }

    public async Task<object> CloneManyAsync(Guid id, List<string> trackingIds, CancellationToken ct = default)
    {
        var results = new List<object>();
        foreach (var tid in (trackingIds ?? []).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var r = await CloneAsync(id, tid, ct);
                if (r is not null) results.Add(r);
            }
            catch (Exception ex) { results.Add(new { trackingId = tid, error = ex.Message }); }
        }
        return new { count = results.Count, results };
    }

    public async Task<object?> CloneAsync(Guid id, string? newTrackingId, CancellationToken ct = default)
    {
        var src = await db.Assets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);
        if (src is null) return null;
        var tracking = string.IsNullOrWhiteSpace(newTrackingId)
            ? Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
            : newTrackingId.Trim();
        if (await IsDuplicateTrackingIdAsync(tracking, ct))
            throw new InvalidOperationException($"TrackingId '{tracking}' already exists");

        var copy = new AssetItem
        {
            OwnerId = user.OwnerId,
            TrackingId = tracking,
            Name = src.Name,
            Location = src.Location,
            Value = src.Value,
            Tags = src.Tags.ToList(),
            Attributes = new Dictionary<string, string>(src.Attributes),
            CurrentWarranty = src.CurrentWarranty is null ? null : new WarrantyRecord
            {
                CoveredBy = src.CurrentWarranty.CoveredBy,
                From = src.CurrentWarranty.From,
                To = src.CurrentWarranty.To,
                Details = src.CurrentWarranty.Details
            },
            CreatedBy = user.UserId,
            CreatedOn = DateTime.UtcNow
        };
        db.Assets.Add(copy); await db.SaveChangesAsync(ct); return Map(copy);
    }

    public async Task<object> ImportArtefactsAsync(List<CreateAssetRequest> items, CancellationToken ct = default)
    {
        var results = new List<object>();
        foreach (var item in items ?? [])
        {
            try { results.Add(await CreateAsync(item, ct)); }
            catch (Exception ex) { results.Add(new { error = ex.Message, item.TrackingId }); }
        }
        return new { count = results.Count, results };
    }

    public async Task<bool> IsDuplicateTrackingIdAsync(string trackingId, CancellationToken ct = default)
        => await db.Assets.AsNoTracking()
            .AnyAsync(a => a.OwnerId == user.OwnerId && a.TrackingId == trackingId && a.Status != AssetStatus.Disabled, ct);

    public async Task<object> LocationWiseReportAsync(CancellationToken ct = default)
    {
        var list = await db.Assets.AsNoTracking()
            .Where(a => a.OwnerId == user.OwnerId && a.Status != AssetStatus.Disabled)
            .ToListAsync(ct);
        var groups = list.GroupBy(a => a.Location ?? "unassigned")
            .Select(g => new
            {
                location = g.Key,
                count = g.Count(),
                totalValue = g.Sum(x => x.Value),
                assets = g.Select(x => new { x.Id, x.TrackingId, x.Name, x.Value, Status = x.Status.ToString() })
            });
        return new { total = list.Count, byLocation = groups };
    }

    private async Task<AssetItem?> Load(Guid id, CancellationToken ct)
        => await db.Assets.FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == user.OwnerId, ct);

    private void Stamp(AssetItem a) { a.ModifiedBy = user.UserId; a.ModifiedOn = DateTime.UtcNow; }

    private static object Map(AssetItem a) => new
    {
        a.Id, a.TrackingId, a.Name, a.Location, a.Value,
        Status = a.Status.ToString(), a.Tags, a.Attributes,
        a.LastMaintenance, a.CurrentWarranty, a.CreatedOn,
        MaintenanceCount = a.Maintenance.Count
    };
}

public sealed class CreateAssetRequest
{
    public string? TrackingId { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public decimal Value { get; set; }
    public List<string>? Tags { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}

/// <summary>Cmd/Qry facades matching original handler/qry names.</summary>
public sealed class AssetCmd(AssetService assets)
{
    public Task<object> Create(CreateAssetRequest req, CancellationToken ct = default) => assets.CreateAsync(req, ct);
    public Task<bool> Enable(Guid id, CancellationToken ct = default) => assets.EnableAsync(id, ct);
    public Task<bool> Disable(Guid id, CancellationToken ct = default) => assets.DisableAsync(id, ct);
    public Task<bool> SetAttributes(Guid id, Dictionary<string, string> attrs, CancellationToken ct = default) => assets.SetAttributesAsync(id, attrs, ct);
    public Task<bool> SetTags(Guid id, List<string> tags, CancellationToken ct = default) => assets.SetTagsAsync(id, tags, ct);
    public Task<bool> SetValue(Guid id, decimal value, CancellationToken ct = default) => assets.SetValueAsync(id, value, ct);
    public Task<bool> Relocate(Guid id, string location, CancellationToken ct = default) => assets.RelocateAsync(id, location, ct);
    public Task<bool> UpdateTrackingId(Guid id, string trackingId, CancellationToken ct = default) => assets.UpdateTrackingIdAsync(id, trackingId, ct);
    public Task<bool> Maintain(Guid id, string? action, CancellationToken ct = default) => assets.MaintainAsync(id, action, ct);
    public Task<bool> SetWarranty(Guid id, string? coveredBy, DateTime? from, DateTime? to, string? details, CancellationToken ct = default)
        => assets.SetWarrantyAsync(id, coveredBy, from, to, details, ct);
    public Task<object?> Clone(Guid id, string? newTrackingId, CancellationToken ct = default) => assets.CloneAsync(id, newTrackingId, ct);
    public Task<object> CloneMany(Guid id, List<string> trackingIds, CancellationToken ct = default) => assets.CloneManyAsync(id, trackingIds, ct);
    public Task<bool> ClearAttributes(Guid id, CancellationToken ct = default) => assets.ClearAttributesAsync(id, ct);
    public Task<bool> ClearTags(Guid id, CancellationToken ct = default) => assets.ClearTagsAsync(id, ct);
    public Task<bool> Clear(Guid id, CancellationToken ct = default) => assets.ClearAsync(id, ct);
}

public sealed class AssetQry(AssetService assets)
{
    public Task<IReadOnlyList<object>> Get(CancellationToken ct = default) => assets.GetAllAsync(ct);
    public Task<object?> GetAssetById(Guid id, CancellationToken ct = default) => assets.GetByIdAsync(id, ct);
    public Task<bool> IsDuplicateTrackingId(string trackingId, CancellationToken ct = default) => assets.IsDuplicateTrackingIdAsync(trackingId, ct);
}
