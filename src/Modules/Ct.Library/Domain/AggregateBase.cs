using System.Collections;

namespace Ct.Library.Postgres.Domain;

public interface IAggregate
{
    string Id { get; }
    long Version { get; }
    void ApplyEvent(object @event);
    ICollection GetUncommittedEvents();
    void ClearUncommittedEvents();
}

/// <summary>
/// Production-ready aggregate base with C# 14 primary constructor style helpers.
/// Version is long to align with Marten stream versions.
/// </summary>
public abstract class AggregateBase : IAggregate, IEquatable<IAggregate>
{
    private readonly List<object> _uncommittedEvents = [];
    private IRouteEvents? _registeredRoutes;

    protected AggregateBase() { }

    protected AggregateBase(IRouteEvents handler)
    {
        RegisteredRoutes = handler ?? throw new ArgumentNullException(nameof(handler));
        RegisteredRoutes.Register(this);
    }

    protected IRouteEvents RegisteredRoutes
    {
        get => _registeredRoutes ??= new ConventionEventRouter(true, this);
        set => _registeredRoutes = value ?? throw new InvalidOperationException("Aggregate must have an event router");
    }

    public string Id { get; protected set; } = string.Empty;

    /// <summary>
    /// Current version of the aggregate (number of applied events).
    /// </summary>
    public long Version { get; protected set; }

    void IAggregate.ApplyEvent(object @event)
    {
        Version++;
        RegisteredRoutes.Dispatch(@event);
    }

    ICollection IAggregate.GetUncommittedEvents() => _uncommittedEvents;

    void IAggregate.ClearUncommittedEvents() => _uncommittedEvents.Clear();

    protected void Apply(object @event)
    {
        // Apply to state first
        ((IAggregate)this).ApplyEvent(@event);
        // Then record as uncommitted
        _uncommittedEvents.Add(@event);
    }

    public bool Equals(IAggregate? other) =>
        other is not null && string.Equals(Id, other.Id, StringComparison.Ordinal);

    public override bool Equals(object? obj) => Equals(obj as IAggregate);

    public override int GetHashCode() => Id.GetHashCode(StringComparison.Ordinal);

    public static bool operator ==(AggregateBase? left, AggregateBase? right) =>
        Equals(left, right);

    public static bool operator !=(AggregateBase? left, AggregateBase? right) =>
        !Equals(left, right);
}

/// <summary>
/// Simple convention-based event router (ported & cleaned).
/// </summary>
public interface IRouteEvents
{
    void Register(object aggregate);
    void Dispatch(object @event);
}

public sealed class ConventionEventRouter(bool throwOnApplyNotFound, object? aggregate) : IRouteEvents
{
    private readonly Dictionary<Type, Action<object>> _handlers = [];
    private object? _aggregate = aggregate;

    public void Register(object aggregate)
    {
        _aggregate = aggregate;
        var methods = aggregate.GetType()
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Where(m => m is { Name: "Apply", GetParameters().Length: 1 });

        foreach (var method in methods)
        {
            var eventType = method.GetParameters()[0].ParameterType;
            _handlers[eventType] = e => method.Invoke(aggregate, [e]);
        }
    }

    public void Dispatch(object @event)
    {
        if (_handlers.TryGetValue(@event.GetType(), out var handler))
        {
            handler(@event);
            return;
        }

        if (throwOnApplyNotFound)
        {
            throw new InvalidOperationException(
                $"No Apply method found for event type {@event.GetType().FullName} on aggregate {_aggregate?.GetType().FullName}");
        }
    }
}
