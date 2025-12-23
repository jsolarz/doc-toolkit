using DocToolkit.Engines;
using Xunit;

namespace DocToolkit.Tests.Engines;

public class SummarizationEngineTests
{
    private readonly SummarizationEngine _engine;

    public SummarizationEngineTests()
    {
        _engine = new SummarizationEngine();
    }

    [Fact]
    public void SummarizeText_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var result = _engine.SummarizeText("");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SummarizeText_WithNull_ReturnsEmptyString()
    {
        // Act
        var result = _engine.SummarizeText(null!);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void SummarizeText_WithShortText_ReturnsOriginalText()
    {
        // Arrange
        var text = "This is a short text.";

        // Act
        var result = _engine.SummarizeText(text);

        // Assert
        Assert.Equal(text, result);
    }

    [Fact]
    public void SummarizeText_WithTextUnder800Chars_ReturnsOriginalText()
    {
        // Arrange
        var text = new string('a', 799) + ".";

        // Act
        var result = _engine.SummarizeText(text);

        // Assert
        Assert.Equal(text, result);
    }

    [Fact]
    public void SummarizeText_WithLongText_ReturnsSummary()
    {
        // Arrange - Create text longer than 800 chars (each sentence ~100 chars, need 9+ sentences)
        var longSentence = "This is a very long sentence that contains many words and should definitely be summarized because it exceeds the 800 character threshold when combined with other sentences.";
        var sentences = Enumerable.Range(1, 10)
            .Select(i => $"{longSentence} {i}")
            .ToArray();
        var text = string.Join(". ", sentences) + ".";

        // Act
        var result = _engine.SummarizeText(text, maxSentences: 5);

        // Assert
        Assert.NotEqual(text, result);
        Assert.True(result.Length < text.Length);
        var resultSentences = result.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(resultSentences.Length <= 5);
    }

    [Fact]
    public void SummarizeText_WithMaxSentences_LimitsOutput()
    {
        // Arrange - Create text longer than 800 chars
        var longSentence = "This is a very long sentence that contains many words and should definitely be summarized because it exceeds the 800 character threshold when combined with other sentences.";
        var sentences = Enumerable.Range(1, 10)
            .Select(i => $"{longSentence} {i}")
            .ToArray();
        var text = string.Join(". ", sentences) + ".";

        // Act
        var result = _engine.SummarizeText(text, maxSentences: 3);

        // Assert
        var resultSentences = result.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(resultSentences.Length <= 3);
    }

    [Fact]
    public void SummarizeText_WithFewerSentencesThanMax_ReturnsAllSentences()
    {
        // Arrange
        var text = "First sentence. Second sentence. Third sentence.";

        // Act
        var result = _engine.SummarizeText(text, maxSentences: 10);

        // Assert
        Assert.Contains("First sentence", result);
        Assert.Contains("Second sentence", result);
        Assert.Contains("Third sentence", result);
    }

    [Fact]
    public void SummarizeText_WithMultipleSentenceEndings_HandlesCorrectly()
    {
        // Arrange
        var text = "First sentence! Second sentence? Third sentence.";

        // Act
        var result = _engine.SummarizeText(text, maxSentences: 2);

        // Assert
        Assert.Contains("First sentence", result);
        Assert.Contains("Second sentence", result);
    }

    [Fact]
    public void SummarizeText_WithDefaultMaxSentences_UsesFive()
    {
        // Arrange - Create text longer than 800 chars
        var longSentence = "This is a very long sentence that contains many words and should definitely be summarized because it exceeds the 800 character threshold when combined with other sentences.";
        var sentences = Enumerable.Range(1, 10)
            .Select(i => $"{longSentence} {i}")
            .ToArray();
        var text = string.Join(". ", sentences) + ".";

        // Act
        var result = _engine.SummarizeText(text);

        // Assert
        var resultSentences = result.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(resultSentences.Length <= 5);
    }

    [Fact]
    public void SummarizeText_WithVeryLongText_ProducesReasonableSummary()
    {
        // Arrange
        var sentences = Enumerable.Range(1, 100)
            .Select(i => $"This is a very long sentence number {i} that contains many words and should be summarized properly.")
            .ToArray();
        var text = string.Join(" ", sentences);

        // Act
        var result = _engine.SummarizeText(text, maxSentences: 5);

        // Assert
        Assert.True(result.Length < text.Length);
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.EndsWith(".", result);
    }
}
