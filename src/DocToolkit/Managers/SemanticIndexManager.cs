using DocToolkit.Models;
using DocToolkit.Interfaces.Managers;
using DocToolkit.Interfaces.Engines;
using DocToolkit.Interfaces.Accessors;
using DocToolkit.Services.Engines;
using DocToolkit.Engines;
using DocToolkit.Accessors;

namespace DocToolkit.Managers;

/// <summary>
/// Manager for semantic indexing workflow.
/// Encapsulates workflow volatility: indexing orchestration could change.
/// </summary>
/// <remarks>
/// Component Type: Manager (Workflow Volatility)
/// Volatility: Indexing workflow and orchestration
/// Pattern: Orchestrates Engines and Accessors - knows "when", not "how"
/// </remarks>
public class SemanticIndexManager : ISemanticIndexManager, IDisposable
{
    private readonly IDocumentExtractionEngine _extractor;
    private readonly IEmbeddingEngine _embedding;
    private readonly IVectorStorageAccessor _storage;
    private readonly ITextChunkingEngine _chunkingEngine;

    public SemanticIndexManager()
    {
        _extractor = new DocumentExtractionEngine();
        _embedding = new EmbeddingEngine();
        _storage = new VectorStorageAccessor();
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
    /// Orchestrates: DocumentExtractionEngine (Engine), EmbeddingEngine (Engine), 
    ///                TextChunkingEngine (Engine), VectorStorageAccessor (Accessor)
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
        if (_embedding is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
