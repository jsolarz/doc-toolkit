using DocToolkit.Events;

namespace DocToolkit.Infrastructure;

/// <summary>
/// Event bus interface for pub/sub communication.
/// Follows IDesign Method™ message bus pattern for cross-component communication.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="eventData">Event data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task PublishAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class, IEvent;

    /// <summary>
    /// Publishes an event synchronously (for backward compatibility).
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="eventData">Event data</param>
    void Publish<T>(T eventData) where T : class, IEvent;

    /// <summary>
    /// Subscribes to events of a specific type.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="handler">Event handler</param>
    /// <returns>Subscription ID for unsubscribing</returns>
    string Subscribe<T>(Func<T, Task> handler) where T : class, IEvent;

    /// <summary>
    /// Subscribes to events of a specific type (synchronous handler).
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="handler">Event handler</param>
    /// <returns>Subscription ID for unsubscribing</returns>
    string Subscribe<T>(Action<T> handler) where T : class, IEvent;

    /// <summary>
    /// Unsubscribes from events using subscription ID.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID returned from Subscribe</param>
    void Unsubscribe(string subscriptionId);

    /// <summary>
    /// Gets the number of pending events in the queue.
    /// </summary>
    /// <returns>Number of pending events</returns>
    int GetPendingEventCount();

    /// <summary>
    /// Gets the number of failed events that need retry.
    /// </summary>
    /// <returns>Number of failed events</returns>
    int GetFailedEventCount();
}
