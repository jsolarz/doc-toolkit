using DocToolkit.ifx.Interfaces.IEngines;

namespace DocToolkit.Engines;

/// <summary>
/// Engine for vector similarity calculations.
/// Encapsulates algorithm volatility: similarity metric could change (cosine, euclidean, dot product).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Similarity calculation algorithm
/// Pattern: Pure function - accepts vectors, returns similarity score (no I/O)
/// </remarks>
public class SimilarityEngine : ISimilarityEngine
{
    /// <summary>
    /// Calculates cosine similarity between two vectors.
    /// </summary>
    /// <param name="vectorA">First vector</param>
    /// <param name="vectorB">Second vector</param>
    /// <returns>Cosine similarity score (0.0 to 1.0)</returns>
    /// <exception cref="ArgumentException">Thrown when vectors have different lengths</exception>
    public double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA == null || vectorB == null)
        {
            throw new ArgumentNullException(vectorA == null ? nameof(vectorA) : nameof(vectorB));
        }

        if (vectorA.Length != vectorB.Length)
        {
            throw new ArgumentException("Vectors must have the same length");
        }

        double dotProduct = 0.0;
        double normA = 0.0;
        double normB = 0.0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        if (normA == 0.0 || normB == 0.0)
        {
            return 0.0;
        }

        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    /// <summary>
    /// Finds top K most similar vectors.
    /// </summary>
    /// <param name="queryVector">Query vector</param>
    /// <param name="vectors">Array of vectors to search</param>
    /// <param name="topK">Number of top results to return</param>
    /// <returns>List of (index, score) tuples sorted by score descending</returns>
    public List<(int index, double score)> FindTopSimilar(
        float[] queryVector,
        float[][] vectors,
        int topK)
    {
        // Pre-allocate scores list with known size (topK) to avoid reallocations
        // We'll add all scores first, then sort and take topK
        var scores = new List<(int index, double score)>(vectors.Length);

        for (int i = 0; i < vectors.Length; i++)
        {
            var similarity = CosineSimilarity(queryVector, vectors[i]);
            scores.Add((i, similarity));
        }

        return scores
            .OrderByDescending(s => s.score)
            .Take(topK)
            .ToList();
    }
}
