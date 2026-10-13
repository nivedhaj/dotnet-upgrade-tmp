using Ct.Arte.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ct.Arte.Infrastructure.Persistence;

/// <summary>
/// Automatically populates audit fields on SaveChanges (replacement for Raven OnBeforeStore).
/// </summary>
public sealed class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    public AuditInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        var now = DateTimeOffset.UtcNow;
        var userId = _currentUser.UserId ?? "system";

        foreach (var entry in context.ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = now;
                entry.Entity.CreatedBy = userId;
                entry.Entity.ModifiedOn = null;
                entry.Entity.ModifiedBy = null;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedOn = now;
                entry.Entity.ModifiedBy = userId;
                // Prevent overwriting Created*
                entry.Property(nameof(IAuditable.CreatedOn)).IsModified = false;
                entry.Property(nameof(IAuditable.CreatedBy)).IsModified = false;
            }
        }
    }
}

/// <summary>
/// Lightweight current-user abstraction (replaces Raven session on UserContext).
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }
    string? OwnerId { get; }
    string? Email { get; }
    bool IsRootAdmin { get; }
}

public sealed class CurrentUser : ICurrentUser
{
    public string? UserId { get; init; }
    public string? OwnerId { get; init; }
    public string? Email { get; init; }
    public bool IsRootAdmin { get; init; }
}
