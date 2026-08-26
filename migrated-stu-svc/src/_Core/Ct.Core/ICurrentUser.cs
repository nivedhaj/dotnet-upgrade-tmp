namespace Ct.Core;
public interface ICurrentUser { string? UserId { get; } string? UserName { get; } }
public sealed class CurrentUser(IHttpContextAccessorAccessor accessor) : ICurrentUser
{
    public string? UserId => accessor.UserId;
    public string? UserName => accessor.UserName;
}
public interface IHttpContextAccessorAccessor { string? UserId { get; } string? UserName { get; } }
