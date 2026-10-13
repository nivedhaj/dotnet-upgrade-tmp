namespace Ct.Connect.Security;

public interface IUserContext
{
    string SessionId { get; }
    string UserId { get; }
    string InstId { get; }
    string OrgId { get; }
    bool BoundByInst { get; }
    bool BoundByOrg { get; }
    bool BoundByUser { get; }
    bool IsRootAdmin { get; }
    string? Token { get; }
    bool Persist { get; set; }
}

public interface IUserContextAccessor
{
    IUserContext? Current { get; }
}
