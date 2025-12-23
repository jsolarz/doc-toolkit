using DocToolkit.ifx.Interfaces.IAccessors;
using DocToolkit.ifx.Interfaces.IEngines;
using DocToolkit.ifx.Interfaces.IManagers;
using DocToolkit.ifx.Models;

namespace DocToolkit.Managers;

/// <summary>
/// Manager for semantic search workflow.
/// Encapsulates workflow volatility: search orchestration could change.
/// </summary>
/// <remarks>
/// Component Type: Manager (Workflow Volatility)
/// Volatility: Search workflow and orchestration
/// Pattern: Orchestrates Engines and Accessors - knows "when", not "how"
/// </remarks>
public class SemanticSearchManager : ISemanticSearchManager, IDisposable
{
    private readonly IEmbeddingEngine _embedding;
    private readonly IVectorStorageAccessor _storage;
    private readonly ISimilarityEngine _similarityEngine;

    /// <summary>
    /// Initializes a new instance of the SemanticSearchManager.
    /// </summary>
    /// <param name="embedding">Embedding engine</param>
    /// <param name="storage">Vector storage accessor</param>
    /// <param name="similarityEngine">Similarity engine</param>
    public SemanticSearchManager(
        IEmbeddingEngine embedding,
        IVectorStorageAccessor storage,
        ISimilarityEngine similarityEngine)
    {
        _embedding = embedding ?? throw new ArgumentNullException(nameof(embedding));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _similarityEngine = similarityEngine ?? throw new ArgumentNullException(nameof(similarityEngine));
    }

    /// <summary>
    /// Searches the semantic index for similar content.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="indexPath">Index directory path</param>
    /// <param name="topK">Number of top results to return</param>
    /// <returns>List of search results sorted by similarity score</returns>
    /// <exception cref="FileNotFoundException">Thrown when index not found</exception>
    /// <remarks>
    /// Service Boundary: Called by SearchCommand (Client)
    /// Orchestrates: VectorStorageAccessor (Accessor), EmbeddingEngine (Engine), 
    ///                SimilarityEngine (Engine)
    /// Authentication: None (local CLI tool)
    /// Authorization: None (local CLI tool)
    /// Transaction: None (read-only operations)
    /// </remarks>
    public List<SearchResult> Search(string query, string indexPath, int topK)
    {
        if (!_storage.IndexExists(indexPath))
        {
            throw new FileNotFoundException($"Semantic index not found at: {indexPath}");
        }

        // Orchestrate: Load index and vectors (Accessor)
        var entries = _storage.LoadIndex(indexPath);
        var vectors = _storage.LoadVectors(indexPath);

        if (entries.Count == 0 || vectors.Length == 0)
        {
            return new List<SearchResult>();
        }

        // Orchestrate: Generate query embedding (Engine)
        var queryEmbedding = _embedding.GenerateEmbedding(query);

        // Orchestrate: Calculate similarities (Engine)
        var topResults = _similarityEngine.FindTopSimilar(queryEmbedding, vectors, topK);

        // Build result list
        var results = new List<SearchResult>();

        foreach (var (index, score) in topResults)
        {
            if (index < entries.Count)
            {
                var entry = entries[index];
                results.Add(new SearchResult
                {
                    File = entry.File,
                    Path = entry.Path,
                    Score = score,
                    Chunk = entry.Chunk
                });
            }
        }

        return results;
    }

    public void Dispose()
    {
        if (_embedding is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
