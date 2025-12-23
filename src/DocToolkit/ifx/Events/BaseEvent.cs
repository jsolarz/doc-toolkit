namespace DocToolkit.ifx.Events;

/// <summary>
/// Base class for all events.
/// </summary>
public abstract class BaseEvent : IEvent
{
    /// <summary>
    /// Event ID (unique identifier).
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>
    /// Event type name.
    /// </summary>
    public string EventType => GetType().Name;

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;
}
