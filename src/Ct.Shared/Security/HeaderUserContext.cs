using Ct.Shared.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Ct.Shared.Security;

public sealed class HeaderUserContext(IHttpContextAccessor accessor) : IUserContext
{
    public Guid OwnerId => ReadGuid("X-Owner-Id") ?? Guid.Empty;
    public Guid? ParentId => ReadGuid("X-Parent-Id");
    public string UserId => ReadString("X-User-Id") ?? "system";
    public string? InstId => ReadString("X-Inst-Id") ?? ReadString("X-Owner-Id");
    public IReadOnlyCollection<string> Permissions =>
        (ReadString("X-Permissions") ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    public bool IsAuthenticated => OwnerId != Guid.Empty;

    private Guid? ReadGuid(string h) => Guid.TryParse(ReadString(h), out var g) ? g : null;
    private string? ReadString(string h) =>
        accessor.HttpContext?.Request.Headers.TryGetValue(h, out var v) == true ? v.ToString() : null;
}
