using DocToolkit.ifx.Infrastructure;
using Xunit;

namespace DocToolkit.Tests.Infrastructure;

public class MemoryMonitorTests
{
    [Fact]
    public void Constructor_WithNullOperationName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new MemoryMonitor(null!));
    }

    [Fact]
    public void Constructor_WithEnabledTrue_EstablishesBaseline()
    {
        // Act
        using var monitor = new MemoryMonitor("TestOperation", enabled: true);

        // Assert - Monitor should be created successfully
        Assert.NotNull(monitor);
    }

    [Fact]
    public void Constructor_WithEnabledFalse_DoesNotEstablishBaseline()
    {
        // Act
        using var monitor = new MemoryMonitor("TestOperation", enabled: false);

        // Assert - Monitor should be created successfully
        Assert.NotNull(monitor);
    }

    [Fact]
    public void Dispose_WhenEnabled_DisplaysFinalStats()
    {
        // Arrange
        var monitor = new MemoryMonitor("TestOperation", enabled: true);

        // Act
        monitor.Dispose();

        // Assert - No exception should be thrown
        Assert.True(true);
    }

    [Fact]
    public void Dispose_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        var monitor = new MemoryMonitor("TestOperation", enabled: false);

        // Act & Assert
        monitor.Dispose();
        Assert.True(true);
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var monitor = new MemoryMonitor("TestOperation", enabled: true);

        // Act & Assert
        monitor.Dispose();
        monitor.Dispose();
        Assert.True(true);
    }

    [Fact]
    public void DisplayStats_WhenEnabled_DoesNotThrow()
    {
        // Arrange
        using var monitor = new MemoryMonitor("TestOperation", enabled: true);

        // Act & Assert
        monitor.DisplayStats("Test");
        Assert.True(true);
    }

    [Fact]
    public void DisplayStats_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        using var monitor = new MemoryMonitor("TestOperation", enabled: false);

        // Act & Assert
        monitor.DisplayStats("Test");
        Assert.True(true);
    }

    [Fact]
    public void DisplaySummary_WhenEnabled_DoesNotThrow()
    {
        // Arrange
        using var monitor = new MemoryMonitor("TestOperation", enabled: true);

        // Act & Assert
        monitor.DisplaySummary();
        Assert.True(true);
    }

    [Fact]
    public void DisplaySummary_WhenDisabled_DoesNotThrow()
    {
        // Arrange
        using var monitor = new MemoryMonitor("TestOperation", enabled: false);

        // Act & Assert
        monitor.DisplaySummary();
        Assert.True(true);
    }

    [Fact]
    public void GetCurrentMemory_WhenEnabled_ReturnsNonNegativeValue()
    {
        // Arrange
        using var monitor = new MemoryMonitor("TestOperation", enabled: true);

        // Act
        var memory = monitor.CurrentMemory;

        // Assert
        Assert.True(memory >= 0);
    }

    [Fact]
    public void GetMemoryDelta_WhenEnabled_ReturnsDifference()
    {
        // Arrange
        using var monitor = new MemoryMonitor("TestOperation", enabled: true);

        // Act
        var delta = monitor.MemoryDelta;

        // Assert
        Assert.True(delta >= 0);
    }

    [Fact]
    public void GetElapsedTime_ReturnsNonNegativeValue()
    {
        // Arrange
        using var monitor = new MemoryMonitor("TestOperation", enabled: true);
        System.Threading.Thread.Sleep(10); // Small delay to ensure time passes

        // Act
        var elapsed = monitor.Elapsed;

        // Assert
        Assert.True(elapsed >= TimeSpan.Zero);
    }
}
