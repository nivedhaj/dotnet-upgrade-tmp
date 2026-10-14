namespace Ct.Library.Application;

public interface ILibraryUserContext
{
    string OwnerId { get; }
    string? UserId { get; }
}

public sealed class HeaderLibraryUserContext(IHttpContextAccessor http) : ILibraryUserContext
{
    public string OwnerId =>
        http.HttpContext?.Request.Headers["X-Owner-Id"].FirstOrDefault()
        ?? http.HttpContext?.Request.Headers["X-Inst-Id"].FirstOrDefault()
        ?? "default";

    public string? UserId =>
        http.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault()
        ?? http.HttpContext?.User?.Identity?.Name;
}
