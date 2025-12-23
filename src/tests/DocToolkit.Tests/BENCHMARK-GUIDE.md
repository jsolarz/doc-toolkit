# Benchmark Guide

## Quick Start

### Run All Benchmarks
```bash
cd src/tests/DocToolkit.Tests
dotnet run -c Release -- benchmark
```

### Run Specific Benchmark
```bash
cd src/tests/DocToolkit.Tests
dotnet run -c Release -- bench <name>
```

Available benchmark names:
- `textchunking` or `chunking` - Text chunking performance
- `similarity` - Similarity calculation performance
- `entity` or `entityextraction` - Entity extraction performance
- `summarization` or `summarize` - Summarization performance

## Output Format

The benchmark runner uses Spectre.Console to display beautiful, formatted results:

- **Benchmark Name**: The method being benchmarked
- **Mean**: Average execution time
- **Error**: Standard error
- **StdDev**: Standard deviation
- **Gen 0/1/2**: Garbage collection statistics
- **Allocated**: Memory allocated per operation

## Example Output

```
  ____                      _            _      
 | __ )  ___ _ __ ___   ___| | _____ _ __| | __ 
 |  _ \ / _ \ '_ ` _ \ / _ \ |/ / _ \ '__| |/ / 
 | |_) |  __/ | | | | |  __/   <  __/ |  |   <  
 |____/ \___|_| |_| |_|\___|_|\_\___|_|  |_|\_\ 

Running all benchmarks...

┌─────────────────────────────────────────────────────────────┐
│                    Benchmark Results                         │
└─────────────────────────────────────────────────────────────┘

┌──────────────────────────┬──────────┬─────────┬──────────┬───────────┐
│ Benchmark                │ Mean     │ Error   │ StdDev   │ Allocated │
├──────────────────────────┼──────────┼─────────┼──────────┼───────────┤
│ ChunkText_SmallChunks    │ 2.45 ms  │ 0.12 ms │ 0.34 ms  │ 1.23 MB   │
│ ChunkText_MediumChunks   │ 5.67 ms  │ 0.23 ms │ 0.67 ms  │ 2.45 MB   │
└──────────────────────────┴──────────┴─────────┴──────────┴───────────┘

Total time: 45.23s
```

## Notes

- Benchmarks should be run in **Release mode** (`-c Release`) for accurate results
- Benchmark execution can take several minutes as BenchmarkDotNet performs warmup and multiple iterations
- Results are displayed in a formatted table with color coding
- Memory allocation is shown in bytes, KB, or MB as appropriate
