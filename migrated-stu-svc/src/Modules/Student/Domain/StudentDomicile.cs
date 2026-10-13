namespace Ct.Modules.Admissions.Domain;

/// <summary>
/// Legacy `Student.Domicile` (spec section 15). Modeled as a dedicated table
/// per the spec's preference ("if future expansion is expected") since caste,
/// mother tongue, etc. are independently queried demographic fields.
/// </summary>
public class StudentDomicile
{
    public Guid StudentId { get; set; }

    public string? Caste { get; set; }
    public string? Country { get; set; }
    public string? MotherTongue { get; set; }
    public string? State { get; set; }
}
