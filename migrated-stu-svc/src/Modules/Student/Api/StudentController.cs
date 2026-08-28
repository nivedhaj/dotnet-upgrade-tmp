using Ct.BuildingBlocks.Application;
using Ct.Modules.Academics.Application.Students;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Modules.Academics.Api;

[ApiController, Route("api/stu/[controller]")]
public sealed class StudentController(IStudentQueries queries, IStudentCommands commands) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<StudentListItem>>>> Get(
        [FromQuery] string? id,
        [FromQuery] bool activeStudentsOnly = false,
        [FromQuery] bool todaysAbsenteesOnly = false,
        [FromQuery] bool tcStudentsOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int per = 50,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(id))
        {
            if (!Guid.TryParse(id, out var studentId))
                return BadRequest("id must be a valid GUID.");

            var student = await queries.GetByIdAsync(studentId, ct);
            return student is null
                ? NotFound()
                : Ok(ApiResponse<StudentDetails>.Ok(student, "Retrieved student"));
        }

        var result = await queries.GetAsync(null, activeStudentsOnly, tcStudentsOnly, page, per, ct);
        return Ok(ApiResponse<PagedResult<StudentListItem>>.Ok(result, "Retrieved students", result.TotalRecords));
    }

    [HttpGet("all")]
    public async Task<IActionResult> All([FromQuery] bool includeRTE = false, CancellationToken ct = default) =>
        Ok(ApiResponse<IReadOnlyList<StudentListItem>>.Ok(
            await queries.GetAllAsync(includeRTE, ct),
            $"Retrieved students. RTE included: {includeRTE}"));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ById(Guid id, CancellationToken ct) =>
        (await queries.GetByIdAsync(id, ct)) is { } student
            ? Ok(ApiResponse<StudentDetails>.Ok(student, "Retrieved student"))
            : NotFound();

    [HttpPost]
    public async Task<IActionResult> Post(CreateStudentRequest request, CancellationToken ct)
    {
        var student = await commands.CreateAsync(request, ct);
        return CreatedAtAction(nameof(ById), new { id = student.Id }, student);
    }

    [HttpPost("enable/{id:guid}")]
    public async Task<IActionResult> Enable(Guid id, CancellationToken ct) =>
        await commands.EnableAsync(id, ct) ? Ok(id) : NotFound();

    [HttpPost("disable/{id:guid}")]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct) =>
        await commands.DisableAsync(id, ct) ? Ok(id) : NotFound();

    [HttpPost("tc/{id:guid}")]
    public async Task<IActionResult> Tc(Guid id, CancellationToken ct) =>
        await commands.TcAsync(id, ct) ? Ok(id) : NotFound();

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateStudentRequest request, CancellationToken ct) =>
        (await commands.UpdateAsync(id, request, ct)) is { } student
            ? Ok(student)
            : NotFound();
}
