namespace StudentService.Api.Models;

public enum StudentStatus
{
    Unknown = -1,
    Active = 1,
    Disabled = 99
}

public enum EnrollmentStatus
{
    Unknown = -1,
    Active = 1,
    Promoted = 10,
    TC = 98,
    Disabled = 99
}

public sealed class Student
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public DateTime? Dob { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? AadharNumber { get; set; }
    public string? Attendance { get; set; }
    public string? Category { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;

    // Keep initially flexible data as JSONB instead of reproducing Raven's document shape.
    public Dictionary<string, object?>? Domicile { get; set; }
    public Dictionary<string, object?>? Documents { get; set; }

    public List<StudentEnrollment> Enrollments { get; set; } = [];
}

public sealed class StudentEnrollment
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string StudentId { get; set; } = "";
    public Student Student { get; set; } = null!;

    public string InstituteId { get; set; } = "";
    public string? CourseId { get; set; }
    public string? CourseName { get; set; }
    public string? TermName { get; set; }
    public string? SectionName { get; set; }
    public string? Branch { get; set; }
    public string? RollNo { get; set; }
    public string? StudentNumber { get; set; }
    public string? AdmissionNumber { get; set; }
    public string? ApplicationNumber { get; set; }
    public DateTime? DateOfAdmission { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
    public Dictionary<string, object?>? Attributes { get; set; }
}
