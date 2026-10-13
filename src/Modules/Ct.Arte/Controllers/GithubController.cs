using Ct.Arte.Application.Dtos;
using Ct.Arte.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Arte.Controllers;

[ApiController]
[Route("api/arte/github")]
[Route("api/Github")]
public sealed class GithubController(IArtefactService svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(await svc.SearchAsync(new ArtefactSearchRequest { Search = "github", Take = 100 }, ct));

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert([FromBody] CreateArtefactRequest request, CancellationToken ct)
    {
        request.Tags ??= [];
        if (!request.Tags.Contains("github")) request.Tags.Add("github");
        return Ok(await svc.UpsertAsync(request, ct));
    }

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] IdBody body, CancellationToken ct)
        => await svc.EnableAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] IdBody body, CancellationToken ct)
        => await svc.DisableAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string id, CancellationToken ct)
    {
        var a = await svc.GetAsync(id, ct);
        return a is null ? NotFound() : Ok(new { a.Id, a.Url, a.FileName, content = a.Description });
    }

    [HttpGet("getFiles")]
    public async Task<IActionResult> GetFiles(CancellationToken ct)
        => Ok(await svc.SearchAsync(new ArtefactSearchRequest { Search = "github", Take = 200 }, ct));

    [HttpGet("filesWithContent")]
    public async Task<IActionResult> GetFilesWithContent(CancellationToken ct)
        => Ok(await svc.SearchAsync(new ArtefactSearchRequest { Search = "github", Take = 200 }, ct));

    [HttpPost("updateDescription")]
    public async Task<IActionResult> UpdateDescription([FromBody] DescriptionBody body, CancellationToken ct)
        => await svc.UpdateDescriptionAsync(body.Id, body.Description, ct) ? Ok() : NotFound();

    [HttpGet("specsFile")]
    public IActionResult GetSpecsFile() => Ok(new { message = "Specs file — store as artefact with tag specs." });
}
