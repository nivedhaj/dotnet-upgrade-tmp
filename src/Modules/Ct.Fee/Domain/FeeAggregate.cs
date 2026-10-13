namespace Ct.Fee.Domain;

public sealed class FeeDefinition : AuditableEntity
{
    public string Name { get; set; } = "";
    public string NameLower { get; set; } = "";
    public string? DisplayText { get; set; }
    public decimal Amount { get; set; }
    public string[] Tags { get; set; } = [];
    public bool CollectStudentWise { get; set; }
    public bool IsTxDone { get; set; }
    public FeeStatus Status { get; set; } = FeeStatus.Active;
    public bool SpansAcademicYears { get; set; }
    public List<FeeInstallment> Installments { get; set; } = [];
    public List<FeeFine> Fines { get; set; } = [];
    public List<FeeStudent> Students { get; set; } = [];
    public List<FeeCourse> Courses { get; set; } = [];

    public void Normalize()
    {
        Name = Name.Trim();
        NameLower = Name.ToLowerInvariant();
        Amount = decimal.Round(Amount, 2, MidpointRounding.AwayFromZero);
    }
}

public sealed class FeeInstallment : AuditableEntity
{
    public Guid FeeDefinitionId { get; set; }
    public int InstallmentNo { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly? DueDate { get; set; }
    public FeeDefinition? FeeDefinition { get; set; }
}

public sealed class FeeFine : AuditableEntity
{
    public Guid FeeDefinitionId { get; set; }
    public int FineNo { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public int? OverdueByDays { get; set; }
    public FeeDefinition? FeeDefinition { get; set; }
}

public sealed class FeeStudent : AuditableEntity
{
    public Guid FeeDefinitionId { get; set; }
    public string StudentId { get; set; } = "";
    public FeeDefinition? FeeDefinition { get; set; }
}

public sealed class FeeCourse : AuditableEntity
{
    public Guid FeeDefinitionId { get; set; }
    public string CourseId { get; set; } = "";
    public FeeDefinition? FeeDefinition { get; set; }
}
