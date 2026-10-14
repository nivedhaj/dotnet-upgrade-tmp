using Ct.Admission.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Admission.Controllers;

[ApiController]
[Route("api/admission/application")]
[Route("api/Application")]
public sealed class ApplicationController(ApplicationService apps) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest request, CancellationToken ct)
        => Ok(await apps.CreateAsync(request, ct));

    [HttpGet]
    public async Task<IActionResult> GetApplications(CancellationToken ct) => Ok(await apps.GetAllAsync(ct));

    [HttpGet("getByEmail")]
    public async Task<IActionResult> GetApplicationByEmail([FromQuery] string email, CancellationToken ct)
    {
        var a = await apps.GetByEmailAsync(email, ct);
        return a is null ? NotFound() : Ok(a);
    }

    [HttpPost("updateIdentity")]
    public async Task<IActionResult> Identity([FromBody] DictBody body, CancellationToken ct)
        => await apps.UpdateIdentityAsync(body.Id, body.Data ?? new(), ct) ? Ok() : NotFound();

    [HttpPost("updateFatherDetails")]
    public async Task<IActionResult> FatherDetails([FromBody] DictBody body, CancellationToken ct)
        => await apps.UpdateFatherAsync(body.Id, body.Data ?? new(), ct) ? Ok() : NotFound();

    [HttpPost("updateMotherDetails")]
    public async Task<IActionResult> MotherDetails([FromBody] DictBody body, CancellationToken ct)
        => await apps.UpdateMotherAsync(body.Id, body.Data ?? new(), ct) ? Ok() : NotFound();

    [HttpPost("updateGuardianDetails")]
    public async Task<IActionResult> GuardianDetails([FromBody] DictBody body, CancellationToken ct)
        => await apps.UpdateGuardianAsync(body.Id, body.Data ?? new(), ct) ? Ok() : NotFound();

    [HttpPost("updateAcademicHistory")]
    public async Task<IActionResult> UpdateAcademicDetails([FromBody] AcademicBody body, CancellationToken ct)
        => await apps.UpdateAcademicAsync(body.Id, body.History ?? [], ct) ? Ok() : NotFound();

    [HttpPost("updatePhotoUrl")]
    public async Task<IActionResult> UpdatePhotoURL([FromBody] DocBody body, CancellationToken ct)
        => await apps.UpdateDocumentAsync(body.Id, "photo", body.Url, ct) ? Ok() : NotFound();

    [HttpPost("updateAadharUrl")]
    public async Task<IActionResult> UpdateAadharURL([FromBody] DocBody body, CancellationToken ct)
        => await apps.UpdateDocumentAsync(body.Id, "aadhar", body.Url, ct) ? Ok() : NotFound();

    [HttpPost("updateHSCMarksCardUrl")]
    public async Task<IActionResult> UpdateHSCUrl([FromBody] DocBody body, CancellationToken ct)
        => await apps.UpdateDocumentAsync(body.Id, "hsc", body.Url, ct) ? Ok() : NotFound();

    [HttpPost("updateSSCMarksCardUrl")]
    public async Task<IActionResult> UpdateSSCUrl([FromBody] DocBody body, CancellationToken ct)
        => await apps.UpdateDocumentAsync(body.Id, "ssc", body.Url, ct) ? Ok() : NotFound();

    [HttpPost("casteCertificateUrl")]
    public async Task<IActionResult> UpdateCasteCertificateUrl([FromBody] DocBody body, CancellationToken ct)
        => await apps.UpdateDocumentAsync(body.Id, "caste", body.Url, ct) ? Ok() : NotFound();

    [HttpPost("leavingCertificateUrl")]
    public async Task<IActionResult> UpdateLeavingCertificateUrl([FromBody] DocBody body, CancellationToken ct)
        => await apps.UpdateDocumentAsync(body.Id, "leaving", body.Url, ct) ? Ok() : NotFound();

    [HttpPost("domicileCertificateUrl")]
    public async Task<IActionResult> UpdateDomicileCertificateUrl([FromBody] DocBody body, CancellationToken ct)
        => await apps.UpdateDocumentAsync(body.Id, "domicile", body.Url, ct) ? Ok() : NotFound();

    [HttpPost("transferCertificateUrl")]
    public async Task<IActionResult> UpdateTransferCertificateUrl([FromBody] DocBody body, CancellationToken ct)
        => await apps.UpdateDocumentAsync(body.Id, "transfer", body.Url, ct) ? Ok() : NotFound();

    [HttpPost("birthCertificateUrl")]
    public async Task<IActionResult> UpdateBirthCertificateUrl([FromBody] DocBody body, CancellationToken ct)
        => await apps.UpdateDocumentAsync(body.Id, "birth", body.Url, ct) ? Ok() : NotFound();

    [HttpPost("admit")]
    public async Task<IActionResult> AdmitApplicant([FromBody] AdmIdBody body, CancellationToken ct)
        => await apps.AdmitAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpGet("admittedApplicants")]
    public async Task<IActionResult> GetAdmittedApplicants(CancellationToken ct)
        => Ok(await apps.GetAdmittedAsync(ct));

    [HttpPost("shortlist")]
    public async Task<IActionResult> Shortlist([FromBody] AdmIdBody body, CancellationToken ct)
        => await apps.ShortlistAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("select")]
    public async Task<IActionResult> Select([FromBody] AdmIdBody body, CancellationToken ct)
        => await apps.SelectAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("decline")]
    public async Task<IActionResult> Decline([FromBody] AdmIdBody body, CancellationToken ct)
        => await apps.DeclineAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("reject")]
    public async Task<IActionResult> Reject([FromBody] AdmIdBody body, CancellationToken ct)
        => await apps.RejectAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("select/remove")]
    public async Task<IActionResult> RemoveFromSelection([FromBody] AdmIdBody body, CancellationToken ct)
        => await apps.RemoveFromSelectionAsync(body.Id, ct) ? Ok() : NotFound();

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] AdmIdBody body, CancellationToken ct)
        => await apps.SubmitAsync(body.Id, ct) ? Ok() : NotFound();
}

public sealed class AdmIdBody { public required Guid Id { get; set; } }
public sealed class DictBody { public required Guid Id { get; set; } public Dictionary<string, string>? Data { get; set; } }
public sealed class AcademicBody { public required Guid Id { get; set; } public List<Dictionary<string, string>>? History { get; set; } }
public sealed class DocBody { public required Guid Id { get; set; } public required string Url { get; set; } }
