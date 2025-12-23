using System.Collections.Concurrent;
using System.Text.Json;
using DocToolkit.Events;

namespace DocToolkit.Infrastructure;

/// <summary>
/// Simple in-memory event bus with persistence and retry policies.
/// For production, consider using a message queue (RabbitMQ, Azure Service Bus, etc.).
/// </summary>
public class EventBus : IEventBus, IDisposable
{
    private readonly ConcurrentDictionary<Type, List<Subscription>> _subscribers = new();
    private readonly EventPersistence _persistence;
    private readonly Timer _retryTimer;
    private readonly int _maxRetries;
    private readonly TimeSpan _retryInterval;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the EventBus.
    /// </summary>
    /// <param name="dbPath">Path to SQLite database file (optional)</param>
    /// <param name="maxRetries">Maximum number of retries for failed events (default: 3)</param>
    /// <param name="retryInterval">Interval between retry attempts (default: 5 minutes)</param>
    public EventBus(string? dbPath = null, int maxRetries = 3, TimeSpan? retryInterval = null)
    {
        _persistence = new EventPersistence(dbPath);
        _maxRetries = maxRetries;
        _retryInterval = retryInterval ?? TimeSpan.FromMinutes(5);

        // Start background retry timer
        _retryTimer = new Timer(ProcessRetries, null, _retryInterval, _retryInterval);
    }

    /// <summary>
    /// Publishes an event asynchronously to all subscribers.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="eventData">Event data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task PublishAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class, IEvent
    {
        if (eventData == null)
        {
            return;
        }

        // Persist event
        _persistence.SaveEvent(eventData);

        // Publish to subscribers
        await PublishToSubscribersAsync(eventData, cancellationToken);
    }

    /// <summary>
    /// Publishes an event synchronously (for backward compatibility).
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="eventData">Event data</param>
    public void Publish<T>(T eventData) where T : class, IEvent
    {
        if (eventData == null)
        {
            return;
        }

        // Persist event
        _persistence.SaveEvent(eventData);

        // Publish to subscribers synchronously
        PublishToSubscribers(eventData);
    }

    /// <summary>
    /// Subscribes to events of a specific type.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="handler">Event handler</param>
    /// <returns>Subscription ID for unsubscribing</returns>
    public string Subscribe<T>(Func<T, Task> handler) where T : class, IEvent
    {
        var subscriptionId = Guid.NewGuid().ToString();
        var subscription = new Subscription
        {
            Id = subscriptionId,
            AsyncHandler = async (obj) => await handler((T)obj),
            EventType = typeof(T)
        };

        _subscribers.AddOrUpdate(
            typeof(T),
            new List<Subscription> { subscription },
            (key, existing) =>
            {
                existing.Add(subscription);
                return existing;
            });

        return subscriptionId;
    }

    /// <summary>
    /// Subscribes to events of a specific type (synchronous handler).
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="handler">Event handler</param>
    /// <returns>Subscription ID for unsubscribing</returns>
    public string Subscribe<T>(Action<T> handler) where T : class, IEvent
    {
        var subscriptionId = Guid.NewGuid().ToString();
        var subscription = new Subscription
        {
            Id = subscriptionId,
            SyncHandler = (obj) => handler((T)obj),
            EventType = typeof(T)
        };

        _subscribers.AddOrUpdate(
            typeof(T),
            new List<Subscription> { subscription },
            (key, existing) =>
            {
                existing.Add(subscription);
                return existing;
            });

        return subscriptionId;
    }

    /// <summary>
    /// Unsubscribes from events using subscription ID.
    /// </summary>
    /// <param name="subscriptionId">Subscription ID returned from Subscribe</param>
    public void Unsubscribe(string subscriptionId)
    {
        foreach (var (eventType, subscriptions) in _subscribers)
        {
            subscriptions.RemoveAll(s => s.Id == subscriptionId);
            if (subscriptions.Count == 0)
            {
                _subscribers.TryRemove(eventType, out _);
            }
        }
    }

    /// <summary>
    /// Gets the number of pending events in the queue.
    /// </summary>
    /// <returns>Number of pending events</returns>
    public int GetPendingEventCount()
    {
        return _persistence.GetPendingEventCount();
    }

    /// <summary>
    /// Gets the number of failed events that need retry.
    /// </summary>
    /// <returns>Number of failed events</returns>
    public int GetFailedEventCount()
    {
        return _persistence.GetFailedEventCount();
    }

    private async Task PublishToSubscribersAsync<T>(T eventData, CancellationToken cancellationToken) where T : class, IEvent
    {
        if (!_subscribers.TryGetValue(typeof(T), out var handlers))
        {
            return;
        }

        var tasks = new List<Task>();

        foreach (var subscription in handlers)
        {
            if (subscription.AsyncHandler != null)
            {
                tasks.Add(InvokeAsyncHandler(eventData, subscription, cancellationToken));
            }
            else if (subscription.SyncHandler != null)
            {
                tasks.Add(Task.Run(() => InvokeSyncHandler(eventData, subscription), cancellationToken));
            }
        }

        await Task.WhenAll(tasks);
    }

    private void PublishToSubscribers<T>(T eventData) where T : class, IEvent
    {
        if (!_subscribers.TryGetValue(typeof(T), out var handlers))
        {
            return;
        }

        foreach (var subscription in handlers)
        {
            try
            {
                if (subscription.AsyncHandler != null)
                {
                    // Run async handler synchronously
                    Task.Run(async () => await InvokeAsyncHandler(eventData, subscription, CancellationToken.None)).Wait();
                }
                else if (subscription.SyncHandler != null)
                {
                    InvokeSyncHandler(eventData, subscription);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't stop other handlers
                // In production, use proper logging
                Console.WriteLine($"Error invoking handler for event {eventData.EventType}: {ex.Message}");
            }
        }
    }

    private async Task InvokeAsyncHandler<T>(T eventData, Subscription subscription, CancellationToken cancellationToken) where T : class, IEvent
    {
        try
        {
            if (subscription.AsyncHandler != null)
            {
                await subscription.AsyncHandler(eventData);
                _persistence.MarkEventProcessed(eventData.EventId);
            }
        }
        catch (Exception ex)
        {
            var retryCount = _persistence.MarkEventFailed(eventData.EventId, ex.Message);
            if (retryCount >= _maxRetries)
            {
                Console.WriteLine($"Event {eventData.EventId} failed after {retryCount} retries: {ex.Message}");
            }
        }
    }

    private void InvokeSyncHandler<T>(T eventData, Subscription subscription) where T : class, IEvent
    {
        try
        {
            if (subscription.SyncHandler != null)
            {
                subscription.SyncHandler(eventData);
                _persistence.MarkEventProcessed(eventData.EventId);
            }
        }
        catch (Exception ex)
        {
            var retryCount = _persistence.MarkEventFailed(eventData.EventId, ex.Message);
            if (retryCount >= _maxRetries)
            {
                Console.WriteLine($"Event {eventData.EventId} failed after {retryCount} retries: {ex.Message}");
            }
        }
    }

    private async void ProcessRetries(object? state)
    {
        if (!await _processingSemaphore.WaitAsync(0))
        {
            return; // Already processing
        }

        try
        {
            var failedEvents = _persistence.GetFailedEventsForRetry(_maxRetries);

            foreach (var persistedEvent in failedEvents)
            {
                try
                {
                    // Deserialize event
                    var eventType = Type.GetType($"DocToolkit.Events.{persistedEvent.EventType}");
                    if (eventType == null)
                    {
                        continue;
                    }

                    var eventData = JsonSerializer.Deserialize(persistedEvent.EventData, eventType);
                    if (eventData is IEvent evt)
                    {
                        // Reset to pending and republish
                        _persistence.ResetEventForRetry(evt.EventId);
                        await PublishAsync(evt);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing retry for event {persistedEvent.EventId}: {ex.Message}");
                }
            }
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _retryTimer?.Dispose();
        _persistence?.Dispose();
        _processingSemaphore?.Dispose();
        _disposed = true;
    }

    private class Subscription
    {
        public string Id { get; set; } = string.Empty;
        public Type EventType { get; set; } = null!;
        public Func<object, Task>? AsyncHandler { get; set; }
        public Action<object>? SyncHandler { get; set; }
    }
}
