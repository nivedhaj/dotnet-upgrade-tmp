namespace Ct.Scheduler.Application;

public interface ISchedulerUserContext
{
    string OwnerId { get; }
    string? UserId { get; }
    string? InstId { get; }
}

public sealed class HeaderSchedulerUserContext(IHttpContextAccessor http) : ISchedulerUserContext
{
    public string OwnerId =>
        http.HttpContext?.Request.Headers["X-Owner-Id"].FirstOrDefault()
        ?? http.HttpContext?.Request.Headers["X-Inst-Id"].FirstOrDefault()
        ?? "default";

    public string? UserId =>
        http.HttpContext?.Request.Headers["X-User-Id"].FirstOrDefault()
        ?? http.HttpContext?.User?.Identity?.Name;

    public string? InstId =>
        http.HttpContext?.Request.Headers["X-Inst-Id"].FirstOrDefault();
}
