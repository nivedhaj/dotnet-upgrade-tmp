namespace Ct.Arte.Domain.Entities;

public enum TagStatus
{
    Unknown = 0,
    Active = 1,
    Disabled = 99
}

/// <summary>
/// Stand-alone tag entity (previously stored as documents in Raven).
/// Can be used for both ArtefactTag and ContentTag via Type discriminator if desired.
/// </summary>
public class ArtefactTag : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public bool Predefined { get; set; }
    /// <summary>Client Short Name. "*" means applicable across all institutes.</summary>
    public string Csn { get; set; } = "*";
    public Dictionary<string, string> Meta { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public TagStatus Status { get; set; } = TagStatus.Active;
}
