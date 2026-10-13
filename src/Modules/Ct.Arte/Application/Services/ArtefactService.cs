using System.Security.Cryptography;
using System.Text;
using Ct.Arte.Application.Dtos;
using Ct.Arte.Domain.Entities;
using Ct.Arte.Infrastructure.Persistence.Repositories;

namespace Ct.Arte.Application.Services;

public interface IArtefactService
{
    Task<ArtefactDto?> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<ArtefactDto>> SearchAsync(ArtefactSearchRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ArtefactDto>> GetAllAsync(CancellationToken ct = default);
    Task<ArtefactDto> CreateAsync(CreateArtefactRequest request, CancellationToken ct = default);
    Task<ArtefactDto?> UpdateAsync(string id, UpdateArtefactRequest request, CancellationToken ct = default);
    Task SoftDeleteAsync(string id, CancellationToken ct = default);
    Task<bool> EnableAsync(string id, CancellationToken ct = default);
    Task<bool> DisableAsync(string id, CancellationToken ct = default);
    Task<bool> PublishAsync(string id, bool toPublic, CancellationToken ct = default);
    Task<bool> UnpublishAsync(string id, bool publicOnly, CancellationToken ct = default);
    Task<bool> UpdateTagsAsync(string id, List<string> tags, CancellationToken ct = default);
    Task<bool> UpdateDescriptionAsync(string id, string? description, CancellationToken ct = default);
    Task<bool> AddCommentAsync(string id, string text, string? html, CancellationToken ct = default);
    Task<bool> AddAttributeAsync(string id, string key, object? value, long itemId, CancellationToken ct = default);
    Task<bool> AddVideoLinkAsync(string id, string type, string url, long itemId, CancellationToken ct = default);
    Task<bool> UpdatePublicUrlAsync(string id, string url, CancellationToken ct = default);
    Task<bool> RemovePublicUrlAsync(string id, string url, CancellationToken ct = default);
    Task<bool> ResetPublicUrlAsync(string id, CancellationToken ct = default);
    Task<ArtefactDto> UpsertAsync(CreateArtefactRequest request, CancellationToken ct = default);
    Task<ArtefactDto> UpsertStudentDocumentAsync(CreateArtefactRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ArtefactDto>> GetDataTemplatesAsync(string? mode, CancellationToken ct = default);
    Task<IReadOnlyList<ArtefactDto>> GetReportsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ArtefactDto>> GetByMimeTypeAsync(string mimeType, CancellationToken ct = default);
    Task<object?> GetTemplateAsync(string id, CancellationToken ct = default);
    Task<object?> GetModelAsync(string id, CancellationToken ct = default);
    Task<object?> GetUrlAsync(string id, CancellationToken ct = default);
    Task<object> GetFileNamesAsync(CancellationToken ct = default);
    Task<object> WidgetBandwidthAsync(CancellationToken ct = default);
    Task<object> WidgetCountAsync(CancellationToken ct = default);
    Task<object> WidgetSizeBucketsAsync(CancellationToken ct = default);
    Task<object> WidgetAuthorCountAsync(CancellationToken ct = default);
    Task<object> WidgetFileCountAsync(CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken ct = default);
    Task<object> CreateTagAsync(string name, CancellationToken ct = default);
    Task<byte[]> StudentAttendanceTemplateAsync();
    Task<object> GenerateEtlStubAsync(string kind, CancellationToken ct = default);
    Task<object> RunEtlStubAsync(string? artefactId, CancellationToken ct = default);
}

public sealed class ArtefactService : IArtefactService
{
    private readonly IArtefactRepository _repo;
    private readonly ICurrentUser _user;

    public ArtefactService(IArtefactRepository repo, ICurrentUser user)
    {
        _repo = repo;
        _user = user;
    }

    private string Owner => _user.OwnerId ?? throw new InvalidOperationException("OwnerId is required.");

    public async Task<ArtefactDto?> GetAsync(string id, CancellationToken ct = default)
        => (await _repo.GetByIdAsync(id, ct))?.ToDto();

    public async Task<IReadOnlyList<ArtefactDto>> SearchAsync(ArtefactSearchRequest request, CancellationToken ct = default)
    {
        var list = await _repo.SearchAsync(Owner, request.Search, request.Status, request.Skip, request.Take, ct);
        return list.Select(a => a.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<ArtefactDto>> GetAllAsync(CancellationToken ct = default)
        => (await _repo.ListByOwnerAsync(Owner, ct)).Select(a => a.ToDto()).ToList();

    public async Task<ArtefactDto> CreateAsync(CreateArtefactRequest request, CancellationToken ct = default)
    {
        var entity = request.ToEntity(Owner);
        entity.Status = ArtefactStatus.Active;
        await _repo.AddAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        return entity.ToDto();
    }

    public async Task<ArtefactDto?> UpdateAsync(string id, UpdateArtefactRequest request, CancellationToken ct = default)
    {
        var entity = await _repo.GetByIdTrackedAsync(id, ct);
        if (entity is null || entity.OwnerId != Owner) return null;
        if (request.Title is not null) entity.Title = request.Title;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.Url is not null) entity.Url = request.Url;
        if (request.MimeType is not null) entity.MimeType = request.MimeType;
        if (request.FileName is not null) entity.FileName = request.FileName;
        if (request.Tags is not null) entity.Tags = request.Tags;
        if (request.Template is not null) entity.Template = request.Template;
        if (request.Model is not null) entity.Model = request.Model;
        if (request.Csv is not null) entity.Csv = request.Csv;
        await _repo.UpdateAsync(entity, ct);
        await _repo.SaveChangesAsync(ct);
        return entity.ToDto();
    }

    public async Task SoftDeleteAsync(string id, CancellationToken ct = default)
    {
        await _repo.SoftDeleteAsync(id, ct);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task<bool> EnableAsync(string id, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        e.Status = ArtefactStatus.Active;
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> DisableAsync(string id, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        e.Status = ArtefactStatus.Disabled;
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> PublishAsync(string id, bool toPublic, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        e.Status = toPublic ? ArtefactStatus.PublishedToPublic : ArtefactStatus.Published;
        e.PublishedOn = DateTimeOffset.UtcNow;
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UnpublishAsync(string id, bool publicOnly, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        e.Status = publicOnly && e.Status == ArtefactStatus.PublishedToPublic
            ? ArtefactStatus.Published
            : ArtefactStatus.Active;
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UpdateTagsAsync(string id, List<string> tags, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        e.Tags = tags.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UpdateDescriptionAsync(string id, string? description, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        e.Description = description;
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AddCommentAsync(string id, string text, string? html, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        e.Comments.Add(new Comment
        {
            By = _user.UserId ?? "system",
            TimeStamp = DateTimeOffset.UtcNow,
            Text = text,
            Html = html,
            ItemId = e.Comments.Count + 1
        });
        e.Comments = e.Comments.ToList();
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AddAttributeAsync(string id, string key, object? value, long itemId, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        var block = e.DataAttributes.FirstOrDefault(a => a.ItemId == itemId);
        if (block is null) { block = new DataAttribute { ItemId = itemId }; e.DataAttributes.Add(block); }
        block.Attributes.RemoveAll(a => a.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        block.Attributes.Add(new KeyValueAttr { Key = key, Value = value });
        e.DataAttributes = e.DataAttributes.ToList();
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> AddVideoLinkAsync(string id, string type, string url, long itemId, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        Enum.TryParse<VideoType>(type, true, out var vt);
        e.VideoLinks.Add(new VideoLink { Type = vt, Url = url, ItemId = itemId });
        e.VideoLinks = e.VideoLinks.ToList();
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> UpdatePublicUrlAsync(string id, string url, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        if (!e.PublicUrls.Contains(url)) e.PublicUrls.Add(url);
        e.PublicUrls = e.PublicUrls.ToList();
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> RemovePublicUrlAsync(string id, string url, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        e.PublicUrls = e.PublicUrls.Where(u => u != url).ToList();
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<bool> ResetPublicUrlAsync(string id, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdTrackedAsync(id, ct);
        if (e is null || e.OwnerId != Owner) return false;
        e.PublicUrls = [];
        await _repo.UpdateAsync(e, ct); await _repo.SaveChangesAsync(ct); return true;
    }

    public async Task<ArtefactDto> UpsertAsync(CreateArtefactRequest request, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            var existing = await _repo.GetByFilenameAsync(Owner, request.FileName!, ct);
            if (existing is not null)
            {
                var tracked = await _repo.GetByIdTrackedAsync(existing.Id, ct);
                if (tracked is not null)
                {
                    tracked.Title = request.Title ?? tracked.Title;
                    tracked.Description = request.Description ?? tracked.Description;
                    tracked.Url = request.Url ?? tracked.Url;
                    tracked.MimeType = request.MimeType ?? tracked.MimeType;
                    if (request.Tags is { Count: > 0 }) tracked.Tags = request.Tags;
                    await _repo.UpdateAsync(tracked, ct);
                    await _repo.SaveChangesAsync(ct);
                    return tracked.ToDto();
                }
            }
        }
        return await CreateAsync(request, ct);
    }

    public Task<ArtefactDto> UpsertStudentDocumentAsync(CreateArtefactRequest request, CancellationToken ct = default)
    {
        request.Tags ??= [];
        if (!request.Tags.Contains("student-document")) request.Tags.Add("student-document");
        return UpsertAsync(request, ct);
    }

    public async Task<IReadOnlyList<ArtefactDto>> GetDataTemplatesAsync(string? mode, CancellationToken ct = default)
    {
        var list = await _repo.GetByTagsAsync(Owner, new[] { "data-template", "template" }, ct);
        if (string.Equals(mode, "live", StringComparison.OrdinalIgnoreCase))
            list = list.Where(a => a.Status is ArtefactStatus.Published or ArtefactStatus.PublishedToPublic).ToList();
        else if (string.Equals(mode, "working", StringComparison.OrdinalIgnoreCase))
            list = list.Where(a => a.Status == ArtefactStatus.Active).ToList();
        return list.Select(a => a.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<ArtefactDto>> GetReportsAsync(CancellationToken ct = default)
        => (await _repo.GetByTagsAsync(Owner, new[] { "report" }, ct)).Select(a => a.ToDto()).ToList();

    public async Task<IReadOnlyList<ArtefactDto>> GetByMimeTypeAsync(string mimeType, CancellationToken ct = default)
        => (await _repo.GetByMimeTypeAsync(Owner, mimeType, ct)).Select(a => a.ToDto()).ToList();

    public async Task<object?> GetTemplateAsync(string id, CancellationToken ct = default)
    {
        var a = await _repo.GetByIdAsync(id, ct);
        return a is null ? null : new { a.Id, a.Template, a.Title };
    }

    public async Task<object?> GetModelAsync(string id, CancellationToken ct = default)
    {
        var a = await _repo.GetByIdAsync(id, ct);
        return a is null ? null : new { a.Id, a.Model, a.Title };
    }

    public async Task<object?> GetUrlAsync(string id, CancellationToken ct = default)
    {
        var a = await _repo.GetByIdAsync(id, ct);
        return a is null ? null : new { a.Id, a.Url, a.FileName, a.PublicUrls };
    }

    public async Task<object> GetFileNamesAsync(CancellationToken ct = default)
    {
        var list = await _repo.ListByOwnerAsync(Owner, ct);
        return list.Select(a => new { a.Id, a.FileName, a.Title, a.MimeType }).ToList();
    }

    public async Task<object> WidgetBandwidthAsync(CancellationToken ct = default)
    {
        var list = await _repo.ListByOwnerAsync(Owner, ct);
        return new { totalBytes = list.Sum(a => a.FileSize), count = list.Count };
    }

    public async Task<object> WidgetCountAsync(CancellationToken ct = default)
    {
        var list = await _repo.ListByOwnerAsync(Owner, ct);
        return new
        {
            total = list.Count,
            byStatus = list.GroupBy(a => a.Status.ToString()).Select(g => new { status = g.Key, count = g.Count() })
        };
    }

    public async Task<object> WidgetSizeBucketsAsync(CancellationToken ct = default)
    {
        var list = await _repo.ListByOwnerAsync(Owner, ct);
        return new
        {
            small = list.Count(a => a.FileSize < 1_000_000),
            medium = list.Count(a => a.FileSize is >= 1_000_000 and < 10_000_000),
            large = list.Count(a => a.FileSize >= 10_000_000)
        };
    }

    public async Task<object> WidgetAuthorCountAsync(CancellationToken ct = default)
    {
        var list = await _repo.ListByOwnerAsync(Owner, ct);
        return list.GroupBy(a => a.CreatedBy).Select(g => new { author = g.Key, count = g.Count() }).ToList();
    }

    public async Task<object> WidgetFileCountAsync(CancellationToken ct = default)
    {
        var list = await _repo.ListByOwnerAsync(Owner, ct);
        return new { files = list.Count(a => !string.IsNullOrEmpty(a.FileName)), total = list.Count };
    }

    public async Task<IReadOnlyList<string>> GetTagsAsync(CancellationToken ct = default)
    {
        var list = await _repo.ListByOwnerAsync(Owner, ct);
        return list.SelectMany(a => a.Tags).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(t => t).ToList();
    }

    public Task<object> CreateTagAsync(string name, CancellationToken ct = default)
        => Task.FromResult<object>(new { name, note = "Tags are stored on artefacts; use updateTags to assign." });

    public Task<byte[]> StudentAttendanceTemplateAsync()
        => Task.FromResult(Encoding.UTF8.GetBytes("StudentId,Name,Course,Section,Date,Status\n"));

    public Task<object> GenerateEtlStubAsync(string kind, CancellationToken ct = default)
        => Task.FromResult<object>(new { kind, status = "generated", note = "ETL payload scaffold — wire worker for execution." });

    public Task<object> RunEtlStubAsync(string? artefactId, CancellationToken ct = default)
        => Task.FromResult<object>(new { artefactId, status = "queued", note = "Run ETL via background worker." });
}
