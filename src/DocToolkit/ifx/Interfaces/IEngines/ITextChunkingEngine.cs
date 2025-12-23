namespace DocToolkit.ifx.Interfaces.IEngines;

/// <summary>
/// Engine interface for text chunking operations.
/// </summary>
public interface ITextChunkingEngine
{
    /// <summary>
    /// Chunks text into overlapping segments.
    /// </summary>
    /// <param name="text">Text to chunk</param>
    /// <param name="chunkSize">Number of words per chunk</param>
    /// <param name="chunkOverlap">Number of words to overlap between chunks</param>
    /// <returns>List of text chunks</returns>
    List<string> ChunkText(string text, int chunkSize, int chunkOverlap);
}
