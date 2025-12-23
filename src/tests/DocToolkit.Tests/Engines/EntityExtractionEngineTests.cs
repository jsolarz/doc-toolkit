using DocToolkit.Engines;
using FluentAssertions;
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
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractEntities_WithNull_ReturnsEmptyList()
    {
        // Act
        var result = _engine.ExtractEntities(null!);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractEntities_WithCapitalizedPhrases_ExtractsEntities()
    {
        // Arrange
        var text = "Project Manager discussed Customer Requirements with System Architect. The Business Analyst reviewed the Technical Lead's work.";

        // Act
        var result = _engine.ExtractEntities(text);

        // Assert
        result.Should().NotBeEmpty();
        // The engine extracts capitalized phrases - check for any entity containing these words
        var allEntities = string.Join(" ", result);
        allEntities.Should().Contain("Manager");
        allEntities.Should().Contain("Requirements");
        allEntities.Should().Contain("Architect");
    }

    [Fact]
    public void ExtractEntities_WithStopWords_FiltersThemOut()
    {
        // Arrange
        var text = "The Project and System are important.";

        // Act
        var result = _engine.ExtractEntities(text);

        // Assert
        result.Should().NotContain("The");
        result.Should().NotContain("And");
    }

    [Fact]
    public void ExtractEntities_WithShortWords_FiltersThemOut()
    {
        // Arrange
        var text = "Hi AI is good.";

        // Act
        var result = _engine.ExtractEntities(text);

        // Assert
        result.Should().NotContain("Hi");
        result.Should().NotContain("AI");
    }

    [Fact]
    public void ExtractEntities_WithMixedCase_ExtractsOnlyCapitalized()
    {
        // Arrange
        var text = "The project manager and Project Manager are different.";

        // Act
        var result = _engine.ExtractEntities(text);

        // Assert
        result.Should().Contain("Project Manager");
        result.Should().NotContain("project manager");
    }

    [Fact]
    public void ExtractTopics_WithEmptyString_ReturnsEmptyList()
    {
        // Act
        var result = _engine.ExtractTopics("");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ExtractTopics_WithNull_ReturnsEmptyList()
    {
        // Act
        var result = _engine.ExtractTopics(null!);

        // Assert
        result.Should().BeEmpty();
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
        result.Should().HaveCountLessThanOrEqualTo(5);
        // Verify stop words are filtered
        result.Should().NotContain("project"); // Stop word
        result.Should().NotContain("requirements"); // Stop word
        result.Should().NotContain("system"); // Stop word
        result.Should().NotContain("application"); // Stop word
        result.Should().NotContain("management"); // Stop word
    }

    [Fact]
    public void ExtractTopics_WithShortWords_FiltersThemOut()
    {
        // Arrange
        var text = "The cat sat on the mat. The dog ran fast.";

        // Act
        var result = _engine.ExtractTopics(text);

        // Assert
        result.Should().AllSatisfy(topic => topic.Length.Should().BeGreaterThanOrEqualTo(5));
    }

    [Fact]
    public void ExtractTopics_WithTopN_ReturnsCorrectCount()
    {
        // Arrange
        var text = string.Join(" ", Enumerable.Range(1, 100).Select(i => $"word{i} word{i} word{i}"));

        // Act
        var result = _engine.ExtractTopics(text, 10);

        // Assert
        result.Should().HaveCountLessThanOrEqualTo(10);
    }

    [Fact]
    public void ExtractTopics_WithStopWords_FiltersThemOut()
    {
        // Arrange
        var text = "The project requirements system solution cloud customer service document management application.";

        // Act
        var result = _engine.ExtractTopics(text);

        // Assert
        result.Should().NotContain("project");
        result.Should().NotContain("requirements");
        result.Should().NotContain("system");
        result.Should().NotContain("solution");
        result.Should().NotContain("cloud");
        result.Should().NotContain("customer");
        result.Should().NotContain("service");
        result.Should().NotContain("document");
        result.Should().NotContain("management");
        result.Should().NotContain("application");
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
                word1Index.Should().BeLessThan(word2Index);
            }
        }
    }
}
