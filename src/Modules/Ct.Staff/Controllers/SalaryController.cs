using Ct.Staff.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Staff.Controllers;

[ApiController]
[Route("api/staff/salary")]
[Route("api/Salary")]
public sealed class SalaryController(SalaryService salary) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateSalaryStructure([FromBody] CreateSalaryStructureRequest request, CancellationToken ct)
        => Ok(await salary.CreateStructureAsync(request, ct));

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await salary.GetAllAsync(ct));

    [HttpGet("currentSalaryStructure")]
    public async Task<IActionResult> GetCurrentSalaryStructure(CancellationToken ct)
    {
        var s = await salary.GetCurrentAsync(ct);
        return s is null ? NotFound() : Ok(s);
    }
}
