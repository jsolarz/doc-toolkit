using DocToolkit.ifx.Interfaces.IAccessors;
using DocToolkit.ifx.Interfaces.IEngines;
using DocToolkit.ifx.Models;
using DocToolkit.Managers;
using FluentAssertions;
using Moq;
using Xunit;

namespace DocToolkit.Tests.Managers;

public class SemanticSearchManagerTests
{
    private readonly Mock<IEmbeddingEngine> _mockEmbedding;
    private readonly Mock<IVectorStorageAccessor> _mockStorage;
    private readonly Mock<ISimilarityEngine> _mockSimilarity;
    private readonly SemanticSearchManager _manager;

    public SemanticSearchManagerTests()
    {
        _mockEmbedding = new Mock<IEmbeddingEngine>();
        _mockStorage = new Mock<IVectorStorageAccessor>();
        _mockSimilarity = new Mock<ISimilarityEngine>();

        _manager = new SemanticSearchManager(
            _mockEmbedding.Object,
            _mockStorage.Object,
            _mockSimilarity.Object);
    }

    [Fact]
    public void Search_WithNonExistentIndex_ThrowsFileNotFoundException()
    {
        // Arrange
        var indexPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _mockStorage.Setup(x => x.IndexExists(indexPath)).Returns(false);

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => 
            _manager.Search("query", indexPath, 10));
    }

    [Fact]
    public void Search_WithValidIndex_ReturnsResults()
    {
        // Arrange
        var indexPath = "./test-index";
        var query = "test query";
        var queryVector = new float[] { 0.1f, 0.2f, 0.3f };
        var vectors = new[] { new float[] { 0.1f, 0.2f, 0.3f }, new float[] { 0.4f, 0.5f, 0.6f } };
        var entries = new List<IndexEntry>
        {
            new() { File = "file1.txt", Path = "./file1.txt", Chunk = "chunk1", Index = 0 },
            new() { File = "file2.txt", Path = "./file2.txt", Chunk = "chunk2", Index = 1 }
        };

        _mockStorage.Setup(x => x.IndexExists(indexPath)).Returns(true);
        _mockStorage.Setup(x => x.LoadVectors(indexPath)).Returns(vectors);
        _mockStorage.Setup(x => x.LoadIndex(indexPath)).Returns(entries);
        _mockEmbedding.Setup(x => x.GenerateEmbedding(query)).Returns(queryVector);
        _mockSimilarity.Setup(x => x.FindTopSimilar(queryVector, vectors, 10))
            .Returns(new List<(int index, double score)> { (0, 0.95), (1, 0.85) });

        // Act
        var result = _manager.Search(query, indexPath, 10);

        // Assert
        result.Should().HaveCount(2);
        result[0].Score.Should().Be(0.95);
        result[0].File.Should().Be("file1.txt");
        _mockEmbedding.Verify(x => x.GenerateEmbedding(query), Times.Once);
        _mockStorage.Verify(x => x.LoadVectors(indexPath), Times.Once);
        _mockStorage.Verify(x => x.LoadIndex(indexPath), Times.Once);
        _mockSimilarity.Verify(x => x.FindTopSimilar(queryVector, vectors, 10), Times.Once);
    }

    [Fact]
    public void Search_WithTopK_LimitsResults()
    {
        // Arrange
        var indexPath = "./test-index";
        var query = "test";
        var queryVector = new float[] { 0.1f, 0.2f };
        var vectors = new[] { 
            new float[] { 0.1f, 0.2f }, 
            new float[] { 0.3f, 0.4f },
            new float[] { 0.5f, 0.6f }
        };
        var entries = new List<IndexEntry>
        {
            new() { Index = 0, Chunk = "chunk1" },
            new() { Index = 1, Chunk = "chunk2" },
            new() { Index = 2, Chunk = "chunk3" }
        };

        _mockStorage.Setup(x => x.IndexExists(indexPath)).Returns(true);
        _mockStorage.Setup(x => x.LoadVectors(indexPath)).Returns(vectors);
        _mockStorage.Setup(x => x.LoadIndex(indexPath)).Returns(entries);
        _mockEmbedding.Setup(x => x.GenerateEmbedding(query)).Returns(queryVector);
        _mockSimilarity.Setup(x => x.FindTopSimilar(queryVector, vectors, 2))
            .Returns(new List<(int index, double score)> { (0, 0.95), (1, 0.85) });

        // Act
        var result = _manager.Search(query, indexPath, 2);

        // Assert
        result.Should().HaveCount(2);
    }
}
