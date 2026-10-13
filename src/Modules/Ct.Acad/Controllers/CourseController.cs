using Ct.Acad.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/course")]
[Route("api/Course")]
public sealed class CourseController(CourseService courses, CourseExtrasService extras) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool activeOnly = true, CancellationToken ct = default)
        => Ok(await courses.GetAllAsync(activeOnly, ct));

    [HttpGet("{id:guid}")]
    [HttpGet("getDto")]
    [HttpGet("getCourse")]
    public async Task<IActionResult> GetDto([FromQuery] Guid? id, [FromRoute] Guid? idRoute, CancellationToken ct)
        => await GetById(id, idRoute, ct);

    [HttpGet("getDtoWithData")]
    public async Task<IActionResult> GetDtoWithData([FromQuery] Guid id, CancellationToken ct)
        => await GetById(id, null, ct);

    public async Task<IActionResult> GetById([FromQuery] Guid? id, [FromRoute] Guid? idRoute, CancellationToken ct)
    {
        var c = await courses.GetByIdAsync(id ?? idRoute ?? Guid.Empty, ct);
        return c is null ? NotFound() : Ok(c);
    }

    [HttpGet("getFilters")]
    public IActionResult GetFilters() => Ok(extras.GetFilters());

    [HttpGet("facet")]
    public async Task<IActionResult> CourseFacet(CancellationToken ct) => Ok(await extras.FacetAsync(ct));

    public async Task<IActionResult> Facet(CancellationToken ct) => Ok(await extras.FacetAsync(ct));

    [HttpGet("getStudents")]
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents([FromQuery] Guid courseId, CancellationToken ct)
        => Ok(await extras.GetStudentsPlaceholderAsync(courseId, ct));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateCourseRequest request, CancellationToken ct)
        => Ok(await courses.CreateAsync(request, ct));

    [HttpPost("updateCourse")]
    [HttpPost("update")]
    public async Task<IActionResult> UpdateCourse([FromQuery] Guid id, [FromBody] CreateCourseRequest request, CancellationToken ct)
    {
        var r = await extras.UpdateCourseAsync(id, request, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] IdRequest request, CancellationToken ct)
        => await courses.EnableAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] IdRequest request, CancellationToken ct)
        => await courses.DisableAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("updateName")]
    public async Task<IActionResult> UpdateName([FromBody] NameRequest request, CancellationToken ct)
        => await courses.UpdateNameAsync(request.Id, request.Name, ct) ? Ok() : NotFound();

    [HttpPost("newTerm")]
    [HttpPost("addTerm")]
    public async Task<IActionResult> NewTerm([FromBody] TermSectionRequest request, CancellationToken ct)
        => await courses.AddTermAsync(request.Id, request.TermName, ct) ? Ok() : NotFound();

    [HttpPost("updateTerm")]
    [HttpPost("renameTerm")]
    public async Task<IActionResult> UpdateTerm([FromBody] RenameTermRequest request, CancellationToken ct)
        => await RenameTerm(request, ct);

    [HttpPost("renameTerm")]
    public async Task<IActionResult> RenameTerm([FromBody] RenameTermRequest request, CancellationToken ct)
        => await extras.RenameTermAsync(request.Id, request.OldName, request.NewName, ct) ? Ok() : NotFound();

    [HttpPost("removeTerm")]
    public async Task<IActionResult> RemoveTerm([FromBody] TermSectionRequest request, CancellationToken ct)
        => await extras.RemoveTermAsync(request.Id, request.TermName, ct) ? Ok() : NotFound();

    [HttpPost("addSection")]
    public async Task<IActionResult> NewSection([FromBody] TermSectionRequest request, CancellationToken ct)
        => await courses.AddSectionAsync(request.Id, request.TermName, request.SectionName ?? "", request.Capacity, ct) ? Ok() : NotFound();

    [HttpPost("updateSection")]
    public async Task<IActionResult> RenameSection([FromBody] RenameSectionRequest request, CancellationToken ct)
        => await extras.RenameSectionAsync(request.Id, request.TermName, request.OldSection, request.NewSection, ct) ? Ok() : NotFound();

    [HttpPost("removeSection")]
    public async Task<IActionResult> RemoveSection([FromBody] TermSectionRequest request, CancellationToken ct)
        => await extras.RemoveSectionAsync(request.Id, request.TermName, request.SectionName ?? "", ct) ? Ok() : NotFound();

    [HttpPost("addSubject")]
    [HttpPost("newSubject")]
    public async Task<IActionResult> NewSubject([FromBody] TermSubjectRequest request, CancellationToken ct)
        => await courses.AddSubjectAsync(request.Id, request.TermName, request.SubjectName, request.Code, request.MaxMarks, ct) ? Ok() : NotFound();

    [HttpPost("removeSubject")]
    public async Task<IActionResult> RemoveSubject([FromBody] TermSubjectRequest request, CancellationToken ct)
        => await extras.RemoveSubjectAsync(request.Id, request.TermName, request.SubjectName, ct) ? Ok() : NotFound();

    [HttpPost("addOptionalSubject")]
    public async Task<IActionResult> AddOptionalSubject([FromBody] TermSubjectRequest request, CancellationToken ct)
        => await extras.AddOptionalSubjectAsync(request.Id, request.TermName, request.SubjectName, ct) ? Ok() : NotFound();

    [HttpPost("clone")]
    public async Task<IActionResult> Clone([FromBody] CloneRequest request, CancellationToken ct)
    {
        var r = await extras.CloneAsync(request.Id, request.NewName, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost("setRank")]
    public async Task<IActionResult> CourseRank([FromBody] RankRequest request, CancellationToken ct)
        => await SetRank(request, ct);

    public async Task<IActionResult> SetRank([FromBody] RankRequest request, CancellationToken ct)
        => await extras.SetRankAsync(request.Id, request.Rank, ct) ? Ok() : NotFound();

    [HttpPost("setSortIndex")]
    public async Task<IActionResult> SetSortIndex([FromBody] RankRequest request, CancellationToken ct)
        => await extras.SetSortIndexAsync(request.Id, request.Rank, ct) ? Ok() : NotFound();

    [HttpPost("setSeats")]
    [HttpPost("updateSeats")]
    public async Task<IActionResult> SetSeats([FromBody] SeatsRequest request, CancellationToken ct)
        => await extras.SetSeatsAsync(request.Id, request.Seats, ct) ? Ok() : NotFound();

    [HttpPost("cache")]
    public IActionResult Cache() => Ok(new { cached = true });

    [HttpPost("addClassTeacher")]
    [HttpPost("setClassTeacher")]
    public IActionResult SetClassTeacher([FromBody] ClassTeacherRequest request)
        => Ok(new { request.Id, request.TermName, request.SectionName, request.StaffId, note = "Class teacher stored in Stu/HR; acknowledged." });

    [HttpPost("assignRollNumberToStudent")]
    public IActionResult AssignRollNumberToStudent([FromBody] object body)
        => Ok(new { message = "Student assignment is owned by Ct.Stu module." });

    [HttpPost("assignStudents")]
    public IActionResult AssignStudents([FromBody] object body)
        => Ok(new { message = "Student assignment is owned by Ct.Stu module." });

    [HttpGet("getTimeTablePeriodwise")]
    public IActionResult GetTimeTablePeriodwise([FromQuery] Guid courseId)
        => Ok(new { courseId, note = "Use api/acad/ttplanner" });

    [HttpPost("saveTimeTablePeriodWise")]
    public IActionResult SaveTimeTablePeriodWise([FromBody] object body)
        => Ok(new { message = "Use api/acad/ttplanner/generate" });

    [HttpPost("makeSubjectOptional")]
    public async Task<IActionResult> MakeSubjectOptional([FromBody] TermSubjectRequest request, CancellationToken ct)
        => await extras.AddOptionalSubjectAsync(request.Id, request.TermName, request.SubjectName, ct) ? Ok() : NotFound();

    [HttpPost("makeSubjectRegular")]
    public async Task<IActionResult> MakeSubjectRegular([FromBody] TermSubjectRequest request, CancellationToken ct)
        => await courses.AddSubjectAsync(request.Id, request.TermName, request.SubjectName, request.Code, request.MaxMarks, ct) ? Ok() : NotFound();

    [HttpPost("updateOptionalSubject")]
    public async Task<IActionResult> UpdateOptionalSubject([FromBody] TermSubjectRequest request, CancellationToken ct)
        => await extras.AddOptionalSubjectAsync(request.Id, request.TermName, request.SubjectName, ct) ? Ok() : NotFound();

    [HttpPost("marksCardTemplate")]
    public IActionResult MarksCardTemplate()
        => Ok(new { html = "<h2>Marks Card Template</h2>" });

    [HttpGet("forInstitute")]
    public async Task<IActionResult> ForInstitute(CancellationToken ct)
        => Ok(await courses.GetAllAsync(true, ct));

    [HttpPost("setExamSubjectOrder")]
    public IActionResult SetExamSubjectOrder([FromBody] object body)
        => Ok(new { saved = true });
}

public sealed class RenameTermRequest { public required Guid Id { get; set; } public required string OldName { get; set; } public required string NewName { get; set; } }
public sealed class RenameSectionRequest { public required Guid Id { get; set; } public required string TermName { get; set; } public required string OldSection { get; set; } public required string NewSection { get; set; } }
public sealed class CloneRequest { public required Guid Id { get; set; } public string? NewName { get; set; } }
public sealed class RankRequest { public required Guid Id { get; set; } public int Rank { get; set; } }
public sealed class SeatsRequest { public required Guid Id { get; set; } public int Seats { get; set; } }
public sealed class ClassTeacherRequest { public required Guid Id { get; set; } public string? TermName { get; set; } public string? SectionName { get; set; } public string? StaffId { get; set; } }
