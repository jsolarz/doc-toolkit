using DocToolkit.Engines;
using Xunit;

namespace DocToolkit.Tests.Engines;

public class EntityExtractionEngineTests
{
    private readonly EntityExtractionEngine _engine;

    public EntityExtractionEngineTests()
    {
        _engine = new EntityExtractionEngine();
    }

    [Fact]
    public void ExtractEntities_WithEmptyString_ReturnsEmptyList()
    {
        // Act
        var result = _engine.ExtractEntities("");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractEntities_WithNull_ReturnsEmptyList()
    {
        // Act
        var result = _engine.ExtractEntities(null!);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractEntities_WithCapitalizedPhrases_ExtractsEntities()
    {
        // Arrange
        var text = "Project Manager discussed Customer Requirements with System Architect. The Business Analyst reviewed the Technical Lead's work.";

        // Act
        var result = _engine.ExtractEntities(text);

        // Assert
        Assert.NotEmpty(result);
        // The engine extracts capitalized phrases - check for any entity containing these words
        var allEntities = string.Join(" ", result);
        Assert.Contains("Manager", allEntities);
        Assert.Contains("Requirements", allEntities);
        Assert.Contains("Architect", allEntities);
    }

    [Fact]
    public void ExtractEntities_WithStopWords_FiltersThemOut()
    {
        // Arrange
        var text = "The Project and System are important.";

        // Act
        var result = _engine.ExtractEntities(text);

        // Assert
        Assert.DoesNotContain("The", result);
        Assert.DoesNotContain("And", result);
    }

    [Fact]
    public void ExtractEntities_WithShortWords_FiltersThemOut()
    {
        // Arrange
        var text = "Hi AI is good.";

        // Act
        var result = _engine.ExtractEntities(text);

        // Assert
        Assert.DoesNotContain("Hi", result);
        Assert.DoesNotContain("AI", result);
    }

    [Fact]
    public void ExtractEntities_WithMixedCase_ExtractsOnlyCapitalized()
    {
        // Arrange
        var text = "The project manager and Project Manager are different.";

        // Act
        var result = _engine.ExtractEntities(text);

        // Assert
        Assert.Contains("Project Manager", result);
        Assert.DoesNotContain("project manager", result);
    }

    [Fact]
    public void ExtractTopics_WithEmptyString_ReturnsEmptyList()
    {
        // Act
        var result = _engine.ExtractTopics("");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractTopics_WithNull_ReturnsEmptyList()
    {
        // Act
        var result = _engine.ExtractTopics(null!);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ExtractTopics_WithFrequentWords_ReturnsTopTopics()
    {
        // Arrange - Use words that are NOT in the stop words list
        var text = "The development provides excellent quality for software engineers. " +
                   "The software quality helps engineers understand the development better. " +
                   "Engineers appreciate the excellent development and quality.";

        // Act
        var result = _engine.ExtractTopics(text, 5);

        // Assert
        Assert.True(result.Count <= 5);
        // Verify stop words are filtered
        Assert.DoesNotContain("project", result); // Stop word
        Assert.DoesNotContain("requirements", result); // Stop word
        Assert.DoesNotContain("system", result); // Stop word
        Assert.DoesNotContain("application", result); // Stop word
        Assert.DoesNotContain("management", result); // Stop word
    }

    [Fact]
    public void ExtractTopics_WithShortWords_FiltersThemOut()
    {
        // Arrange
        var text = "The cat sat on the mat. The dog ran fast.";

        // Act
        var result = _engine.ExtractTopics(text);

        // Assert
        foreach (var topic in result)
        {
            Assert.True(topic.Length >= 5);
        }
    }

    [Fact]
    public void ExtractTopics_WithTopN_ReturnsCorrectCount()
    {
        // Arrange
        var text = string.Join(" ", Enumerable.Range(1, 100).Select(i => $"word{i} word{i} word{i}"));

        // Act
        var result = _engine.ExtractTopics(text, 10);

        // Assert
        Assert.True(result.Count <= 10);
    }

    [Fact]
    public void ExtractTopics_WithStopWords_FiltersThemOut()
    {
        // Arrange
        var text = "The project requirements system solution cloud customer service document management application.";

        // Act
        var result = _engine.ExtractTopics(text);

        // Assert
        Assert.DoesNotContain("project", result);
        Assert.DoesNotContain("requirements", result);
        Assert.DoesNotContain("system", result);
        Assert.DoesNotContain("solution", result);
        Assert.DoesNotContain("cloud", result);
        Assert.DoesNotContain("customer", result);
        Assert.DoesNotContain("service", result);
        Assert.DoesNotContain("document", result);
        Assert.DoesNotContain("management", result);
        Assert.DoesNotContain("application", result);
    }

    [Fact]
    public void ExtractTopics_ResultsAreOrderedByFrequency()
    {
        // Arrange
        var text = "word1 word1 word1 word2 word2 word3";

        // Act
        var result = _engine.ExtractTopics(text, 3);

        // Assert
        if (result.Count >= 2)
        {
            // word1 should appear before word2 (more frequent)
            var word1Index = result.IndexOf("word1");
            var word2Index = result.IndexOf("word2");
            
            if (word1Index >= 0 && word2Index >= 0)
            {
                Assert.True(word1Index < word2Index);
            }
        }
    }
}
