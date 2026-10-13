using Ct.Fee.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Fee.Controllers;

/// <summary>Email endpoints — mirrors original EmailController (api/fee/Email).</summary>
[ApiController]
[Route("api/fee/email")]
public sealed class EmailController(ReportService reports, FeeService fees) : ControllerBase
{
    [HttpPost("feeReceipt")]
    public async Task<ActionResult> EmailFeeReceipt(EmailFeeReceiptRequest request, CancellationToken ct)
    {
        var html = await reports.GetPrintFeeReceiptAsync(request.TxId, ct);
        if (html is null) return NotFound(new { message = $"Transaction {request.TxId} not found." });
        await fees.EmailFeeReceiptAsync(request, ct);
        return Ok(new { message = "Email is sent", receipt = html });
    }
}
