using Ct.Asset.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ct.Asset.Controllers;

[ApiController]
[Route("api/asset/ping")]
[Route("api/Ping")]
public sealed class PingController(AssetDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
        => Ok(new { ok = await db.Database.CanConnectAsync(ct), module = "asset" });

    [HttpPost]
    public async Task<IActionResult> Post(CancellationToken ct) => await Get(ct);
}
