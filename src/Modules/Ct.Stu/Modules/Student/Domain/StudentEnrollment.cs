namespace Ct.Modules.Admissions.Domain;

public class StudentEnrollment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StudentId { get; set; }

    public string InstId { get; set; } = string.Empty;
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? TermName { get; set; }
    public string? SectionName { get; set; }
    public string? Branch { get; set; }

    /// <summary>Institute-scoped student identifier (legacy per-institute StudentId on the enrollment).</summary>
    public string? StudentInstId { get; set; }

    public string? ApplicationNo { get; set; }
    public string? AdmissionNo { get; set; }
    public DateOnly? Doa { get; set; }
    public string? RollNo { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;

    public string? TcNo { get; set; }
    public DateTime? TcAppliedOn { get; set; }
    public DateTime? TcIssuedOn { get; set; }
    public string? Character { get; set; }
    public string? Conduct { get; set; }
    public string? Remarks { get; set; }

    public Dictionary<string, object?> Attributes { get; set; } = new();
}
