using DocToolkit.Engines;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DocToolkit.Tests.Engines;

public class DocumentExtractionEngineTests : IDisposable
{
    private readonly DocumentExtractionEngine _engine;
    private readonly string _testDir;

    public DocumentExtractionEngineTests()
    {
        _engine = new DocumentExtractionEngine();
        _testDir = Path.Combine(Path.GetTempPath(), $"test_extract_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void ExtractText_WithNonExistentFile_ReturnsEmptyString()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "nonexistent.txt");

        // Act
        var result = _engine.ExtractText(filePath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractText_WithNullPath_ReturnsEmptyString()
    {
        // Act
        var result = _engine.ExtractText(null!);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractText_WithTextFile_ReturnsContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "test.txt");
        var content = "This is test content.";
        File.WriteAllText(filePath, content);

        // Act
        var result = _engine.ExtractText(filePath);

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void ExtractText_WithMarkdownFile_ReturnsContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "test.md");
        var content = "# Test\n\nThis is markdown content.";
        File.WriteAllText(filePath, content);

        // Act
        var result = _engine.ExtractText(filePath);

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void ExtractText_WithCsvFile_ReturnsContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "test.csv");
        var content = "Name,Age\nJohn,30\nJane,25";
        File.WriteAllText(filePath, content);

        // Act
        var result = _engine.ExtractText(filePath);

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void ExtractText_WithJsonFile_ReturnsContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "test.json");
        var content = "{\"name\": \"test\", \"value\": 123}";
        File.WriteAllText(filePath, content);

        // Act
        var result = _engine.ExtractText(filePath);

        // Assert
        Assert.Equal(content, result);
    }

    [Fact]
    public void ExtractText_WithImageFile_ReturnsEmptyString()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "test.png");
        File.Create(filePath).Dispose();

        // Act
        var result = _engine.ExtractText(filePath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractText_WithUnsupportedExtension_ReturnsEmptyString()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "test.xyz");
        File.WriteAllText(filePath, "content");

        // Act
        var result = _engine.ExtractText(filePath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractText_WithLogger_LogsWarnings()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DocumentExtractionEngine>>();
        var engineWithLogger = new DocumentExtractionEngine(mockLogger.Object);
        var filePath = Path.Combine(_testDir, "nonexistent.txt");

        // Act
        var result = engineWithLogger.ExtractText(filePath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractText_WithEmptyTextFile_ReturnsEmptyString()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "empty.txt");
        File.WriteAllText(filePath, string.Empty);

        // Act
        var result = _engine.ExtractText(filePath);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractText_WithLargeTextFile_ReturnsContent()
    {
        // Arrange
        var filePath = Path.Combine(_testDir, "large.txt");
        var content = string.Join("\n", Enumerable.Range(1, 1000).Select(i => $"Line {i}"));
        File.WriteAllText(filePath, content);

        // Act
        var result = _engine.ExtractText(filePath);

        // Assert
        Assert.Equal(content, result);
    }
}
