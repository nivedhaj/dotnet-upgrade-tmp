using Ct.Connect.Domain;
using Ct.Connect.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ct.Connect.Infrastructure.Persistence;

/// <summary>
/// Mirrors the original Raven Store_OnBeforeStore behaviour.
/// </summary>
public sealed class AuditInterceptor(IUserContextAccessor userContextAccessor) : SaveChangesInterceptor
{
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

        var userId = userContextAccessor.Current?.UserId ?? string.Empty;
        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<Entity>())
        {
            if (entry.Entity.Audited) continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.CreatedOn == default || entry.Entity.CreatedOn == DateTime.MinValue)
                        entry.Entity.CreatedOn = now;
                    if (string.IsNullOrEmpty(entry.Entity.CreatedBy))
                        entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedOn = now;
                    entry.Entity.ModifiedBy = userId;
                    // Never overwrite Created*
                    entry.Property(e => e.CreatedOn).IsModified = false;
                    entry.Property(e => e.CreatedBy).IsModified = false;
                    break;
            }
        }
    }
}
