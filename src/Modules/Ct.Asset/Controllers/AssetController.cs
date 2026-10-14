using Ct.Asset.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Asset.Controllers;

[ApiController]
[Route("api/asset")]
[Route("api/Asset")]
public sealed class AssetController(AssetService assets) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateAssetRequest request, CancellationToken ct)
    {
        try { return Ok(await assets.CreateAsync(request, ct)); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportArtefacts([FromBody] List<CreateAssetRequest> items, CancellationToken ct)
        => Ok(await assets.ImportArtefactsAsync(items, ct));

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? id, CancellationToken ct)
    {
        if (id is not null)
        {
            var one = await assets.GetByIdAsync(id.Value, ct);
            return one is null ? NotFound() : Ok(one);
        }
        return Ok(await assets.GetAllAsync(ct));
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromQuery] Guid id, CancellationToken ct)
        => await assets.DisableAsync(id, ct) ? Ok() : NotFound();

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromQuery] Guid id, CancellationToken ct)
        => await assets.EnableAsync(id, ct) ? Ok() : NotFound();

    [HttpPost("setAttributes")]
    public async Task<IActionResult> SetAttributes([FromBody] AssetAttrBody body, CancellationToken ct)
        => await assets.SetAttributesAsync(body.Id, body.Attributes ?? new(), ct) ? Ok() : NotFound();

    [HttpPost("setTags")]
    public async Task<IActionResult> SetTags([FromBody] AssetTagsBody body, CancellationToken ct)
        => await assets.SetTagsAsync(body.Id, body.Tags ?? [], ct) ? Ok() : NotFound();

    [HttpPost("setValue")]
    public async Task<IActionResult> SetValue([FromBody] AssetValueBody body, CancellationToken ct)
        => await assets.SetValueAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    [HttpPost("relocate")]
    public async Task<IActionResult> Relocate([FromBody] AssetStringBody body, CancellationToken ct)
        => await assets.RelocateAsync(body.Id, body.Value, ct) ? Ok() : NotFound();

    /// <summary>Alias for MoveAsset command</summary>
    [HttpPost("move")]
    public async Task<IActionResult> Move([FromBody] AssetStringBody body, CancellationToken ct)
        => await Relocate(body, ct);

    [HttpPost("updateTrackingId")]
    public async Task<IActionResult> UpdateTrackingId([FromBody] AssetStringBody body, CancellationToken ct)
    {
        try { return await assets.UpdateTrackingIdAsync(body.Id, body.Value, ct) ? Ok() : NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("maintain")]
    public async Task<IActionResult> Maintain([FromBody] AssetMaintainBody body, CancellationToken ct)
        => await assets.MaintainAsync(body.Id, body.ActionTaken, ct) ? Ok() : NotFound();

    [HttpPost("setWarranty")]
    public async Task<IActionResult> SetWarranty([FromBody] AssetWarrantyBody body, CancellationToken ct)
        => await assets.SetWarrantyAsync(body.Id, body.CoveredBy, body.From, body.To, body.Details, ct) ? Ok() : NotFound();

    [HttpPost("clone")]
    public async Task<IActionResult> Clone([FromBody] AssetCloneBody body, CancellationToken ct)
    {
        try
        {
            if (body.TrackingIds is { Count: > 0 })
                return Ok(await assets.CloneManyAsync(body.Id, body.TrackingIds, ct));
            var a = await assets.CloneAsync(body.Id, body.NewTrackingId, ct);
            return a is null ? NotFound() : Ok(a);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost("clear")]
    public async Task<IActionResult> Clear([FromQuery] Guid id, CancellationToken ct)
        => await assets.ClearAsync(id, ct) ? Ok() : NotFound();

    [HttpPost("clearAttributes")]
    public async Task<IActionResult> ClearAttributes([FromBody] AssetIdBody body, CancellationToken ct)
        => await assets.ClearAttributesAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("clearTags")]
    public async Task<IActionResult> ClearTags([FromBody] AssetIdBody body, CancellationToken ct)
        => await assets.ClearTagsAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpGet("getById")]
    [HttpGet("getAssetById")]
    public async Task<IActionResult> GetAssetById([FromQuery] Guid id, CancellationToken ct)
    {
        var a = await assets.GetByIdAsync(id, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpGet("isDuplicateTrackingId")]
    public async Task<IActionResult> IsDuplicateTrackingId([FromQuery] string trackingId, CancellationToken ct)
        => Ok(new { trackingId, duplicate = await assets.IsDuplicateTrackingIdAsync(trackingId, ct) });
}

public sealed class AssetAttrBody { public required Guid Id { get; set; } public Dictionary<string, string>? Attributes { get; set; } }
public sealed class AssetTagsBody { public required Guid Id { get; set; } public List<string>? Tags { get; set; } }
public sealed class AssetValueBody { public required Guid Id { get; set; } public decimal Value { get; set; } }
public sealed class AssetStringBody { public required Guid Id { get; set; } public required string Value { get; set; } }
public sealed class AssetMaintainBody { public required Guid Id { get; set; } public string? ActionTaken { get; set; } }
public sealed class AssetWarrantyBody
{
    public required Guid Id { get; set; }
    public string? CoveredBy { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Details { get; set; }
}
public sealed class AssetCloneBody
{
    public required Guid Id { get; set; }
    public string? NewTrackingId { get; set; }
    public List<string>? TrackingIds { get; set; }
}
public sealed class AssetIdBody { public required Guid Id { get; set; } }
