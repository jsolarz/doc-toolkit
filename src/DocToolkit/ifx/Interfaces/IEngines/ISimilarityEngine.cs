namespace DocToolkit.Interfaces.Engines;

/// <summary>
/// Engine interface for vector similarity calculations.
/// </summary>
public interface ISimilarityEngine
{
    /// <summary>
    /// Calculates cosine similarity between two vectors.
    /// </summary>
    /// <param name="vectorA">First vector</param>
    /// <param name="vectorB">Second vector</param>
    /// <returns>Cosine similarity score (0.0 to 1.0)</returns>
    double CosineSimilarity(float[] vectorA, float[] vectorB);

    /// <summary>
    /// Finds top K most similar vectors.
    /// </summary>
    /// <param name="queryVector">Query vector</param>
    /// <param name="vectors">Array of vectors to search</param>
    /// <param name="topK">Number of top results to return</param>
    /// <returns>List of (index, score) tuples sorted by score descending</returns>
    List<(int index, double score)> FindTopSimilar(float[] queryVector, float[][] vectors, int topK);
}
