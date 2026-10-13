using System.Text.RegularExpressions;
using Ct.Connect.Application;
using Ct.Connect.Domain;
using Ct.Connect.Infrastructure.Persistence;
using Ct.Connect.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ct.Connect.Infrastructure.Repositories;

/// <summary>Full Postgres port of original ConnectCmd + SMSTemplateCmd.</summary>
public sealed class ConnectCmd(
    ConnectDbContext db,
    IUserContextAccessor ucAccessor,
    IConnectEventPublisher events,
    IConfiguration config,
    ILogger<ConnectCmd> logger)
{
    private IUserContext Uc => ucAccessor.Current
        ?? throw new InvalidOperationException("User context is not available.");

    private void EnsureInstScope()
    {
        if (!Uc.BoundByInst)
            throw new UnauthorizedAccessException($"Not in scope for institute {Uc.InstId}");
    }

    // ── SMS templates ────────────────────────────────────────────────────────

    public async Task<SmsMessage> RequestApprovalAsync(string message, string? instId = null, bool approve = false, CancellationToken ct = default)
    {
        EnsureInstScope();
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        var ownerId = string.IsNullOrWhiteSpace(instId) ? Uc.InstId : instId!;
        var msg = new SmsMessage
        {
            Message = message.Trim(),
            Status = approve ? SmsMessageStatusEnum.Active : SmsMessageStatusEnum.Pending
        };
        msg.SetId();
        msg.SetOwner(ownerId);
        msg.CreatedBy = Uc.UserId;
        msg.CreatedOn = DateTime.UtcNow;
        db.SmsMessages.Add(msg);
        await db.SaveChangesAsync(ct);
        return msg;
    }

    public Task<SmsMessage> CreateSmsTemplateAsync(string message, bool approve = false, CancellationToken ct = default)
        => RequestApprovalAsync(message, null, approve, ct);

    public async Task EnableSmsTemplateAsync(string id, CancellationToken ct = default)
    {
        EnsureInstScope();
        var msg = await LoadSmsMessageAsync(id, ct);
        msg.Enable();
        await db.SaveChangesAsync(ct);
    }

    public async Task DisableSmsTemplateAsync(string id, CancellationToken ct = default)
    {
        EnsureInstScope();
        var msg = await LoadSmsMessageAsync(id, ct);
        msg.Disable();
        await db.SaveChangesAsync(ct);
    }

    public async Task ApproveSmsTemplateAsync(string id, string? reason = null, CancellationToken ct = default)
    {
        EnsureInstScope();
        var msg = await LoadSmsMessageAsync(id, ct);
        msg.Approve(string.IsNullOrWhiteSpace(reason) ? "approved" : reason!);
        await db.SaveChangesAsync(ct);
    }

    public async Task DisapproveSmsTemplateAsync(string id, string? reason, CancellationToken ct = default)
    {
        EnsureInstScope();
        var msg = await LoadSmsMessageAsync(id, ct);
        msg.Disapprove(string.IsNullOrWhiteSpace(reason) ? "disapproved" : reason!);
        await db.SaveChangesAsync(ct);
    }

    // ── SMS send (persist + enqueue like original SendSMS / SMSEnqueJob) ─────

    public async Task<SMS> SendSmsAsync(
        SendSmsRequest request,
        string? senderId = null,
        CancellationToken ct = default)
    {
        EnsureInstScope();
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Message);
        if (request.Recipients.Count == 0)
            throw new ArgumentException("At least one recipient is required.");

        senderId ??= config["Connect:Sms:SenderId"] ?? config["Sms:SenderId"] ?? "";

        var recipients = request.Recipients
            .Select(r => new SMSRecipient
            {
                Name = r.Name,
                Mobile = NormalizeMobile(r.Mobile) ?? "",
                Status = "queued"
            })
            .Where(r => !string.IsNullOrEmpty(r.Mobile))
            .GroupBy(r => r.Mobile)
            .Select(g => g.First())
            .ToList();

        if (recipients.Count == 0)
            throw new ArgumentException("No valid mobile numbers.");

        // Chunk 256 like original
        SMS? last = null;
        foreach (var chunk in recipients.Chunk(256))
        {
            var sms = new SMS
            {
                Message = request.Message.Trim(),
                SMSRefId = request.SmsRefId,
                Gateway = SMSGatewayEnum.Infini,
                Recipients = chunk.ToList()
            };
            sms.SetId();
            sms.SetOwner(Uc.InstId);
            sms.CreatedBy = Uc.UserId;
            sms.CreatedOn = DateTime.UtcNow;
            db.Sms.Add(sms);
            await db.SaveChangesAsync(ct);

            var mobiles = chunk.Select(c => c.Mobile).ToList();
            await events.PublishSmsJobAsync(new ConnectJobMessage
            {
                id = Guid.NewGuid().ToString(),
                title = "sms_send",
                user_id = Uc.UserId,
                client_id = Uc.InstId,
                payload = new SmsEnqueuePayload
                {
                    mobiles = mobiles,
                    message = sms.Message,
                    sender_id = senderId,
                    sms_id = sms.Id,
                    inst_id = Uc.InstId
                }
            }, ct);

            logger.LogInformation("SMS {Id} enqueued for {Count} recipients (credits≈{Credits})",
                sms.Id, mobiles.Count, mobiles.Count * SmsCreditHelper.CreditsPerMessage(sms.Message.Length));
            last = sms;
        }
        return last!;
    }

    /// <summary>Original controller SMS([FromBody] RecipientPostData).</summary>
    public async Task<SMS?> SendSmsFromRecipientPostAsync(RecipientPostData data, string? senderId, CancellationToken ct = default)
    {
        var mobiles = (data.Recipients?.Manual ?? [])
            .Select(NormalizeMobile)
            .Where(m => !string.IsNullOrEmpty(m))
            .Distinct()
            .Select(m => new SmsRecipientDto { Mobile = m! })
            .ToList();
        if (mobiles.Count == 0 || string.IsNullOrWhiteSpace(data.Text))
            return null;
        return await SendSmsAsync(new SendSmsRequest
        {
            Message = data.Text!,
            SmsRefId = data.SMSRefId,
            Recipients = mobiles
        }, senderId, ct);
    }

    // ── Email (persist + EnqueEmail) ─────────────────────────────────────────

    public async Task<Email> SendEmailAsync(SendEmailRequest request, CancellationToken ct = default)
    {
        EnsureInstScope();
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Message);
        var recipients = request.Recipients
            .Select(NormalizeEmail)
            .Where(e => !string.IsNullOrEmpty(e))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToList();
        if (recipients.Count == 0)
            throw new ArgumentException("At least one valid recipient is required.");

        var email = new Email
        {
            Type = request.Type,
            From = request.From ?? config["Connect:Email:From"] ?? "",
            Subject = request.Subject.Trim(),
            Message = request.Message,
            Recipients = recipients
        };
        email.SetId();
        email.SetOwner(Uc.InstId);
        email.CreatedBy = Uc.UserId;
        email.CreatedOn = DateTime.UtcNow;
        db.Emails.Add(email);
        await db.SaveChangesAsync(ct);

        await events.PublishEmailJobAsync(new EmailEnqueuePayload
        {
            subject = email.Subject,
            from = email.From,
            to = recipients,
            html = email.Message,
            inst_id = Uc.InstId,
            inst_short_name = Uc.InstId.Length >= 4 ? Uc.InstId[..4] : Uc.InstId,
            sender_id = email.From
        }, ct);

        return email;
    }

    public async Task<Email?> SendEmailFromDtoAsync(CreateEmailDto dto, CancellationToken ct = default)
    {
        var to = new List<string>();
        if (dto.To is { Count: > 0 }) to.AddRange(dto.To);
        if (dto.Recipients?.Manual is { Count: > 0 }) to.AddRange(dto.Recipients.Manual);
        if (to.Count == 0) return null;
        return await SendEmailAsync(new SendEmailRequest
        {
            Type = dto.Type,
            From = dto.From,
            Subject = dto.Subject ?? "",
            Message = dto.Html ?? dto.Message ?? "",
            Recipients = to
        }, ct);
    }

    public List<ContactLookupDto> Lookup(IEnumerable<string> contacts)
    {
        var result = new List<ContactLookupDto>();
        foreach (var raw in contacts.Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            var c = raw.Trim();
            if (c.Contains('@'))
            {
                var norm = NormalizeEmail(c);
                result.Add(new ContactLookupDto { Contact = c, IsValid = norm is not null, Normalized = norm, Kind = "email" });
            }
            else
            {
                var norm = NormalizeMobile(c);
                result.Add(new ContactLookupDto
                {
                    Contact = c,
                    IsValid = norm is { Length: 10 },
                    Normalized = norm,
                    Kind = "mobile"
                });
            }
        }
        return result;
    }

    private async Task<SmsMessage> LoadSmsMessageAsync(string id, CancellationToken ct) =>
        await db.SmsMessages.FirstOrDefaultAsync(m => m.Id == id && m.OwnerId == Uc.InstId, ct)
        ?? throw new KeyNotFoundException($"SmsMessage '{id}' not found.");

    private static string? NormalizeMobile(string? mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile)) return null;
        var digits = Regex.Replace(mobile, @"\D", "");
        if (digits.Length > 10) digits = digits[^10..];
        return digits;
    }

    private static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        email = email.Trim().ToLowerInvariant();
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") ? email : null;
    }
}
