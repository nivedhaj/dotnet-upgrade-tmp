using Ct.Fee.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Fee.Controllers;

/// <summary>Fee definition endpoints — mirrors original FeeController (api/Fee).</summary>
[ApiController]
[Route("api/fee/fees")]
public sealed class FeesController(FeeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] Guid? id, CancellationToken ct)
    {
        if (id is null || id == Guid.Empty)
            return Ok(await service.GetFeesAsync(ct));
        var fee = await service.GetFeeAsync(id.Value, ct);
        return fee is null ? NotFound() : Ok(fee);
    }

    [HttpGet("getById")]
    public async Task<ActionResult<FeeDefinitionDto>> GetById([FromQuery] Guid id, CancellationToken ct)
    {
        var fee = await service.GetFeeAsync(id, ct);
        return fee is null ? NotFound() : Ok(fee);
    }

    [HttpGet("feeInstFlattened")]
    public async Task<ActionResult<IReadOnlyList<FeeInstallmentFlattenedDto>>> FeeInstFlattened(
        [FromQuery] string? courseId, CancellationToken ct)
        => Ok(await service.GetFeeInstallmentsFlattenedAsync(courseId, ct));

    [HttpPost]
    public async Task<ActionResult<FeeDefinitionDto>> Create(FeeDefinitionRequest request, CancellationToken ct)
    {
        var created = await service.CreateFeeAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("updateFee")]
    public async Task<ActionResult<FeeDefinitionDto>> Update(FeeDefinitionUpdateRequest request, CancellationToken ct)
    {
        var updated = await service.UpdateFeeAsync(request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPost("canRemoveStudent")]
    public async Task<ActionResult<CanRemoveStudentResult>> CanRemoveStudent(
        FeeStudentCheckRequest request, CancellationToken ct)
        => Ok(await service.CanRemoveStudentAsync(request.FeeId, request.StudentId, ct));

    [HttpPost("disable")]
    public async Task<ActionResult> Disable([FromQuery] Guid id, CancellationToken ct)
    {
        var ok = await service.SetFeeStatusAsync(id, Domain.FeeStatus.Inactive, ct);
        return ok ? Ok(new { id }) : NotFound();
    }

    [HttpPost("enable")]
    public async Task<ActionResult> Enable([FromQuery] Guid id, CancellationToken ct)
    {
        var ok = await service.SetFeeStatusAsync(id, Domain.FeeStatus.Active, ct);
        return ok ? Ok(new { id }) : NotFound();
    }

    /// <summary>Port of FeeCmd.AssignStudent.</summary>
    [HttpPost("assignStudent")]
    public async Task<ActionResult<FeeDefinitionDto>> AssignStudent(AssignStudentRequest request, CancellationToken ct)
    {
        var updated = await service.AssignStudentAsync(request.FeeId, request.StudentId, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    /// <summary>Port of FeeQry.GetDates.</summary>
    [HttpGet("dates")]
    public ActionResult<DateRangeDto> GetDates(
        [FromQuery] string rangeType = "monthly",
        [FromQuery] int month = 0,
        [FromQuery] int year = 0,
        [FromQuery] DateOnly? academicYearFrom = null,
        [FromQuery] DateOnly? academicYearTo = null)
    {
        if (month == 0) month = DateTime.UtcNow.Month;
        if (year == 0) year = DateTime.UtcNow.Year;
        return Ok(service.GetDates(rangeType, month, year, academicYearFrom, academicYearTo));
    }

    /// <summary>Port of FeeQry.FeesExpectedCollected.</summary>
    [HttpGet("expectedCollected")]
    public async Task<ActionResult<IReadOnlyList<FeeExpectedCollectedDto>>> FeesExpectedCollected(
        [FromQuery] DateOnly? academicYearFrom,
        [FromQuery] DateOnly? academicYearTo,
        CancellationToken ct)
        => Ok(await service.FeesExpectedCollectedAsync(academicYearFrom, academicYearTo, ct));

}
