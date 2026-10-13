namespace Ct.Library.Postgres.ReadModels;

/// <summary>
/// Read-model document for library materials (ported from original MaterialView).
/// Stored as a Marten document.
/// </summary>
public sealed class MaterialView
{
    public string Id { get; set; } = string.Empty; // Aggregate / Asset Id

    public string TrackingId { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? Publisher { get; set; }
    public string? Location { get; set; }

    public List<Owner> OwnerShip { get; set; } = [];
    public Dictionary<string, object?> Attributes { get; set; } = [];
    public List<string> Tags { get; set; } = [];

    public decimal? Value { get; set; }
    public int? Pages { get; set; }
    public string Status { get; set; } = "Available";

    /// <summary>
    /// Convenience – current owner.
    /// </summary>
    public string? OwnerId =>
        OwnerShip.FirstOrDefault(o => o.Status == OwnerStatus.Current)?.OwnerId;

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset? ModifiedOn { get; set; }
}

public sealed class Owner
{
    public string OwnerId { get; set; } = string.Empty;
    public OwnerStatus Status { get; set; }
    public DateTimeOffset From { get; set; }
    public DateTimeOffset? To { get; set; }
}

public enum OwnerStatus
{
    Current,
    Past
}
