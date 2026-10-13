namespace Ct.Connect.Domain;

public enum SMSGatewayEnum
{
    Unknown = -1,
    Infini = 2
}

public class SMSRecipient
{
    public string? Name { get; set; }
    public string Mobile { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? GatewayRef { get; set; }
}

/// <summary>
/// Sent SMS record.
/// </summary>
public class SMS : Entity
{
    public SMSGatewayEnum Gateway { get; set; } = SMSGatewayEnum.Unknown;

    public string? GatewayResult { get; set; }

    /// <summary>Stored as JSONB.</summary>
    public List<SMSRecipient> Recipients { get; set; } = [];

    public string Message { get; set; } = string.Empty;

    public string? SMSRefId { get; set; }
}
