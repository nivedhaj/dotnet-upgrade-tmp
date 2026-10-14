using Ct.Library.Application;
using Ct.Library.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Library.Controllers;

[ApiController]
[Route("api/library/material")]
[Route("api/Material")]
public sealed class MaterialController(MaterialService materials) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await materials.GetAllAsync(ct));

    [HttpGet("getDto")]
    public async Task<IActionResult> GetDtoWithoutData([FromQuery] Guid id, CancellationToken ct)
    {
        var m = await materials.GetByIdAsync(id, ct);
        return m is null ? NotFound() : Ok(m);
    }

    [HttpGet("getDtoWithData")]
    public async Task<IActionResult> GetDtoWithData([FromQuery] Guid id, CancellationToken ct)
        => await GetDtoWithoutData(id, ct);

    [HttpGet("verificationMaterial")]
    public async Task<IActionResult> GetStockVerificationMaterial(CancellationToken ct)
        => Ok(await materials.GetStockVerificationAsync(ct));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateMaterialRequest request, CancellationToken ct)
        => Ok(await materials.CreateAsync(request, ct));

    [HttpPost("update")]
    public async Task<IActionResult> Update([FromQuery] Guid id, [FromBody] CreateMaterialRequest request, CancellationToken ct)
    {
        var m = await materials.UpdateAsync(id, request, ct);
        return m is null ? NotFound() : Ok(m);
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] LibIdBody body, CancellationToken ct)
        => await materials.SetStatusAsync(body.Id, MaterialStatus.Disabled, ct) ? Ok() : NotFound();

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] LibIdBody body, CancellationToken ct)
        => await materials.SetStatusAsync(body.Id, MaterialStatus.Available, ct) ? Ok() : NotFound();

    [HttpPost("maintenance")]
    public async Task<IActionResult> Maintenance([FromBody] LibIdBody body, CancellationToken ct)
        => await materials.MaintenanceAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("returnMaintenanceMaterial")]
    public async Task<IActionResult> MaintenanceMaterialReturn([FromBody] LibIdBody body, CancellationToken ct)
        => await materials.MaintenanceReturnAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("updateISBN")]
    public async Task<IActionResult> UpdateISBN([FromBody] StringFieldBody body, CancellationToken ct)
        => await materials.UpdateIsbnAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    [HttpPost("clone")]
    public async Task<IActionResult> Clone([FromBody] LibIdBody body, CancellationToken ct)
    {
        var m = await materials.CloneAsync(body.Id, ct);
        return m is null ? NotFound() : Ok(m);
    }

    [HttpPost("relocate")]
    public async Task<IActionResult> Relocate([FromBody] StringFieldBody body, CancellationToken ct)
        => await materials.RelocateAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    [HttpPost("setAttributes")]
    public async Task<IActionResult> SetAttributes([FromBody] AttrBody body, CancellationToken ct)
        => await materials.SetAttributesAsync(body.Id, body.Attributes ?? new(), ct) ? Ok() : NotFound();

    [HttpPost("setTags")]
    public async Task<IActionResult> SetTags([FromBody] TagsBody body, CancellationToken ct)
        => await materials.SetTagsAsync(body.Id, body.Tags ?? [], ct) ? Ok() : NotFound();

    [HttpPost("setValue")]
    public async Task<IActionResult> SetValue([FromBody] ValueBody body, CancellationToken ct)
        => await materials.SetValueAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    [HttpPost("setPages")]
    public async Task<IActionResult> SetPages([FromBody] PagesBody body, CancellationToken ct)
        => await materials.SetPagesAsync(body.Id, body.Pages, ct) ? Ok() : NotFound();

    [HttpPost("setOwnerId")]
    public async Task<IActionResult> SetOwnerId([FromBody] StringFieldBody body, CancellationToken ct)
        => await materials.SetOwnerIdAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    [HttpPost("verifyStock")]
    public async Task<IActionResult> VerifyStock([FromBody] LibIdBody body, CancellationToken ct)
        => await materials.VerifyStockAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpGet("widget/materialBreakup")]
    public async Task<IActionResult> GetMaterialBreakupWidgetData(CancellationToken ct)
        => Ok(await materials.WidgetBreakupAsync(ct));
}

public sealed class LibIdBody { public required Guid Id { get; set; } }
public sealed class StringFieldBody { public required Guid Id { get; set; } public required string Value { get; set; } }
public sealed class AttrBody { public required Guid Id { get; set; } public Dictionary<string, string>? Attributes { get; set; } }
public sealed class TagsBody { public required Guid Id { get; set; } public List<string>? Tags { get; set; } }
public sealed class ValueBody { public required Guid Id { get; set; } public decimal Value { get; set; } }
public sealed class PagesBody { public required Guid Id { get; set; } public int Pages { get; set; } }
