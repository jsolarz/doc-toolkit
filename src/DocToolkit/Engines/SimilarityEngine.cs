using DocToolkit.ifx.Interfaces.IEngines;

namespace DocToolkit.Engines;

/// <summary>
/// Engine for vector similarity calculations.
/// Encapsulates algorithm volatility: similarity metric could change (cosine, euclidean, dot product).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Component Type:</strong> Engine (Algorithm Volatility)
/// </para>
/// <para>
/// <strong>Volatility Encapsulated:</strong> Similarity calculation algorithm. The metric used to
/// calculate similarity between vectors could change (cosine similarity, euclidean distance,
/// dot product, etc.). The algorithm implementation could also change (optimized versions,
/// different precision, etc.).
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Pure function - accepts vectors as parameters, returns
/// similarity scores. No I/O operations, no state, no side effects. This makes the engine
/// easily testable and allows for algorithm changes without affecting other components.
/// </para>
/// <para>
/// <strong>Service Boundary:</strong> Called by Managers (SemanticSearchManager).
/// Engines are pure functions and do not call Accessors directly. Managers orchestrate
/// the flow: Manager → Engine (for processing) → Manager → Accessor (for storage).
/// </para>
/// <para>
/// <strong>IDesign Method™ Compliance:</strong>
/// </para>
/// <list type="bullet">
/// <item>Encapsulates algorithm volatility (similarity metric)</item>
/// <item>Pure function pattern (no I/O, no state)</item>
/// <item>Accepts data as parameters (vectors)</item>
/// <item>Returns processed data (similarity scores)</item>
/// <item>No direct Accessor calls (follows IDesign principle)</item>
/// </list>
/// </remarks>
public class SimilarityEngine : ISimilarityEngine
{
    /// <summary>
    /// Calculates cosine similarity between two vectors.
    /// </summary>
    /// <param name="vectorA">
    /// The first vector. Must not be null and must have the same length as <paramref name="vectorB"/>.
    /// </param>
    /// <param name="vectorB">
    /// The second vector. Must not be null and must have the same length as <paramref name="vectorA"/>.
    /// </param>
    /// <returns>
    /// Cosine similarity score between 0.0 and 1.0, where:
    /// <list type="bullet">
    /// <item>1.0 indicates identical vectors (same direction)</item>
    /// <item>0.0 indicates orthogonal vectors (perpendicular)</item>
    /// <item>Negative values indicate opposite directions (rare for normalized embeddings)</item>
    /// </list>
    /// Returns 0.0 if either vector has zero magnitude (division by zero protection).
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Algorithm:</strong>
    /// </para>
    /// <para>
    /// Cosine similarity is calculated as: <c>cos(θ) = (A · B) / (||A|| × ||B||)</c>
    /// </para>
    /// <list type="number">
    /// <item>Calculate dot product: <c>A · B = Σ(A[i] × B[i])</c></item>
    /// <item>Calculate norms: <c>||A|| = √(Σ(A[i]²))</c>, <c>||B|| = √(Σ(B[i]²))</c></item>
    /// <item>Return: <c>dotProduct / (normA × normB)</c></item>
    /// </list>
    /// <para>
    /// <strong>Performance:</strong>
    /// Time complexity: O(n) where n is the vector dimension.
    /// Space complexity: O(1) - only uses local variables.
    /// </para>
    /// <para>
    /// <strong>Example:</strong>
    /// </para>
    /// <code>
    /// var engine = new SimilarityEngine();
    /// var similarity = engine.CosineSimilarity(
    ///     new float[] { 1.0f, 0.0f },
    ///     new float[] { 1.0f, 0.0f }
    /// );
    /// // Returns: 1.0 (identical vectors)
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="vectorA"/> or <paramref name="vectorB"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="vectorA"/> and <paramref name="vectorB"/> have different lengths.
    /// </exception>
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
    /// Finds the top K most similar vectors to a query vector using cosine similarity.
    /// </summary>
    /// <param name="queryVector">
    /// The query vector to find similar vectors for. Must not be null and must have the same
    /// dimension as all vectors in <paramref name="vectors"/>.
    /// </param>
    /// <param name="vectors">
    /// Array of vectors to search. Must not be null. All vectors must have the same dimension
    /// as <paramref name="queryVector"/>.
    /// </param>
    /// <param name="topK">
    /// The number of top results to return. Must be greater than 0 and less than or equal to
    /// the number of vectors. If topK exceeds the number of vectors, returns all vectors sorted.
    /// </param>
    /// <returns>
    /// A list of tuples containing (index, score) pairs, sorted by score in descending order
    /// (most similar first). The list contains at most <paramref name="topK"/> elements.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Algorithm:</strong>
    /// </para>
    /// <list type="number">
    /// <item>Calculate cosine similarity between query vector and each vector in the array</item>
    /// <item>Store (index, score) pairs for all vectors</item>
    /// <item>Sort by score in descending order</item>
    /// <item>Take top K results</item>
    /// </list>
    /// <para>
    /// <strong>Memory Optimization:</strong>
    /// The scores list is pre-allocated with capacity equal to the number of vectors to avoid
    /// reallocations during score collection. This follows Step 1 of the memory optimization strategy.
    /// </para>
    /// <para>
    /// <strong>Performance:</strong>
    /// Time complexity: O(n × d + n log n) where n is the number of vectors and d is the vector dimension.
    /// Space complexity: O(n) for storing scores.
    /// </para>
    /// <para>
    /// <strong>Example:</strong>
    /// </para>
    /// <code>
    /// var engine = new SimilarityEngine();
    /// var results = engine.FindTopSimilar(
    ///     queryVector: new float[] { 1.0f, 0.0f },
    ///     vectors: new[] {
    ///         new float[] { 1.0f, 0.0f },
    ///         new float[] { 0.0f, 1.0f },
    ///         new float[] { 0.707f, 0.707f }
    ///     },
    ///     topK: 2
    /// );
    /// // Returns: [(0, 1.0), (2, 0.707)] - indices 0 and 2 with their scores
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="queryVector"/> or <paramref name="vectors"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="topK"/> is less than or equal to 0, or when vectors have
    /// mismatched dimensions.
    /// </exception>
    public List<(int index, double score)> FindTopSimilar(
        float[] queryVector,
        float[][] vectors,
        int topK)
    {
        // IDesign C# Coding Standard: Validate arguments
        if (queryVector == null)
        {
            throw new ArgumentNullException(nameof(queryVector));
        }

        if (vectors == null)
        {
            throw new ArgumentNullException(nameof(vectors));
        }

        if (topK <= 0)
        {
            throw new ArgumentException("topK must be greater than 0", nameof(topK));
        }

        // Pre-allocate scores list with known size to avoid reallocations
        // Memory Optimization Step 1: Pre-allocate collections
        // We'll add all scores first, then sort and take topK
        var scores = new List<(int index, double score)>(vectors.Length);

        // Calculate similarity for each vector
        // IDesign: Pure function - no I/O, only data transformation
        for (int i = 0; i < vectors.Length; i++)
        {
            // Calculate cosine similarity between query and current vector
            var similarity = CosineSimilarity(queryVector, vectors[i]);
            scores.Add((i, similarity));
        }

        // Sort by score descending and take top K
        // IDesign: Return processed data, no side effects
        return scores
            .OrderByDescending(s => s.score)
            .Take(topK)
            .ToList();
    }
}
