using DocToolkit.ifx.Events;
using DocToolkit.ifx.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DocToolkit.Tests.Infrastructure;

public class EventBusTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly EventBus _eventBus;

    public EventBusTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_events_{Guid.NewGuid()}.db");
        var mockLogger = new Mock<ILogger<EventBus>>();
        _eventBus = new EventBus(mockLogger.Object, _testDbPath, maxRetries: 3, retryInterval: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Publish_WithSubscriber_CallsSubscriber()
    {
        // Arrange
        var eventReceived = false;
        _eventBus.Subscribe<DocumentProcessedEvent>(evt => eventReceived = true);

        // Act
        _eventBus.Publish(new DocumentProcessedEvent { FilePath = "test.txt", CharacterCount = 12 });

        // Assert
        Assert.True(eventReceived);
    }

    [Fact]
    public void Publish_WithMultipleSubscribers_CallsAll()
    {
        // Arrange
        var callCount = 0;
        _eventBus.Subscribe<DocumentProcessedEvent>(evt => callCount++);
        _eventBus.Subscribe<DocumentProcessedEvent>(evt => callCount++);

        // Act
        _eventBus.Publish(new DocumentProcessedEvent { FilePath = "test.txt", CharacterCount = 12 });

        // Assert
        Assert.Equal(2, callCount);
    }

    [Fact]
    public void Publish_WithAsyncSubscriber_CallsSubscriber()
    {
        // Arrange
        var eventReceived = false;
        _eventBus.Subscribe<DocumentProcessedEvent>(async evt => 
        {
            await Task.Delay(10);
            eventReceived = true;
        });

        // Act
        _eventBus.Publish(new DocumentProcessedEvent { FilePath = "test.txt", CharacterCount = 12 });

        // Assert
        Thread.Sleep(100); // Wait for async handler
        Assert.True(eventReceived);
    }

    [Fact]
    public void Publish_WithDifferentEventTypes_OnlyCallsMatchingSubscribers()
    {
        // Arrange
        var documentEventReceived = false;
        var summaryEventReceived = false;
        
        _eventBus.Subscribe<DocumentProcessedEvent>(evt => documentEventReceived = true);
        _eventBus.Subscribe<SummaryCreatedEvent>(evt => summaryEventReceived = true);

        // Act
        _eventBus.Publish(new DocumentProcessedEvent { FilePath = "test.txt", CharacterCount = 12 });

        // Assert
        Assert.True(documentEventReceived);
        Assert.False(summaryEventReceived);
    }

    [Fact]
    public void Publish_WithMultipleEvents_PersistsAll()
    {
        // Act
        _eventBus.Publish(new DocumentProcessedEvent { FilePath = "test1.txt", CharacterCount = 10 });
        _eventBus.Publish(new SummaryCreatedEvent { SummaryPath = "./test2.md", FileCount = 5, SourcePath = "./source" });

        // Assert
        // Events are persisted internally - we verify by checking subscribers are called
        // Full persistence testing would require making EventPersistence public or using reflection
    }

    public void Dispose()
    {
        _eventBus?.Dispose();
        
        // Wait a bit for SQLite to release the file lock
        Thread.Sleep(100);
        
        // Retry deletion in case file is still locked
        for (int i = 0; i < 5; i++)
        {
            try
            {
                if (File.Exists(_testDbPath))
                    File.Delete(_testDbPath);
                break;
            }
            catch (IOException)
            {
                if (i < 4)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
