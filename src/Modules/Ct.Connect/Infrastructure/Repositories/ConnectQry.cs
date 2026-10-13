using Ct.Connect.Application;
using Ct.Connect.Domain;
using Ct.Connect.Infrastructure.Persistence;
using Ct.Connect.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ct.Connect.Infrastructure.Repositories;

/// <summary>Full Postgres port of original ConnectQry + report queries.</summary>
public sealed class ConnectQry(
    ConnectDbContext db,
    IUserContextAccessor ucAccessor,
    IConnectEventPublisher events,
    IConfiguration config)
{
    private IUserContext Uc => ucAccessor.Current
        ?? throw new InvalidOperationException("User context is not available.");

    public string GetSmsSenderId() =>
        config["Connect:Sms:SenderId"]
        ?? config["Sms:SenderId"]
        ?? config[$"Connect:Sms:SenderIds:{Uc.InstId}"]
        ?? "";

    public List<SmsRecipientDto> GetSmsRecipients(RecipientPostData data)
    {
        var result = new List<SmsRecipientDto>();
        if (data.Recipients?.Manual is not { Count: > 0 }) return result;
        foreach (var m in data.Recipients.Manual.Distinct())
        {
            var digits = System.Text.RegularExpressions.Regex.Replace(m ?? "", @"\D", "");
            if (digits.Length > 10) digits = digits[^10..];
            if (digits.Length == 10)
                result.Add(new SmsRecipientDto { Mobile = digits });
        }
        return result;
    }

    public List<string> GetEmailRecipients(CreateEmailDto data)
    {
        var result = new List<string>();
        if (data.To is { Count: > 0 }) result.AddRange(data.To);
        if (data.Recipients?.Manual is { Count: > 0 }) result.AddRange(data.Recipients.Manual);
        return result.Where(e => !string.IsNullOrWhiteSpace(e) && e.Contains('@'))
            .Select(e => e.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();
    }

    public async Task<PagedResult<SmsUsageDto>> GetSmsSentAsync(int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var q = db.Sms.AsNoTracking().Where(s => s.OwnerId == Uc.InstId);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(s => s.CreatedOn)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        var items = rows.Select(s => new SmsUsageDto
        {
            OwnerId = s.OwnerId,
            Date = s.CreatedOn,
            Count = s.Recipients.Count,
            Credits = s.Recipients.Count * (long)SmsCreditHelper.CreditsPerMessage(s.Message?.Length ?? 0)
        }).ToList();
        return new PagedResult<SmsUsageDto>(items, total, page, pageSize);
    }

    public async Task<PagedResult<EmailUsageDto>> GetEmailSentAsync(int page = 1, int pageSize = 50, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var q = db.Emails.AsNoTracking().Where(e => e.OwnerId == Uc.InstId);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(e => e.CreatedOn)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new EmailUsageDto { OwnerId = e.OwnerId, Date = e.CreatedOn, Count = e.Recipients.Count })
            .ToListAsync(ct);
        return new PagedResult<EmailUsageDto>(items, total, page, pageSize);
    }

    public async Task<IReadOnlyList<SmsMessageDto>> GetSmsMessagesAsync(bool activeOnly = false, CancellationToken ct = default)
    {
        var q = db.SmsMessages.AsNoTracking().Where(m => m.OwnerId == Uc.InstId);
        if (activeOnly) q = q.Where(m => m.Status == SmsMessageStatusEnum.Active);
        return await q.OrderByDescending(m => m.CreatedOn)
            .Select(m => new SmsMessageDto
            {
                Id = m.Id, OwnerId = m.OwnerId, Message = m.Message,
                Status = m.Status.ToString(), CreatedOn = m.CreatedOn
            }).ToListAsync(ct);
    }

    public async Task<SmsDto?> GetSmsAsync(string id, CancellationToken ct = default)
    {
        var s = await db.Sms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == Uc.InstId, ct);
        return s is null ? null : new SmsDto
        {
            Id = s.Id, Message = s.Message, SmsRefId = s.SMSRefId, Gateway = s.Gateway.ToString(),
            CreatedOn = s.CreatedOn,
            Recipients = s.Recipients.Select(r => new SmsRecipientDto { Name = r.Name, Mobile = r.Mobile, Status = r.Status }).ToList()
        };
    }

    public async Task<EmailDto?> GetEmailAsync(string id, CancellationToken ct = default)
    {
        var e = await db.Emails.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.OwnerId == Uc.InstId, ct);
        return e is null ? null : new EmailDto
        {
            Id = e.Id, Subject = e.Subject, From = e.From, Type = e.Type,
            Message = e.Message, Recipients = e.Recipients, CreatedOn = e.CreatedOn
        };
    }

    public async Task<object?> GetSmsDeliveryDataAsync(string smsId, CancellationToken ct = default)
    {
        var s = await db.Sms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == smsId && x.OwnerId == Uc.InstId, ct);
        if (s is null) return null;

        // Original also enqueued a delivery-data job
        await events.PublishSmsDeliveryDataJobAsync(new ConnectJobMessage
        {
            id = Guid.NewGuid().ToString(),
            title = "sms_delivery_data",
            user_id = Uc.UserId,
            client_id = Uc.InstId,
            payload = new { sms_id = smsId, mobiles = s.Recipients.Select(r => r.Mobile).ToList() }
        }, ct);

        return new
        {
            s.Id, s.SMSRefId, Gateway = s.Gateway.ToString(), s.GatewayResult,
            Recipients = s.Recipients.Select(r => new { r.Name, r.Mobile, r.Status, r.GatewayRef }),
            Credits = s.Recipients.Count * SmsCreditHelper.CreditsPerMessage(s.Message?.Length ?? 0)
        };
    }

    public async Task<object?> GetEmailDeliveryDataAsync(string emailId, CancellationToken ct = default)
    {
        var e = await db.Emails.AsNoTracking().FirstOrDefaultAsync(x => x.Id == emailId && x.OwnerId == Uc.InstId, ct);
        if (e is null) return null;

        await events.PublishEmailDeliveryDataJobAsync(new ConnectJobMessage
        {
            id = Guid.NewGuid().ToString(),
            title = "email_delivery_data",
            user_id = Uc.UserId,
            client_id = Uc.InstId,
            payload = new { email_id = emailId, recipients = e.Recipients }
        }, ct);

        return new { e.Id, e.Subject, e.From, e.Recipients, e.CreatedOn };
    }

    public async Task<IReadOnlyList<object>> GetSmsTextAsync(IEnumerable<string> smsIds, CancellationToken ct = default)
    {
        var ids = smsIds.ToList();
        return await db.Sms.AsNoTracking()
            .Where(s => s.OwnerId == Uc.InstId && ids.Contains(s.Id))
            .Select(s => new { SMSId = s.Id, Text = s.Message, s.SMSRefId, RecipientCount = s.Recipients.Count })
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<object>> GetInvalidMobilesReportAsync(CancellationToken ct = default)
    {
        var all = await db.Sms.AsNoTracking().Where(s => s.OwnerId == Uc.InstId).ToListAsync(ct);
        return all.SelectMany(s => s.Recipients
            .Where(r => string.IsNullOrWhiteSpace(r.Mobile) || r.Mobile.Length != 10)
            .Select(r => new { SmsId = s.Id, r.Name, r.Mobile, r.Status })).ToList();
    }

    public async Task<IReadOnlyList<object>> GetInvalidEmailsReportAsync(CancellationToken ct = default)
    {
        var all = await db.Emails.AsNoTracking().Where(e => e.OwnerId == Uc.InstId).ToListAsync(ct);
        return all.SelectMany(e => e.Recipients
            .Where(r => string.IsNullOrWhiteSpace(r) || !r.Contains('@'))
            .Select(r => new { EmailId = e.Id, Recipient = r, e.Subject })).ToList();
    }

    public async Task<object> GetSmsUsageReportAsync(CancellationToken ct = default)
    {
        var rows = await db.Sms.AsNoTracking().Where(s => s.OwnerId == Uc.InstId).ToListAsync(ct);
        var byDay = rows.GroupBy(s => s.CreatedOn.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                Recipients = g.Sum(x => x.Recipients.Count),
                Credits = g.Sum(x => x.Recipients.Count * SmsCreditHelper.CreditsPerMessage(x.Message?.Length ?? 0))
            })
            .OrderByDescending(x => x.Date).ToList();
        return new
        {
            TotalSms = rows.Count,
            TotalRecipients = rows.Sum(r => r.Recipients.Count),
            TotalCredits = rows.Sum(r => r.Recipients.Count * SmsCreditHelper.CreditsPerMessage(r.Message?.Length ?? 0)),
            ByDay = byDay
        };
    }
}
