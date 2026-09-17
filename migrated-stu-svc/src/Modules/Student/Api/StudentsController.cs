using Ct.Core.Application;
using Ct.Modules.Admissions.Application.Commands;
using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Modules.Admissions.Api;

/// <summary>
/// Preserves legacy StudentController routes (spec section 50). This
/// controller currently implements the core student/enrollment vertical.
/// Attendance, fees, IEP, essentials, documents, baseline, report, and
/// recipient/facet routes are tracked separately — see
/// docs/migration-matrix.md and docs/api-migration.md for exactly which
/// routes are implemented vs. pending, per spec rule 22 (do not silently
/// remove an endpoint) and rule 121 (do not fabricate verification).
/// </summary>
[ApiController]
[Route("")]
public class StudentsController(
    IStudentCommandService commands,
    IStudentQueryService queries,
    IUserContext userContext) : ControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<PagedResult<StudentDto>>>> GetAll(
        [FromQuery] string? instId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
    {
        var result = await queries.SearchAsync(searchText: null, instId, page, pageSize, ct);
        return Ok(ApiResponse<PagedResult<StudentDto>>.Ok(result));
    }

    [HttpGet("getDto")]
    public async Task<ActionResult<ApiResponse<StudentDto>>> GetDto([FromQuery] Guid id, CancellationToken ct = default)
    {
        var student = await queries.GetByIdAsync(id, ct);
        return student is null
            ? NotFound(ApiResponse<StudentDto>.Fail("Student not found."))
            : Ok(ApiResponse<StudentDto>.Ok(student));
    }

    [HttpGet("getStudentsByTag")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentDto>>>> GetByTag([FromQuery] string tag, CancellationToken ct = default)
    {
        var students = await queries.GetByTagAsync(tag, ct);
        return Ok(ApiResponse<IReadOnlyList<StudentDto>>.Ok(students));
    }

    [HttpPost("flatData")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentFlatDto>>>> FlatData([FromBody] IReadOnlyList<Guid> ids, CancellationToken ct = default)
    {
        var data = await queries.GetFlatDataAsync(ids, ct);
        return Ok(ApiResponse<IReadOnlyList<StudentFlatDto>>.Ok(data));
    }

    [HttpPost("create")]
    public async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] CreateStudentRequest request, CancellationToken ct = default)
    {
        var id = await commands.CreateAsync(request, userContext.UserId, ct);
        return Ok(ApiResponse<Guid>.Ok(id));
    }

    [HttpPost("enable")]
    public async Task<ActionResult<ApiResponse<object>>> Enable([FromQuery] Guid id, CancellationToken ct = default)
    {
        await commands.EnableAsync(id, userContext.UserId, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPost("disable")]
    public async Task<ActionResult<ApiResponse<object>>> Disable([FromQuery] Guid id, CancellationToken ct = default)
    {
        await commands.DisableAsync(id, userContext.UserId, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPost("promote")]
    public async Task<ActionResult<ApiResponse<bool>>> Promote([FromBody] PromoteStudentRequest request, CancellationToken ct = default)
    {
        var promoted = await commands.PromoteAsync(request, userContext.UserId, ct);
        return Ok(ApiResponse<bool>.Ok(promoted));
    }

    [HttpPost("setRollNo")]
    public async Task<ActionResult<ApiResponse<object>>> SetRollNo([FromQuery] Guid id, [FromQuery] string instId, [FromQuery] string rollNo, CancellationToken ct = default)
    {
        await commands.SetRollNoAsync(id, instId, rollNo, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPost("updateCourse")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateCourse(
        [FromQuery] Guid id, [FromQuery] string instId, [FromQuery] string? courseId, [FromQuery] string? courseName,
        [FromQuery] string? termName, [FromQuery] string? sectionName, [FromQuery] string? branch, CancellationToken ct = default)
    {
        await commands.UpdateCourseAsync(id, instId, courseId, courseName, termName, sectionName, branch, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPost("setPrimaryEmail")]
    public async Task<ActionResult<ApiResponse<object>>> SetPrimaryEmail([FromQuery] Guid id, [FromQuery] string email, CancellationToken ct = default)
    {
        await commands.SetPrimaryEmailAsync(id, email, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPost("setPrimaryMobile")]
    public async Task<ActionResult<ApiResponse<object>>> SetPrimaryMobile([FromQuery] Guid id, [FromQuery] string mobile, CancellationToken ct = default)
    {
        await commands.SetPrimaryMobileAsync(id, mobile, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }

    [HttpPost("updateTags")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateTags([FromQuery] Guid id, [FromBody] IReadOnlyList<string> tags, CancellationToken ct = default)
    {
        await commands.SetTagsAsync(id, tags, ct);
        return Ok(ApiResponse<object>.Ok(new { }));
    }
}
