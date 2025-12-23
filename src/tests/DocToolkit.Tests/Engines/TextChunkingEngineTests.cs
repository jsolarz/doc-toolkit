using DocToolkit.Engines;
using FluentAssertions;
using Xunit;

namespace DocToolkit.Tests.Engines;

public class TextChunkingEngineTests
{
    private readonly TextChunkingEngine _engine;

    public TextChunkingEngineTests()
    {
        _engine = new TextChunkingEngine();
    }

    [Fact]
    public void ChunkText_WithEmptyString_ReturnsEmptyList()
    {
        // Act
        var result = _engine.ChunkText("", 100, 20);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ChunkText_WithWhitespace_ReturnsEmptyList()
    {
        // Act
        var result = _engine.ChunkText("   \n\t  ", 100, 20);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ChunkText_WithNull_ReturnsEmptyList()
    {
        // Act
        var result = _engine.ChunkText(null!, 100, 20);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ChunkText_WithShortText_ReturnsSingleChunk()
    {
        // Arrange
        var text = "This is a short text with only a few words.";

        // Act
        var result = _engine.ChunkText(text, 100, 20);

        // Assert
        result.Should().HaveCount(1);
        result[0].Should().Be(text);
    }

    [Fact]
    public void ChunkText_WithLongText_CreatesMultipleChunks()
    {
        // Arrange
        var words = Enumerable.Range(1, 200).Select(i => $"word{i}").ToArray();
        var text = string.Join(" ", words);

        // Act
        var result = _engine.ChunkText(text, 50, 10);

        // Assert
        result.Should().HaveCountGreaterThan(1);
        result.Should().AllSatisfy(chunk => 
            chunk.Split(' ').Length.Should().BeLessThanOrEqualTo(50));
    }

    [Fact]
    public void ChunkText_WithOverlap_CreatesOverlappingChunks()
    {
        // Arrange
        var words = Enumerable.Range(1, 100).Select(i => $"word{i}").ToArray();
        var text = string.Join(" ", words);

        // Act
        var result = _engine.ChunkText(text, 20, 10);

        // Assert
        result.Should().HaveCountGreaterThan(1);
        
        // Check that chunks overlap
        for (int i = 0; i < result.Count - 1; i++)
        {
            var currentWords = result[i].Split(' ');
            var nextWords = result[i + 1].Split(' ');
            
            // Should have some overlap
            var overlap = currentWords.Intersect(nextWords).Count();
            overlap.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void ChunkText_WithNewlines_PreservesStructure()
    {
        // Arrange
        var text = "Line 1\nLine 2\nLine 3\nLine 4";

        // Act
        var result = _engine.ChunkText(text, 10, 2);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(chunk => chunk.Should().NotBeNullOrWhiteSpace());
    }

    [Theory]
    [InlineData(10, 5)]
    [InlineData(100, 20)]
    [InlineData(500, 100)]
    [InlineData(1000, 200)]
    public void ChunkText_WithVariousSizes_ProducesValidChunks(int chunkSize, int overlap)
    {
        // Arrange
        var words = Enumerable.Range(1, chunkSize * 3).Select(i => $"word{i}").ToArray();
        var text = string.Join(" ", words);

        // Act
        var result = _engine.ChunkText(text, chunkSize, overlap);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().AllSatisfy(chunk => 
        {
            var wordCount = chunk.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            wordCount.Should().BeLessThanOrEqualTo(chunkSize);
        });
    }

    [Fact]
    public void ChunkText_WithZeroOverlap_CreatesNonOverlappingChunks()
    {
        // Arrange
        var words = Enumerable.Range(1, 100).Select(i => $"word{i}").ToArray();
        var text = string.Join(" ", words);

        // Act
        var result = _engine.ChunkText(text, 20, 0);

        // Assert
        result.Should().HaveCount(5); // 100 words / 20 words per chunk = 5 chunks
    }

    [Fact]
    public void ChunkText_WithOverlapGreaterThanChunkSize_HandlesGracefully()
    {
        // Arrange
        var text = "word1 word2 word3 word4 word5";

        // Act
        var result = _engine.ChunkText(text, 3, 5); // Overlap > chunk size

        // Assert
        result.Should().NotBeEmpty();
        // Should still produce chunks, just with minimal step
    }
}
