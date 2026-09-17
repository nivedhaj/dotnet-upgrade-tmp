namespace Ct.Modules.Admissions.Application.Dtos;

/// <summary>Legacy `StudentDto` base shape (spec section 53). Field names preserved for client compatibility.</summary>
public class StudentDto
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? CourseId { get; init; }
    public string? CourseName { get; init; }
    public string? StudentId { get; init; }
    public string? Email { get; init; }
    public DateOnly? Dob { get; init; }
    public DateOnly? Doa { get; init; }
    public string? FatherName { get; init; }
    public string? MotherName { get; init; }
    public string? GuardianName { get; init; }
    public string? AadharNumber { get; init; }
    public string? RollNo { get; init; }
    public string? Gender { get; init; }
    public string? Status { get; init; }
    public string? SectionName { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
}

/// <summary>Legacy `StudentFlatDto` (spec section 46) — flattened projection for external workers/reporting.</summary>
public class StudentFlatDto
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? CourseId { get; init; }
    public string? CourseName { get; init; }
    public string? StudentId { get; init; }
    public string? Email { get; init; }
    public DateOnly? Dob { get; init; }
    public DateOnly? Doa { get; init; }
    public string? FatherName { get; init; }
    public string? MotherName { get; init; }
    public string? GuardianName { get; init; }
    public string? AadharNumber { get; init; }
    public string? RollNo { get; init; }
    public string? Gender { get; init; }
    public string? Status { get; init; }
    public string? SectionName { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
}

/// <summary>Request body for POST /create (spec section 35).</summary>
public class CreateStudentRequest
{
    public Guid? Id { get; init; }
    public string? AadharNo { get; init; }
    public string? AdmissionNumber { get; init; }
    public string? ApplicationNumber { get; init; }
    public string? CourseName { get; init; }
    public string? CourseId { get; init; }
    public DateOnly? Doa { get; init; }
    public DateOnly? Dob { get; init; }
    public string? Email { get; init; }
    public string? Gender { get; init; }
    public string? Mobile { get; init; }
    public required string Name { get; init; }
    public string? SecName { get; init; }
    public string? StudentId { get; init; }
    public string? TermName { get; init; }
    public string? Branch { get; init; }
    public string? InstId { get; init; }
}

/// <summary>Request body for POST /promote (spec section 17).</summary>
public class PromoteStudentRequest
{
    public required Guid Id { get; init; }
    public required string BaseInstId { get; init; }
    public required string BaseCourseId { get; init; }
    public string? BaseTerm { get; init; }
    public string? BaseSection { get; init; }
    public required string TargetInstId { get; init; }
    public required string TargetCourseId { get; init; }
    public string? TargetTerm { get; init; }
    public string? TargetSection { get; init; }
}
