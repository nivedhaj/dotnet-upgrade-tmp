using System.Net.Http.Json;
using Ct.Connect.Application;
using Ct.Connect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Ct.Connect.Controllers;

[ApiController]
[Route("api/connect/facet")]
[Route("api/facet")]
[Route("api/Facet")]
public sealed class FacetController(
    ConnectCmd cmd,
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<FacetController> logger) : ControllerBase
{
    [HttpGet("forStudent")]
    public IActionResult ForStudent()
        => Ok(new
        {
            facets = new[]
            {
                new { key = "course", label = "Course" },
                new { key = "section", label = "Section" },
                new { key = "tag", label = "Tag" },
                new { key = "status", label = "Status" }
            }
        });

    [HttpGet("forStaff")]
    public IActionResult ForStaff()
        => Ok(new { facets = new[] { new { key = "department", label = "Department" }, new { key = "role", label = "Role" }, new { key = "tag", label = "Tag" } } });

    [HttpGet("forParent")]
    [HttpGet("forParents")]
    public IActionResult ForParent()
        => Ok(new { facets = new[] { new { key = "course", label = "Course (child)" }, new { key = "section", label = "Section (child)" }, new { key = "tag", label = "Tag" } } });

    [HttpPost("{id}/applyForStudents")]
    [HttpPost("applyForStudents")]
    public async Task<ActionResult<object>> ApplyForStudents([FromBody] FacetApplyRequest request, CancellationToken ct)
    {
        var ids = await ResolveStudentIdsAsync(request, ct);
        return Ok(new { message = "Student Ids Retrieved", facets = request.Facets, studentIds = ids });
    }

    [HttpPost("{id}/applyForStaff")]
    [HttpPost("applyForStaff")]
    public ActionResult<object> ApplyForStaff([FromBody] FacetApplyRequest request)
        => Ok(new { message = "Staff Ids Retrieved", facets = request.Facets, staffIds = Array.Empty<string>() });

    [HttpPost("{id}/applyForParents")]
    [HttpPost("applyForParents")]
    public async Task<ActionResult<object>> ApplyForParents([FromBody] FacetApplyRequest request, CancellationToken ct)
    {
        var ids = await ResolveStudentIdsAsync(request, ct);
        return Ok(new { message = "Parent contacts Retrieved", facets = request.Facets, studentIds = ids, contacts = Array.Empty<object>() });
    }

    [HttpPost("lookup")]
    [HttpPost]
    public ActionResult<IReadOnlyList<ContactLookupDto>> Lookup([FromBody] ContactLookupRequest request)
        => Ok(cmd.Lookup(request.Contacts));

    private async Task<List<string>> ResolveStudentIdsAsync(FacetApplyRequest request, CancellationToken ct)
    {
        var baseUrl = config["Connect:StudentService:BaseUrl"] ?? config["Services:Stu:BaseUrl"] ?? config["Fee:StudentService:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl)) return [];

        try
        {
            var client = httpClientFactory.CreateClient("Stu");
            var tag = request.Facets.FirstOrDefault(f => f.Key?.Equals("tag", StringComparison.OrdinalIgnoreCase) == true)?.Values.FirstOrDefault();
            var url = string.IsNullOrEmpty(tag)
                ? $"{baseUrl.TrimEnd('/')}/api/stu/student/all"
                : $"{baseUrl.TrimEnd('/')}/api/stu/student/byTag?tag={Uri.EscapeDataString(tag)}";
            var resp = await client.GetAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return [];
            var list = await resp.Content.ReadFromJsonAsync<List<JsonStudent>>(cancellationToken: ct) ?? [];
            return list.Select(s => s.Id ?? s.StudentId ?? "").Where(x => x.Length > 0).ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Stu facet resolve failed");
            return [];
        }
    }

    private sealed class JsonStudent
    {
        public string? Id { get; set; }
        public string? StudentId { get; set; }
    }
}
