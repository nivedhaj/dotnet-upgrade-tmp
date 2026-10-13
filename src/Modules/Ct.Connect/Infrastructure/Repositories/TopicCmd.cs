using Ct.Connect.Application;
using Ct.Connect.Domain;
using Ct.Connect.Infrastructure.Persistence;
using Ct.Connect.Security;
using Microsoft.EntityFrameworkCore;

namespace Ct.Connect.Infrastructure.Repositories;

/// <summary>Postgres port of original TopicCmd + TopicQry.</summary>
public sealed class TopicCmd(ConnectDbContext db, IUserContextAccessor ucAccessor, IConnectEventPublisher events)
{
    private IUserContext Uc => ucAccessor.Current
        ?? throw new InvalidOperationException("User context is not available.");

    private void EnsureScope()
    {
        if (!Uc.BoundByInst)
            throw new UnauthorizedAccessException($"Not in scope for institute {Uc.InstId}");
    }

    public async Task<IReadOnlyList<Topic>> CreateTopicsAsync(IEnumerable<CreateTopicRequest> requests, CancellationToken ct = default)
    {
        EnsureScope();
        var created = new List<Topic>();
        foreach (var req in requests)
        {
            var ownerId = string.IsNullOrWhiteSpace(req.OwnerId) ? Uc.InstId : req.OwnerId!;
            var exists = await db.Topics.AnyAsync(t => t.OwnerId == ownerId && t.Name == req.Name, ct);
            if (exists) continue;

            var topic = new Topic
            {
                Name = req.Name.Trim(),
                FriendlyName = req.FriendlyName ?? req.Name,
                Description = req.Description,
                MainTopicId = req.MainTopicId,
                Role = ParseEnum(req.Role, RoleEnum.Member),
                Category = ParseEnum(req.Category, CategoryEnum.PrivateToInstitute),
                Access = ParseEnum(req.Access, AccessEnum.Open),
                CanUnsubscribe = req.CanUnsubscribe,
                CanPublish = req.CanPublish,
                IsSubscriptionAllowed = req.IsSubscriptionAllowed,
                Tags = req.Tags,
                Status = TopicStatusEnum.Active
            };
            topic.SetId();
            topic.SetOwner(ownerId);
            db.Topics.Add(topic);
            created.Add(topic);
        }
        await db.SaveChangesAsync(ct);
        await NotifyTopicUpdatedAsync(ct);
        return created;
    }

    public async Task<Topic> CreateMainTopicAsync(CreateTopicRequest request, CancellationToken ct = default)
    {
        request.MainTopicId = null;
        var list = await CreateTopicsAsync([request], ct);
        return list.FirstOrDefault()
            ?? throw new InvalidOperationException("Topic already exists or could not be created.");
    }

    public async Task EnableAsync(string id, CancellationToken ct = default)
    {
        EnsureScope();
        var t = await LoadAsync(id, ct);
        t.Status = TopicStatusEnum.Active;
        await db.SaveChangesAsync(ct);
        await NotifyTopicUpdatedAsync(ct);
    }

    public async Task DisableAsync(string id, CancellationToken ct = default)
    {
        EnsureScope();
        var t = await LoadAsync(id, ct);
        t.Status = TopicStatusEnum.Disabled;
        await db.SaveChangesAsync(ct);
        await NotifyTopicUpdatedAsync(ct);
    }

    public async Task UpdateFriendlyNameAsync(string id, string? value, CancellationToken ct = default)
    {
        var t = await LoadAsync(id, ct);
        t.FriendlyName = value;
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateDescriptionAsync(string id, string? value, CancellationToken ct = default)
    {
        var t = await LoadAsync(id, ct);
        t.Description = value;
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateCategoryAsync(string id, string value, CancellationToken ct = default)
    {
        var t = await LoadAsync(id, ct);
        t.Category = ParseEnum(value, t.Category);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateRoleAsync(string id, string value, CancellationToken ct = default)
    {
        var t = await LoadAsync(id, ct);
        t.Role = ParseEnum(value, t.Role);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAccessAsync(string id, string value, CancellationToken ct = default)
    {
        var t = await LoadAsync(id, ct);
        t.Access = ParseEnum(value, t.Access);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateSubscriptionsAsync(string id, Dictionary<string, object>? subscriptions, CancellationToken ct = default)
    {
        var t = await LoadAsync(id, ct);
        t.Subscriptions = subscriptions;
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateMainTopicAsync(string id, string? mainTopicId, CancellationToken ct = default)
    {
        var t = await LoadAsync(id, ct);
        t.MainTopicId = mainTopicId;
        await db.SaveChangesAsync(ct);
    }


    public async Task<Topic> UpdateHandleAsync(string id, string handle, CancellationToken ct = default)
    {
        EnsureScope();
        var t = await LoadAsync(id, ct);
        if (string.IsNullOrWhiteSpace(handle))
            throw new ArgumentException("Handle is required.");
        if (!handle.StartsWith(t.Name, StringComparison.OrdinalIgnoreCase)
            && !handle.StartsWith(t.Name.ToLowerInvariant(), StringComparison.Ordinal))
            throw new ArgumentException("Incorrect handle name — must start with topic name.");
        var exists = await db.Topics.AnyAsync(x => x.OwnerId == Uc.InstId && x.Handle == handle && x.Id != id, ct);
        if (exists) throw new InvalidOperationException("Handle already exists");
        t.Handle = handle;
        await db.SaveChangesAsync(ct);
        await NotifyTopicUpdatedAsync(ct);
        return t;
    }

    public async Task SetSubscriptionAllowedAsync(string id, bool allowed, CancellationToken ct = default)
    {
        var t = await LoadAsync(id, ct);
        t.IsSubscriptionAllowed = allowed;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<TopicDto>> GetTopicsAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        var q = db.Topics.AsNoTracking().Where(t => t.OwnerId == Uc.InstId);
        if (activeOnly) q = q.Where(t => t.Status == TopicStatusEnum.Active);
        return await q.OrderBy(t => t.Name).Select(Map).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TopicDto>> GetMainTopicsAsync(CancellationToken ct = default)
    {
        return await db.Topics.AsNoTracking()
            .Where(t => t.OwnerId == Uc.InstId && t.MainTopicId == null)
            .OrderBy(t => t.Name)
            .Select(Map)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetHandlesAsync(CancellationToken ct = default)
    {
        return await db.Topics.AsNoTracking()
            .Where(t => t.OwnerId == Uc.InstId && t.Status == TopicStatusEnum.Active)
            .Select(t => t.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync(ct);
    }

    private async Task NotifyTopicUpdatedAsync(CancellationToken ct)
    {
        try { await events.PublishTopicUpdatedAsync(Uc.InstId, ct); }
        catch { /* non-fatal */ }
    }

    private async Task<Topic> LoadAsync(string id, CancellationToken ct) =>
        await db.Topics.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == Uc.InstId, ct)
        ?? throw new KeyNotFoundException($"Topic '{id}' not found.");

    private static TEnum ParseEnum<TEnum>(string? value, TEnum fallback) where TEnum : struct =>
        Enum.TryParse<TEnum>(value, true, out var v) ? v : fallback;

    private static readonly System.Linq.Expressions.Expression<Func<Topic, TopicDto>> Map = t => new TopicDto
    {
        Id = t.Id,
        OwnerId = t.OwnerId,
        Name = t.Name,
        Handle = t.Handle,
        FriendlyName = t.FriendlyName,
        Description = t.Description,
        MainTopicId = t.MainTopicId,
        Role = t.Role.ToString(),
        Category = t.Category.ToString(),
        Access = t.Access.ToString(),
        Status = t.Status.ToString(),
        CanUnsubscribe = t.CanUnsubscribe,
        CanPublish = t.CanPublish,
        IsSubscriptionAllowed = t.IsSubscriptionAllowed,
        Tags = t.Tags
    };
}
