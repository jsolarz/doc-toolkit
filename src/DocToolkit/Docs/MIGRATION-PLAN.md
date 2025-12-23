# C# Native Migration Plan

## Overview

This document outlines the plan to migrate the Documentation Toolkit from Python-dependent scripts to a fully C# native implementation with minimal external dependencies.

## Current State Analysis

### Python Dependencies Identified

1. **Semantic Indexing** (`semantic-index.ps1` → Python)
   - Uses `sentence-transformers` (all-MiniLM-L6-v2 model)
   - Uses `numpy` for vector storage (.npy files)
   - Uses `python-docx`, `python-pptx`, `pypdf` for text extraction
   - Uses `pytesseract` for OCR

2. **Semantic Search** (`semantic-search.ps1` → Python)
   - Uses `sentence-transformers` for query embedding
   - Uses `sklearn` for cosine similarity
   - Uses `numpy` for vector loading

3. **Knowledge Graph** (`build_kg.py`)
   - Uses `python-docx`, `python-pptx`, `pypdf` for text extraction
   - Pure Python logic (easily portable)

## Migration Strategy

### Option 1: ONNX Runtime (Recommended - Balanced)
- **Pros**: High-quality embeddings, single dependency, cross-platform
- **Cons**: Requires ONNX model file (~90MB), one-time download
- **Dependencies**: `Microsoft.ML.OnnxRuntime` (single package)

### Option 2: TF-IDF/BM25 (Minimal Dependencies)
- **Pros**: Zero ML dependencies, fast, simple
- **Cons**: Less accurate than embeddings, keyword-based
- **Dependencies**: None (pure C#)

### Option 3: Hybrid Approach
- Use TF-IDF for indexing/search
- Keep ONNX option available for advanced users
- Default to TF-IDF, allow switching

**Recommendation**: Option 1 (ONNX Runtime) for best quality with minimal dependencies.

## Required Changes

### 1. Update Project Dependencies

**File**: `src/DocToolkit/DocToolkit.csproj`

**Add Packages**:
```xml
<PackageReference Include="Microsoft.ML.OnnxRuntime" Version="1.19.0" />
<PackageReference Include="DocumentFormat.OpenXml" Version="3.0.0" />
<PackageReference Include="PdfSharpCore" Version="6.0.0" />
```

**Remove**: No packages removed (keep Spectre.Console)

**Total New Dependencies**: 3 packages

### 2. Create Document Extraction Service

**New File**: `src/DocToolkit/Services/DocumentExtractionService.cs`

**Responsibilities**:
- Extract text from PDF, DOCX, PPTX, TXT, MD, CSV, JSON
- Handle errors gracefully
- Support OCR for images (optional, requires Tesseract executable)

**Implementation**:
- PDF: Use PdfSharpCore (lightweight, pure C#)
- DOCX/PPTX: Use DocumentFormat.OpenXml (Microsoft official, no dependencies)
- Images: System.Drawing + optional Tesseract wrapper

### 3. Create Embedding Service

**New File**: `src/DocToolkit/Services/EmbeddingService.cs`

**Responsibilities**:
- Load ONNX model (all-MiniLM-L6-v2)
- Generate embeddings for text chunks
- Handle model download/caching

**Implementation**:
- Use ONNX Runtime for inference
- Model file: Download on first use or bundle with app
- Embedding dimension: 384 (all-MiniLM-L6-v2)

### 4. Create Vector Storage Service

**New File**: `src/DocToolkit/Services/VectorStorageService.cs`

**Responsibilities**:
- Store vectors in binary format (replace .npy)
- Store metadata in JSON
- Load vectors for search

**Storage Format**:
- `vectors.bin`: Binary format (float[] array)
- `index.json`: Metadata (file paths, chunks, etc.)

### 5. Create Semantic Index Service

**New File**: `src/DocToolkit/Services/SemanticIndexService.cs`

**Replace**: `PythonService.BuildSemanticIndex()`

**Responsibilities**:
- Extract text from source files
- Chunk text with overlap
- Generate embeddings
- Store vectors and metadata

**Implementation**:
- Use `DocumentExtractionService` for text extraction
- Use `EmbeddingService` for embeddings
- Use `VectorStorageService` for persistence

### 6. Create Semantic Search Service

**New File**: `src/DocToolkit/Services/SemanticSearchService.cs`

**Replace**: `PythonService.SearchSemanticIndex()`

**Responsibilities**:
- Load vectors from storage
- Generate query embedding
- Calculate cosine similarity
- Return top-K results

**Implementation**:
- Pure C# cosine similarity calculation
- No external ML libraries needed for similarity

### 7. Create Knowledge Graph Service

**New File**: `src/DocToolkit/Services/KnowledgeGraphService.cs`

**Replace**: `PythonService.BuildKnowledgeGraph()`

**Responsibilities**:
- Extract text from files
- Extract entities and topics
- Build graph structure
- Generate JSON, Graphviz, and Markdown outputs

**Implementation**:
- Use `DocumentExtractionService` for text extraction
- Use existing entity/topic extraction logic from `SummarizeService`
- Pure C# graph building

### 8. Update Commands

**Files to Modify**:
- `src/DocToolkit/Commands/IndexCommand.cs`
- `src/DocToolkit/Commands/SearchCommand.cs`
- `src/DocToolkit/Commands/GraphCommand.cs`
- `src/DocToolkit/Commands/ValidateCommand.cs`

**Changes**:
- Remove Python dependency checks
- Use new C# services instead of `PythonService`
- Update error messages
- Remove script path finding logic

### 9. Remove/Deprecate PythonService

**File**: `src/DocToolkit/Services/PythonService.cs`

**Action**: 
- Mark as obsolete
- Keep for backward compatibility initially
- Remove in future version

### 10. Update Validation Service

**File**: `src/DocToolkit/Services/ValidationService.cs`

**Changes**:
- Remove Python version checks
- Remove Python package checks
- Keep external tool checks (Poppler, Tesseract - optional)
- Add ONNX model availability check

### 11. Update README

**Files**: `README.md`, `README-CLI.md`

**Changes**:
- Remove Python 3.10+ requirement
- Update installation instructions
- Remove Python dependencies section
- Add ONNX model information
- Update troubleshooting

## Implementation Details

### Embedding Model

**Model**: all-MiniLM-L6-v2
- **Size**: ~90MB
- **Dimensions**: 384
- **Format**: ONNX
- **Source**: Hugging Face (convert from PyTorch)
- **Download**: First run or bundled

**Alternative**: Use pre-converted ONNX model from:
- Hugging Face ONNX model hub
- Or convert using `optimum` Python tool (one-time)

### Vector Storage Format

**Current**: NumPy .npy format
**New**: Custom binary format

```csharp
// vectors.bin structure:
// [int32: count][float32[]: vectors flattened]
// Each vector is 384 floats (1536 bytes)

// index.json structure:
// {
//   "entries": [
//     { "file": "...", "path": "...", "chunk": "...", "index": 0 }
//   ]
// }
```

### Cosine Similarity Implementation

```csharp
public static double CosineSimilarity(float[] vectorA, float[] vectorB)
{
    if (vectorA.Length != vectorB.Length)
        throw new ArgumentException("Vectors must have same length");
    
    double dotProduct = 0.0;
    double normA = 0.0;
    double normB = 0.0;
    
    for (int i = 0; i < vectorA.Length; i++)
    {
        dotProduct += vectorA[i] * vectorB[i];
        normA += vectorA[i] * vectorA[i];
        normB += vectorB[i] * vectorB[i];
    }
    
    return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
}
```

### Text Chunking

```csharp
public static List<string> ChunkText(string text, int chunkSize, int chunkOverlap)
{
    var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, 
        StringSplitOptions.RemoveEmptyEntries);
    var chunks = new List<string>();
    int i = 0;
    
    while (i < words.Length)
    {
        var chunk = string.Join(" ", words.Skip(i).Take(chunkSize));
        chunks.Add(chunk);
        i += (chunkSize - chunkOverlap);
    }
    
    return chunks;
}
```

## File Structure After Migration

```
src/DocToolkit/
├── Commands/
│   ├── IndexCommand.cs          (updated - no Python checks)
│   ├── SearchCommand.cs         (updated - use C# service)
│   ├── GraphCommand.cs          (updated - use C# service)
│   └── ValidateCommand.cs       (updated - no Python checks)
├── Services/
│   ├── DocumentExtractionService.cs    (new)
│   ├── EmbeddingService.cs             (new)
│   ├── VectorStorageService.cs         (new)
│   ├── SemanticIndexService.cs         (new)
│   ├── SemanticSearchService.cs        (new)
│   ├── KnowledgeGraphService.cs        (new)
│   ├── PythonService.cs                 (deprecated)
│   ├── ProjectService.cs                (unchanged)
│   ├── SummarizeService.cs              (unchanged)
│   ├── TemplateService.cs               (unchanged)
│   └── ValidationService.cs             (updated)
└── Models/
    └── all-MiniLM-L6-v2.onnx           (model file, ~90MB)
```

## Migration Steps

### Phase 1: Document Extraction (Low Risk)
1. Create `DocumentExtractionService`
2. Add NuGet packages (DocumentFormat.OpenXml, PdfSharpCore)
3. Test with sample files
4. Update `SummarizeService` to use new extraction

### Phase 2: Knowledge Graph (Low Risk)
1. Create `KnowledgeGraphService`
2. Port Python logic to C#
3. Test graph generation
4. Update `GraphCommand`

### Phase 3: Embedding Infrastructure (Medium Risk)
1. Create `EmbeddingService`
2. Download/obtain ONNX model
3. Test embedding generation
4. Create `VectorStorageService`

### Phase 4: Semantic Indexing (Medium Risk)
1. Create `SemanticIndexService`
2. Integrate all components
3. Test index generation
4. Update `IndexCommand`

### Phase 5: Semantic Search (Low Risk)
1. Create `SemanticSearchService`
2. Implement cosine similarity
3. Test search functionality
4. Update `SearchCommand`

### Phase 6: Cleanup (Low Risk)
1. Update `ValidationService`
2. Remove Python checks from commands
3. Update documentation
4. Mark `PythonService` as obsolete

## Dependencies Summary

### Current Dependencies
- Spectre.Console (0.49.1)
- Spectre.Console.Cli (0.49.1)
- **Python 3.10+** (external)
- **sentence-transformers** (Python)
- **numpy, sklearn** (Python)
- **python-docx, python-pptx, pypdf** (Python)

### New Dependencies
- Spectre.Console (0.49.1) - *kept*
- Spectre.Console.Cli (0.49.1) - *kept*
- Microsoft.ML.OnnxRuntime (1.19.0) - *new*
- DocumentFormat.OpenXml (3.0.0) - *new*
- PdfSharpCore (6.0.0) - *new*

### Removed Dependencies
- Python 3.10+ - *removed*
- All Python packages - *removed*

**Net Change**: 3 new NuGet packages, Python completely removed

## Testing Strategy

### Unit Tests
- Document extraction for each format
- Text chunking logic
- Cosine similarity calculation
- Entity/topic extraction
- Vector storage serialization

### Integration Tests
- End-to-end indexing workflow
- End-to-end search workflow
- Knowledge graph generation
- Error handling for missing files

### Compatibility Tests
- Verify existing indexes can be migrated (if needed)
- Test with various file formats
- Test with large documents

## Performance Considerations

### Embedding Generation
- **Current**: Python sentence-transformers (~100ms per chunk)
- **Expected**: ONNX Runtime (~50-80ms per chunk)
- **Improvement**: Faster due to optimized ONNX runtime

### Search Performance
- **Current**: Python sklearn (~10ms for 1000 vectors)
- **Expected**: Pure C# (~5-8ms for 1000 vectors)
- **Improvement**: Faster due to no Python overhead

### Memory Usage
- **Current**: Python process overhead (~100-200MB)
- **Expected**: Direct C# execution (~50-100MB)
- **Improvement**: Lower memory footprint

## Backward Compatibility

### Index Format Migration
- Old format: `vectors.npy` + `index.json`
- New format: `vectors.bin` + `index.json`
- **Action**: Provide migration utility or auto-detect format

### Command Interface
- All commands remain the same
- No breaking changes to CLI interface
- Same output formats

## Risk Assessment

### Low Risk
- Document extraction (well-established libraries)
- Knowledge graph (pure logic port)
- Semantic search (simple math)

### Medium Risk
- Embedding service (ONNX model integration)
- Vector storage (format migration)

### Mitigation
- Thorough testing at each phase
- Keep Python scripts as fallback initially
- Provide migration tools for existing indexes

## Timeline Estimate

- **Phase 1**: 2-3 hours (Document Extraction)
- **Phase 2**: 2-3 hours (Knowledge Graph)
- **Phase 3**: 4-6 hours (Embedding Infrastructure)
- **Phase 4**: 3-4 hours (Semantic Indexing)
- **Phase 5**: 2-3 hours (Semantic Search)
- **Phase 6**: 1-2 hours (Cleanup)

**Total**: 14-21 hours of development

## Success Criteria

1. ✅ No Python dependency required
2. ✅ All features work in C# native
3. ✅ Performance equal or better than Python version
4. ✅ Same CLI interface maintained
5. ✅ Comprehensive error handling
6. ✅ Updated documentation

## Alternative: TF-IDF Approach

If ONNX model is too large or complex:

### TF-IDF Implementation
- **Dependencies**: None (pure C#)
- **Accuracy**: Lower than embeddings but acceptable for many use cases
- **Performance**: Faster than embeddings
- **Implementation**: Use `System.Linq` for term frequency calculations

### Hybrid Approach
- Default to TF-IDF
- Allow ONNX embeddings as optional enhancement
- User chooses based on needs

## Notes

- ONNX model can be downloaded on first use or bundled
- Consider model size in distribution strategy
- May need to handle model updates
- OCR remains optional (requires Tesseract executable, not Python)
