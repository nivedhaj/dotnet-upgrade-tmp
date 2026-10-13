using Ct.Arte.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Arte.Controllers;

[ApiController]
[Route("api/arte/anon")]
[Route("api/Anon")]
public sealed class AnonController(IArtefactService svc) : ControllerBase
{
    [HttpGet("getFiles")]
    public async Task<IActionResult> GetFilesFromUrl([FromQuery] string? search, CancellationToken ct)
        => Ok(await svc.SearchAsync(new Application.Dtos.ArtefactSearchRequest { Search = search, Take = 100 }, ct));

    [HttpGet("contents")]
    public async Task<IActionResult> GetFiles([FromQuery] string? search, CancellationToken ct)
        => Ok(await svc.SearchAsync(new Application.Dtos.ArtefactSearchRequest { Search = search, Take = 100 }, ct));

    [HttpGet("contentTags")]
    public async Task<IActionResult> GetContentTags(CancellationToken ct)
        => Ok(await svc.GetTagsAsync(ct));

    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string id, CancellationToken ct)
    {
        var a = await svc.GetAsync(id, ct);
        return a is null ? NotFound() : Ok(new { a.Id, a.Url, a.FileName });
    }
}
