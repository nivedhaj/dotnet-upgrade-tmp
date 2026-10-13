using Ct.Modules.Admissions.Application.Commands;
using Ct.Modules.Admissions.Application.Dtos;
using Ct.Modules.Admissions.Application.Queries;
using Ct.Modules.Admissions.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Modules.Admissions.Api;

/// <summary>
/// Full surface of original ct.stu StudentController (api/Student),
/// backed by Postgres command/query services.
/// </summary>
[ApiController]
[Route("api/stu/student")]
[Route("api/student")]
public sealed class StudentsController(
    IStudentCommandService commands,
    IStudentQueryService queries) : ControllerBase
{
    private string? Actor => User?.Identity?.Name ?? Request.Headers["X-User-Id"].FirstOrDefault();

    [HttpGet]
    public async Task<ActionResult<PagedResult<StudentDto>>> Get(
        [FromQuery] string? q, [FromQuery] string? instId,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        => Ok(await queries.SearchAsync(q, instId, page, pageSize, ct));

    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> All([FromQuery] string? instId, CancellationToken ct)
        => Ok(await queries.GetAllAsync(instId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentDto>> GetById(Guid id, CancellationToken ct)
    {
        var s = await queries.GetByIdAsync(id, ct);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpGet("getDto")]
    public async Task<ActionResult<StudentDto>> GetDto([FromQuery] Guid id, CancellationToken ct)
        => await GetById(id, ct);

    [HttpGet("byTag")]
    public async Task<ActionResult<IReadOnlyList<StudentDto>>> ByTag([FromQuery] string tag, CancellationToken ct)
        => Ok(await queries.GetByTagAsync(tag, ct));

    [HttpGet("facet")]
    public async Task<ActionResult<IReadOnlyList<StudentFacetDto>>> Facet([FromQuery] string? instId, CancellationToken ct)
        => Ok(await queries.GetFacetsAsync(instId, ct));

    [HttpGet("sectionwise")]
    public async Task<ActionResult<IReadOnlyList<SectionwiseStudentDto>>> Sectionwise([FromQuery] string? instId, CancellationToken ct)
        => Ok(await queries.GetSectionwiseAsync(instId, ct));

    [HttpPost]
    [HttpPost("create")]
    public async Task<ActionResult<object>> Post([FromBody] CreateStudentRequest request, CancellationToken ct)
    {
        var id = await commands.CreateAsync(request, Actor, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromQuery] Guid id, CancellationToken ct)
    {
        await commands.EnableAsync(id, Actor, ct);
        return Ok(new { id });
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromQuery] Guid id, CancellationToken ct)
    {
        await commands.DisableAsync(id, Actor, ct);
        return Ok(new { id });
    }

    [HttpPost("tc")]
    public async Task<IActionResult> TC([FromBody] TcRequest request, CancellationToken ct)
    {
        await commands.IssueTcAsync(request, Actor, ct);
        return Ok(new { request.Id });
    }

    [HttpPost("AssignStudentId")]
    public async Task<IActionResult> AssignStudentId([FromBody] AssignStudentIdRequest request, CancellationToken ct)
    {
        await commands.AssignStudentIdAsync(request, ct);
        return Ok(request);
    }

    [HttpPost("promote")]
    public async Task<IActionResult> Promote([FromBody] PromoteStudentRequest request, CancellationToken ct)
    {
        var ok = await commands.PromoteAsync(request, Actor, ct);
        return ok ? Ok(new { request.Id }) : BadRequest();
    }

    [HttpPost("setHomeAddress")]
    public async Task<IActionResult> SetHomeAddress([FromBody] SetHomeAddressRequest request, CancellationToken ct)
    {
        await commands.SetHomeAddressAsync(request, ct);
        return Ok();
    }

    [HttpPost("setHomePhone")]
    public async Task<IActionResult> SetHomePhone([FromBody] SetContactRequest request, CancellationToken ct)
    {
        await commands.SetHomePhoneAsync(request.Id, request.Value, ct);
        return Ok();
    }

    [HttpPost("setPrimaryEmail")]
    public async Task<IActionResult> SetPrimaryEmail([FromBody] SetContactRequest request, CancellationToken ct)
    {
        await commands.SetPrimaryEmailAsync(request.Id, request.Value, ct);
        return Ok();
    }

    [HttpPost("setPrimaryMobile")]
    public async Task<IActionResult> SetPrimaryMobile([FromBody] SetContactRequest request, CancellationToken ct)
    {
        await commands.SetPrimaryMobileAsync(request.Id, request.Value, ct);
        return Ok();
    }

    [HttpPost("updateTags")]
    public async Task<IActionResult> SetTags([FromQuery] Guid id, [FromBody] List<string> tags, CancellationToken ct)
    {
        await commands.SetTagsAsync(id, tags, ct);
        return Ok();
    }

    [HttpPost("updateCourse")]
    public async Task<IActionResult> UpdateCourse([FromBody] UpdateCourseRequest request, CancellationToken ct)
    {
        await commands.UpdateCourseAsync(request.Id, request.InstId, request.CourseId, request.CourseName, request.TermName, request.SectionName, request.Branch, ct);
        return Ok();
    }

    [HttpPost("updateGender")]
    public async Task<IActionResult> UpdateGender([FromBody] UpdateGenderRequest request, CancellationToken ct)
    {
        await commands.UpdateGenderAsync(request.Id, request.Gender, ct);
        return Ok();
    }

    [HttpPost("updateFatherDetails")]
    public async Task<IActionResult> UpdateFatherDetails([FromBody] UpdateParentDetailsRequest request, CancellationToken ct)
    {
        await commands.UpdateParentDetailsAsync(request, RelationType.Father, ct);
        return Ok();
    }

    [HttpPost("updateMotherDetails")]
    public async Task<IActionResult> UpdateMotherDetails([FromBody] UpdateParentDetailsRequest request, CancellationToken ct)
    {
        await commands.UpdateParentDetailsAsync(request, RelationType.Mother, ct);
        return Ok();
    }

    [HttpPost("updateGuardianDetails")]
    public async Task<IActionResult> UpdateGuardianDetails([FromBody] UpdateParentDetailsRequest request, CancellationToken ct)
    {
        await commands.UpdateParentDetailsAsync(request, RelationType.Guardian, ct);
        return Ok();
    }

    [HttpPost("updateFatherEmail")]
    public async Task<IActionResult> UpdateFatherEmail([FromBody] SetContactRequest request, CancellationToken ct)
    {
        await commands.UpdateParentContactAsync(request.Id, RelationType.Father, ContactType.Email, request.Value, ct);
        return Ok();
    }

    [HttpPost("updateFatherMobile")]
    public async Task<IActionResult> UpdateFatherMobile([FromBody] SetContactRequest request, CancellationToken ct)
    {
        await commands.UpdateParentContactAsync(request.Id, RelationType.Father, ContactType.Mobile, request.Value, ct);
        return Ok();
    }

    [HttpPost("updateMotherEmail")]
    public async Task<IActionResult> UpdateMotherEmail([FromBody] SetContactRequest request, CancellationToken ct)
    {
        await commands.UpdateParentContactAsync(request.Id, RelationType.Mother, ContactType.Email, request.Value, ct);
        return Ok();
    }

    [HttpPost("updateMotherMobile")]
    public async Task<IActionResult> UpdateMotherMobile([FromBody] SetContactRequest request, CancellationToken ct)
    {
        await commands.UpdateParentContactAsync(request.Id, RelationType.Mother, ContactType.Mobile, request.Value, ct);
        return Ok();
    }

    [HttpPost("updateGuardianEmail")]
    public async Task<IActionResult> UpdateGuardianEmail([FromBody] SetContactRequest request, CancellationToken ct)
    {
        await commands.UpdateParentContactAsync(request.Id, RelationType.Guardian, ContactType.Email, request.Value, ct);
        return Ok();
    }

    [HttpPost("updateGuardianMobile")]
    public async Task<IActionResult> UpdateGuardianMobile([FromBody] SetContactRequest request, CancellationToken ct)
    {
        await commands.UpdateParentContactAsync(request.Id, RelationType.Guardian, ContactType.Mobile, request.Value, ct);
        return Ok();
    }

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromBody] UpdateStudentRequest request, CancellationToken ct)
    {
        await commands.UpdateAsync(request, Actor, ct);
        return Ok(new { request.Id });
    }

    [HttpPost("setRollNo")]
    public async Task<IActionResult> SetRollNo([FromBody] SetRollNoRequest request, CancellationToken ct)
    {
        await commands.SetRollNoAsync(request.Id, request.InstId, request.RollNo, ct);
        return Ok();
    }

    [HttpPost("getFilters")]
    public async Task<ActionResult<object>> GetFilters([FromBody] StudentFilterRequest? filter, CancellationToken ct)
        => Ok(await queries.GetFiltersAsync(filter?.InstId, ct));

    [HttpPost("getContacts")]
    public async Task<ActionResult<IReadOnlyList<StudentContactDto>>> GetContacts([FromBody] List<Guid> ids, CancellationToken ct)
        => Ok(await queries.GetContactsAsync(ids, ct));

    [HttpPost("getContactsFromFacet")]
    public async Task<ActionResult<IReadOnlyList<StudentContactDto>>> GetContactsFromFacet(
        [FromQuery] string? instId, [FromQuery] string? tag, CancellationToken ct)
    {
        IReadOnlyList<StudentDto> students = string.IsNullOrWhiteSpace(tag)
            ? await queries.GetAllAsync(instId, ct)
            : await queries.GetByTagAsync(tag, ct);
        return Ok(await queries.GetContactsAsync(students.Select(s => s.Id).ToList(), ct));
    }

    [HttpPost("flat")]
    public async Task<ActionResult<IReadOnlyList<StudentFlatDto>>> Flat([FromBody] List<Guid> ids, CancellationToken ct)
        => Ok(await queries.GetFlatDataAsync(ids, ct));

    /// <summary>Port of fee module callback POST api/stu/student/feesReceivable.</summary>
    [HttpPost("feesReceivable")]
    public async Task<IActionResult> FeesReceivable([FromBody] FeesReceivableRequest request, CancellationToken ct)
    {
        await commands.UpdateFeesReceivableAsync(request, ct);
        return Ok(new { message = "updated" });
    }

    [HttpPost("uploadPhoto")]
    public async Task<IActionResult> UploadPhoto([FromQuery] Guid id, [FromQuery] string photoUrl, CancellationToken ct)
    {
        await commands.UploadPhotoAsync(id, photoUrl, ct);
        return Ok();
    }

    // Stubs retained for API parity (SMS/email/cache/timetable need Connect/Acad integrations)
    [HttpPost("sms/absentees")]
    [HttpPost("sms/parent")]
    [HttpPost("email/parent")]
    [HttpPost("smsRecipients")]
    [HttpPost("recipients")]
    [HttpPost("cache")]
    [HttpPost("applyEvents")]
    [HttpPost("timetable")]
    public IActionResult IntegrationPlaceholder()
        => Ok(new { message = "Endpoint reserved — wire Ct.Connect / Acad for full SMS, email, cache, timetable." });
}
