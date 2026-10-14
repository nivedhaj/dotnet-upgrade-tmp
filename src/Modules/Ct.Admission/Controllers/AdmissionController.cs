using Ct.Admission.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Admission.Controllers;

[ApiController]
[Route("api/admission")]
[Route("api/Admission")]
public sealed class AdmissionController(AdmissionService adm) : ControllerBase
{
    [HttpPost("seatMatrix")]
    public async Task<IActionResult> SeatMatrix([FromBody] SeatMatrixRequest request, CancellationToken ct)
        => Ok(await adm.CreateSeatMatrixAsync(request, ct));

    [HttpPost("seatMatrix/update")]
    public async Task<IActionResult> UpdateSeatMatrix([FromQuery] Guid id, [FromBody] SeatMatrixRequest request, CancellationToken ct)
    {
        var s = await adm.UpdateSeatMatrixAsync(id, request, ct);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpGet("seatMatrix")]
    public async Task<IActionResult> GetSeatMatrix(CancellationToken ct)
        => Ok(await adm.GetSeatMatricesAsync(ct));

    [HttpGet("seatsAvailable")]
    public async Task<IActionResult> GetSeatsAvailableForCourse([FromQuery] string courseId, CancellationToken ct)
    {
        var s = await adm.GetSeatsAvailableAsync(courseId, ct);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpGet("applicationsCoursewise")]
    public async Task<IActionResult> ApplicationsCoursewise(CancellationToken ct)
        => Ok(await adm.ApplicationsCoursewiseAsync(ct));

    [HttpGet]
    public async Task<IActionResult> GetAdmissions(CancellationToken ct)
        => Ok(await adm.GetAdmissionsAsync(ct));
}
