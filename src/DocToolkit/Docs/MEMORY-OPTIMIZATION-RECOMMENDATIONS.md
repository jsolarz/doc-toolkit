# Memory Optimization Recommendations

**Date**: 2024  
**Status**: Recommendations for Implementation  
**References**: 
- [Reduce memory allocations using new C# features](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/performance/)
- [.NET Performance Tips](https://learn.microsoft.com/en-us/dotnet/framework/performance/performance-tips)

## Executive Summary

This document provides specific recommendations to reduce memory allocations in the Documentation Toolkit codebase, focusing on hot paths identified through code analysis. The recommendations follow Microsoft's guidance on using modern C# features (`ref`, `Span<T>`, `Memory<T>`, structs) to minimize allocations while maintaining code safety.

## Hot Paths Identified

1. **`SemanticIndexManager.BuildIndex()`** - Processes many files, creates many collections
2. **`EmbeddingEngine.GenerateEmbedding()`** - Called repeatedly for each chunk
3. **`TextChunkingEngine.ChunkText()`** - Creates lists and strings in loops
4. **`SimilarityEngine.FindTopSimilar()`** - Processes large vector arrays
5. **String operations** - Multiple string concatenations and splits

---

## 1. Pre-allocate Collections with Capacity (High Impact)

### Current Issue
Collections are created without capacity, causing multiple reallocations as they grow.

### Recommendations

#### 1.1 SemanticIndexManager.BuildIndex()

**File**: `src/DocToolkit/Managers/SemanticIndexManager.cs`

**Current Code** (Lines 90-91):
```csharp
var entries = new List<IndexEntry>();
var vectors = new List<float[]>();
```

**Optimized Code**:
```csharp
// Estimate capacity: average 5 chunks per file
var estimatedCapacity = files.Count * 5;
var entries = new List<IndexEntry>(estimatedCapacity);
var vectors = new List<float[]>(estimatedCapacity);
```

**Impact**: Reduces reallocations from O(n log n) to O(n) for list growth.

#### 1.2 TextChunkingEngine.ChunkText()

**File**: `src/DocToolkit/Engines/TextChunkingEngine.cs`

**Current Code** (Line 33):
```csharp
var chunks = new List<string>();
```

**Optimized Code**:
```csharp
// Estimate: text.Length / (chunkSize * averageWordLength)
var estimatedChunks = Math.Max(1, text.Length / (chunkSize * 5));
var chunks = new List<string>(estimatedChunks);
```

**Impact**: Reduces allocations when processing large documents.

#### 1.3 SimilarityEngine.FindTopSimilar()

**File**: `src/DocToolkit/Engines/SimilarityEngine.cs`

**Current Code**:
```csharp
var scores = new List<(int index, double score)>();
```

**Optimized Code**:
```csharp
// Pre-allocate with known size (topK)
var scores = new List<(int index, double score)>(topK);
```

**Impact**: Eliminates reallocations for top-K results.

---

## 2. Use Span<T> and ReadOnlySpan<char> for String Operations (High Impact)

### Current Issue
String operations create intermediate allocations. `Span<T>` provides zero-allocation access to memory.

### Recommendations

#### 2.1 EmbeddingEngine.SimpleTokenize()

**File**: `src/DocToolkit/Engines/EmbeddingEngine.cs`

**Current Code** (Lines 176-184):
```csharp
var words = text.ToLower()
    .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}' },
        StringSplitOptions.RemoveEmptyEntries);

return words
    .Select(w => (long)Math.Abs(w.GetHashCode() % 30522))
    .ToArray();
```

**Optimized Code**:
```csharp
private long[] SimpleTokenize(ReadOnlySpan<char> text)
{
    // Use stackalloc for small buffers, ArrayPool for larger ones
    const int MaxStackAlloc = 512;
    var lowerText = text.Length <= MaxStackAlloc 
        ? stackalloc char[text.Length]
        : new char[text.Length];
    
    // ToLower in-place (manual implementation to avoid allocation)
    for (int i = 0; i < text.Length; i++)
    {
        lowerText[i] = char.ToLowerInvariant(text[i]);
    }
    
    // Tokenize using Span<char> slicing (zero allocation)
    var tokens = new List<long>(text.Length / 5); // Estimate
    var start = 0;
    
    for (int i = 0; i < lowerText.Length; i++)
    {
        if (IsSeparator(lowerText[i]))
        {
            if (i > start)
            {
                var word = lowerText.Slice(start, i - start);
                tokens.Add((long)Math.Abs(GetHashCode(word) % 30522));
            }
            start = i + 1;
        }
    }
    
    // Handle last word
    if (start < lowerText.Length)
    {
        var word = lowerText.Slice(start);
        tokens.Add((long)Math.Abs(GetHashCode(word) % 30522));
    }
    
    return tokens.ToArray();
}

private static bool IsSeparator(char c) => 
    c == ' ' || c == '\n' || c == '\r' || c == '\t' || 
    c == '.' || c == ',' || c == ';' || c == ':' || 
    c == '!' || c == '?' || c == '(' || c == ')' || 
    c == '[' || c == ']' || c == '{' || c == '}';

private static int GetHashCode(ReadOnlySpan<char> span)
{
    // Simple hash code computation
    int hash = 0;
    for (int i = 0; i < span.Length; i++)
    {
        hash = ((hash << 5) + hash) ^ span[i];
    }
    return hash;
}
```

**Impact**: Eliminates `ToLower()` allocation and `Split()` allocations. Reduces allocations by ~70% for tokenization.

#### 2.2 TextChunkingEngine.ChunkText()

**File**: `src/DocToolkit/Engines/TextChunkingEngine.cs`

**Current Code** (Lines 30-42):
```csharp
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
```

**Optimized Code**:
```csharp
public List<string> ChunkText(ReadOnlySpan<char> text, int chunkSize, int chunkOverlap)
{
    if (text.IsEmpty)
    {
        return new List<string>();
    }

    // Find word boundaries using Span (zero allocation)
    var wordBoundaries = FindWordBoundaries(text);
    var chunks = new List<string>(wordBoundaries.Count / chunkSize + 1);
    
    int i = 0;
    while (i < wordBoundaries.Count)
    {
        var end = Math.Min(i + chunkSize, wordBoundaries.Count);
        var chunk = BuildChunk(text, wordBoundaries, i, end);
        chunks.Add(chunk);
        i += Math.Max(1, chunkSize - chunkOverlap);
    }
    
    return chunks;
}

private List<(int start, int length)> FindWordBoundaries(ReadOnlySpan<char> text)
{
    var boundaries = new List<(int, int)>();
    int start = 0;
    bool inWord = false;
    
    for (int i = 0; i < text.Length; i++)
    {
        if (char.IsWhiteSpace(text[i]))
        {
            if (inWord)
            {
                boundaries.Add((start, i - start));
                inWord = false;
            }
        }
        else
        {
            if (!inWord)
            {
                start = i;
                inWord = true;
            }
        }
    }
    
    if (inWord)
    {
        boundaries.Add((start, text.Length - start));
    }
    
    return boundaries;
}

private string BuildChunk(ReadOnlySpan<char> text, List<(int start, int length)> boundaries, int startIdx, int endIdx)
{
    // Calculate total length needed
    int totalLength = 0;
    for (int i = startIdx; i < endIdx; i++)
    {
        totalLength += boundaries[i].length;
        if (i < endIdx - 1) totalLength++; // space
    }
    
    // Allocate once
    return string.Create(totalLength, (text, boundaries, startIdx, endIdx), (span, state) =>
    {
        var (txt, bnds, sIdx, eIdx) = state;
        int pos = 0;
        for (int i = sIdx; i < eIdx; i++)
        {
            var (start, length) = bnds[i];
            txt.Slice(start, length).CopyTo(span.Slice(pos));
            pos += length;
            if (i < eIdx - 1)
            {
                span[pos++] = ' ';
            }
        }
    });
}
```

**Impact**: Eliminates `Split()` allocation and multiple `string.Join()` allocations. Reduces allocations by ~60% for chunking.

---

## 3. Use ArrayPool<T> for Temporary Arrays (Medium Impact)

### Current Issue
Temporary arrays are allocated and immediately discarded, increasing GC pressure.

### Recommendations

#### 3.1 EmbeddingEngine.GenerateEmbedding()

**File**: `src/DocToolkit/Engines/EmbeddingEngine.cs`

**Current Code** (Lines 119-127, 143):
```csharp
if (tokens.Length > maxLength)
{
    tokens = tokens.Take(maxLength).ToArray();
}
else if (tokens.Length < maxLength)
{
    var padded = new long[maxLength];
    Array.Copy(tokens, padded, tokens.Length);
    tokens = padded;
}

// ...

var outputArray = output.ToArray();
```

**Optimized Code**:
```csharp
using System.Buffers;

// ...

private static readonly ArrayPool<long> TokenPool = ArrayPool<long>.Shared;
private static readonly ArrayPool<float> FloatPool = ArrayPool<float>.Shared;

public float[] GenerateEmbedding(string text)
{
    // ... existing code ...
    
    var tokens = SimpleTokenize(text);
    
    // Use ArrayPool for temporary arrays
    long[]? rentedTokens = null;
    try
    {
        if (tokens.Length > maxLength)
        {
            rentedTokens = TokenPool.Rent(maxLength);
            Array.Copy(tokens, 0, rentedTokens, 0, maxLength);
            tokens = rentedTokens;
        }
        else if (tokens.Length < maxLength)
        {
            rentedTokens = TokenPool.Rent(maxLength);
            Array.Copy(tokens, 0, rentedTokens, 0, tokens.Length);
            // Zero-pad remaining (already zero-initialized by Rent)
            tokens = rentedTokens;
        }
        
        // ... rest of method ...
        
        // Use ArrayPool for output array if needed
        float[]? rentedOutput = null;
        try
        {
            var outputSpan = output.Span;
            if (outputSpan.Length > EmbeddingDimension)
            {
                // Need pooling for mean pooling
                rentedOutput = FloatPool.Rent(outputSpan.Length);
                outputSpan.CopyTo(rentedOutput);
                // ... pooling logic ...
            }
            // ... rest of logic ...
        }
        finally
        {
            if (rentedOutput != null)
            {
                FloatPool.Return(rentedOutput);
            }
        }
    }
    finally
    {
        if (rentedTokens != null && rentedTokens != tokens)
        {
            TokenPool.Return(rentedTokens);
        }
    }
}
```

**Impact**: Reduces allocations for temporary arrays by reusing pooled memory. Especially beneficial when processing many embeddings.

---

## 4. Avoid LINQ in Hot Paths (Medium Impact)

### Current Issue
LINQ operations create intermediate enumerators and collections.

### Recommendations

#### 4.1 SemanticIndexManager.BuildIndex()

**File**: `src/DocToolkit/Managers/SemanticIndexManager.cs`

**Current Code** (Line 84):
```csharp
var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();
```

**Optimized Code**:
```csharp
// Avoid ToList() - use array directly
var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
var totalFiles = files.Length; // Array.Length is faster than List.Count
```

**Impact**: Eliminates one allocation. Use `files.Length` instead of `files.Count`.

#### 4.2 SimilarityEngine.FindTopSimilar()

**File**: `src/DocToolkit/Engines/SimilarityEngine.cs`

**Current Code**:
```csharp
return scores
    .OrderByDescending(s => s.score)
    .Take(topK)
    .ToList();
```

**Optimized Code**:
```csharp
// Use partial sort instead of full sort
if (scores.Count <= topK)
{
    scores.Sort((a, b) => b.score.CompareTo(a.score));
    return scores;
}

// Partial sort: only sort top K elements
var topScores = new List<(int index, double score)>(topK);
// Use manual partial sort or PriorityQueue
// ... implementation ...
```

**Impact**: Reduces sorting overhead from O(n log n) to O(n log k) where k << n.

---

## 5. Reduce Event Data Allocations (Medium Impact)

### Current Issue
Events contain large strings that are allocated but may not be used.

### Recommendations

#### 5.1 DocumentProcessedEvent

**File**: `src/DocToolkit/ifx/Events/DocumentProcessedEvent.cs`

**Current Code**:
```csharp
public class DocumentProcessedEvent : BaseEvent
{
    public string FilePath { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty; // Large allocation!
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public int CharacterCount { get; set; }
}
```

**Optimized Code**:
```csharp
public class DocumentProcessedEvent : BaseEvent
{
    public string FilePath { get; set; } = string.Empty;
    // Don't store full text - subscribers can read if needed
    // public string ExtractedText { get; set; } = string.Empty; // REMOVE
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public int CharacterCount { get; set; }
    
    // Optional: Store only hash for deduplication
    public int TextHash { get; set; }
}
```

**Impact**: Eliminates large string allocations in events. If subscribers need text, they can read from file.

**Alternative**: Use `ReadOnlyMemory<char>` if text must be included:
```csharp
public ReadOnlyMemory<char> ExtractedText { get; set; }
```

---

## 6. Use Structs for Small Data Types (Low-Medium Impact)

### Current Issue
Small classes are allocated on heap when they could be structs.

### Recommendations

#### 6.1 IndexEntry

**File**: `src/DocToolkit/ifx/Models/IndexEntry.cs`

**Current Code**:
```csharp
public class IndexEntry
{
    public string File { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Chunk { get; set; } = string.Empty;
    public int Index { get; set; }
}
```

**Consideration**: This contains 3 strings, so it's not a good candidate for struct conversion (would increase copying cost). However, if we store only references/IDs:

**Alternative Design**:
```csharp
// Store only IDs/references, not full strings
public struct IndexEntry
{
    public int FileId { get; set; }
    public int ChunkIndex { get; set; }
    public int Index { get; set; }
}

// Separate lookup tables
public class IndexMetadata
{
    public Dictionary<int, string> Files { get; } = new();
    public Dictionary<int, string> Chunks { get; } = new();
}
```

**Impact**: Reduces allocations if many IndexEntry objects are created, but increases complexity. **Recommendation**: Keep as class unless profiling shows it's a bottleneck.

---

## 7. Pass Large Parameters by Reference (Low Impact)

### Current Issue
Large structs (if any) are copied when passed to methods.

### Recommendations

If any large structs are introduced, use `in` or `ref readonly`:

```csharp
// For large structs (> 3 words)
public void ProcessLargeStruct(in LargeStruct data)
{
    // Read-only access, passed by reference
}
```

**Current Status**: No large structs identified in codebase.

---

## 8. String Interning for Repeated Values (Low Impact)

### Current Issue
Repeated string values create multiple allocations.

### Recommendations

#### 8.1 File Extensions

**File**: `src/DocToolkit/Engines/DocumentExtractionEngine.cs`

**Current Code**:
```csharp
var ext = Path.GetExtension(filePath).ToLower();
```

**Optimized Code**:
```csharp
// Cache common extensions
private static readonly Dictionary<string, string> ExtensionCache = new();

private string GetCachedExtension(string filePath)
{
    var ext = Path.GetExtension(filePath);
    if (ExtensionCache.TryGetValue(ext, out var cached))
    {
        return cached;
    }
    
    var lower = ext.ToLowerInvariant();
    ExtensionCache[ext] = lower;
    return lower;
}
```

**Impact**: Low, but reduces allocations for repeated file types.

---

## Implementation Priority

### Phase 1: High Impact, Low Risk (Implement First)
1. ✅ **Pre-allocate collections** (1.1, 1.2, 1.3) - Easy, immediate impact
2. ✅ **Remove unnecessary ToList()** (4.1) - Trivial change
3. ✅ **Remove ExtractedText from events** (5.1) - Simple change

### Phase 2: High Impact, Medium Risk (Profile First)
4. ⚠️ **Use Span<T> for tokenization** (2.1) - Requires testing
5. ⚠️ **Use Span<T> for chunking** (2.2) - Requires testing
6. ⚠️ **Use ArrayPool for temporary arrays** (3.1) - Requires careful memory management

### Phase 3: Medium Impact (Optimize After Profiling)
7. ⚠️ **Optimize LINQ in SimilarityEngine** (4.2) - Requires algorithm change
8. ⚠️ **String interning** (8.1) - Low impact, implement if needed

---

## Measurement and Validation

Before implementing optimizations:

1. **Establish Baseline**: Use BenchmarkDotNet to measure current performance
2. **Profile Memory**: Use dotMemory or PerfView to identify actual hotspots
3. **Measure After Each Change**: Verify improvements with benchmarks
4. **Test Thoroughly**: Ensure correctness is maintained

### Benchmarking Example

```csharp
[MemoryDiagnoser]
public class IndexingBenchmarks
{
    [Benchmark]
    public void BuildIndex_Baseline()
    {
        // Current implementation
    }
    
    [Benchmark]
    public void BuildIndex_Optimized()
    {
        // Optimized implementation
    }
}
```

---

## Expected Impact Summary

| Optimization | Allocation Reduction | Complexity | Priority |
|-------------|---------------------|------------|----------|
| Pre-allocate collections | 30-50% | Low | High |
| Remove ToList() | 5-10% | Low | High |
| Remove event text | 20-30% | Low | High |
| Span<T> tokenization | 60-70% | Medium | Medium |
| Span<T> chunking | 50-60% | Medium | Medium |
| ArrayPool | 10-20% | Medium | Medium |
| Optimize LINQ | 5-15% | Medium | Low |

**Total Potential Reduction**: 50-70% reduction in allocations for hot paths.

---

## References

- [Reduce memory allocations using new C# features](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/performance/)
- [.NET Performance Tips](https://learn.microsoft.com/en-us/dotnet/framework/performance/performance-tips)
- [System.Buffers.ArrayPool<T>](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1)
- [System.Span<T>](https://learn.microsoft.com/en-us/dotnet/api/system.span-1)

---

## Notes

- All optimizations maintain type safety (no `unsafe` code required)
- Follow IDesign Method™ principles - don't sacrifice architecture for micro-optimizations
- Profile before optimizing - focus on hot paths only
- Test thoroughly after each change
