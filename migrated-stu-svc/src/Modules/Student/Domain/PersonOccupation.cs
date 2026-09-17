namespace Ct.Modules.Admissions.Domain;

/// <summary>Legacy `Occupation` (spec section 14).</summary>
public class PersonOccupation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }

    public string? Title { get; set; }
    public string? Particulars { get; set; }
    public DateOnly? AsOn { get; set; }
}
