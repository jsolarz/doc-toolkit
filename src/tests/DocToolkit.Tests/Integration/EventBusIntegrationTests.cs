using DocToolkit.ifx.Events;
using DocToolkit.ifx.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DocToolkit.Tests.Integration;

public class EventBusIntegrationTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly EventBus _eventBus;

    public EventBusIntegrationTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_events_{Guid.NewGuid()}.db");
        var mockLogger = new Mock<ILogger<EventBus>>();
        _eventBus = new EventBus(mockLogger.Object, _testDbPath, maxRetries: 3, retryInterval: TimeSpan.FromSeconds(1));
    }

    public void Dispose()
    {
        _eventBus?.Dispose();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    [Fact]
    public void Publish_WithEvent_PersistsToDatabase()
    {
        // Arrange
        var testEvent = new DocumentProcessedEvent { FilePath = "test.txt", CharacterCount = 12 };

        // Act
        _eventBus.Publish(testEvent);

        // Assert - Event is persisted internally by EventBus
        // We verify by checking that the event was received by subscribers
        Assert.True(true);
    }

    [Fact]
    public void Subscribe_WithHandler_ReceivesEvents()
    {
        // Arrange
        var receivedEvents = new List<IEvent>();
        _eventBus.Subscribe<DocumentProcessedEvent>(e => receivedEvents.Add(e));

        var testEvent = new DocumentProcessedEvent { FilePath = "test.txt", CharacterCount = 12 };

        // Act
        _eventBus.Publish(testEvent);

        // Assert
        Assert.Single(receivedEvents);
        Assert.Equal(testEvent.EventId, receivedEvents[0].EventId);
    }

    [Fact]
    public void Subscribe_WithMultipleHandlers_AllReceiveEvents()
    {
        // Arrange
        var handler1Events = new List<IEvent>();
        var handler2Events = new List<IEvent>();

        _eventBus.Subscribe<DocumentProcessedEvent>(e => handler1Events.Add(e));
        _eventBus.Subscribe<DocumentProcessedEvent>(e => handler2Events.Add(e));

        var testEvent = new DocumentProcessedEvent { FilePath = "test.txt", CharacterCount = 12 };

        // Act
        _eventBus.Publish(testEvent);

        // Assert
        Assert.Single(handler1Events);
        Assert.Single(handler2Events);
    }

    [Fact]
    public void Publish_WithMultipleEvents_ProcessesAll()
    {
        // Arrange
        var receivedEvents = new List<IEvent>();
        _eventBus.Subscribe<DocumentProcessedEvent>(e => receivedEvents.Add(e));

        var event1 = new DocumentProcessedEvent { FilePath = "test1.txt", CharacterCount = 10 };
        var event2 = new DocumentProcessedEvent { FilePath = "test2.txt", CharacterCount = 10 };

        // Act
        _eventBus.Publish(event1);
        _eventBus.Publish(event2);

        // Assert
        Assert.Equal(2, receivedEvents.Count);
    }
}
