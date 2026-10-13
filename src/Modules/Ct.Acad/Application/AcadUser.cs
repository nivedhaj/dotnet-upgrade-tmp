namespace Ct.Acad.Application;

public interface IAcadUserContext
{
    string OwnerId { get; }
    string UserId { get; }
}

public sealed class HeaderAcadUserContext(IHttpContextAccessor http) : IAcadUserContext
{
    public string OwnerId =>
        http.HttpContext?.Request.Headers["X-Owner-Id"].FirstOrDefault()
        ?? http.HttpContext?.Request.Headers["X-Inst-Id"].FirstOrDefault()
        ?? "default";
    public string UserId =>
        http.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault()
        ?? http.HttpContext?.User?.Identity?.Name
        ?? "system";
}
