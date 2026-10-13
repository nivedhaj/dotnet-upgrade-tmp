namespace Ct.Connect.Domain;

public class Attachment
{
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public string? Url { get; set; }
    public long? Size { get; set; }
}

public class Email : Entity
{
    public List<string> Recipients { get; set; } = [];

    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string From { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    /// <summary>Stored as JSONB.</summary>
    public List<Attachment>? Attachments { get; set; }
}
