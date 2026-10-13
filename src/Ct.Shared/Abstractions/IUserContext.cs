namespace Ct.Shared.Abstractions;

/// <summary>Unified tenant/user context for all modules.</summary>
public interface IUserContext
{
    Guid OwnerId { get; }
    Guid? ParentId { get; }
    string UserId { get; }
    string? InstId { get; }
    IReadOnlyCollection<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
