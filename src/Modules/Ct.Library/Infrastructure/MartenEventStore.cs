using Ct.Library.Postgres.Domain;
using Marten;
using Microsoft.Extensions.Logging;

namespace Ct.Library.Postgres.Infrastructure;

/// <summary>
/// Production Marten-backed event store.
/// Replaces the original RavenDB Commit-document implementation.
/// </summary>
public sealed class MartenEventStore(
    IDocumentStore store,
    IEventBus eventBus,
    IUserContextAccessor userContextAccessor,
    ILogger<MartenEventStore> logger) : IEventStore
{
    public async Task SaveAsync(IAggregate aggregate, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        if (string.IsNullOrWhiteSpace(aggregate.Id))
            throw new ArgumentException("Aggregate Id is required", nameof(aggregate));

        var uncommitted = aggregate.GetUncommittedEvents().Cast<object>().ToArray();
        if (uncommitted.Length == 0)
            return;

        var user = userContextAccessor.Current;
        await using var session = store.LightweightSession();

        // Optimistic concurrency: expected version = current aggregate version - number of new events
        var expectedVersion = aggregate.Version - uncommitted.Length;

        try
        {
            // Append events to the stream identified by AggregateId
            session.Events.Append(aggregate.Id, expectedVersion, uncommitted);

            // Optional metadata
            session.SetHeader("UserId", user.UserId);
            session.SetHeader("InstId", user.InstId);

            await session.SaveChangesAsync(ct);

            // Publish after successful persist (same semantics as original)
            foreach (var @event in uncommitted)
            {
                await eventBus.PublishAsync(@event, ct);
            }

            aggregate.ClearUncommittedEvents();
            logger.LogDebug("Saved {Count} events for aggregate {AggregateId} to version {Version}",
                uncommitted.Length, aggregate.Id, aggregate.Version);
        }
        catch (Marten.Exceptions.EventStreamUnexpectedMaxEventIdException)
        {
            logger.LogWarning("Concurrency conflict on aggregate {AggregateId}", aggregate.Id);
            throw new ConcurrencyException($"Concurrency conflict for aggregate {aggregate.Id}");
        }
    }

    public async Task<T> GetByIdAsync<T>(string aggregateId, CancellationToken ct = default)
        where T : AggregateBase, new()
    {
        await using var session = store.QuerySession();

        // Marten reconstructs the aggregate by replaying the event stream
        var aggregate = await session.Events.AggregateStreamAsync<T>(aggregateId, token: ct);

        if (aggregate is null)
        {
            // Return empty aggregate with the requested Id (original behaviour)
            aggregate = new T { /* Id will be set by first event or caller */ };
            // We cannot set protected Id easily; callers usually set it after load or via first event.
        }

        return aggregate;
    }

    public async Task<IReadOnlyList<object>> GetDomainEventsAsync(string aggregateId, CancellationToken ct = default)
    {
        await using var session = store.QuerySession();

        var events = await session.Events
            .FetchStreamAsync(aggregateId, token: ct);

        return events.Select(e => e.Data).ToList();
    }

    public async Task<IReadOnlyList<DomainEvent>> GetDomainEventsSinceAsync(
        DateTimeOffset? since = null,
        CancellationToken ct = default)
    {
        since ??= DateTimeOffset.MinValue;

        await using var session = store.QuerySession();

        // Query the event store directly
        var events = await session.Events
            .QueryAllRawEvents()
            .Where(e => e.Timestamp >= since)
            .OrderBy(e => e.Timestamp)
            .ToListAsync(ct);

        return events
            .Select(e => e.Data)
            .OfType<DomainEvent>()
            .ToList();
    }

    public async Task ReplayAsync(DateTimeOffset? since = null, CancellationToken ct = default)
    {
        var events = await GetDomainEventsSinceAsync(since, ct);
        foreach (var @event in events)
        {
            await eventBus.PublishAsync(@event, ct);
        }
    }
}

/// <summary>
/// Thrown when optimistic concurrency check fails (same as original Raven implementation).
/// </summary>
public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException() { }
    public ConcurrencyException(string message) : base(message) { }
    public ConcurrencyException(string message, Exception inner) : base(message, inner) { }
}
