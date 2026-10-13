using Ct.Arte.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Arte.Controllers;

[ApiController]
[Route("api/arte/etl")]
[Route("api/Etl")]
public sealed class EtlController(IArtefactService svc) : ControllerBase
{
    [HttpPost("generateInstEtl")]
    public async Task<IActionResult> CreateInstituteEtl(CancellationToken ct)
        => Ok(await svc.GenerateEtlStubAsync("institute", ct));

    [HttpPost("generateCourseEtl")]
    public async Task<IActionResult> CreateCourseEtl(CancellationToken ct)
        => Ok(await svc.GenerateEtlStubAsync("course", ct));

    [HttpPost("generateStudentEtl")]
    public async Task<IActionResult> CreateStudentEtl(CancellationToken ct)
        => Ok(await svc.GenerateEtlStubAsync("student", ct));

    [HttpPost("generateFeesEtl")]
    public async Task<IActionResult> CreateFeesEtl(CancellationToken ct)
        => Ok(await svc.GenerateEtlStubAsync("fees", ct));
}
