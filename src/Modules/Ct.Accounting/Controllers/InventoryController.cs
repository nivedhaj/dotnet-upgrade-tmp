using Ct.Accounting.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Accounting.Controllers;

[ApiController]
[Route("api/accounting/inventory")]
[Route("api/Inventory")]
public sealed class InventoryController(InventoryService inv) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateInventoryItemRequest request, CancellationToken ct)
        => Ok(await inv.CreateItemAsync(request, ct));
}
