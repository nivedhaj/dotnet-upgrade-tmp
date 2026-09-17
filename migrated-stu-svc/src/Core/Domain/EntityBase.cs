namespace Ct.Core.Domain;

/// <summary>
/// Legacy "Entity" concept from the RavenDB-era codebase, re-expressed with a
/// database-native Guid key instead of a Raven string id.
/// See spec section 6 (DOMAIN MODEL).
/// </summary>
public abstract class EntityBase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Legacy free-text owner reference (e.g. tenant/owning actor). Preserved as-is; not a FK.</summary>
    public string? OwnerId { get; set; }

    /// <summary>Legacy free-text parent reference. Preserved as-is; not a FK.</summary>
    public string? ParentId { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    /// <summary>Whether changes to this entity should be written to event_logs (spec section 28).</summary>
    public bool Audited { get; set; } = true;
}
