using Ct.Library.Postgres.Domain;

namespace Ct.Library.Postgres.Infrastructure;

/// <summary>
/// Event store contract – kept compatible with the original Raven-based interface.
/// </summary>
public interface IEventStore
{
    Task SaveAsync(IAggregate aggregate, CancellationToken ct = default);

    Task<T> GetByIdAsync<T>(string aggregateId, CancellationToken ct = default)
        where T : AggregateBase, new();

    Task<IReadOnlyList<object>> GetDomainEventsAsync(string aggregateId, CancellationToken ct = default);

    Task<IReadOnlyList<DomainEvent>> GetDomainEventsSinceAsync(DateTimeOffset? since = null, CancellationToken ct = default);

    Task ReplayAsync(DateTimeOffset? since = null, CancellationToken ct = default);
}
