using System.Text.Json.Serialization;

namespace Ct.Connect.Domain;

/// <summary>
/// Base entity matching the original Raven document shape while being EF-friendly.
/// Id remains string (GUID) for external API compatibility.
/// </summary>
public abstract class Entity
{
    public string Id { get; set; } = Guid.NewGuid().ToString("D");

    public string OwnerId { get; set; } = string.Empty;

    public string ParentId { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? ModifiedOn { get; set; }

    public string? ModifiedBy { get; set; }

    /// <summary>
    /// When true the audit interceptor skips automatic population
    /// (mirrors original Raven Audited flag).
    /// </summary>
    [JsonIgnore]
    public bool Audited { get; set; }

    public string SetId(string? id = null)
    {
        if (!string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out var g) && g != Guid.Empty)
        {
            Id = g.ToString("D");
        }
        else
        {
            Id = Guid.NewGuid().ToString("D");
        }
        return Id;
    }

    public virtual void SetOwner(string ownerId) => OwnerId = ownerId ?? string.Empty;

    public virtual void SetParent(string parentId) => ParentId = parentId ?? string.Empty;

    public virtual void ApplyAudit(DateTime createdOn, string createdBy, DateTime? modifiedOn = null, string? modifiedBy = null)
    {
        CreatedOn = createdOn;
        CreatedBy = createdBy;
        ModifiedOn = modifiedOn;
        ModifiedBy = modifiedBy;
        Audited = true;
    }
}
