# DocToolkit Tests

This directory contains comprehensive tests for the DocToolkit project, including unit tests, integration tests, and benchmarking tests.

## Test Structure

```
tests/DocToolkit.Tests/
├── Engines/              # Unit tests for Engine classes
│   ├── TextChunkingEngineTests.cs
│   ├── SimilarityEngineTests.cs
│   ├── EntityExtractionEngineTests.cs
│   └── SummarizationEngineTests.cs
├── Managers/            # Unit tests for Manager classes
│   ├── SemanticIndexManagerTests.cs
│   └── SemanticSearchManagerTests.cs
├── Accessors/           # Unit tests for Accessor classes
│   └── VectorStorageAccessorTests.cs
├── Infrastructure/      # Tests for infrastructure components
│   └── EventBusTests.cs
├── Integration/         # Integration tests
│   └── SemanticIndexingIntegrationTests.cs
└── Benchmarks/          # Performance benchmarking tests
    ├── TextChunkingBenchmarks.cs
    ├── SimilarityBenchmarks.cs
    ├── EntityExtractionBenchmarks.cs
    └── SummarizationBenchmarks.cs
```

## Running Tests

### Run All Tests
```bash
# Standard xUnit output
dotnet test

# With detailed output
dotnet test --logger "console;verbosity=detailed"

# Using the test runner (with Spectre.Console formatting)
cd src/tests/DocToolkit.Tests
dotnet run -- test
```

### Run Specific Test Category
```bash
# Run only unit tests
dotnet test --filter Category=Unit

# Run only integration tests
dotnet test --filter Category=Integration
```

### Run Benchmarks

Benchmarks use Spectre.Console for beautiful output:

```bash
cd src/tests/DocToolkit.Tests

# Run all benchmarks
dotnet run -c Release -- benchmark

# Run specific benchmark
dotnet run -c Release -- bench textchunking
dotnet run -c Release -- bench similarity
dotnet run -c Release -- bench entity
dotnet run -c Release -- bench summarization
```

The benchmark runner will display:
- Beautiful formatted tables with results
- Mean execution time
- Error and standard deviation
- Memory allocation (Gen 0, Gen 1, Gen 2)
- Total execution time

## Test Coverage

### Engines (Algorithm Volatility)
- ✅ TextChunkingEngine - 10 tests
- ✅ SimilarityEngine - 11 tests
- ✅ EntityExtractionEngine - 12 tests
- ✅ SummarizationEngine - 10 tests

### Managers (Workflow Volatility)
- ✅ SemanticIndexManager - 6 tests
- ✅ SemanticSearchManager - 3 tests

### Accessors (Storage Volatility)
- ✅ VectorStorageAccessor - 6 tests

### Infrastructure
- ✅ EventBus - 5 tests

### Integration Tests
- ✅ Semantic Indexing Integration - 1 test (requires ONNX model for full testing)

### Benchmarks
- ✅ Text Chunking - 4 benchmarks
- ✅ Similarity Calculations - 5 benchmarks
- ✅ Entity Extraction - 4 benchmarks
- ✅ Summarization - 5 benchmarks

## Test Statistics

- **Total Tests**: 50+
- **Test Framework**: xUnit
- **Assertion Library**: xUnit Assert (standard .NET testing)
- **Mocking Framework**: Moq
- **Benchmarking**: BenchmarkDotNet

## Test Principles

Tests follow IDesign Method™ principles:
- **Engines**: Pure function tests (no I/O, no dependencies)
- **Managers**: Orchestration tests with mocked dependencies
- **Accessors**: Storage abstraction tests with file system operations
- **Integration**: End-to-end workflow tests

## Notes

- Some integration tests require ONNX model files and are marked as `[Skip]`
- Benchmark tests should be run in Release mode for accurate results
- All tests use temporary directories that are cleaned up after execution
