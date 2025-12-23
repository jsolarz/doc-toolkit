using DocToolkit.Models;
using DocToolkit.Services.Engines;

namespace DocToolkit.Services;

/// <summary>
/// Manager for semantic indexing workflow.
/// Encapsulates workflow volatility: indexing orchestration could change.
/// </summary>
/// <remarks>
/// Component Type: Manager (Workflow Volatility)
/// Volatility: Indexing workflow and orchestration
/// Pattern: Orchestrates Engines and Accessors - knows "when", not "how"
/// </remarks>
public class SemanticIndexService : IDisposable
{
    private readonly DocumentExtractionService _extractor;
    private readonly EmbeddingService _embedding;
    private readonly VectorStorageService _storage;
    private readonly TextChunkingEngine _chunkingEngine;

    public SemanticIndexService()
    {
        _extractor = new DocumentExtractionService();
        _embedding = new EmbeddingService();
        _storage = new VectorStorageService();
        _chunkingEngine = new TextChunkingEngine();
    }

    /// <summary>
    /// Builds a semantic index from source files.
    /// </summary>
    /// <param name="sourcePath">Source directory path</param>
    /// <param name="outputPath">Output directory path</param>
    /// <param name="chunkSize">Text chunk size in words</param>
    /// <param name="chunkOverlap">Chunk overlap in words</param>
    /// <param name="progressCallback">Progress callback (0-100)</param>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// Service Boundary: Called by IndexCommand (Client)
    /// Orchestrates: DocumentExtractionService (Engine), EmbeddingService (Engine), 
    ///                TextChunkingEngine (Engine), VectorStorageService (Accessor)
    /// Authentication: None (local CLI tool)
    /// Authorization: None (local CLI tool)
    /// Transaction: None (file-based operations)
    /// </remarks>
    public bool BuildIndex(
        string sourcePath,
        string outputPath,
        int chunkSize,
        int chunkOverlap,
        Action<double>? progressCallback = null)
    {
        if (!Directory.Exists(sourcePath))
        {
            return false;
        }

        var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();
        var totalFiles = files.Count;
        var processedFiles = 0;

        var entries = new List<IndexEntry>();
        var vectors = new List<float[]>();

        foreach (var file in files)
        {
            try
            {
                // Orchestrate: Extract text (Engine)
                var text = _extractor.ExtractText(file);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                // Orchestrate: Chunk text (Engine)
                var chunks = _chunkingEngine.ChunkText(text, chunkSize, chunkOverlap);

                foreach (var chunk in chunks)
                {
                    if (string.IsNullOrWhiteSpace(chunk))
                    {
                        continue;
                    }

                    // Orchestrate: Generate embedding (Engine)
                    var embedding = _embedding.GenerateEmbedding(chunk);
                    vectors.Add(embedding);

                    entries.Add(new IndexEntry
                    {
                        File = Path.GetFileName(file),
                        Path = file,
                        Chunk = chunk,
                        Index = entries.Count
                    });
                }

                processedFiles++;
                progressCallback?.Invoke((double)processedFiles / totalFiles * 100);
            }
            catch
            {
                // Skip files that can't be processed
            }
        }

        if (vectors.Count == 0)
        {
            return false;
        }

        // Orchestrate: Save vectors and index (Accessor)
        _storage.SaveVectors(vectors.ToArray(), outputPath);
        _storage.SaveIndex(entries, outputPath);

        return true;
    }

    public void Dispose()
    {
        _embedding?.Dispose();
    }
}
