using Ct.Arte.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Arte.Controllers;

[ApiController]
[Route("api/arte/report")]
[Route("api/Report")]
public sealed class ReportController(IArtefactService svc) : ControllerBase
{
    [HttpPost("artefactList")]
    public async Task<IActionResult> GetArtefactsReport(CancellationToken ct)
        => Ok(await svc.GetAllAsync(ct));
}
