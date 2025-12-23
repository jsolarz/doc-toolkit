using DocToolkit.Engines;
using FluentAssertions;
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
        result.Should().BeApproximately(1.0, 0.0001);
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
        result.Should().BeApproximately(0.0, 0.0001);
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
        result.Should().BeApproximately(-1.0, 0.0001);
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
        result.Should().Be(0.0);
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
        result.Should().BeApproximately(0.707, 0.01);
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
        result.Should().BeEmpty();
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
        result.Should().HaveCount(1);
        result[0].index.Should().Be(0);
        result[0].score.Should().BeApproximately(1.0, 0.0001);
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
        result.Should().HaveCount(2);
        result[0].score.Should().BeGreaterThan(result[1].score);
        result[0].index.Should().Be(0); // Most similar
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
        result.Should().HaveCount(2);
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
        result.Should().HaveCount(3);
        for (int i = 0; i < result.Count - 1; i++)
        {
            result[i].score.Should().BeGreaterThanOrEqualTo(result[i + 1].score);
        }
    }
}
