using StudentService.Api.Contracts;

namespace StudentService.Api.Services;

public interface IStudentService
{
    Task<StudentResponse?> GetAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<StudentResponse>> SearchAsync(string? instituteId, string? search, bool activeOnly, int skip, int take, CancellationToken ct);
    Task<StudentResponse> CreateAsync(CreateStudentRequest request, CancellationToken ct);
    Task SetEnabledAsync(string id, bool enabled, CancellationToken ct);
    Task PromoteAsync(string id, PromoteStudentRequest request, CancellationToken ct);
}
