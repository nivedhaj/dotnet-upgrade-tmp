using Microsoft.AspNetCore.Mvc;
using StudentService.Api.Contracts;
using StudentService.Api.Services;

namespace StudentService.Api.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController(IStudentService service) : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<StudentResponse>> Get(string id, CancellationToken ct)
        => (await service.GetAsync(id, ct)) is { } student ? Ok(student) : NotFound();

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StudentResponse>>> Search(
        [FromQuery] string? instituteId,
        [FromQuery] string? search,
        [FromQuery] bool activeOnly = false,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
        => Ok(await service.SearchAsync(instituteId, search, activeOnly, skip, take, ct));

    [HttpPost]
    public async Task<ActionResult<StudentResponse>> Create(
        CreateStudentRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPost("{id}/status")]
    public async Task<IActionResult> Status(
        string id, UpdateStatusRequest request, CancellationToken ct)
    {
        await service.SetEnabledAsync(id, request.Enabled, ct);
        return NoContent();
    }

    [HttpPost("{id}/promote")]
    public async Task<IActionResult> Promote(
        string id, PromoteStudentRequest request, CancellationToken ct)
    {
        await service.PromoteAsync(id, request, ct);
        return NoContent();
    }
}
