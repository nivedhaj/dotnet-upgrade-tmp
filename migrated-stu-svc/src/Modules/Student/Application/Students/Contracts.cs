using Ct.BuildingBlocks.Application;
using Ct.Modules.Academics.Domain;

namespace Ct.Modules.Academics.Application.Students;

public sealed record CreateStudentRequest(
    string StudentId,
    string Name,
    string? AdmissionNumber,
    string? ApplicationNumber,
    string? CourseName,
    string? CourseId,
    DateTime? DateOfAdmission,
    DateTime? DateOfBirth,
    string? Email,
    string? Gender,
    string? Mobile,
    string? AadharNumber);

public sealed record UpdateStudentRequest(
    string Name,
    string? Email,
    string? Mobile,
    string? CourseName,
    string? CourseId,
    string? Section,
    string? Branch);

public interface IStudentQueries
{
    Task<PagedResult<StudentListItem>> GetAsync(string? studentId, bool activeOnly, bool tcOnly, int page, int pageSize, CancellationToken ct);
    Task<StudentDetails?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<StudentListItem>> GetAllAsync(bool includeRte, CancellationToken ct);
}

public interface IStudentCommands
{
    Task<StudentDetails> CreateAsync(CreateStudentRequest request, CancellationToken ct);
    Task<bool> EnableAsync(Guid id, CancellationToken ct);
    Task<bool> DisableAsync(Guid id, CancellationToken ct);
    Task<bool> TcAsync(Guid id, CancellationToken ct);
    Task<StudentDetails?> UpdateAsync(Guid id, UpdateStudentRequest request, CancellationToken ct);
}

public sealed record StudentListItem(
    Guid Id,
    string StudentId,
    string Name,
    StudentStatus Status,
    string? CourseName,
    string? TermName,
    string? SectionName);

public sealed record StudentDetails(
    Guid Id,
    string StudentId,
    string Name,
    string? AdmissionNumber,
    string? ApplicationNumber,
    DateTime? DateOfBirth,
    DateTime? DateOfAdmission,
    string? Email,
    string? Gender,
    string? Mobile,
    string? AadharNumber,
    StudentStatus Status,
    string Enrollments,
    string Contacts,
    string Metadata);
