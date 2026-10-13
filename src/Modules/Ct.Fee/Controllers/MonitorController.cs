using Microsoft.AspNetCore.Mvc;

namespace Ct.Fee.Controllers;

[ApiController]
[Route("api/fee/monitor")]
public sealed class MonitorController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { status = "ok", service = "ct.fee", utc = DateTimeOffset.UtcNow });
}
