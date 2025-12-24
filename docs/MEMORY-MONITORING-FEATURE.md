# Memory Monitoring Feature

**Date**: 2024  
**Status**: ✅ Complete

## Summary

Added memory monitoring capabilities to track memory usage during application operations. This feature helps verify the effectiveness of Steps 1 and 2 memory optimizations in real-world usage.

---

## Features

### MemoryMonitor Class

**Location**: `src/DocToolkit/ifx/Infrastructure/MemoryMonitor.cs`

**Capabilities**:
- ✅ Tracks memory usage before, during, and after operations
- ✅ Displays detailed memory statistics (current, delta, GC collections)
- ✅ Provides compact summary format for progress updates
- ✅ Forces GC before measurement for accurate baseline
- ✅ Formats memory in human-readable units (B, KB, MB, GB, TB)

**Key Methods**:
- `DisplayStats(label)`: Shows detailed memory statistics table
- `DisplaySummary()`: Shows compact one-line summary
- `ForceGC()`: Forces garbage collection for accurate measurement
- `Dispose()`: Automatically displays final stats on disposal

---

## Integration

### Commands with Memory Monitoring

All major operations now support `--monitor-memory` flag:

1. **IndexCommand** (`doc index`)
   - Monitors memory during semantic index building
   - Updates every 10% progress

2. **SearchCommand** (`doc search`)
   - Monitors memory during search operations
   - Shows stats before and after search

3. **GraphCommand** (`doc graph`)
   - Monitors memory during knowledge graph building
   - Updates every 10% progress

4. **SummarizeCommand** (`doc summarize`)
   - Monitors memory during summarization
   - Updates every 10% progress

---

## Usage

### Basic Usage

```bash
# Index with memory monitoring
doc index --monitor-memory

# Search with memory monitoring
doc search "query" --monitor-memory

# Graph with memory monitoring
doc graph --monitor-memory

# Summarize with memory monitoring
doc summarize --monitor-memory
```

### Combined with Other Options

```bash
# Index with custom settings and memory monitoring
doc index --source ./docs --chunk-size 1000 --monitor-memory

# Search with custom top-k and memory monitoring
doc search "requirements" --top-k 10 --monitor-memory
```

---

## Output Format

### Detailed Statistics Table

When memory monitoring is enabled, you'll see tables like:

```
┌─────────────────────────────────────┐
│ Memory Stats: Indexing              │
├─────────────────────────────────────┤
│ Current Memory    │ 125.45 MB       │
│ Memory Delta      │ +45.23 MB       │
│ Elapsed Time      │ 12.34s         │
│ GC Gen 0          │ 15              │
│ GC Gen 1          │ 3               │
│ GC Gen 2          │ 0               │
└─────────────────────────────────────┘
```

### Compact Summary

During progress updates, you'll see compact summaries:

```
Memory: +45.23 MB | Time: 12.34s | GC: 15/3/0
```

### Final Statistics

At the end of operations, final statistics are automatically displayed:

```
┌─────────────────────────────────────┐
│ Memory Stats: Final                 │
├─────────────────────────────────────┤
│ Current Memory    │ 98.12 MB        │
│ Memory Delta      │ +18.90 MB       │
│ Elapsed Time      │ 45.67s         │
│ GC Gen 0          │ 42              │
│ GC Gen 1          │ 8               │
│ GC Gen 2          │ 1               │
└─────────────────────────────────────┘
```

---

## Memory Metrics Explained

### Current Memory
- Total managed memory currently allocated
- Measured using `GC.GetTotalMemory(false)`

### Memory Delta
- Change in memory since monitoring started
- Positive values (yellow) indicate memory growth
- Negative values (green) indicate memory freed
- Includes forced GC before baseline measurement

### Elapsed Time
- Total time since monitoring started
- Useful for calculating memory allocation rate

### GC Collections
- **Gen 0**: Short-lived objects (most frequent)
- **Gen 1**: Objects that survived Gen 0 collection
- **Gen 2**: Long-lived objects (least frequent, most expensive)

---

## Best Practices

### When to Use Memory Monitoring

1. **Performance Analysis**: 
   - Identify memory-intensive operations
   - Verify optimization effectiveness

2. **Debugging**:
   - Detect memory leaks
   - Identify unexpected allocations

3. **Optimization Verification**:
   - Measure impact of Steps 1 + 2 optimizations
   - Compare before/after memory usage

### Interpreting Results

**Good Signs**:
- ✅ Memory delta stabilizes after initial allocation
- ✅ GC Gen 0/1 collections are frequent but not excessive
- ✅ GC Gen 2 collections are rare
- ✅ Memory delta is reasonable for operation size

**Warning Signs**:
- ⚠️ Memory delta continuously grows (potential leak)
- ⚠️ Excessive GC Gen 2 collections (long-lived objects)
- ⚠️ Memory delta much larger than expected
- ⚠️ GC collections increasing over time

---

## Example Output

### Indexing Operation

```bash
$ doc index --monitor-memory

Building semantic index from: ./source

┌─────────────────────────────────────┐
│ Memory Stats: Initial               │
├─────────────────────────────────────┤
│ Current Memory    │ 45.23 MB       │
│ Memory Delta      │ 0.00 B         │
│ Elapsed Time      │ 0.00s          │
│ GC Gen 0          │ 0              │
│ GC Gen 1          │ 0              │
│ GC Gen 2          │ 0              │
└─────────────────────────────────────┘

Processing files... [████████████] 100%

Memory: +125.45 MB | Time: 12.34s | GC: 15/3/0
Memory: +98.12 MB | Time: 23.45s | GC: 28/5/0
Memory: +87.34 MB | Time: 34.56s | GC: 35/7/0

┌─────────────────────────────────────┐
│ Memory Stats: Final                 │
├─────────────────────────────────────┤
│ Current Memory    │ 98.12 MB       │
│ Memory Delta      │ +52.89 MB      │
│ Elapsed Time      │ 45.67s         │
│ GC Gen 0          │ 42             │
│ GC Gen 1          │ 8              │
│ GC Gen 2          │ 1              │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ Index Complete                      │
├─────────────────────────────────────┤
│ ✓ Semantic index created at:        │
│   ./semantic-index                  │
│                                     │
│ Files created:                      │
│   • ./semantic-index/vectors.bin     │
│   • ./semantic-index/index.json     │
└─────────────────────────────────────┘
```

---

## Technical Details

### Implementation

- **Baseline Measurement**: Forces full GC before starting to ensure accurate baseline
- **Progress Updates**: Updates every 10% progress during long operations
- **Final Stats**: Automatically displayed on disposal (using `using` statement)
- **Non-Intrusive**: Only active when `--monitor-memory` flag is used

### Performance Impact

- **Minimal**: Memory monitoring has negligible performance impact
- **GC Forcing**: Only done at start/end, not during operation
- **Display Updates**: Only shown every 10% progress to minimize I/O

---

## Files Modified

1. `src/DocToolkit/ifx/Infrastructure/MemoryMonitor.cs` - New memory monitoring utility
2. `src/DocToolkit/ifx/Commands/IndexCommand.cs` - Added `--monitor-memory` flag
3. `src/DocToolkit/ifx/Commands/SearchCommand.cs` - Added `--monitor-memory` flag
4. `src/DocToolkit/ifx/Commands/GraphCommand.cs` - Added `--monitor-memory` flag
5. `src/DocToolkit/ifx/Commands/SummarizeCommand.cs` - Added `--monitor-memory` flag

---

## Verification

✅ Build succeeded  
✅ All commands compile correctly  
✅ Memory monitoring is optional (default: disabled)  
✅ No breaking changes to existing functionality

---

## Next Steps

1. **Test with Real Data**: Run memory monitoring on actual indexing operations
2. **Compare Results**: Compare memory usage before/after Steps 1 + 2 optimizations
3. **Profile Hot Paths**: Use results to identify additional optimization opportunities
4. **Document Patterns**: Document typical memory usage patterns for different workloads

---

## Related Documentation

- `docs/MEMORY-OPTIMIZATION-STEP1-COMPLETE.md` - Pre-allocation optimizations
- `docs/MEMORY-OPTIMIZATION-STEP2-COMPLETE.md` - Event optimization
- `docs/MEMORY-OPTIMIZATION-BENCHMARK-RESULTS.md` - Benchmark results
- `docs/MEMORY-OPTIMIZATION-RECOMMENDATIONS.md` - Full optimization strategy
