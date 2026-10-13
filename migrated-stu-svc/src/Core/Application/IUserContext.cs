namespace Ct.Core.Application;

/// <summary>
/// Replacement for the legacy Raven-aware `UserContext` (spec section 55,
/// 79). Concrete implementation reads the authenticated principal; do not
/// bypass authentication just because persistence changed.
/// </summary>
public interface IUserContext
{
    string? UserId { get; }
    string? UserEmail { get; }
    string? InstId { get; }
    bool IsAuthenticated { get; }
}

public class HttpUserContext(IHttpContextAccessor accessor) : IUserContext
{
    private System.Security.Claims.ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public string? UserId => Principal?.FindFirst("sub")?.Value;
    public string? UserEmail => Principal?.FindFirst("email")?.Value;
    public string? InstId => Principal?.FindFirst("inst_id")?.Value;
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
}
