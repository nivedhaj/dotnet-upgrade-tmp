using System.Text.RegularExpressions;

namespace Ct.Modules.Admissions.Domain;

public partial class PersonContact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }

    public ContactType ContactType { get; set; }
    public LocationType Location { get; set; }
    public string Info { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public ContactStatus Status { get; set; } = ContactStatus.Active;
    public string? Notes { get; set; }
    public DateTime? ArchivedOn { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>Legacy `HasValidMobile`: Mobile type, primary, exactly 10 characters (spec section 11).</summary>
    public bool HasValidMobile() =>
        ContactType == ContactType.Mobile
        && IsPrimary
        && Info.Length == 10;

    /// <summary>Legacy `HasValidEmail`: Email type, primary, RFC-shaped address (spec section 11).</summary>
    public bool HasValidEmail() =>
        ContactType == ContactType.Email
        && IsPrimary
        && EmailPattern().IsMatch(Info);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
