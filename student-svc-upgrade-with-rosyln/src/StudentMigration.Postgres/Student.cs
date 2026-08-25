namespace StudentMigration.Postgres;

public enum StudentStatus { Active, Disabled }

public sealed class Student
{
    public Guid Id { get; set; }
    public string StudentId { get; set; } = "";
    public string Name { get; set; } = "";
    public StudentStatus Status { get; set; } = StudentStatus.Active;
    public string? AadharNumber { get; set; }
    public string? UDID { get; set; }
    public string? UserId { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Category { get; set; }
    public string? Attendance { get; set; }
    public string? DomicileJson { get; set; }
    public string? DocumentsJson { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<StudentEnrollment> Enrollments { get; set; } = [];
}

public sealed class StudentEnrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string InstId { get; set; } = "";
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? TermName { get; set; }
    public string? SectionName { get; set; }
    public string? Branch { get; set; }
    public string? ApplicationNo { get; set; }
    public string? AdmissionNo { get; set; }
    public string? AssignedStudentId { get; set; }
    public DateTime? Doa { get; set; }
}
