using Ct.Admission.Application;
using Ct.Admission.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Admission.Controllers;

[ApiController]
[Route("api/admission/template")]
[Route("api/ApplicationFormTemplate")]
public sealed class ApplicationFormTemplateController(TemplateService templates) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateApplicationFormTemplate([FromBody] CreateTemplateRequest request, CancellationToken ct)
        => Ok(await templates.CreateAsync(request, ct));

    [HttpGet]
    public async Task<IActionResult> GetApplicationFormTemplate(CancellationToken ct)
        => Ok(await templates.GetAllAsync(ct));

    [HttpGet("getById")]
    public async Task<IActionResult> GetApplicationFormTemplateById([FromQuery] Guid id, CancellationToken ct)
    {
        var t = await templates.GetByIdAsync(id, ct);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateApplicationFormTemplate([FromQuery] Guid id, [FromBody] CreateTemplateRequest request, CancellationToken ct)
    {
        var t = await templates.UpdateAsync(id, request, ct);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpPost("disable")]
    public async Task<IActionResult> DisableApplicationFormTemplate([FromBody] AdmIdBody body, CancellationToken ct)
        => await templates.SetStatusAsync(body.Id, TemplateStatus.Disabled, ct) ? Ok() : NotFound();

    [HttpPost("enable")]
    public async Task<IActionResult> EnableApplicationFormTemplate([FromBody] AdmIdBody body, CancellationToken ct)
        => await templates.SetStatusAsync(body.Id, TemplateStatus.Draft, ct) ? Ok() : NotFound();

    [HttpPost("publish")]
    public async Task<IActionResult> PublishApplicationFormTemplate([FromBody] AdmIdBody body, CancellationToken ct)
        => await templates.SetStatusAsync(body.Id, TemplateStatus.Published, ct) ? Ok() : NotFound();

    [HttpPost("unpublish")]
    public async Task<IActionResult> UnPublishApplicationFormTemplate([FromBody] AdmIdBody body, CancellationToken ct)
        => await templates.SetStatusAsync(body.Id, TemplateStatus.Draft, ct) ? Ok() : NotFound();

    [HttpPost("addShortlist")]
    public async Task<IActionResult> AddShortlist([FromBody] ShortlistBody body, CancellationToken ct)
        => await templates.AddShortlistAsync(body.Id, body.Criterion, ct) ? Ok() : NotFound();

    [HttpPost("updateShortlist")]
    public async Task<IActionResult> UpdateShortlist([FromBody] ShortlistListBody body, CancellationToken ct)
        => await templates.UpdateShortlistAsync(body.Id, body.Criteria ?? [], ct) ? Ok() : NotFound();

    [HttpGet("shortlist")]
    public async Task<IActionResult> GetShortlist([FromQuery] Guid id, CancellationToken ct)
    {
        var t = await templates.GetShortlistAsync(id, ct);
        return t is null ? NotFound() : Ok(t);
    }
}

public sealed class ShortlistBody { public required Guid Id { get; set; } public required string Criterion { get; set; } }
public sealed class ShortlistListBody { public required Guid Id { get; set; } public List<string>? Criteria { get; set; } }
