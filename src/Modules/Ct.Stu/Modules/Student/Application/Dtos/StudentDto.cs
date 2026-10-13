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

// ── Additional request/response DTOs (original StudentController surface) ────

public sealed class AssignStudentIdRequest
{
    public required Guid Id { get; init; }
    public required string StudentId { get; init; }
}

public sealed class TcRequest
{
    public required Guid Id { get; init; }
    public string? Reason { get; init; }
    public DateOnly? TcDate { get; init; }
}

public sealed class SetHomeAddressRequest
{
    public required Guid Id { get; init; }
    public string? Line1 { get; init; }
    public string? Line2 { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? PostalCode { get; init; }
    public string? Country { get; init; }
}

public sealed class SetContactRequest
{
    public required Guid Id { get; init; }
    public required string Value { get; init; }
}

public sealed class UpdateGenderRequest
{
    public required Guid Id { get; init; }
    public required string Gender { get; init; }
}

public sealed class UpdateParentDetailsRequest
{
    public required Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Occupation { get; init; }
    public string? Email { get; init; }
    public string? Mobile { get; init; }
}

public sealed class UpdateStudentRequest
{
    public required Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Gender { get; init; }
    public DateOnly? Dob { get; init; }
    public string? AadharNo { get; init; }
    public string? Email { get; init; }
    public string? Mobile { get; init; }
    public string? StudentId { get; init; }
    public IReadOnlyList<string>? Tags { get; init; }
}

public sealed class SetRollNoRequest
{
    public required Guid Id { get; init; }
    public required string InstId { get; init; }
    public required string RollNo { get; init; }
}

public sealed class UpdateCourseRequest
{
    public required Guid Id { get; init; }
    public required string InstId { get; init; }
    public string? CourseId { get; init; }
    public string? CourseName { get; init; }
    public string? TermName { get; init; }
    public string? SectionName { get; init; }
    public string? Branch { get; init; }
}

public sealed class FeesReceivableRequest
{
    public required string Id { get; init; }
    public string? InstId { get; init; }
    public string? FeeId { get; init; }
    public string? FeeName { get; init; }
    public string? TxId { get; init; }
    public decimal Amount { get; init; }
}

public sealed class StudentFilterRequest
{
    public string? InstId { get; init; }
    public string? CourseId { get; init; }
    public string? Section { get; init; }
    public string? Tag { get; init; }
    public string? Status { get; init; }
    public string? Search { get; init; }
}

public sealed class StudentFacetDto
{
    public string Key { get; init; } = "";
    public IReadOnlyList<StudentFacetBucketDto> Buckets { get; init; } = [];
}

public sealed class StudentFacetBucketDto
{
    public string Value { get; init; } = "";
    public long Count { get; init; }
}

public sealed class StudentContactDto
{
    public Guid StudentId { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Mobile { get; init; }
    public string? FatherMobile { get; init; }
    public string? MotherMobile { get; init; }
    public IReadOnlyList<string> ParentEmails { get; init; } = [];
}

public sealed class SectionwiseStudentDto
{
    public string? CourseId { get; init; }
    public string? CourseName { get; init; }
    public string? Section { get; init; }
    public int Count { get; init; }
    public IReadOnlyList<StudentDto> Students { get; init; } = [];
}
