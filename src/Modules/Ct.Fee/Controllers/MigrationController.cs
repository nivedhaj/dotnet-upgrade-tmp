using Ct.Fee.Migration;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Fee.Controllers;

[ApiController]
[Route("api/fee/migration")]
public sealed class MigrationController(RavenExportImporter importer) : ControllerBase
{
    [HttpPost("raven-export")]
    [RequestSizeLimit(104_857_600)]
    public async Task<ActionResult<ImportResult>> Import(IFormFile file, CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await importer.ImportAsync(stream, ct));
    }
}
