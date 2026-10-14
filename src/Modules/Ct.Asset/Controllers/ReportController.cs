using Ct.Asset.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Asset.Controllers;

[ApiController]
[Route("api/asset/report")]
[Route("api/Report")]
public sealed class ReportController(AssetService assets) : ControllerBase
{
    [HttpPost("locationWiseAssets")]
    public async Task<IActionResult> GetLocationwiseAssetsReport(CancellationToken ct)
        => Ok(await assets.LocationWiseReportAsync(ct));

    [HttpPost("locationWiseAssets/html")]
    [HttpGet("locationWiseAssets/html")]
    public async Task<IActionResult> GetLocationwiseAssetsHtml(CancellationToken ct)
    {
        var data = await assets.LocationWiseReportAsync(ct);
        dynamic d = data;
        var sb = new System.Text.StringBuilder();
        sb.Append("<html><body><h2>Location-wise Assets</h2>");
        sb.Append($"<p>Total: {d.total}</p><table border='1' cellpadding='4'><tr><th>Location</th><th>Count</th><th>Value</th></tr>");
        foreach (var g in d.byLocation)
            sb.Append($"<tr><td>{System.Net.WebUtility.HtmlEncode((string)g.location)}</td><td>{g.count}</td><td>{g.totalValue}</td></tr>");
        sb.Append("</table></body></html>");
        return Ok(new { html = sb.ToString(), data });
    }
}
