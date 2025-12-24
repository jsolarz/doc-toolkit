using DocToolkit.Engines;
using DocToolkit.ifx.Events;
using DocToolkit.ifx.Infrastructure;
using DocToolkit.ifx.Interfaces.IAccessors;
using DocToolkit.ifx.Interfaces.IEngines;
using DocToolkit.Managers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DocToolkit.Tests.Managers;

public class SemanticIndexManagerTests
{
    private readonly Mock<IDocumentExtractionEngine> _mockExtractor;
    private readonly Mock<IEmbeddingEngine> _mockEmbedding;
    private readonly Mock<IVectorStorageAccessor> _mockStorage;
    private readonly Mock<ITextChunkingEngine> _mockChunking;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly Mock<ILogger<SemanticIndexManager>> _mockLogger;
    private readonly SemanticIndexManager _manager;

    public SemanticIndexManagerTests()
    {
        _mockExtractor = new Mock<IDocumentExtractionEngine>();
        _mockEmbedding = new Mock<IEmbeddingEngine>();
        _mockStorage = new Mock<IVectorStorageAccessor>();
        _mockChunking = new Mock<ITextChunkingEngine>();
        _mockEventBus = new Mock<IEventBus>();
        _mockLogger = new Mock<ILogger<SemanticIndexManager>>();

        _manager = new SemanticIndexManager(
            _mockExtractor.Object,
            _mockEmbedding.Object,
            _mockStorage.Object,
            _mockChunking.Object,
            _mockEventBus.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void BuildIndex_WithNonExistentSourcePath_ReturnsFalse()
    {
        // Arrange
        var sourcePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var result = _manager.BuildIndex(sourcePath, "./output", 100, 20);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void BuildIndex_WithEmptyDirectory_ReturnsFalse()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            // Act
            var result = _manager.BuildIndex(tempDir, "./output", 100, 20);

            // Assert
            Assert.False(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void BuildIndex_WithValidFiles_BuildsIndex()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(outputDir);
        
        var testFile = Path.Combine(tempDir, "test.txt");
        File.WriteAllText(testFile, "This is test content.");

        _mockExtractor.Setup(x => x.ExtractText(It.IsAny<string>()))
            .Returns("This is test content.");
        _mockChunking.Setup(x => x.ChunkText(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<string> { "This is test content." });
        _mockEmbedding.Setup(x => x.GenerateEmbedding(It.IsAny<string>()))
            .Returns(new float[] { 0.1f, 0.2f, 0.3f });

        try
        {
            // Act
            var result = _manager.BuildIndex(tempDir, outputDir, 100, 20);

            // Assert
            Assert.True(result);
            _mockExtractor.Verify(x => x.ExtractText(It.IsAny<string>()), Times.AtLeastOnce);
            _mockChunking.Verify(x => x.ChunkText(It.IsAny<string>(), 100, 20), Times.AtLeastOnce);
            _mockEmbedding.Verify(x => x.GenerateEmbedding(It.IsAny<string>()), Times.AtLeastOnce);
            _mockStorage.Verify(x => x.SaveVectors(It.IsAny<float[][]>(), outputDir), Times.Once);
            _mockStorage.Verify(x => x.SaveIndex(It.IsAny<List<DocToolkit.ifx.Models.IndexEntry>>(), outputDir), Times.Once);
            _mockEventBus.Verify(x => x.Publish(It.IsAny<IndexBuiltEvent>()), Times.Once);
        }
        finally
        {
            Directory.Delete(tempDir, true);
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);
        }
    }

    [Fact]
    public void BuildIndex_WithEmptyText_SkipsFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(outputDir);
        
        var testFile = Path.Combine(tempDir, "test.txt");
        File.WriteAllText(testFile, "");

        _mockExtractor.Setup(x => x.ExtractText(It.IsAny<string>()))
            .Returns("");

        try
        {
            // Act
            var result = _manager.BuildIndex(tempDir, outputDir, 100, 20);

            // Assert
            Assert.False(result);
            _mockChunking.Verify(x => x.ChunkText(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
        finally
        {
            Directory.Delete(tempDir, true);
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);
        }
    }

    [Fact]
    public void BuildIndex_CallsProgressCallback()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(outputDir);
        
        var testFile = Path.Combine(tempDir, "test.txt");
        File.WriteAllText(testFile, "Test content");

        _mockExtractor.Setup(x => x.ExtractText(It.IsAny<string>()))
            .Returns("Test content");
        _mockChunking.Setup(x => x.ChunkText(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<string> { "Test content" });
        _mockEmbedding.Setup(x => x.GenerateEmbedding(It.IsAny<string>()))
            .Returns(new float[] { 0.1f, 0.2f });

        var progressValues = new List<double>();

        try
        {
            // Act
            var result = _manager.BuildIndex(tempDir, outputDir, 100, 20, progress => progressValues.Add(progress));

            // Assert
            Assert.True(result);
            Assert.NotEmpty(progressValues);
            Assert.Equal(100.0, progressValues.Last());
        }
        finally
        {
            Directory.Delete(tempDir, true);
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);
        }
    }

    [Fact]
    public void BuildIndex_PublishesDocumentProcessedEvent()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        Directory.CreateDirectory(outputDir);
        
        var testFile = Path.Combine(tempDir, "test.txt");
        File.WriteAllText(testFile, "Test content");

        _mockExtractor.Setup(x => x.ExtractText(It.IsAny<string>()))
            .Returns("Test content");
        _mockChunking.Setup(x => x.ChunkText(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new List<string> { "Test content" });
        _mockEmbedding.Setup(x => x.GenerateEmbedding(It.IsAny<string>()))
            .Returns(new float[] { 0.1f, 0.2f });

        try
        {
            // Act
            _manager.BuildIndex(tempDir, outputDir, 100, 20);

            // Assert
            _mockEventBus.Verify(x => x.Publish(It.IsAny<DocumentProcessedEvent>()), Times.AtLeastOnce);
        }
        finally
        {
            Directory.Delete(tempDir, true);
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);
        }
    }
}
