namespace Ct.Modules.Admissions.Domain;

/// <summary>
/// Links a Student to a Father/Mother/Guardian Person (spec section 10).
/// Parents are Persons in their own right, not duplicated fields on Student
/// (spec section 124, rule 5).
/// </summary>
public class StudentRelation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }
    public Guid PersonId { get; set; }
    public RelationType RelationType { get; set; }

    public Person? Person { get; set; }
}
