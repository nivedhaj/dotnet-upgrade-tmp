namespace Ct.Modules.Admissions.Domain;

/// <summary>Legacy `Address` (spec section 12).</summary>
public class PersonAddress
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }

    public LocationType Location { get; set; }
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Pincode { get; set; }
    public DateTime? ArchivedOn { get; set; }
    public AddressStatus Status { get; set; } = AddressStatus.Active;
}
