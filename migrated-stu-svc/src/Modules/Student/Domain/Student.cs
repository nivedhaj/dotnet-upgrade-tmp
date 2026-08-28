namespace Ct.Modules.Academics.Domain;

public enum StudentStatus
{
    Active = 1,
    Disabled = 2,
    TransferCertificate = 3
}

public sealed class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string StudentId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? AdmissionNumber { get; set; }
    public string? ApplicationNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? DateOfAdmission { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Gender { get; set; }
    public string? AadharNumber { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    public string Enrollments { get; set; } = "[]";
    public string Contacts { get; set; } = "[]";
    public string Metadata { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
