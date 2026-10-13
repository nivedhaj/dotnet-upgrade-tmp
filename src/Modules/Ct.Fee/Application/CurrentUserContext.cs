namespace Ct.Fee.Application;

public interface ICurrentUserContext
{
    Guid OwnerId { get; }
    Guid? ParentId { get; }
    string UserId { get; }
}

public sealed class HeaderCurrentUserContext(IHttpContextAccessor accessor) : ICurrentUserContext
{
    public Guid OwnerId => ReadGuid("X-Owner-Id") ?? Guid.Empty;
    public Guid? ParentId => ReadGuid("X-Parent-Id");
    public string UserId => ReadString("X-User-Id") ?? "system";

    private Guid? ReadGuid(string header) => Guid.TryParse(ReadString(header), out var parsed) ? parsed : null;
    private string? ReadString(string header) => accessor.HttpContext?.Request.Headers.TryGetValue(header, out var value) == true ? value.ToString() : null;
}

public sealed class FixedCurrentUserContext(Guid ownerId, string userId = "test", Guid? parentId = null) : ICurrentUserContext
{
    public Guid OwnerId { get; } = ownerId;
    public Guid? ParentId { get; } = parentId;
    public string UserId { get; } = userId;
}
