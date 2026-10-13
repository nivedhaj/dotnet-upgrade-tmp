using Ct.Connect.Application;
using Ct.Connect.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Ct.Connect.Controllers;

[ApiController]
[Route("api/connect")]
[Route("api/Connect")]
public sealed class ConnectController(ConnectCmd cmd, ConnectQry qry) : ControllerBase
{
    [HttpGet("SMSSent")]
    [HttpGet("sms/sent")]
    public async Task<ActionResult<PagedResult<SmsUsageDto>>> SMSSent([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        => Ok(await qry.GetSmsSentAsync(page, pageSize, ct));

    [HttpGet("EmailSent")]
    [HttpGet("email/sent")]
    public async Task<ActionResult<PagedResult<EmailUsageDto>>> EmailSent([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        => Ok(await qry.GetEmailSentAsync(page, pageSize, ct));

    [HttpGet("smsMessages")]
    public async Task<ActionResult<IReadOnlyList<SmsMessageDto>>> SMSMessages([FromQuery] bool activeOnly = false, CancellationToken ct = default)
        => Ok(await qry.GetSmsMessagesAsync(activeOnly, ct));

    [HttpPost("requestApproval")]
    [HttpPost("sms/approval")]
    public async Task<IActionResult> RequestApproval([FromBody] object body, CancellationToken ct)
    {
        // Accept string or ApprovalRequest
        string message;
        string? instId = null;
        bool approve = false;
        if (body is System.Text.Json.JsonElement je)
        {
            if (je.ValueKind == System.Text.Json.JsonValueKind.String)
                message = je.GetString() ?? "";
            else
            {
                message = je.TryGetProperty("message", out var m) ? m.GetString() ?? ""
                    : je.TryGetProperty("Message", out var m2) ? m2.GetString() ?? "" : "";
                if (je.TryGetProperty("instId", out var i) || je.TryGetProperty("InstId", out i))
                    instId = i.GetString();
                if (je.TryGetProperty("approve", out var a) || je.TryGetProperty("Approve", out a))
                    approve = a.ValueKind == System.Text.Json.JsonValueKind.True;
            }
        }
        else
            message = body?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(message)) return BadRequest("message required");
        var msg = await cmd.RequestApprovalAsync(message, instId, approve, ct);
        return Accepted(new { msg.Id, Status = msg.Status.ToString() });
    }

    /// <summary>Original POST sms with RecipientPostData (manual mobiles + text).</summary>
    [HttpPost("sms")]
    public async Task<IActionResult> SMS([FromBody] RecipientPostData json, CancellationToken ct)
    {
        var senderId = qry.GetSmsSenderId();
        if (string.IsNullOrEmpty(senderId))
            return StatusCode(500, new { message = "SMS sender id is not configured (Connect:Sms:SenderId)." });

        var recipients = qry.GetSmsRecipients(json);
        if (recipients.Count == 0 || string.IsNullOrWhiteSpace(json.Text))
            return BadRequest(new { message = "Recipients and Text are required." });

        var sms = await cmd.SendSmsAsync(new SendSmsRequest
        {
            Message = json.Text!,
            SmsRefId = json.SMSRefId,
            Recipients = recipients
        }, senderId, ct);
        return Ok(new { sms.Id, recipients = recipients.Count });
    }

    /// <summary>Original POST email with CreateEmailDto.</summary>
    [HttpPost("email")]
    public async Task<IActionResult> Email([FromBody] CreateEmailDto json, CancellationToken ct)
    {
        var email = await cmd.SendEmailFromDtoAsync(json, ct);
        if (email is null) return BadRequest(new { message = "Recipients and message required." });
        return Ok(new { email.Id });
    }

    [HttpPost("lookup")]
    public ActionResult<IReadOnlyList<ContactLookupDto>> LookupContacts([FromBody] ContactLookupRequest request)
        => Ok(cmd.Lookup(request.Contacts));

    [HttpGet("smsDeliveryData")]
    public async Task<IActionResult> SMSDeliveryData([FromQuery] string smsId, CancellationToken ct)
    {
        var data = await qry.GetSmsDeliveryDataAsync(smsId, ct);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpGet("emailDeliveryData")]
    public async Task<IActionResult> EmailDeliveryData([FromQuery] string emailId, CancellationToken ct)
    {
        var data = await qry.GetEmailDeliveryDataAsync(emailId, ct);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPost("smsText")]
    public async Task<IActionResult> SMSText([FromBody] List<string> smsIds, CancellationToken ct)
        => Ok(await qry.GetSmsTextAsync(smsIds, ct));

    [HttpGet("sms/{id}")]
    public async Task<IActionResult> GetSms(string id, CancellationToken ct)
    {
        var s = await qry.GetSmsAsync(id, ct);
        return s is null ? NotFound() : Ok(s);
    }

    [HttpGet("email/{id}")]
    public async Task<IActionResult> GetEmail(string id, CancellationToken ct)
    {
        var e = await qry.GetEmailAsync(id, ct);
        return e is null ? NotFound() : Ok(e);
    }

    [HttpPost]
    public IActionResult Post([FromBody] string? id) => Ok("To Do");

    [HttpPost("enable")]
    public async Task<IActionResult> Enable([FromQuery] string id, CancellationToken ct)
    {
        await cmd.EnableSmsTemplateAsync(id, ct);
        return Ok();
    }

    [HttpPost("disable")]
    public async Task<IActionResult> Disable([FromQuery] string id, CancellationToken ct)
    {
        await cmd.DisableSmsTemplateAsync(id, ct);
        return Ok();
    }
}
