namespace DocToolkit.ifx.Interfaces.IEngines;

/// <summary>
/// Engine interface for text summarization operations.
/// </summary>
public interface ISummarizationEngine
{
    /// <summary>
    /// Summarizes text by extracting key sentences.
    /// </summary>
    /// <param name="text">Text to summarize</param>
    /// <param name="maxSentences">Maximum number of sentences in summary</param>
    /// <returns>Summarized text</returns>
    string SummarizeText(string text, int maxSentences = 5);
}
