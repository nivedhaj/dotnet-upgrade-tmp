using System.Text.Json.Serialization;

namespace Ct.Arte.Domain.Entities;

public enum ArtefactStatus
{
    Unknown = 0,
    Active = 1,
    Etl = 60,
    Published = 70,
    PublishedToPublic = 75,
    Uploaded = 80,
    Downloaded = 90,
    Disabled = 99
}

public enum VideoType
{
    Record = 10,
    Drive = 20,
    Stream = 30
}

public sealed class KeyValueAttr
{
    public string Key { get; set; } = string.Empty;
    public object? Value { get; set; }
}

public sealed class DataAttribute
{
    public long ItemId { get; set; }
    public List<KeyValueAttr> Attributes { get; set; } = [];
}

public sealed class VideoLink
{
    public VideoType Type { get; set; }
    public string Url { get; set; } = string.Empty;
    public long ItemId { get; set; }
}

public sealed class Comment
{
    public string By { get; set; } = string.Empty;
    public DateTimeOffset TimeStamp { get; set; }
    public long ItemId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? Html { get; set; }
}

public sealed class ChangeRef
{
    public string By { get; set; } = string.Empty;
    public DateTimeOffset On { get; set; }
    public string Ref { get; set; } = string.Empty;
}

/// <summary>
/// Primary domain entity – maps to table "artefacts".
/// Nested collections are stored as JSONB for flexibility and parity with Raven documents.
/// </summary>
public class Artefact : EntityBase
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? MimeType { get; set; }
    public string? FileName { get; set; }
    public double FileSize { get; set; }
    public string? Sha1 { get; set; }
    public ArtefactStatus Status { get; set; } = ArtefactStatus.Active;

    public string? Model { get; set; }
    public string? Template { get; set; }
    public string? Csv { get; set; }

    public DateTimeOffset? PublishedOn { get; set; }

    // JSONB columns
    public Dictionary<string, string> MetaData { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Tags { get; set; } = [];
    public List<Comment> Comments { get; set; } = [];
    public List<VideoLink> VideoLinks { get; set; } = [];
    public List<DataAttribute> DataAttributes { get; set; } = [];
    public List<ChangeRef> ChangeSet { get; set; } = [];
    public List<string> PublicUrls { get; set; } = [];
    public List<string> Thumbnails { get; set; } = [];

    // Convenience helpers (not mapped)
    [JsonIgnore]
    public bool IsActive => Status == ArtefactStatus.Active;

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag)) return;
        if (!Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            Tags.Add(tag);
    }

    public void AddComment(string by, string text, string? html = null)
    {
        Comments.Add(new Comment
        {
            By = by,
            Text = text,
            Html = html,
            TimeStamp = DateTimeOffset.UtcNow,
            ItemId = Comments.Count + 1
        });
    }
}
