using Ct.Acad.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/anon")]
[Route("api/Anon")]
public sealed class AnonController(AssessmentService assessments, LmsService lms) : ControllerBase
{
    [HttpGet("assessment/publishedAssessments")]
    public async Task<IActionResult> GetPublishedAssessments(CancellationToken ct)
        => Ok(await assessments.GetAllAsync(true, ct));

    [HttpGet("assessment/assessmentById")]
    public async Task<IActionResult> GetAssessmentById([FromQuery] Guid id, CancellationToken ct)
    {
        var a = await assessments.GetByIdAsync(id, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpGet("assessment/tags")]
    public async Task<IActionResult> GetTags(CancellationToken ct) => Ok(await assessments.GetTagsAsync(ct));

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestionsForAnon([FromQuery] string? tag, CancellationToken ct)
        => Ok(await lms.GetQuestionsAsync(tag, null, true, ct));

    [HttpGet("random")]
    public async Task<IActionResult> GetRandomQuestions([FromQuery] int count = 10, [FromQuery] string? tag = null, CancellationToken ct = default)
        => Ok(await lms.GetRandomAsync(count, tag, ct));

    [HttpPost("random")]
    public async Task<IActionResult> SubmitAnswer([FromBody] AnswerRequest request, CancellationToken ct)
    {
        var r = await lms.SubmitAnswerAsync(request.QuestionId, request.Answer, ct);
        return r is null ? NotFound() : Ok(r);
    }
}
