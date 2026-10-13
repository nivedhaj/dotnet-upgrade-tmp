using Ct.Fee.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Fee.Controllers;

/// <summary>Suspense-account endpoints (dedicated route; same ops also under transactions).</summary>
[ApiController]
[Route("api/fee/suspense")]
public sealed class SuspenseController(FeeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] Guid? id, CancellationToken ct)
    {
        if (id is null || id == Guid.Empty)
            return Ok(await service.GetSuspenseTransactionsAsync(ct));
        var item = await service.GetSuspenseTransactionAsync(id.Value, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<SuspenseTransactionDto>> Create(
        SuspenseTransactionRequest request, CancellationToken ct)
        => Ok(await service.CreateSuspenseAsync(request, ct));

    [HttpPost("{id:guid}/allocations")]
    public async Task<ActionResult<SuspenseTransactionDto>> Allocate(
        Guid id, SuspenseAllocationRequest request, CancellationToken ct)
        => Ok(await service.AllocateSuspenseAsync(id, request, ct));

    [HttpPost("{id:guid}/disable")]
    public async Task<ActionResult> Disable(Guid id, CancellationToken ct)
    {
        var ok = await service.SetSuspenseStatusAsync(id, Domain.SuspenseStatus.Reversed, ct);
        return ok ? Ok() : NotFound();
    }

    [HttpPost("{id:guid}/enable")]
    public async Task<ActionResult> Enable(Guid id, CancellationToken ct)
    {
        var ok = await service.SetSuspenseStatusAsync(id, Domain.SuspenseStatus.Unidentified, ct);
        return ok ? Ok() : NotFound();
    }
}
