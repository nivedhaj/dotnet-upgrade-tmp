namespace Ct.Library.Postgres.Infrastructure;

/// <summary>
/// Minimal user context abstraction (compatible with original IUserContext).
/// </summary>
public interface IUserContextAccessor
{
    UserContext Current { get; }
}

public sealed record UserContext(
    string UserId,
    string InstId,
    string? SessionId = null);

/// <summary>
/// Simple in-process event bus abstraction. Replace with your existing IEventBus implementation.
/// </summary>
public interface IEventBus
{
    Task PublishAsync(object @event, CancellationToken ct = default);
}

/// <summary>
/// Default in-memory / synchronous bus that can be replaced by RabbitMQ publisher.
/// </summary>
public sealed class InProcessEventBus(IEnumerable<IEventHandler> handlers, ILogger<InProcessEventBus> logger) : IEventBus
{
    public async Task PublishAsync(object @event, CancellationToken ct = default)
    {
        var eventType = @event.GetType();
        foreach (var handler in handlers)
        {
            if (handler.CanHandle(eventType))
            {
                try
                {
                    await handler.HandleAsync(@event, ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling event {EventType}", eventType.Name);
                    throw;
                }
            }
        }
    }
}

public interface IEventHandler
{
    bool CanHandle(Type eventType);
    Task HandleAsync(object @event, CancellationToken ct = default);
}
