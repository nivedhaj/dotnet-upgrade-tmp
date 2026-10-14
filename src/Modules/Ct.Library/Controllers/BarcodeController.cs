using Ct.Library.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Library.Controllers;

[ApiController]
[Route("api/library/barcode")]
[Route("api/Barcode")]
public sealed class BarcodeController(BarcodeService barcodes) : ControllerBase
{
    [HttpPost("generate")]
    public IActionResult Generate([FromBody] BarcodeRequest? request)
        => Ok(barcodes.Generate(request?.TrackingId, request?.Count ?? 1));
}

public sealed class BarcodeRequest { public string? TrackingId { get; set; } public int Count { get; set; } = 1; }
