using Ct.Arte.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Ct.Arte.Application.Dtos;

public sealed record ArtefactDto(
    string Id,
    string OwnerId,
    string Title,
    string? Description,
    string? Url,
    string? MimeType,
    string? FileName,
    double FileSize,
    string? Sha1,
    ArtefactStatus Status,
    string? Model,
    DateTimeOffset CreatedOn,
    string CreatedBy,
    DateTimeOffset? ModifiedOn,
    IReadOnlyList<string> Tags,
    IReadOnlyDictionary<string, string> MetaData,
    DateTimeOffset? PublishedOn);

public sealed class CreateArtefactRequest
{
    [Required, MaxLength(512)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(2048)]
    public string? Url { get; set; }

    [MaxLength(128)]
    public string? MimeType { get; set; }

    [MaxLength(512)]
    public string? FileName { get; set; }

    public double FileSize { get; set; }

    [MaxLength(64)]
    public string? Sha1 { get; set; }

    public string? Model { get; set; }

    public List<string>? Tags { get; set; }

    public Dictionary<string, string>? MetaData { get; set; }

    public string? Template { get; set; }
    public string? Model { get; set; }
    public string? Csv { get; set; }
}

public sealed class UpdateArtefactRequest
{
    [MaxLength(512)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    public ArtefactStatus? Status { get; set; }

    public List<string>? Tags { get; set; }

    public Dictionary<string, string>? MetaData { get; set; }

    public string? Url { get; set; }
    public string? MimeType { get; set; }
    public string? FileName { get; set; }
    public string? Template { get; set; }
    public string? Model { get; set; }
    public string? Csv { get; set; }
}

public sealed class ArtefactSearchRequest
{
    public string? Search { get; set; }
    public ArtefactStatus? Status { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
}

public static class ArtefactMappings
{
    public static ArtefactDto ToDto(this Artefact a) => new(
        a.Id,
        a.OwnerId,
        a.Title,
        a.Description,
        a.Url,
        a.MimeType,
        a.FileName,
        a.FileSize,
        a.Sha1,
        a.Status,
        a.Model,
        a.CreatedOn,
        a.CreatedBy,
        a.ModifiedOn,
        a.Tags,
        a.MetaData,
        a.PublishedOn);

    public static Artefact ToEntity(this CreateArtefactRequest req, string ownerId)
    {
        var entity = new Artefact
        {
            Title = req.Title,
            Description = req.Description,
            Url = req.Url,
            MimeType = req.MimeType,
            FileName = req.FileName,
            FileSize = req.FileSize,
            Sha1 = req.Sha1,
            Model = req.Model,
            Status = ArtefactStatus.Active,
            Tags = req.Tags ?? [],
            MetaData = req.MetaData ?? new()
        };
        entity.EnsureId();
        entity.SetOwner(ownerId);
        return entity;
    }
}

public sealed class IdBody { public required string Id { get; set; } }
public sealed class TagsBody { public required string Id { get; set; } public List<string> Tags { get; set; } = []; }
public sealed class DescriptionBody { public required string Id { get; set; } public string? Description { get; set; } }
public sealed class CommentBody { public required string Id { get; set; } public required string Text { get; set; } public string? Html { get; set; } }
public sealed class AttributeBody { public required string Id { get; set; } public required string Key { get; set; } public object? Value { get; set; } public long ItemId { get; set; } }
public sealed class VideoLinkBody { public required string Id { get; set; } public string Type { get; set; } = "Stream"; public required string Url { get; set; } public long ItemId { get; set; } }
public sealed class PublicUrlBody { public required string Id { get; set; } public required string Url { get; set; } }
public sealed class TagNameBody { public required string Name { get; set; } public string? Id { get; set; } }
public sealed class UploadMeta
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
    public string? FileName { get; set; }
    public string? MimeType { get; set; }
    public string? Url { get; set; }
}
