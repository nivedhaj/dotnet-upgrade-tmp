using Ct.Staff.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ct.Staff.Controllers;

[ApiController]
[Route("api/staff/ping")]
[Route("api/PingDB")]
public sealed class PingDBController(StaffDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var can = await db.Database.CanConnectAsync(ct);
        return Ok(new { ok = can, module = "staff" });
    }

    [HttpPost]
    public async Task<IActionResult> Post(CancellationToken ct) => await Get(ct);
}
