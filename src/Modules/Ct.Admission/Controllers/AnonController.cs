using Ct.Admission.Application;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Admission.Controllers;

[ApiController]
[Route("api/admission/anon")]
[Route("api/Anon")]
public sealed class AnonController(OtpService otp) : ControllerBase
{
    [HttpPost("otp")]
    public async Task<IActionResult> OTP([FromBody] OtpBody body, CancellationToken ct)
        => Ok(await otp.RequestOtpAsync(body.MobileOrEmail, ct));
}

public sealed class OtpBody { public required string MobileOrEmail { get; set; } }
