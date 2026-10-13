using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ct.Fee.Application.Integrations;

/// <summary>
/// Abstraction over Stu/Redis student roster used by original FeeTxQry / ReportService.
/// Implementations may call the Stu module HTTP API or return empty enrichment.
/// </summary>
public interface IStudentDirectory
{
    Task<StudentDirectoryEntry?> GetAsync(string studentId, CancellationToken ct = default);
    Task<IReadOnlyList<StudentDirectoryEntry>> ListAsync(
        string? courseId = null,
        bool includeRte = true,
        bool includeTc = true,
        CancellationToken ct = default);
}

public sealed record StudentDirectoryEntry(
    string StudentId,
    string? Name,
    string? CourseId,
    string? CourseName,
    string? Section,
    string? Branch,
    string EnrollmentStatus,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Emails)
{
    public bool IsRte => Tags.Any(t => t.Equals("RTE", StringComparison.OrdinalIgnoreCase)
                                    || t.Equals("RightToEducation", StringComparison.OrdinalIgnoreCase));
    public bool IsTc => Tags.Any(t => t.Equals("TC", StringComparison.OrdinalIgnoreCase)
                                  || t.Equals("Transfer", StringComparison.OrdinalIgnoreCase)
                                  || t.Equals("TransferCertificate", StringComparison.OrdinalIgnoreCase));
    public bool IsActive => EnrollmentStatus is "1" or "Active" or "active";
}

/// <summary>No external Stu data — fee module still works offline.</summary>
public sealed class NullStudentDirectory : IStudentDirectory
{
    public Task<StudentDirectoryEntry?> GetAsync(string studentId, CancellationToken ct = default) =>
        Task.FromResult<StudentDirectoryEntry?>(new StudentDirectoryEntry(
            studentId, null, null, null, null, null, "1", Array.Empty<string>(), Array.Empty<string>()));

    public Task<IReadOnlyList<StudentDirectoryEntry>> ListAsync(
        string? courseId = null, bool includeRte = true, bool includeTc = true, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<StudentDirectoryEntry>>(Array.Empty<StudentDirectoryEntry>());
}

/// <summary>HTTP client to Stu module (replaces RedisCache.GetStudentDetails).</summary>
public sealed class HttpStudentDirectory(
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<HttpStudentDirectory> logger) : IStudentDirectory
{
    private string BaseUrl =>
        config["Fee:StudentService:BaseUrl"]
        ?? config["Services:Stu:BaseUrl"]
        ?? "";

    public async Task<StudentDirectoryEntry?> GetAsync(string studentId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl)) return null;
        try
        {
            var client = httpClientFactory.CreateClient("Stu");
            var resp = await client.GetAsync($"{BaseUrl.TrimEnd('/')}/api/stu/student/{Uri.EscapeDataString(studentId)}", ct);
            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadFromJsonAsync<StuStudentPayload>(cancellationToken: ct);
            return json is null ? null : Map(json);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resolve student {StudentId} from Stu service", studentId);
            return null;
        }
    }

    public async Task<IReadOnlyList<StudentDirectoryEntry>> ListAsync(
        string? courseId = null, bool includeRte = true, bool includeTc = true, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(BaseUrl)) return Array.Empty<StudentDirectoryEntry>();
        try
        {
            var client = httpClientFactory.CreateClient("Stu");
            var url = $"{BaseUrl.TrimEnd('/')}/api/stu/student?active=true";
            if (!string.IsNullOrWhiteSpace(courseId))
                url += $"&courseId={Uri.EscapeDataString(courseId)}";
            var resp = await client.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return Array.Empty<StudentDirectoryEntry>();
            var list = await resp.Content.ReadFromJsonAsync<List<StuStudentPayload>>(cancellationToken: ct)
                       ?? [];
            return list.Select(Map)
                .Where(s => includeRte || !s.IsRte)
                .Where(s => includeTc || !s.IsTc)
                .Where(s => s.IsActive)
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to list students from Stu service");
            return Array.Empty<StudentDirectoryEntry>();
        }
    }

    private static StudentDirectoryEntry Map(StuStudentPayload p) => new(
        p.StudentId ?? p.Id ?? "",
        p.Name,
        p.CourseId,
        p.CourseName,
        p.Section ?? p.SecName,
        p.Branch,
        p.EnrollmentStatus ?? p.Status ?? "1",
        p.Tags ?? [],
        p.Emails ?? (string.IsNullOrEmpty(p.Email) ? [] : [p.Email]));

    private sealed class StuStudentPayload
    {
        public string? Id { get; set; }
        public string? StudentId { get; set; }
        public string? Name { get; set; }
        public string? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? Section { get; set; }
        public string? SecName { get; set; }
        public string? Branch { get; set; }
        public string? EnrollmentStatus { get; set; }
        public string? Status { get; set; }
        public string? Email { get; set; }
        public List<string>? Emails { get; set; }
        public List<string>? Tags { get; set; }
    }
}
