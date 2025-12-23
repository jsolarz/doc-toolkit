using DocToolkit.ifx.Events;
using DocToolkit.ifx.Infrastructure;
using DocToolkit.ifx.Interfaces.IAccessors;
using DocToolkit.ifx.Interfaces.IEngines;
using DocToolkit.ifx.Interfaces.IManagers;
using DocToolkit.ifx.Models;

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
    private readonly IEventBus _eventBus;

    /// <summary>
    /// Initializes a new instance of the SemanticIndexManager.
    /// </summary>
    /// <param name="extractor">Document extraction engine</param>
    /// <param name="embedding">Embedding engine</param>
    /// <param name="storage">Vector storage accessor</param>
    /// <param name="chunkingEngine">Text chunking engine</param>
    /// <param name="eventBus">Event bus for publishing events</param>
    public SemanticIndexManager(
        IDocumentExtractionEngine extractor,
        IEmbeddingEngine embedding,
        IVectorStorageAccessor storage,
        ITextChunkingEngine chunkingEngine,
        IEventBus eventBus)
    {
        _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        _embedding = embedding ?? throw new ArgumentNullException(nameof(embedding));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _chunkingEngine = chunkingEngine ?? throw new ArgumentNullException(nameof(chunkingEngine));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
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

                // Publish event: Document processed
                _eventBus.Publish(new DocumentProcessedEvent
                {
                    FilePath = file,
                    ExtractedText = text,
                    FileSize = new FileInfo(file).Length,
                    FileType = Path.GetExtension(file),
                    CharacterCount = text.Length
                });

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

        // Publish event: Index built
        _eventBus.Publish(new IndexBuiltEvent
        {
            IndexPath = outputPath,
            EntryCount = entries.Count,
            VectorCount = vectors.Count,
            SourcePath = sourcePath
        });

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
