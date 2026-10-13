using Ct.Connect.Application;
using Ct.Connect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Connect.Controllers;

[ApiController]
[Route("api/connect/smsTemplate")]
[Route("api/SMSTemplate")]
public sealed class SMSTemplateController(ConnectCmd cmd, ConnectQry qry) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool activeOnly = false, CancellationToken ct = default)
        => Ok(await qry.GetSmsMessagesAsync(activeOnly, ct));

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ApprovalRequest request, CancellationToken ct)
    {
        var msg = await cmd.CreateSmsTemplateAsync(request.Message, request.Approve, ct);
        return Ok(new { msg.Id, Status = msg.Status.ToString() });
    }

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromBody] IdRequest request, CancellationToken ct)
    {
        await cmd.EnableSmsTemplateAsync(request.Id, ct);
        return Ok();
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromBody] IdRequest request, CancellationToken ct)
    {
        await cmd.DisableSmsTemplateAsync(request.Id, ct);
        return Ok();
    }

    [HttpPost("approve")]
    public async Task<IActionResult> Approve([FromBody] IdRequest request, CancellationToken ct)
    {
        await cmd.ApproveSmsTemplateAsync(request.Id, null, ct);
        return Ok();
    }

    [HttpPost("disapprove")]
    public async Task<IActionResult> Disapprove([FromBody] DisapproveRequest request, CancellationToken ct)
    {
        await cmd.DisapproveSmsTemplateAsync(request.Id, request.Reason, ct);
        return Ok();
    }
}
