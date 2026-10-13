using Ct.Fee.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Fee.Controllers;

/// <summary>Refund endpoints — mirrors original RefundController (api/Refund).</summary>
[ApiController]
[Route("api/fee/refunds")]
public sealed class RefundsController(FeeService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RefundDto>> Create(RefundRequest request, CancellationToken ct)
        => Ok(await service.CreateRefundAsync(request, ct));

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RefundDto>>> GetAll(CancellationToken ct)
        => Ok(await service.GetRefundsAsync(ct));

    [HttpPost("disable")]
    public async Task<ActionResult> Disable([FromQuery] Guid id, CancellationToken ct)
    {
        var ok = await service.SetRefundStatusAsync(id, Domain.RefundStatus.Cancelled, ct);
        return ok ? Ok(new { id }) : NotFound();
    }

    [HttpPost("enable")]
    public async Task<ActionResult> Enable([FromQuery] Guid id, CancellationToken ct)
    {
        var ok = await service.SetRefundStatusAsync(id, Domain.RefundStatus.Approved, ct);
        return ok ? Ok(new { id }) : NotFound();
    }
}
