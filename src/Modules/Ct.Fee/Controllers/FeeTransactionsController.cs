using Ct.Fee.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Fee.Controllers;

/// <summary>Fee transaction endpoints — mirrors original FeeTxController (api/FeeTx).</summary>
[ApiController]
[Route("api/fee/transactions")]
public sealed class FeeTransactionsController(FeeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] string? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Ok(await service.GetTransactionsAsync(null, ct));
        var tx = await service.GetTransactionAsync(id, ct);
        return tx is null ? NotFound() : Ok(tx);
    }

    [HttpGet("getTxHTML")]
    public async Task<ActionResult<FeeTxHtmlDto>> GetTxHtml([FromQuery] Guid id, CancellationToken ct)
    {
        var html = await service.GetTransactionHtmlAsync(id, ct);
        return html is null ? NotFound() : Ok(html);
    }

    [HttpPost]
    [HttpPost("transact")]
    public async Task<ActionResult<FeeTransactionDto>> Transact(FeeTransactionRequest request, CancellationToken ct)
    {
        var created = await service.CreateTransactionAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id.ToString() }, created);
    }

    [HttpPost("disable")]
    public async Task<ActionResult> Disable([FromQuery] Guid id, CancellationToken ct)
    {
        var ok = await service.SetTransactionStatusAsync(id, Domain.TransactionStatus.Cancelled, ct);
        return ok ? Ok() : NotFound();
    }

    [HttpPost("enable")]
    public async Task<ActionResult> Enable([FromQuery] Guid id, CancellationToken ct)
    {
        var ok = await service.SetTransactionStatusAsync(id, Domain.TransactionStatus.Completed, ct);
        return ok ? Ok() : NotFound();
    }

    [HttpGet("studentTX")]
    public async Task<ActionResult<StudentFeeDetailDto>> StudentTx([FromQuery] string id, CancellationToken ct)
        => Ok(await service.GetStudentFeeDetailsAsync(id, ct));

    [HttpGet("feesToBePaid")]
    public async Task<ActionResult<IReadOnlyList<FeeToPayDto>>> FeesToBePaid(
        [FromQuery] string studentId, CancellationToken ct)
        => Ok(await service.GetFeesToBePaidAsync(studentId, ct));

    [HttpPost("updateTx")]
    public async Task<ActionResult<FeeTransactionDto>> UpdateTx(FeeTransactionUpdateRequest request, CancellationToken ct)
    {
        var updated = await service.UpdateTransactionAsync(request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpGet("feeCollectionAccounts")]
    public async Task<ActionResult<IReadOnlyList<FeeCollectionAccountDto>>> FeeCollectionAccounts(CancellationToken ct)
        => Ok(await service.GetFeeCollectionAccountsAsync(ct));

    [HttpPost("suspenseAccountTx")]
    public async Task<ActionResult<SuspenseTransactionDto>> CreateSuspense(
        SuspenseTransactionRequest request, CancellationToken ct)
        => Ok(await service.CreateSuspenseAsync(request, ct));

    [HttpGet("suspenseAccountTx")]
    public async Task<ActionResult> GetSuspense([FromQuery] Guid? id, CancellationToken ct)
    {
        if (id is null || id == Guid.Empty)
            return Ok(await service.GetSuspenseTransactionsAsync(ct));
        var item = await service.GetSuspenseTransactionAsync(id.Value, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost("allocateSuspenseAccountTx")]
    public async Task<ActionResult<SuspenseTransactionDto>> AllocateSuspense(
        [FromQuery] Guid suspenseAccountTxId,
        FeeTransactionRequest feeTx,
        CancellationToken ct)
    {
        var createdTx = await service.CreateTransactionAsync(feeTx, ct);
        var allocated = await service.AllocateSuspenseAsync(
            suspenseAccountTxId,
            new SuspenseAllocationRequest(createdTx.Id, createdTx.Amount),
            ct);
        return Ok(allocated);
    }

    [HttpPost("disableSuspenseAccountTx")]
    public async Task<ActionResult> DisableSuspense([FromQuery] Guid id, CancellationToken ct)
    {
        var ok = await service.SetSuspenseStatusAsync(id, Domain.SuspenseStatus.Reversed, ct);
        return ok ? Ok(new { message = "Suspense account transaction disabled" }) : NotFound();
    }

    [HttpPost("enableSuspenseAccountTx")]
    public async Task<ActionResult> EnableSuspense([FromQuery] Guid id, CancellationToken ct)
    {
        var ok = await service.SetSuspenseStatusAsync(id, Domain.SuspenseStatus.Unidentified, ct);
        return ok ? Ok(new { message = "Suspense account transaction enabled" }) : NotFound();
    }

    /// <summary>Port of FeeTxQry.GetFeeTxByRefNo.</summary>
    [HttpGet("byRefNo")]
    public async Task<ActionResult<FeeTransactionDto>> GetByRefNo([FromQuery] string refNo, CancellationToken ct)
    {
        var item = await service.GetFeeTxByRefNoAsync(refNo, ct);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Port of FeeTxQry.GetTransactedFees.</summary>
    [HttpGet("transactedFees")]
    public async Task<ActionResult<IReadOnlyList<Guid>>> TransactedFees(CancellationToken ct)
        => Ok(await service.GetTransactedFeesAsync(ct));

    /// <summary>Port of FeeTxQry.GetStudentFeesExpectedPaid2.</summary>
    [HttpGet("expectedPaid")]
    public async Task<ActionResult<IReadOnlyList<StudentFeesExpectedPaidDto>>> ExpectedPaid(
        [FromQuery] string? studentId, CancellationToken ct)
        => Ok(await service.GetStudentFeesExpectedPaidAsync(studentId, ct));

    /// <summary>Port of FeeTxQry.GetClosingBalance.</summary>
    [HttpGet("closingBalance")]
    public async Task<ActionResult<IReadOnlyList<StudentClosingBalanceDto>>> ClosingBalance(CancellationToken ct)
        => Ok(await service.GetClosingBalanceAsync(ct));

    /// <summary>Port of FeeTxCmd.Transact2 (alias of POST /transact).</summary>
    [HttpPost("transact2")]
    public async Task<ActionResult<TransactResultDto>> Transact2(FeeTransactionRequest request, CancellationToken ct)
        => Ok(await service.Transact2Async(request, ct));

}
