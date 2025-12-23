namespace DocToolkit.Events;

/// <summary>
/// Base interface for all events in the system.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Event ID (unique identifier).
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Event type name.
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    DateTime Timestamp { get; }
}
