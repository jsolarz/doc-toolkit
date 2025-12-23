using DocToolkit.Interfaces.Engines;

namespace DocToolkit.Engines;

/// <summary>
/// Engine for text chunking operations.
/// Encapsulates algorithm volatility: chunking strategy could change (size, overlap, method).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Chunking algorithm and strategy
/// Pattern: Pure function - accepts text, returns chunks (no I/O)
/// </remarks>
public class TextChunkingEngine : ITextChunkingEngine
{
    /// <summary>
    /// Chunks text into overlapping segments.
    /// </summary>
    /// <param name="text">Text to chunk</param>
    /// <param name="chunkSize">Number of words per chunk</param>
    /// <param name="chunkOverlap">Number of words to overlap between chunks</param>
    /// <returns>List of text chunks</returns>
    public List<string> ChunkText(string text, int chunkSize, int chunkOverlap)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        var words = text.Split(new[] { ' ', '\n', '\r', '\t' },
            StringSplitOptions.RemoveEmptyEntries);

        var chunks = new List<string>();
        int i = 0;

        while (i < words.Length)
        {
            var chunkWords = words.Skip(i).Take(chunkSize);
            var chunk = string.Join(" ", chunkWords);
            chunks.Add(chunk);
            i += Math.Max(1, chunkSize - chunkOverlap);
        }

        return chunks;
    }
}
