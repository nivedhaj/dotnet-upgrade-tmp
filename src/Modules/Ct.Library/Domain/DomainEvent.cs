namespace Ct.Library.Postgres.Domain;

/// <summary>
/// Marker interface for all domain events.
/// </summary>
public interface IDomainEvent;

/// <summary>
/// Base class for domain events. Keep parameterless constructor for serializers.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <summary>
    /// Optional correlation / causation metadata that can be set by the event store.
    /// </summary>
    public Guid? EventId { get; set; }

    public DateTimeOffset OccurredOn { get; set; } = DateTimeOffset.UtcNow;
}
