using DocToolkit.ifx.Interfaces.IEngines;

namespace DocToolkit.Engines;

/// <summary>
/// Engine for text chunking operations.
/// Encapsulates algorithm volatility: chunking strategy could change (size, overlap, method).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Component Type:</strong> Engine (Algorithm Volatility)
/// </para>
/// <para>
/// <strong>Volatility Encapsulated:</strong> Chunking algorithm and strategy. The algorithm for
/// splitting text into chunks could change (word-based, sentence-based, semantic-based, etc.).
/// The chunk size and overlap parameters could change based on requirements.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Pure function - accepts text as parameter, returns chunks.
/// No I/O operations, no state, no side effects. This makes the engine easily testable and
/// allows for algorithm changes without affecting other components.
/// </para>
/// <para>
/// <strong>Service Boundary:</strong> Called by Managers (SemanticIndexManager).
/// Engines are pure functions and do not call Accessors directly. Managers orchestrate
/// the flow: Manager → Engine (for processing) → Manager → Accessor (for storage).
/// </para>
/// <para>
/// <strong>IDesign Method™ Compliance:</strong>
/// </para>
/// <list type="bullet">
/// <item>Encapsulates algorithm volatility (chunking strategy)</item>
/// <item>Pure function pattern (no I/O, no state)</item>
/// <item>Accepts data as parameters (text, chunkSize, chunkOverlap)</item>
/// <item>Returns processed data (list of chunks)</item>
/// <item>No direct Accessor calls (follows IDesign principle)</item>
/// </list>
/// </remarks>
public class TextChunkingEngine : ITextChunkingEngine
{
    /// <summary>
    /// Chunks text into overlapping segments based on word count.
    /// </summary>
    /// <param name="text">
    /// The text to chunk. Must not be null. Empty or whitespace-only text returns an empty list.
    /// </param>
    /// <param name="chunkSize">
    /// The number of words per chunk. Must be greater than 0. Typical values range from 100 to 1000 words.
    /// </param>
    /// <param name="chunkOverlap">
    /// The number of words to overlap between consecutive chunks. Must be non-negative and less than chunkSize.
    /// Overlap helps maintain context across chunk boundaries.
    /// </param>
    /// <returns>
    /// A list of text chunks, where each chunk contains approximately <paramref name="chunkSize"/> words.
    /// Returns an empty list if the input text is null, empty, or whitespace-only.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Algorithm:</strong>
    /// </para>
    /// <list type="number">
    /// <item>Splits text into words using whitespace delimiters (space, newline, carriage return, tab)</item>
    /// <item>Creates chunks by taking <paramref name="chunkSize"/> words at a time</item>
    /// <item>Advances by (<paramref name="chunkSize"/> - <paramref name="chunkOverlap"/>) words for next chunk</item>
    /// <item>Joins words back into text for each chunk</item>
    /// </list>
    /// <para>
    /// <strong>Memory Optimization:</strong>
    /// The chunks list is pre-allocated with a capacity estimate based on text length to reduce
    /// reallocations during chunk creation. This follows Step 1 of the memory optimization strategy.
    /// </para>
    /// <para>
    /// <strong>Example:</strong>
    /// </para>
    /// <code>
    /// var engine = new TextChunkingEngine();
    /// var chunks = engine.ChunkText("word1 word2 word3 word4 word5", chunkSize: 3, chunkOverlap: 1);
    /// // Returns: ["word1 word2 word3", "word3 word4 word5"]
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="text"/> is null.
    /// </exception>
    public List<string> ChunkText(string text, int chunkSize, int chunkOverlap)
    {
        // IDesign C# Coding Standard: Validate arguments
        if (text == null)
        {
            throw new ArgumentNullException(nameof(text));
        }

        // Early return for empty input (pure function pattern - no side effects)
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        // Validate chunk parameters
        if (chunkSize <= 0)
        {
            throw new ArgumentException("Chunk size must be greater than 0", nameof(chunkSize));
        }

        if (chunkOverlap < 0)
        {
            throw new ArgumentException("Chunk overlap must be non-negative", nameof(chunkOverlap));
        }

        if (chunkOverlap >= chunkSize)
        {
            throw new ArgumentException("Chunk overlap must be less than chunk size", nameof(chunkOverlap));
        }

        // Split text into words using whitespace delimiters
        // IDesign: Pure function - no I/O, only data transformation
        var words = text.Split(new[] { ' ', '\n', '\r', '\t' },
            StringSplitOptions.RemoveEmptyEntries);

        // Pre-allocate chunks list with capacity estimate to reduce reallocations
        // Memory Optimization Step 1: Pre-allocate collections
        // Estimate: text length / (chunkSize * average word length ~5 chars)
        var estimatedChunks = Math.Max(1, text.Length / (chunkSize * 5));
        var chunks = new List<string>(estimatedChunks);

        // Create overlapping chunks
        int i = 0;
        while (i < words.Length)
        {
            // Take chunkSize words starting at position i
            var chunkWords = words.Skip(i).Take(chunkSize);
            var chunk = string.Join(" ", chunkWords);
            
            // Only add non-empty chunks
            if (!string.IsNullOrWhiteSpace(chunk))
            {
                chunks.Add(chunk);
            }

            // Advance by (chunkSize - chunkOverlap) to create overlap
            // Math.Max(1, ...) ensures we always advance at least 1 word
            i += Math.Max(1, chunkSize - chunkOverlap);
        }

        return chunks;
    }
}
