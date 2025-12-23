namespace DocToolkit.Interfaces.Engines;

/// <summary>
/// Engine interface for semantic embedding generation.
/// </summary>
public interface IEmbeddingEngine : IDisposable
{
    /// <summary>
    /// Generates a semantic embedding vector for the given text.
    /// </summary>
    /// <param name="text">Text to generate embedding for</param>
    /// <returns>Embedding vector</returns>
    float[] GenerateEmbedding(string text);
}
