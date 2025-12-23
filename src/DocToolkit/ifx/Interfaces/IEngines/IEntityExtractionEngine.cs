namespace DocToolkit.Interfaces.Engines;

/// <summary>
/// Engine interface for entity and topic extraction from text.
/// </summary>
public interface IEntityExtractionEngine
{
    /// <summary>
    /// Extracts entities (capitalized multi-word phrases) from text.
    /// </summary>
    /// <param name="text">Text to extract entities from</param>
    /// <returns>List of extracted entities</returns>
    List<string> ExtractEntities(string text);

    /// <summary>
    /// Extracts topics (frequent meaningful words) from text.
    /// </summary>
    /// <param name="text">Text to extract topics from</param>
    /// <param name="topN">Number of top topics to return</param>
    /// <returns>List of extracted topics</returns>
    List<string> ExtractTopics(string text, int topN = 10);
}
