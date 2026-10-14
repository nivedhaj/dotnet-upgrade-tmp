namespace Ct.Asset.Domain;

public enum AssetStatus { Active = 1, Maintenance = 50, Cleared = 90, Disabled = 99 }

public class AssetItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OwnerId { get; set; } = "";
    public string TrackingId { get; set; } = "";
    public string? Name { get; set; }
    public string? Location { get; set; }
    public decimal Value { get; set; }
    public AssetStatus Status { get; set; } = AssetStatus.Active;
    public Dictionary<string, string> Attributes { get; set; } = new();
    public List<string> Tags { get; set; } = [];
    public List<MaintenanceRecord> Maintenance { get; set; } = [];
    public WarrantyRecord? CurrentWarranty { get; set; }
    public DateTime? LastMaintenance { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class MaintenanceRecord
{
    public DateTime On { get; set; } = DateTime.UtcNow;
    public string? By { get; set; }
    public string? ActionTaken { get; set; }
}

public class WarrantyRecord
{
    public string? CoveredBy { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Details { get; set; }
}
