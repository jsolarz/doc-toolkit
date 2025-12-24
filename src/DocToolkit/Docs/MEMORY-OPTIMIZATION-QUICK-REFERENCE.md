# Memory Optimization - Quick Reference

## 🎯 Top 5 Quick Wins (Implement First)

### 1. Pre-allocate Collections ✅ Easy
**File**: `SemanticIndexManager.cs` (Line 90-91)
```csharp
// BEFORE
var entries = new List<IndexEntry>();
var vectors = new List<float[]>();

// AFTER
var estimatedCapacity = files.Count * 5;
var entries = new List<IndexEntry>(estimatedCapacity);
var vectors = new List<float[]>(estimatedCapacity);
```
**Impact**: 30-50% reduction in list reallocations

### 2. Remove Unnecessary ToList() ✅ Trivial
**File**: `SemanticIndexManager.cs` (Line 84)
```csharp
// BEFORE
var files = Directory.GetFiles(...).ToList();

// AFTER
var files = Directory.GetFiles(...); // Use array directly
var totalFiles = files.Length;
```
**Impact**: Eliminates one allocation per call

### 3. Remove Large Strings from Events ✅ Simple
**File**: `DocumentProcessedEvent.cs` (Line 16)
```csharp
// BEFORE
public string ExtractedText { get; set; } = string.Empty; // Large!

// AFTER
// Remove ExtractedText - subscribers can read file if needed
public int TextHash { get; set; } // Optional: for deduplication
```
**Impact**: 20-30% reduction in event allocations

### 4. Pre-allocate Chunking List ✅ Easy
**File**: `TextChunkingEngine.cs` (Line 33)
```csharp
// BEFORE
var chunks = new List<string>();

// AFTER
var estimatedChunks = Math.Max(1, text.Length / (chunkSize * 5));
var chunks = new List<string>(estimatedChunks);
```
**Impact**: Reduces reallocations for large documents

### 5. Pre-allocate Similarity Scores ✅ Easy
**File**: `SimilarityEngine.cs` (Line 66)
```csharp
// BEFORE
var scores = new List<(int index, double score)>();

// AFTER
var scores = new List<(int index, double score)>(topK);
```
**Impact**: Eliminates reallocations for top-K results

---

## 📊 Expected Impact

| Optimization | Allocation Reduction | Effort | Priority |
|-------------|---------------------|--------|----------|
| Pre-allocate collections | 30-50% | Low | ⭐⭐⭐ |
| Remove ToList() | 5-10% | Low | ⭐⭐⭐ |
| Remove event text | 20-30% | Low | ⭐⭐⭐ |
| Pre-allocate chunking | 10-20% | Low | ⭐⭐ |
| Pre-allocate scores | 5-10% | Low | ⭐⭐ |

**Total Quick Wins**: ~50-70% reduction in allocations

---

## 🔧 Advanced Optimizations (Profile First)

### Use Span<T> for String Operations
- **Tokenization**: Eliminate `ToLower()` and `Split()` allocations
- **Chunking**: Zero-allocation string slicing
- **Impact**: 60-70% reduction, but requires testing

### Use ArrayPool<T> for Temporary Arrays
- **Embedding generation**: Reuse temporary arrays
- **Impact**: 10-20% reduction, requires careful management

### Optimize LINQ in Hot Paths
- **SimilarityEngine**: Use partial sort instead of full sort
- **Impact**: 5-15% reduction, requires algorithm change

---

## 📈 Measurement

Before optimizing:
1. Run benchmarks: `dotnet run -c Release -- benchmark`
2. Profile with dotMemory or PerfView
3. Measure after each change

---

## 📚 Full Documentation

See [MEMORY-OPTIMIZATION-RECOMMENDATIONS.md](./MEMORY-OPTIMIZATION-RECOMMENDATIONS.md) for:
- Detailed code examples
- Implementation guidance
- References to Microsoft Learn articles
- Advanced optimization techniques

---

## ⚠️ Important Notes

- **Profile First**: Don't optimize without measuring
- **Hot Paths Only**: Focus on frequently executed code
- **Maintain Safety**: All optimizations use safe code (no `unsafe`)
- **Test Thoroughly**: Verify correctness after each change
