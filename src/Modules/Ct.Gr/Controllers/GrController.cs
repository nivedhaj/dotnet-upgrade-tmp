using Ct.Gr.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Gr.Controllers;

/// <summary>Port of original GrController (api/gr/Gr).</summary>
[ApiController]
[Route("api/gr")]
[Route("api/gr/Gr")]
public sealed class GrController(ReceiptService receipts) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ReceiptDto>>> Get(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string sort = "desc",
        CancellationToken ct = default)
        => Ok(await receipts.GetAsync(from, to, status, page, pageSize, sort, ct));

    [HttpGet("getDto")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReceiptDto>> GetDto([FromQuery] Guid? id, [FromRoute] Guid? idRoute, CancellationToken ct)
    {
        var rid = id ?? idRoute ?? Guid.Empty;
        if (rid == Guid.Empty) return BadRequest();
        var r = await receipts.GetByIdAsync(rid, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpGet("getDtoWithData")]
    public async Task<ActionResult<ReceiptDto>> GetDtoWithData([FromQuery] Guid id, CancellationToken ct)
        => await GetDto(id, null, ct);

    [HttpGet("dates")]
    public async Task<IActionResult> GetDates(CancellationToken ct)
        => Ok(await receipts.GetDatesAsync(ct));

    [HttpPost]
    public async Task<ActionResult<ReceiptDto>> Post([FromBody] CreateReceiptRequest request, CancellationToken ct)
    {
        var r = await receipts.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetDto), new { id = r.Id }, r);
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateReceipt([FromBody] UpdateReceiptRequest request, CancellationToken ct)
    {
        var r = await receipts.UpdateReceiptAsync(request, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost("updatePaymentDetails")]
    public async Task<IActionResult> UpdatePaymentDetails([FromBody] UpdatePaymentDetailsRequest request, CancellationToken ct)
    {
        var r = await receipts.UpdatePaymentDetailsAsync(request, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost("updateOrderItemParticular")]
    public async Task<IActionResult> UpdateOrderItemParticular([FromBody] UpdateOrderItemRequest request, CancellationToken ct)
    {
        var r = await receipts.UpdateOrderItemParticularAsync(request, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost("updateOrderItemAmount")]
    public async Task<IActionResult> UpdateOrderItemAmount([FromBody] UpdateOrderItemRequest request, CancellationToken ct)
    {
        var r = await receipts.UpdateOrderItemAmountAsync(request, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost("updateOrderItemExtras")]
    public async Task<IActionResult> UpdateOrderItemExtras([FromBody] UpdateOrderItemRequest request, CancellationToken ct)
    {
        var r = await receipts.UpdateOrderItemExtrasAsync(request, ct);
        return r is null ? NotFound() : Ok(r);
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelReceipt([FromBody] IdRequest request, CancellationToken ct)
        => await receipts.CancelAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("activate")]
    public async Task<IActionResult> ActivateReceipt([FromBody] IdRequest request, CancellationToken ct)
        => await receipts.ActivateAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpPost("printReceipt")]
    public async Task<IActionResult> PrintReceipt([FromBody] IdRequest request, CancellationToken ct)
    {
        var print = await receipts.PrintAsync(request.Id, ct);
        return print is null ? NotFound() : Ok(print);
    }

    [HttpPost("audit")]
    public async Task<IActionResult> Audit([FromBody] IdRequest request, CancellationToken ct)
        => await receipts.AuditAsync(request.Id, ct) ? Ok() : NotFound();

    [HttpGet("widget/receiptCollection")]
    public async Task<IActionResult> ReceiptCollection(CancellationToken ct)
        => Ok(await receipts.GetMonthwiseCollectionAsync(ct));

    [HttpPost("setRegulatoryCompliance")]
    public async Task<IActionResult> SetRegulatoryCompliance([FromBody] RegulatoryComplianceRequest request, CancellationToken ct)
        => await receipts.SetRegulatoryComplianceAsync(request.Id, request.Compliant, request.Notes, ct)
            ? Ok(new { request.Id, request.Compliant })
            : NotFound();

    [HttpPost("exportCsv")]
    public async Task<IActionResult> ExportCsv([FromBody] ReportFilterRequest filter, CancellationToken ct)
    {
        var csv = await receipts.ExportCsvAsync(filter, ct);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "receipts.csv");
    }
}
