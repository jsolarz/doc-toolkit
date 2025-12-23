# Patch: IDesign Method™ Implementation

## Overview

This patch implements IDesign Method™ principles in the codebase, focusing on:
1. Making Engines pure (no I/O, receive data as parameters)
2. Moving business logic from Managers to Engines
3. Adding XML documentation with volatility notes
4. Ensuring proper component taxonomy

## Files to Create

### 1. `src/DocToolkit/Services/Engines/TextChunkingEngine.cs`

**Purpose**: Encapsulate text chunking algorithm volatility (chunking strategy could change).

```csharp
namespace DocToolkit.Services.Engines;

/// <summary>
/// Engine for text chunking operations.
/// Encapsulates algorithm volatility: chunking strategy could change (size, overlap, method).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Chunking algorithm and strategy
/// Pattern: Pure function - accepts text, returns chunks (no I/O)
/// </remarks>
public class TextChunkingEngine
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
```

### 2. `src/DocToolkit/Services/Engines/SimilarityEngine.cs`

**Purpose**: Encapsulate similarity calculation algorithm volatility (could use different metrics).

```csharp
namespace DocToolkit.Services.Engines;

/// <summary>
/// Engine for vector similarity calculations.
/// Encapsulates algorithm volatility: similarity metric could change (cosine, euclidean, dot product).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Similarity calculation algorithm
/// Pattern: Pure function - accepts vectors, returns similarity score (no I/O)
/// </remarks>
public class SimilarityEngine
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
        var scores = new List<(int index, double score)>();

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
```

### 3. `src/DocToolkit/Services/Engines/EntityExtractionEngine.cs`

**Purpose**: Encapsulate entity and topic extraction algorithm volatility (extraction methods could change).

```csharp
using System.Text.RegularExpressions;

namespace DocToolkit.Services.Engines;

/// <summary>
/// Engine for entity and topic extraction from text.
/// Encapsulates algorithm volatility: extraction methods could change (regex, NLP, ML models).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Entity/topic extraction algorithm
/// Pattern: Pure function - accepts text, returns entities/topics (no I/O)
/// </remarks>
public class EntityExtractionEngine
{
    /// <summary>
    /// Extracts entities (capitalized multi-word phrases) from text.
    /// </summary>
    /// <param name="text">Text to extract entities from</param>
    /// <returns>List of extracted entities</returns>
    public List<string> ExtractEntities(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        // Simple heuristic: capitalized multi-word phrases
        var pattern = @"\b([A-Z][a-zA-Z0-9]+(?:\s+[A-Z][a-zA-Z0-9]+)*)\b";
        var matches = Regex.Matches(text, pattern);
        var stopWords = new HashSet<string> 
        { 
            "The", "This", "That", "And", "For", "With", "From", 
            "Project", "Customer", "System", "Service" 
        };

        return matches
            .Cast<Match>()
            .Select(m => m.Value.Trim())
            .Where(e => !stopWords.Contains(e) && e.Length > 2)
            .ToList();
    }

    /// <summary>
    /// Extracts topics (frequent meaningful words) from text.
    /// </summary>
    /// <param name="text">Text to extract topics from</param>
    /// <param name="topN">Number of top topics to return</param>
    /// <returns>List of extracted topics</returns>
    public List<string> ExtractTopics(string text, int topN = 10)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        var words = Regex.Matches(text.ToLower(), @"\b[a-zA-Z]{5,}\b")
            .Cast<Match>()
            .Select(m => m.Value);

        var stopWords = new HashSet<string> 
        { 
            "project", "requirements", "system", "solution", 
            "cloud", "customer", "service", "document", 
            "management", "application" 
        };

        return words
            .Where(w => !stopWords.Contains(w))
            .GroupBy(w => w)
            .OrderByDescending(g => g.Count())
            .Take(topN)
            .Select(g => g.Key)
            .ToList();
    }
}
```

## Files to Modify

### 1. `src/DocToolkit/Services/SemanticIndexService.cs`

**Changes**:
- Remove `ChunkText` method (moved to `TextChunkingEngine`)
- Add dependency on `TextChunkingEngine`
- Add XML documentation
- Ensure Manager only orchestrates

```csharp
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

    // REMOVED: ChunkText method (moved to TextChunkingEngine)

    public void Dispose()
    {
        _embedding?.Dispose();
    }
}
```

### 2. `src/DocToolkit/Services/SemanticSearchService.cs`

**Changes**:
- Remove `CosineSimilarity` method (moved to `SimilarityEngine`)
- Add dependency on `SimilarityEngine`
- Add XML documentation
- Use `SimilarityEngine.FindTopSimilar` instead of inline calculation

```csharp
using DocToolkit.Models;
using DocToolkit.Services.Engines;
using SearchResult = DocToolkit.Models.SearchResult;

namespace DocToolkit.Services;

/// <summary>
/// Manager for semantic search workflow.
/// Encapsulates workflow volatility: search orchestration could change.
/// </summary>
/// <remarks>
/// Component Type: Manager (Workflow Volatility)
/// Volatility: Search workflow and orchestration
/// Pattern: Orchestrates Engines and Accessors - knows "when", not "how"
/// </remarks>
public class SemanticSearchService : IDisposable
{
    private readonly EmbeddingService _embedding;
    private readonly VectorStorageService _storage;
    private readonly SimilarityEngine _similarityEngine;

    public SemanticSearchService()
    {
        _embedding = new EmbeddingService();
        _storage = new VectorStorageService();
        _similarityEngine = new SimilarityEngine();
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
    /// Orchestrates: VectorStorageService (Accessor), EmbeddingService (Engine), 
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

    // REMOVED: CosineSimilarity method (moved to SimilarityEngine)

    public void Dispose()
    {
        _embedding?.Dispose();
    }
}
```

### 3. `src/DocToolkit/Services/KnowledgeGraphService.cs`

**Changes**:
- Remove `ExtractEntities` and `ExtractTopics` methods (moved to `EntityExtractionEngine`)
- Add dependency on `EntityExtractionEngine`
- Add XML documentation
- Ensure Manager only orchestrates

```csharp
using System.Text;
using System.Text.Json;
using DocToolkit.Models;
using DocToolkit.Services.Engines;

namespace DocToolkit.Services;

/// <summary>
/// Manager for knowledge graph generation workflow.
/// Encapsulates workflow volatility: graph generation orchestration could change.
/// </summary>
/// <remarks>
/// Component Type: Manager (Workflow Volatility)
/// Volatility: Graph generation workflow and orchestration
/// Pattern: Orchestrates Engines and Accessors - knows "when", not "how"
/// </remarks>
public class KnowledgeGraphService
{
    private readonly DocumentExtractionService _extractor;
    private readonly EntityExtractionEngine _extractionEngine;

    public KnowledgeGraphService()
    {
        _extractor = new DocumentExtractionService();
        _extractionEngine = new EntityExtractionEngine();
    }

    /// <summary>
    /// Builds a knowledge graph from source files.
    /// </summary>
    /// <param name="sourcePath">Source directory path</param>
    /// <param name="outputPath">Output directory path</param>
    /// <param name="progressCallback">Progress callback (0-100)</param>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// Service Boundary: Called by GraphCommand (Client)
    /// Orchestrates: DocumentExtractionService (Engine), EntityExtractionEngine (Engine)
    /// Authentication: None (local CLI tool)
    /// Authorization: None (local CLI tool)
    /// Transaction: None (file-based operations)
    /// </remarks>
    public bool BuildGraph(string sourcePath, string outputPath, Action<double>? progressCallback = null)
    {
        if (!Directory.Exists(sourcePath))
        {
            return false;
        }

        var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();
        var totalFiles = files.Count;
        var processedFiles = 0;

        var nodes = new GraphNodes();
        var edges = new List<GraphEdge>();
        var fileTexts = new Dictionary<string, string>();
        var entityGlobalCounts = new Dictionary<string, int>();
        var topicGlobalCounts = new Dictionary<string, int>();

        // Process each file
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

                var fileId = $"file:{file}";
                fileTexts[fileId] = text;

                nodes.File.Add(new GraphNode
                {
                    Id = fileId,
                    Type = "file",
                    Name = Path.GetFileName(file),
                    Path = file
                });

                // Orchestrate: Extract entities and topics (Engine)
                var entities = _extractionEngine.ExtractEntities(text);
                var topics = _extractionEngine.ExtractTopics(text, 10);

                // Count entities and topics
                foreach (var entity in entities)
                {
                    entityGlobalCounts.TryGetValue(entity, out var count);
                    entityGlobalCounts[entity] = count + 1;
                }

                foreach (var topic in topics)
                {
                    topicGlobalCounts.TryGetValue(topic, out var count);
                    topicGlobalCounts[topic] = count + 1;
                }

                // Create entity nodes and edges
                foreach (var entity in entities.Distinct())
                {
                    var entityId = $"entity:{entity}";
                    if (!nodes.Entity.Any(e => e.Id == entityId))
                    {
                        nodes.Entity.Add(new GraphNode
                        {
                            Id = entityId,
                            Type = "entity",
                            Name = entity
                        });
                    }

                    edges.Add(new GraphEdge
                    {
                        Type = "FILE_CONTAINS_ENTITY",
                        From = fileId,
                        To = entityId,
                        Weight = entities.Count(e => e == entity)
                    });
                }

                // Create topic nodes and edges
                foreach (var topic in topics.Distinct())
                {
                    var topicId = $"topic:{topic}";
                    if (!nodes.Topic.Any(t => t.Id == topicId))
                    {
                        nodes.Topic.Add(new GraphNode
                        {
                            Id = topicId,
                            Type = "topic",
                            Name = topic
                        });
                    }

                    edges.Add(new GraphEdge
                    {
                        Type = "FILE_CONTAINS_TOPIC",
                        From = fileId,
                        To = topicId,
                        Weight = topics.Count(t => t == topic)
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

        // Build entity-entity and entity-topic co-occurrence edges
        foreach (var (fileId, text) in fileTexts)
        {
            var entities = _extractionEngine.ExtractEntities(text).Distinct().ToList();
            var topics = _extractionEngine.ExtractTopics(text, 10).Distinct().ToList();

            // Entity-entity relationships
            for (int i = 0; i < entities.Count; i++)
            {
                for (int j = i + 1; j < entities.Count; j++)
                {
                    edges.Add(new GraphEdge
                    {
                        Type = "ENTITY_RELATED_TO_ENTITY",
                        From = $"entity:{entities[i]}",
                        To = $"entity:{entities[j]}",
                        File = fileId
                    });
                }
            }

            // Entity-topic relationships
            foreach (var entity in entities)
            {
                foreach (var topic in topics)
                {
                    edges.Add(new GraphEdge
                    {
                        Type = "ENTITY_RELATED_TO_TOPIC",
                        From = $"entity:{entity}",
                        To = $"topic:{topic}",
                        File = fileId
                    });
                }
            }
        }

        // Create graph data
        var graph = new GraphData
        {
            Nodes = nodes,
            Edges = edges,
            Stats = new GraphStats
            {
                NumFiles = nodes.File.Count,
                NumEntities = nodes.Entity.Count,
                NumTopics = nodes.Topic.Count
            }
        };

        // Save outputs
        Directory.CreateDirectory(outputPath);
        SaveJson(graph, outputPath);
        SaveGraphviz(graph, outputPath);
        SaveMarkdown(graph, entityGlobalCounts, topicGlobalCounts, outputPath);

        return true;
    }

    // REMOVED: ExtractEntities and ExtractTopics methods (moved to EntityExtractionEngine)

    // ... rest of SaveJson, SaveGraphviz, SaveMarkdown methods remain unchanged ...
}
```

### 4. `src/DocToolkit/Services/EmbeddingService.cs`

**Changes**:
- Add XML documentation
- Note that it's an Engine (algorithm volatility)
- Document that it's pure (accepts text, returns embedding)

```csharp
// Add at the top of the class:

/// <summary>
/// Engine for semantic embedding generation using ONNX Runtime.
/// Encapsulates algorithm volatility: embedding model could change (different ONNX models, tokenization).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Embedding algorithm and model
/// Pattern: Pure function - accepts text, returns embedding vector (no I/O except model loading)
/// Service Boundary: Called by Managers (SemanticIndexService, SemanticSearchService)
/// </remarks>
public class EmbeddingService : IDisposable
{
    // ... existing code ...

    /// <summary>
    /// Generates a semantic embedding vector for the given text.
    /// </summary>
    /// <param name="text">Text to generate embedding for</param>
    /// <returns>Embedding vector (384 dimensions for all-MiniLM-L6-v2)</returns>
    /// <remarks>
    /// Pure function: Accepts text, returns embedding. No I/O during execution.
    /// Model loading is lazy and cached.
    /// </remarks>
    public float[] GenerateEmbedding(string text)
    {
        // ... existing implementation ...
    }
}
```

### 5. `src/DocToolkit/Services/DocumentExtractionService.cs`

**Changes**:
- Add XML documentation
- Note that it's an Engine (algorithm volatility)
- Document that it's pure (accepts file path, returns text)

```csharp
// Add at the top of the class:

/// <summary>
/// Engine for text extraction from various document formats.
/// Encapsulates algorithm volatility: extraction logic could change (new formats, improved OCR).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Document format support and extraction algorithms
/// Pattern: Pure function - accepts file path, returns extracted text
/// Service Boundary: Called by Managers (SemanticIndexService, KnowledgeGraphService, SummarizeService)
/// </remarks>
public class DocumentExtractionService
{
    /// <summary>
    /// Extracts text from a document file.
    /// </summary>
    /// <param name="filePath">Path to the document file</param>
    /// <returns>Extracted text, or empty string if extraction fails</returns>
    /// <remarks>
    /// Pure function: Accepts file path, returns text. File I/O is encapsulated.
    /// Supports: TXT, MD, CSV, JSON, DOCX, PPTX, PDF, Images (OCR not yet implemented)
    /// </remarks>
    public string ExtractText(string filePath)
    {
        // ... existing implementation ...
    }
}
```

### 6. `src/DocToolkit/Services/VectorStorageService.cs`

**Changes**:
- Add XML documentation
- Note that it's an Accessor (storage volatility)

```csharp
// Add at the top of the class:

/// <summary>
/// Accessor for vector storage operations.
/// Encapsulates storage volatility: storage format could change (file-based → database, binary → JSON).
/// </summary>
/// <remarks>
/// Component Type: Accessor (Storage Volatility)
/// Volatility: Storage technology and format
/// Pattern: Dumb CRUD operations - no business logic
/// Service Boundary: Called by Managers (SemanticIndexService, SemanticSearchService)
/// </remarks>
public class VectorStorageService
{
    // Add XML docs to all public methods
}
```

### 7. `src/DocToolkit/Services/SummarizeService.cs`

**Changes**:
- Add XML documentation
- Note that it's a Manager (workflow volatility)
- Consider extracting summarization logic to an Engine (future enhancement)

```csharp
// Add at the top of the class:

/// <summary>
/// Manager for document summarization workflow.
/// Encapsulates workflow volatility: summarization orchestration could change.
/// </summary>
/// <remarks>
/// Component Type: Manager (Workflow Volatility)
/// Volatility: Summarization workflow and orchestration
/// Pattern: Orchestrates Engines and Accessors
/// Service Boundary: Called by SummarizeCommand (Client)
/// </remarks>
public class SummarizeService
{
    // Add XML docs to all public methods
}
```

### 8. `src/DocToolkit/Services/TemplateService.cs`

**Changes**:
- Add XML documentation
- Note that it's an Accessor (storage volatility)

```csharp
// Add at the top of the class:

/// <summary>
/// Accessor for document template operations.
/// Encapsulates storage volatility: template location could change (local → cloud, file → database).
/// </summary>
/// <remarks>
/// Component Type: Accessor (Storage Volatility)
/// Volatility: Template storage location and format
/// Pattern: Dumb CRUD operations - no business logic
/// Service Boundary: Called by GenerateCommand (Client)
/// </remarks>
public class TemplateService
{
    // Add XML docs to all public methods
}
```

### 9. `src/DocToolkit/Services/ProjectService.cs`

**Changes**:
- Add XML documentation
- Note that it's an Accessor (storage volatility)

```csharp
// Add at the top of the class:

/// <summary>
/// Accessor for project workspace operations.
/// Encapsulates storage volatility: file system operations could change (local → cloud storage).
/// </summary>
/// <remarks>
/// Component Type: Accessor (Storage Volatility)
/// Volatility: File system and storage technology
/// Pattern: Dumb CRUD operations - no business logic
/// Service Boundary: Called by InitCommand (Client)
/// </remarks>
public class ProjectService
{
    // Add XML docs to all public methods
}
```

## Summary of Changes

### Files Created (3)
1. `src/DocToolkit/Services/Engines/TextChunkingEngine.cs` - Text chunking algorithm
2. `src/DocToolkit/Services/Engines/SimilarityEngine.cs` - Similarity calculation algorithm
3. `src/DocToolkit/Services/Engines/EntityExtractionEngine.cs` - Entity/topic extraction algorithm

### Files Modified (9)
1. `src/DocToolkit/Services/SemanticIndexService.cs` - Remove ChunkText, add TextChunkingEngine, add XML docs
2. `src/DocToolkit/Services/SemanticSearchService.cs` - Remove CosineSimilarity, add SimilarityEngine, add XML docs
3. `src/DocToolkit/Services/KnowledgeGraphService.cs` - Remove ExtractEntities/ExtractTopics, add EntityExtractionEngine, add XML docs
4. `src/DocToolkit/Services/EmbeddingService.cs` - Add XML documentation
5. `src/DocToolkit/Services/DocumentExtractionService.cs` - Add XML documentation
6. `src/DocToolkit/Services/VectorStorageService.cs` - Add XML documentation
7. `src/DocToolkit/Services/SummarizeService.cs` - Add XML documentation
8. `src/DocToolkit/Services/TemplateService.cs` - Add XML documentation
9. `src/DocToolkit/Services/ProjectService.cs` - Add XML documentation

### Benefits
1. ✅ Engines are now pure (no business logic in Managers)
2. ✅ Business logic moved to Engines (chunking, similarity, extraction)
3. ✅ Managers only orchestrate (follow IDesign Method™)
4. ✅ XML documentation added with volatility notes
5. ✅ Component taxonomy clearly documented
6. ✅ Better testability (Engines can be tested without I/O)

### Breaking Changes
- None - all changes are internal refactoring
- Public APIs remain the same
- Backward compatible

### Testing Required
1. Test `TextChunkingEngine.ChunkText` with various inputs
2. Test `SimilarityEngine.CosineSimilarity` with edge cases
3. Test `EntityExtractionEngine.ExtractEntities` and `ExtractTopics`
4. Integration tests for Managers to ensure orchestration works
5. Verify all existing functionality still works

## Implementation Order

1. Create Engine classes (TextChunkingEngine, SimilarityEngine, EntityExtractionEngine)
2. Update SemanticIndexService (remove ChunkText, add TextChunkingEngine)
3. Update SemanticSearchService (remove CosineSimilarity, add SimilarityEngine)
4. Update KnowledgeGraphService (remove ExtractEntities/ExtractTopics, add EntityExtractionEngine)
5. Add XML documentation to all services
6. Run tests to verify functionality
7. Update documentation if needed
