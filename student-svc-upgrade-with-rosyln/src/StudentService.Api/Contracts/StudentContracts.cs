namespace StudentService.Api.Contracts;

public sealed record CreateStudentRequest(
    string Name,
    DateTime? Dob,
    string? Email,
    string? Mobile,
    string InstituteId,
    string? CourseId,
    string? CourseName,
    string? TermName,
    string? SectionName,
    string? Branch,
    string? StudentId,
    string? AdmissionNumber,
    string? ApplicationNumber,
    DateTime? Doa,
    string? AadharNumber);

public sealed record UpdateStatusRequest(bool Enabled);

public sealed record PromoteStudentRequest(
    string BaseInstituteId,
    string BaseCourseId,
    string TargetInstituteId,
    string TargetCourseId,
    string? TargetTerm,
    string? TargetSection,
    string? TargetCourseName);

public sealed record StudentResponse(
    string Id,
    string Name,
    DateTime? Dob,
    string? Email,
    string? Mobile,
    string Status,
    IReadOnlyList<EnrollmentResponse> Enrollments);

public sealed record EnrollmentResponse(
    string InstituteId,
    string? CourseId,
    string? CourseName,
    string? TermName,
    string? SectionName,
    string? Branch,
    string? RollNo,
    string? StudentNumber,
    string Status);
