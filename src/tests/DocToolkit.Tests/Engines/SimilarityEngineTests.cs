using DocToolkit.Engines;
using Xunit;

namespace DocToolkit.Tests.Engines;

public class SimilarityEngineTests
{
    private readonly SimilarityEngine _engine;

    public SimilarityEngineTests()
    {
        _engine = new SimilarityEngine();
    }

    [Fact]
    public void CosineSimilarity_WithIdenticalVectors_ReturnsOne()
    {
        // Arrange
        var vector = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };

        // Act
        var result = _engine.CosineSimilarity(vector, vector);

        // Assert
        Assert.True(Math.Abs(1.0 - result) < 0.0001);
    }

    [Fact]
    public void CosineSimilarity_WithOrthogonalVectors_ReturnsZero()
    {
        // Arrange
        var vectorA = new float[] { 1.0f, 0.0f, 0.0f };
        var vectorB = new float[] { 0.0f, 1.0f, 0.0f };

        // Act
        var result = _engine.CosineSimilarity(vectorA, vectorB);

        // Assert
        Assert.True(Math.Abs(0.0 - result) < 0.0001);
    }

    [Fact]
    public void CosineSimilarity_WithOppositeVectors_ReturnsNegativeOne()
    {
        // Arrange
        var vectorA = new float[] { 1.0f, 0.0f };
        var vectorB = new float[] { -1.0f, 0.0f };

        // Act
        var result = _engine.CosineSimilarity(vectorA, vectorB);

        // Assert
        Assert.True(Math.Abs(-1.0 - result) < 0.0001);
    }

    [Fact]
    public void CosineSimilarity_WithNullVectorA_ThrowsArgumentNullException()
    {
        // Arrange
        var vectorB = new float[] { 1.0f, 2.0f };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _engine.CosineSimilarity(null!, vectorB));
    }

    [Fact]
    public void CosineSimilarity_WithNullVectorB_ThrowsArgumentNullException()
    {
        // Arrange
        var vectorA = new float[] { 1.0f, 2.0f };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _engine.CosineSimilarity(vectorA, null!));
    }

    [Fact]
    public void CosineSimilarity_WithDifferentLengths_ThrowsArgumentException()
    {
        // Arrange
        var vectorA = new float[] { 1.0f, 2.0f };
        var vectorB = new float[] { 1.0f, 2.0f, 3.0f };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _engine.CosineSimilarity(vectorA, vectorB));
    }

    [Fact]
    public void CosineSimilarity_WithZeroVectors_ReturnsZero()
    {
        // Arrange
        var vectorA = new float[] { 0.0f, 0.0f, 0.0f };
        var vectorB = new float[] { 0.0f, 0.0f, 0.0f };

        // Act
        var result = _engine.CosineSimilarity(vectorA, vectorB);

        // Assert
        Assert.Equal(0.0, result);
    }

    [Fact]
    public void CosineSimilarity_WithNormalizedVectors_ReturnsCorrectValue()
    {
        // Arrange
        var vectorA = new float[] { 1.0f, 0.0f };
        var vectorB = new float[] { 0.707f, 0.707f }; // 45 degrees

        // Act
        var result = _engine.CosineSimilarity(vectorA, vectorB);

        // Assert
        Assert.True(Math.Abs(0.707 - result) < 0.01);
    }

    [Fact]
    public void FindTopSimilar_WithEmptyVectors_ReturnsEmptyList()
    {
        // Arrange
        var queryVector = new float[] { 1.0f, 2.0f };
        var vectors = Array.Empty<float[]>();

        // Act
        var result = _engine.FindTopSimilar(queryVector, vectors, 5);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void FindTopSimilar_WithSingleVector_ReturnsSingleResult()
    {
        // Arrange
        var queryVector = new float[] { 1.0f, 0.0f };
        var vectors = new[] { new float[] { 1.0f, 0.0f } };

        // Act
        var result = _engine.FindTopSimilar(queryVector, vectors, 5);

        // Assert
        Assert.Single(result);
        Assert.Equal(0, result[0].index);
        Assert.True(Math.Abs(1.0 - result[0].score) < 0.0001);
    }

    [Fact]
    public void FindTopSimilar_WithMultipleVectors_ReturnsTopK()
    {
        // Arrange
        var queryVector = new float[] { 1.0f, 0.0f };
        var vectors = new[]
        {
            new float[] { 1.0f, 0.0f },      // Most similar
            new float[] { 0.707f, 0.707f },  // Medium similarity
            new float[] { 0.0f, 1.0f },      // Least similar
            new float[] { 0.9f, 0.1f }       // High similarity
        };

        // Act
        var result = _engine.FindTopSimilar(queryVector, vectors, 2);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result[0].score > result[1].score);
        Assert.Equal(0, result[0].index); // Most similar
    }

    [Fact]
    public void FindTopSimilar_WithTopKGreaterThanVectors_ReturnsAllVectors()
    {
        // Arrange
        var queryVector = new float[] { 1.0f, 0.0f };
        var vectors = new[]
        {
            new float[] { 1.0f, 0.0f },
            new float[] { 0.0f, 1.0f }
        };

        // Act
        var result = _engine.FindTopSimilar(queryVector, vectors, 10);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FindTopSimilar_ResultsAreSortedDescending()
    {
        // Arrange
        var queryVector = new float[] { 1.0f, 0.0f };
        var vectors = new[]
        {
            new float[] { 0.0f, 1.0f },
            new float[] { 1.0f, 0.0f },
            new float[] { 0.707f, 0.707f }
        };

        // Act
        var result = _engine.FindTopSimilar(queryVector, vectors, 3);

        // Assert
        Assert.Equal(3, result.Count);
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].score >= result[i + 1].score);
        }
    }
}
