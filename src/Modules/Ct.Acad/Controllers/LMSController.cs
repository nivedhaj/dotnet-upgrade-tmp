using Ct.Acad.Application;
using Ct.Acad.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Acad.Controllers;

[ApiController]
[Route("api/acad/lms")]
[Route("api/LMS")]
public sealed class LMSController(LmsService lms, AssessmentService assessments) : ControllerBase
{
    [HttpPost("question")]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request, CancellationToken ct)
        => Ok(await lms.CreateQuestionAsync(request, ct));

    [HttpGet("questions")]
    public async Task<IActionResult> GetQuestions([FromQuery] string? tag, [FromQuery] string? subject, [FromQuery] bool publishedOnly = false, CancellationToken ct = default)
        => Ok(await lms.GetQuestionsAsync(tag, subject, publishedOnly, ct));

    [HttpPost("updateQuestion")]
    public async Task<IActionResult> UpdateQuestion([FromQuery] Guid id, [FromBody] CreateQuestionRequest request, CancellationToken ct)
        => await lms.UpdateQuestionAsync(id, request, ct) ? Ok() : NotFound();

    [HttpPost("enableQuestion")]
    public async Task<IActionResult> EnableQuestion([FromBody] IdRequest request, CancellationToken ct)
        => await lms.SetStatusAsync(request.Id, QuestionStatus.Published, ct) ? Ok() : NotFound();

    [HttpPost("disableQuestion")]
    public async Task<IActionResult> DisableQuestion([FromBody] IdRequest request, CancellationToken ct)
        => await lms.SetStatusAsync(request.Id, QuestionStatus.Disabled, ct) ? Ok() : NotFound();

    [HttpPost("publish")]
    public async Task<IActionResult> PublishQuestions([FromBody] List<Guid> ids, CancellationToken ct)
    {
        foreach (var id in ids) await lms.SetStatusAsync(id, QuestionStatus.Published, ct);
        return Ok();
    }

    [HttpPost("unpublish")]
    public async Task<IActionResult> UnpublishQuestions([FromBody] List<Guid> ids, CancellationToken ct)
    {
        foreach (var id in ids) await lms.SetStatusAsync(id, QuestionStatus.Draft, ct);
        return Ok();
    }

    [HttpPost("archive")]
    public async Task<IActionResult> ArchiveQuestion([FromBody] IdRequest request, CancellationToken ct)
        => await lms.SetStatusAsync(request.Id, QuestionStatus.Archived, ct) ? Ok() : NotFound();

    [HttpPost("unarchive")]
    public async Task<IActionResult> UnArchiveQuestion([FromBody] IdRequest request, CancellationToken ct)
        => await lms.SetStatusAsync(request.Id, QuestionStatus.Draft, ct) ? Ok() : NotFound();

    [HttpPost("updateQuestionsTags")]
    public async Task<IActionResult> UpdateQuestionsTags([FromBody] UpdateTagsRequest request, CancellationToken ct)
        => await lms.UpdateQuestionsTagsAsync(request.Ids, request.Tags, ct) ? Ok() : NotFound();

    [HttpGet("random")]
    public async Task<IActionResult> GetRandomQuestions([FromQuery] int count = 10, [FromQuery] string? tag = null, CancellationToken ct = default)
        => Ok(await lms.GetRandomAsync(count, tag, ct));

    [HttpPost("random")]
    public async Task<IActionResult> SubmitAnswer([FromBody] AnswerRequest request, CancellationToken ct)
    {
        var r = await lms.SubmitAnswerAsync(request.QuestionId, request.Answer, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost("remodelQuestion")]
    public async Task<IActionResult> RemodelQuestion([FromQuery] Guid id, [FromBody] CreateQuestionRequest request, CancellationToken ct)
        => await lms.UpdateQuestionAsync(id, request, ct) ? Ok() : NotFound();

    [HttpGet("tags")]
    public async Task<IActionResult> GetTags(CancellationToken ct) => Ok(await assessments.GetTagsAsync(ct));

    [HttpPost("tag")]
    public async Task<IActionResult> CreateTag([FromBody] TagRequest request, CancellationToken ct)
        => Ok(await assessments.CreateTagAsync(request.Name, ct));

    [HttpPost("imageTag")]
    public async Task<IActionResult> CreateImageTag([FromBody] ImageTagRequest request, CancellationToken ct)
        => Ok(await lms.CreateImageTagAsync(request.Name, request.ImageUrl, ct));

    [HttpPost("enableTag")]
    public async Task<IActionResult> EnableTag([FromBody] IdRequest request, CancellationToken ct)
        => await assessments.SetTagEnabledAsync(request.Id, true, ct) ? Ok() : NotFound();

    [HttpPost("disableTag")]
    public async Task<IActionResult> DisableTag([FromBody] IdRequest request, CancellationToken ct)
        => await assessments.SetTagEnabledAsync(request.Id, false, ct) ? Ok() : NotFound();

    [HttpPost("updateTag")]
    public async Task<IActionResult> UpdateTag([FromBody] NameRequest request, CancellationToken ct)
        => Ok(await assessments.CreateTagAsync(request.Name, ct)); // rename via new tag; full rename can be extended

    [HttpGet("imageTags")]
    public async Task<IActionResult> GetImageTags(CancellationToken ct)
    {
        var tags = await assessments.GetTagsAsync(ct);
        return Ok(tags.Where(t => t.ToString()!.Contains("img:", StringComparison.OrdinalIgnoreCase)));
    }
}

public sealed class AnswerRequest { public required Guid QuestionId { get; set; } public required string Answer { get; set; } }
public sealed class UpdateTagsRequest { public required List<Guid> Ids { get; set; } public required List<string> Tags { get; set; } }
public sealed class ImageTagRequest { public required string Name { get; set; } public string? ImageUrl { get; set; } }
