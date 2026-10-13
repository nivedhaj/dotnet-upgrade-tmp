namespace Ct.Connect.Domain;

public enum SmsMessageStatusEnum
{
    Pending = 0,
    Active = 1,
    Disapproved = 90,
    Disabled = 99
}

/// <summary>
/// SMS Template / Approval document (formerly SmsMessage in Raven).
/// </summary>
public class SmsMessage : Entity
{
    public string Message { get; set; } = string.Empty;

    public SmsMessageStatusEnum Status { get; set; } = SmsMessageStatusEnum.Pending;

    public string? Reason { get; set; }

    public void Enable() => Status = SmsMessageStatusEnum.Active;

    public void Disable() => Status = SmsMessageStatusEnum.Disabled;

    public void Approve(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status = SmsMessageStatusEnum.Active;
        Reason = reason;
    }

    public void Disapprove(string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        Status = SmsMessageStatusEnum.Disapproved;
        Reason = reason;
    }

    public int GetSmsCredits(int smsLen)
    {
        var len = smsLen <= 160 ? (smsLen - 7) / 153.0m : smsLen / 153.0m;
        var credits = (int)Math.Ceiling(len);
        return credits <= 0 ? 1 : credits;
    }
}
