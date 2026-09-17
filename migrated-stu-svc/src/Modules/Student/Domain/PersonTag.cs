namespace Ct.Modules.Admissions.Domain;

/// <summary>
/// Legacy `List&lt;string&gt; Tags` on Person, normalized into a relational
/// table so GetStudentsByTag runs as a real query instead of JSONB scanning
/// (spec section 13).
/// </summary>
public class PersonTag
{
    public Guid PersonId { get; set; }
    public string Tag { get; set; } = string.Empty;
}
