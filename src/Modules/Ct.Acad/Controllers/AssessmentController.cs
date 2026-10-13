using Ct.Acad.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/assessment")]
[Route("api/Assessment")]
public sealed class AssessmentController(AssessmentService assessments, AssessmentExtrasService extras) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAssessments(CancellationToken ct)
        => Ok(await assessments.GetAllAsync(false, ct));

    [HttpGet("publishedAssessments")]
    public async Task<IActionResult> GetPublishedAssessments(CancellationToken ct)
        => Ok(await assessments.GetAllAsync(true, ct));

    [HttpGet("assessmentById")]
    public async Task<IActionResult> GetAssessmentById([FromQuery] Guid id, CancellationToken ct)
    {
        var a = await assessments.GetByIdAsync(id, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentRequest request, CancellationToken ct)
        => Ok(await assessments.CreateAsync(request, ct));

    [HttpPost("update")]
    public async Task<IActionResult> UpdateAssessment([FromQuery] Guid id, [FromBody] CreateAssessmentRequest request, CancellationToken ct)
        => await assessments.UpdateAsync(id, request, ct) ? Ok() : NotFound();

    [HttpPost("publish")]
    public async Task<IActionResult> PublishAssessment([FromBody] IdRequest request, CancellationToken ct)
        => await assessments.PublishAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("unpublish")]
    public async Task<IActionResult> UnPublishAssessment([FromBody] IdRequest request, CancellationToken ct)
        => await assessments.UnpublishAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("archive")]
    public async Task<IActionResult> ArchiveAssessment([FromBody] IdRequest request, CancellationToken ct)
        => await assessments.ArchiveAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitAssessment([FromBody] SubmitRequest request, CancellationToken ct)
    {
        var id = await assessments.SubmitAsync(request.AssessmentId, request.StudentId, request.StudentName, request.Answers ?? new(), ct);
        return Ok(new { id });
    }

    [HttpGet("submissions")]
    public async Task<IActionResult> GetAssessmentSubmissions([FromQuery] Guid assessmentId, CancellationToken ct)
        => Ok(await assessments.GetSubmissionsAsync(assessmentId, ct));

    [HttpPost("evaluate")]
    public async Task<IActionResult> EvaluateAssessment([FromBody] EvaluateRequest request, CancellationToken ct)
        => await assessments.EvaluateAsync(request.SubmissionId, request.Score, ct) ? Ok() : NotFound();

    [HttpGet("tags")]
    public async Task<IActionResult> GetTags(CancellationToken ct) => Ok(await assessments.GetTagsAsync(ct));

    [HttpPost("tag")]
    public async Task<IActionResult> CreateTag([FromBody] TagRequest request, CancellationToken ct)
        => Ok(await assessments.CreateTagAsync(request.Name, ct));

    [HttpPost("enableTag")]
    public async Task<IActionResult> EnableTag([FromBody] IdRequest request, CancellationToken ct)
        => await assessments.SetTagEnabledAsync(request.Id, true, ct) ? Ok() : NotFound();

    [HttpPost("disableTag")]
    public async Task<IActionResult> DisableTag([FromBody] IdRequest request, CancellationToken ct)
        => await assessments.SetTagEnabledAsync(request.Id, false, ct) ? Ok() : NotFound();

    [HttpPost("updateSubject")]
    public async Task<IActionResult> UpdateAssessmentSubject([FromBody] NameRequest request, CancellationToken ct)
        => await extras.UpdateSubjectAsync(request.Id, request.Name, ct) ? Ok() : NotFound();

    [HttpPost("updateDuration")]
    public async Task<IActionResult> UpdateAssessmentDuration([FromBody] DurationRequest request, CancellationToken ct)
        => await extras.UpdateDurationAsync(request.Id, request.Minutes, ct) ? Ok() : NotFound();

    [HttpPost("updateTotalMarks")]
    public async Task<IActionResult> UpdateAssessmentTotalMarks([FromBody] MarksRequest request, CancellationToken ct)
        => await extras.UpdateTotalMarksAsync(request.Id, request.Marks, ct) ? Ok() : NotFound();

    [HttpPost("unarchive")]
    public async Task<IActionResult> UnarchiveAssessment([FromBody] IdRequest request, CancellationToken ct)
        => await extras.UnarchiveAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpGet("studentSumbissions")]
    [HttpGet("studentSubmissions")]
    public async Task<IActionResult> StudentSubmissions([FromQuery] string studentId, CancellationToken ct)
        => Ok(await extras.StudentSubmissionsAsync(studentId, ct));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    { await extras.RefreshAsync(ct); return Ok(); }

    [HttpGet("submissionsCount")]
    public async Task<IActionResult> GetSubmissionsCount([FromQuery] Guid assessmentId, CancellationToken ct)
        => Ok(await extras.SubmissionsCountAsync(assessmentId, ct));

    [HttpGet("getAssessmentByQuestionIds")]
    public async Task<IActionResult> GetAssessmentsByQuestionIds([FromQuery] List<Guid> ids, CancellationToken ct)
        => Ok(await extras.ByQuestionIdsAsync(ids ?? [], ct));
}

public sealed class DurationRequest { public required Guid Id { get; set; } public int Minutes { get; set; } }
public sealed class MarksRequest { public required Guid Id { get; set; } public decimal Marks { get; set; } }


public sealed class SubmitRequest
{
    public required Guid AssessmentId { get; set; }
    public required string StudentId { get; set; }
    public string? StudentName { get; set; }
    public Dictionary<string, string>? Answers { get; set; }
}
public sealed class EvaluateRequest { public required Guid SubmissionId { get; set; } public decimal Score { get; set; } }
