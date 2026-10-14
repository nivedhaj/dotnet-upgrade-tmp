using Ct.Accounting.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Accounting.Controllers;

[ApiController]
[Route("api/accounting/ledger")]
[Route("api/Ledger")]
public sealed class LedgerController(LedgerService ledger) : ControllerBase
{
    [HttpPost("assets")]
    public async Task<IActionResult> Assets([FromBody] CreateLedgerRequest request, CancellationToken ct)
        => Ok(await ledger.AssetsAsync(request, ct));

    [HttpPost("liabilities")]
    public async Task<IActionResult> Liabilities([FromBody] CreateLedgerRequest request, CancellationToken ct)
        => Ok(await ledger.LiabilitiesAsync(request, ct));

    [HttpPost("chartOfAccounts")]
    [HttpGet("chartOfAccounts")]
    public async Task<IActionResult> chartOfAccounts(CancellationToken ct)
        => Ok(await ledger.ChartOfAccountsAsync(ct));

    [HttpGet("voucherTargetAccounts")]
    public async Task<IActionResult> GetVoucherTargetAccounts(CancellationToken ct)
        => Ok(await ledger.GetVoucherTargetAccountsAsync(ct));

    [HttpGet("voucherSourceAccounts")]
    public async Task<IActionResult> GetVoucherSourceAccounts(CancellationToken ct)
        => Ok(await ledger.GetVoucherSourceAccountsAsync(ct));
}
