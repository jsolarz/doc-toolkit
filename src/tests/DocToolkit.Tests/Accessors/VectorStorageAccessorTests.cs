using DocToolkit.Accessors;
using DocToolkit.ifx.Models;
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
        Assert.Equal(3, loaded.Length);
        Assert.Equal(vectors[0], loaded[0], new FloatArrayComparer());
        Assert.Equal(vectors[1], loaded[1], new FloatArrayComparer());
        Assert.Equal(vectors[2], loaded[2], new FloatArrayComparer());
    }

    private class FloatArrayComparer : IEqualityComparer<float[]>
    {
        public bool Equals(float[]? x, float[]? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;
            for (int i = 0; i < x.Length; i++)
            {
                if (Math.Abs(x[i] - y[i]) > 0.0001f) return false;
            }
            return true;
        }

        public int GetHashCode(float[] obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
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
        Assert.Equal(2, loaded.Count);
        Assert.Equal("file1.txt", loaded[0].File);
        Assert.Equal("file2.txt", loaded[1].File);
    }

    [Fact]
    public void IndexExists_WithNoIndex_ReturnsFalse()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var result = _accessor.IndexExists(nonExistentPath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IndexExists_WithIndex_ReturnsTrue()
    {
        // Arrange
        var entries = new List<IndexEntry> { new() { File = "test.txt", Index = 0 } };
        var vectors = new[] { new float[] { 0.1f, 0.2f, 0.3f } };
        _accessor.SaveVectors(vectors, _testDir);
        _accessor.SaveIndex(entries, _testDir);

        // Act
        var result = _accessor.IndexExists(_testDir);

        // Assert
        Assert.True(result);
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
        Assert.Empty(loaded);
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
        Assert.Empty(loaded);
    }
}
