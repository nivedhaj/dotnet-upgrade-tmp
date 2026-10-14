using Ct.Accounting.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Accounting.Controllers;

[ApiController]
[Route("api/accounting/voucher")]
[Route("api/Voucher")]
public sealed class VoucherController(VoucherService vouchers) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> NewVoucher([FromBody] CreateVoucherRequest request, CancellationToken ct)
    {
        try { return Ok(await vouchers.CreateAsync(request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await vouchers.GetAllAsync(ct));

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromQuery] Guid id, CancellationToken ct)
        => await vouchers.DisableAsync(id, ct) ? Ok() : NotFound();

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromQuery] Guid id, CancellationToken ct)
        => await vouchers.EnableAsync(id, ct) ? Ok() : NotFound();
}
