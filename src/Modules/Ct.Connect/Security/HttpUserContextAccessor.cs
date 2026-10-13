using Microsoft.AspNetCore.Http;

namespace Ct.Connect.Security;

/// <summary>
/// Minimal implementation. Replace with your full JWT / header parsing logic
/// from the original UserContextProvider.
/// </summary>
public sealed class HttpUserContextAccessor(IHttpContextAccessor http) : IUserContextAccessor
{
    public IUserContext? Current
    {
        get
        {
            var ctx = http.HttpContext;
            if (ctx is null) return null;

            // Placeholder – wire real claims / headers here
            return new SimpleUserContext
            {
                SessionId = ctx.TraceIdentifier,
                UserId = ctx.User?.FindFirst("sub")?.Value ?? "system",
                InstId = ctx.Request.Headers["X-Inst-Id"].FirstOrDefault() ?? string.Empty,
                OrgId = ctx.Request.Headers["X-Org-Id"].FirstOrDefault() ?? string.Empty,
                BoundByInst = true,
                BoundByOrg = false,
                BoundByUser = false,
                IsRootAdmin = false
            };
        }
    }
}

file sealed class SimpleUserContext : IUserContext
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string InstId { get; set; } = string.Empty;
    public string OrgId { get; set; } = string.Empty;
    public bool BoundByInst { get; set; }
    public bool BoundByOrg { get; set; }
    public bool BoundByUser { get; set; }
    public bool IsRootAdmin { get; set; }
    public string? Token { get; set; }
    public bool Persist { get; set; } = true;
}
