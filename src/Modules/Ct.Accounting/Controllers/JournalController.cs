using Ct.Accounting.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Accounting.Controllers;

[ApiController]
[Route("api/accounting/journal")]
[Route("api/Journal")]
public sealed class JournalController(InventoryService inv, SmsCreditService sms) : ControllerBase
{
    [HttpPost("inventory")]
    public async Task<IActionResult> InventoryJournal([FromBody] InventoryJournalRequest request, CancellationToken ct)
        => Ok(await inv.JournalAsync(request, ct));

    [HttpPost("purchase")]
    public async Task<IActionResult> PurchaseItems([FromBody] List<InventoryJournalRequest> items, CancellationToken ct)
        => Ok(await inv.PurchaseAsync(items, ct));

    [HttpPost("sales")]
    public async Task<IActionResult> SalesItems([FromBody] List<InventoryJournalRequest> items, CancellationToken ct)
        => Ok(await inv.SalesAsync(items, ct));

    [HttpPost("smsInventoryCredit")]
    public async Task<IActionResult> AddSMSCredits([FromBody] SmsCreditBody body, CancellationToken ct)
        => Ok(await sms.AddCreditsAsync(body.Credits, body.Description, ct));

    [HttpGet("smsTx")]
    public async Task<IActionResult> GetSMSTx(CancellationToken ct)
        => Ok(await sms.GetTxAsync(ct));
}

public sealed class SmsCreditBody { public int Credits { get; set; } public string? Description { get; set; } }
