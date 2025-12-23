using DocToolkit.ifx.Interfaces.IEngines;

namespace DocToolkit.Engines;

/// <summary>
/// Engine for text summarization operations.
/// Encapsulates algorithm volatility: summarization algorithm could change (sentence extraction, ML models).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Summarization algorithm
/// Pattern: Pure function - accepts text, returns summary (no I/O)
/// </remarks>
public class SummarizationEngine : ISummarizationEngine
{
    /// <summary>
    /// Summarizes text by extracting key sentences.
    /// </summary>
    /// <param name="text">Text to summarize</param>
    /// <param name="maxSentences">Maximum number of sentences in summary</param>
    /// <returns>Summarized text</returns>
    public string SummarizeText(string text, int maxSentences = 5)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (text.Length <= 800)
        {
            return text;
        }

        var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var summaryCount = Math.Min(maxSentences, sentences.Length);
        return string.Join(" ", sentences.Take(summaryCount)) + ".";
    }
}
