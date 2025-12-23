using DocToolkit.Accessors;
using DocToolkit.ifx.Models;
using FluentAssertions;
using Xunit;

namespace DocToolkit.Tests.Accessors;

public class VectorStorageAccessorTests
{
    private readonly VectorStorageAccessor _accessor;
    private readonly string _testDir;

    public VectorStorageAccessorTests()
    {
        _accessor = new VectorStorageAccessor();
        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
    }

    [Fact]
    public void SaveVectors_And_LoadVectors_RoundTrip()
    {
        // Arrange
        var vectors = new[]
        {
            new float[] { 0.1f, 0.2f, 0.3f },
            new float[] { 0.4f, 0.5f, 0.6f },
            new float[] { 0.7f, 0.8f, 0.9f }
        };

        // Act
        _accessor.SaveVectors(vectors, _testDir);
        var loaded = _accessor.LoadVectors(_testDir);

        // Assert
        loaded.Should().HaveCount(3);
        loaded[0].Should().BeEquivalentTo(vectors[0]);
        loaded[1].Should().BeEquivalentTo(vectors[1]);
        loaded[2].Should().BeEquivalentTo(vectors[2]);
    }

    [Fact]
    public void SaveIndex_And_LoadIndex_RoundTrip()
    {
        // Arrange
        var entries = new List<IndexEntry>
        {
            new() { File = "file1.txt", Path = "./file1.txt", Chunk = "chunk1", Index = 0 },
            new() { File = "file2.txt", Path = "./file2.txt", Chunk = "chunk2", Index = 1 }
        };

        // Act
        _accessor.SaveIndex(entries, _testDir);
        var loaded = _accessor.LoadIndex(_testDir);

        // Assert
        loaded.Should().HaveCount(2);
        loaded[0].File.Should().Be("file1.txt");
        loaded[1].File.Should().Be("file2.txt");
    }

    [Fact]
    public void IndexExists_WithNoIndex_ReturnsFalse()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var result = _accessor.IndexExists(nonExistentPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IndexExists_WithIndex_ReturnsTrue()
    {
        // Arrange
        var entries = new List<IndexEntry> { new() { File = "test.txt", Index = 0 } };
        _accessor.SaveIndex(entries, _testDir);

        // Act
        var result = _accessor.IndexExists(_testDir);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SaveVectors_WithEmptyArray_HandlesGracefully()
    {
        // Arrange
        var vectors = Array.Empty<float[]>();

        // Act
        _accessor.SaveVectors(vectors, _testDir);
        var loaded = _accessor.LoadVectors(_testDir);

        // Assert
        loaded.Should().BeEmpty();
    }

    [Fact]
    public void SaveIndex_WithEmptyList_HandlesGracefully()
    {
        // Arrange
        var entries = new List<IndexEntry>();

        // Act
        _accessor.SaveIndex(entries, _testDir);
        var loaded = _accessor.LoadIndex(_testDir);

        // Assert
        loaded.Should().BeEmpty();
    }
}
