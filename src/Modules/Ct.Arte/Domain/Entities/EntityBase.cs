namespace Ct.Arte.Domain.Entities;

/// <summary>
/// Base contract for all persistent entities (replaces Raven Entity).
/// </summary>
public interface IEntity
{
    string Id { get; set; }
    string OwnerId { get; set; }
    string? ParentId { get; set; }
}

public interface IAuditable
{
    DateTimeOffset CreatedOn { get; set; }
    string CreatedBy { get; set; }
    DateTimeOffset? ModifiedOn { get; set; }
    string? ModifiedBy { get; set; }
}

/// <summary>
/// Abstract base used by EF Core. Audit fields are set by interceptor.
/// </summary>
public abstract class EntityBase : IEntity, IAuditable
{
    public string Id { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string? ParentId { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Generates a new GUID-based string Id if empty.
    /// Compatible with legacy Raven behaviour.
    /// </summary>
    public string EnsureId(string? preferred = null)
    {
        if (!string.IsNullOrWhiteSpace(Id))
            return Id;

        if (!string.IsNullOrWhiteSpace(preferred) && Guid.TryParse(preferred, out var g) && g != Guid.Empty)
        {
            Id = g.ToString("D");
            return Id;
        }

        Id = Guid.NewGuid().ToString("D");
        return Id;
    }

    public virtual void SetOwner(string ownerId) => OwnerId = ownerId ?? string.Empty;
    public virtual void SetParent(string? parentId) => ParentId = parentId;
}
