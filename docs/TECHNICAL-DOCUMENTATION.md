# Documentation Toolkit - Technical Documentation

**Version**: 1.0  
**Last Updated**: 2024  
**Status**: Current

---

## Table of Contents

1. [Introduction](#introduction)
2. [Architecture Overview](#architecture-overview)
3. [Component Documentation](#component-documentation)
4. [API Reference](#api-reference)
5. [Memory Management](#memory-management)
6. [Event System](#event-system)
7. [Code Examples](#code-examples)
8. [Best Practices](#best-practices)

---

## Introduction

The Documentation Toolkit is a C# command-line application built on .NET 9.0 that provides semantic indexing, knowledge graph generation, and document summarization capabilities. The application follows the IDesign Method™ principles for volatility-based decomposition and service-oriented architecture.

### Key Features

- **Semantic Indexing**: Build searchable indexes from document collections using embeddings
- **Knowledge Graph Generation**: Extract entities and relationships to build knowledge graphs
- **Document Summarization**: Generate context documents from source files
- **Memory Monitoring**: Track memory usage during operations for optimization verification
- **Event-Driven Architecture**: Decoupled communication between components using an event bus

### Technology Stack

- **.NET 9.0**: Runtime and framework
- **Spectre.Console**: Rich console output and CLI framework
- **Microsoft.ML.OnnxRuntime**: ONNX model inference for embeddings
- **Microsoft.Extensions.Logging**: Structured logging
- **Microsoft.Extensions.DependencyInjection**: Dependency injection container
- **SQLite**: Event persistence

---

## Architecture Overview

The Documentation Toolkit follows the IDesign Method™ architecture pattern, which emphasizes volatility-based decomposition rather than functional decomposition.

### Component Taxonomy

The application is organized into four primary component types:

1. **Clients**: Encapsulate UI volatility (CLI commands)
2. **Managers**: Encapsulate workflow volatility (orchestration)
3. **Engines**: Encapsulate algorithm volatility (business logic)
4. **Accessors**: Encapsulate storage volatility (resource abstraction)

### Call Chain Pattern

The application follows a strict call chain pattern:

```
Client (Command) → Manager → Engine/Accessor
```

Components can only call components in the tier immediately underneath, following the closed architecture pattern.

### Service Boundaries

- **Clients → Managers**: Service boundary (authentication/authorization if needed)
- **Managers → Engines/Accessors**: Service boundary (closed architecture)
- **Managers → Event Bus**: Event publishing for cross-manager communication

---

## Component Documentation

### MemoryMonitor Class

**Namespace**: `DocToolkit.ifx.Infrastructure`  
**Type**: Infrastructure Utility  
**Purpose**: Track memory usage during operations

#### Overview

The `MemoryMonitor` class provides real-time tracking of managed memory usage, garbage collection statistics, and elapsed time during application operations. It is designed to help verify the effectiveness of memory optimizations and identify potential memory leaks.

#### Key Features

- **Baseline Establishment**: Forces garbage collection before starting to establish accurate baseline
- **Real-Time Tracking**: Monitors current memory, memory delta, and elapsed time
- **GC Statistics**: Tracks garbage collection counts across all generations
- **Multiple Display Formats**: Detailed tables and compact summaries
- **Automatic Finalization**: Displays final statistics on disposal

#### Usage Example

```csharp
using var monitor = new MemoryMonitor("Indexing", enabled: true);
monitor.DisplayStats("Initial");

// Perform operation...
_indexManager.BuildIndex(sourcePath, outputPath, chunkSize, chunkOverlap);

// Display compact summary during progress
monitor.DisplaySummary();

// Dispose automatically displays final stats
```

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `CurrentMemory` | `long` | Current total managed memory in bytes |
| `MemoryDelta` | `long` | Change in memory since monitoring started |
| `Elapsed` | `TimeSpan` | Elapsed time since monitoring started |

#### Methods

##### `DisplayStats(string? label = null)`

Displays a detailed table of memory statistics including:
- Current Memory
- Memory Delta (with sign)
- Elapsed Time
- GC Gen 0, 1, 2 collection counts

**Parameters**:
- `label`: Optional label for the statistics table. If null, uses the operation name.

##### `DisplaySummary()`

Displays a compact one-line summary suitable for progress updates:
```
Memory: +45.23 MB | Time: 12.34s | GC: 15/3/0
```

##### `ForceGC()`

Forces a full garbage collection across all generations. Use this method when you need accurate memory measurements after an operation completes.

#### Implementation Details

**Baseline Establishment**:
When enabled, the constructor performs:
1. Full GC collection across all generations
2. Wait for pending finalizers
3. Second full GC collection
4. Capture total managed memory as baseline

**Memory Formatting**:
The `FormatBytes` method converts byte counts to human-readable format:
- Automatically selects appropriate unit (B, KB, MB, GB, TB)
- Uses base 1024 (binary) conversions
- Supports sign indicators for deltas

---

### SemanticIndexManager Class

**Namespace**: `DocToolkit.Managers`  
**Type**: Manager (Workflow Volatility)  
**Purpose**: Orchestrate semantic indexing workflow

#### Overview

The `SemanticIndexManager` encapsulates the workflow volatility of indexing operations. It orchestrates document extraction, text chunking, embedding generation, and vector storage without knowing the implementation details of these operations.

#### Responsibilities

- **Orchestration**: Coordinates the indexing workflow
- **Progress Reporting**: Provides progress callbacks to clients
- **Event Publishing**: Publishes events for cross-manager communication
- **Error Handling**: Handles errors gracefully with logging

#### Workflow

1. Validate source directory exists
2. Get all files from source directory
3. Pre-allocate collections with capacity estimates (memory optimization)
4. For each file:
   - Extract text using `DocumentExtractionEngine`
   - Chunk text using `TextChunkingEngine`
   - Generate embeddings using `EmbeddingEngine`
   - Store in collections
   - Publish `DocumentProcessedEvent`
   - Report progress
5. Save vectors and index using `VectorStorageAccessor`
6. Publish `IndexBuiltEvent`

#### Memory Optimizations

The manager implements Step 1 memory optimizations:

```csharp
// Pre-allocate collections with capacity estimate
var estimatedCapacity = Math.Max(1, totalFiles * 5);
var entries = new List<IndexEntry>(estimatedCapacity);
var vectors = new List<float[]>(estimatedCapacity);
```

This reduces list reallocations from O(n log n) to O(n) for list growth.

#### Dependencies

- `IDocumentExtractionEngine`: Extracts text from documents
- `IEmbeddingEngine`: Generates semantic embeddings
- `IVectorStorageAccessor`: Stores vectors and index
- `ITextChunkingEngine`: Chunks text into manageable pieces
- `IEventBus`: Publishes events
- `ILogger<SemanticIndexManager>`: Structured logging

---

### Event Bus System

**Namespace**: `DocToolkit.ifx.Infrastructure`  
**Type**: Infrastructure Component  
**Purpose**: Decoupled cross-component communication

#### Overview

The event bus implements an in-memory publish/subscribe pattern with SQLite persistence for event history and retry capabilities.

#### Features

- **Type-Safe Subscriptions**: Subscribe to specific event types
- **Async Support**: Handles both sync and async event handlers
- **Persistence**: Stores events in SQLite database
- **Retry Logic**: Automatic retry of failed event handlers
- **Error Handling**: Logs errors without breaking the application

#### Event Types

- `IndexBuiltEvent`: Published when semantic index is built
- `GraphBuiltEvent`: Published when knowledge graph is built
- `SummaryCreatedEvent`: Published when summary is created
- `DocumentProcessedEvent`: Published for each document processed (metadata only, no text)

#### Usage Example

```csharp
// Subscribe to events
eventBus.Subscribe<IndexBuiltEvent>(evt =>
{
    logger.LogInformation("Index built: {IndexPath}", evt.IndexPath);
});

// Publish events
eventBus.Publish(new IndexBuiltEvent
{
    IndexPath = outputPath,
    EntryCount = entries.Count,
    VectorCount = vectors.Count
});
```

---

## API Reference

### Commands

#### IndexCommand

Builds a semantic index from source files.

**Usage**:
```bash
doc index [options]
```

**Options**:
- `-s, --source <path>`: Source directory (default: `./source`)
- `-o, --output <path>`: Index output directory (default: `./semantic-index`)
- `--chunk-size <number>`: Chunk size in words (default: 800)
- `--chunk-overlap <number>`: Chunk overlap in words (default: 200)
- `--monitor-memory`: Enable memory monitoring during indexing

**Example**:
```bash
doc index --source ./docs --chunk-size 1000 --monitor-memory
```

#### SearchCommand

Searches the semantic index.

**Usage**:
```bash
doc search <query> [options]
```

**Options**:
- `-i, --index <path>`: Index directory (default: `./semantic-index`)
- `-k, --top-k <number>`: Number of results (default: 5)
- `--monitor-memory`: Enable memory monitoring during search

**Example**:
```bash
doc search "customer requirements" --top-k 10 --monitor-memory
```

---

## Memory Management

### Optimization Strategy

The application implements a multi-step memory optimization strategy:

#### Step 1: Pre-allocate Collections ✅

**Components Optimized**:
- `SemanticIndexManager`: Pre-allocates entries and vectors lists
- `TextChunkingEngine`: Pre-allocates chunks list
- `SimilarityEngine`: Pre-allocates scores list

**Impact**: 30-50% reduction in allocations for hot paths

#### Step 2: Remove Large Strings from Events ✅

**Change**: Removed `ExtractedText` property from `DocumentProcessedEvent`

**Impact**: 20-30% reduction in event allocations

**Rationale**: Events are published frequently, and text is already stored in `IndexEntry`. Subscribers can read files if needed using `FilePath`.

### Memory Monitoring

Use the `--monitor-memory` flag on commands to track memory usage:

```bash
doc index --monitor-memory
```

This displays:
- Initial memory statistics
- Progress updates (every 10%)
- Final memory statistics

### Best Practices

1. **Pre-allocate Collections**: Use capacity estimates when creating lists
2. **Avoid Unnecessary Allocations**: Remove `.ToList()` calls when arrays suffice
3. **Minimize Event Payloads**: Store only metadata in events, not large objects
4. **Monitor Regularly**: Use memory monitoring to verify optimizations

---

## Event System

### Event Flow

```
Manager → EventBus.Publish() → Subscribers → EventPersistence (SQLite)
```

### Event Persistence

Events are persisted to SQLite database (`events.db`) for:
- **History**: Track all events for auditing
- **Retry**: Automatic retry of failed handlers
- **Debugging**: Inspect event flow

### Event Subscriptions

Configured in `EventSubscriptions.ConfigureSubscriptions()`:

```csharp
eventBus.Subscribe<IndexBuiltEvent>(evt =>
{
    logger.LogInformation("Index built: {IndexPath}", evt.IndexPath);
});
```

---

## Code Examples

### Using MemoryMonitor

```csharp
using var monitor = new MemoryMonitor("Indexing", enabled: true);
monitor.DisplayStats("Initial");

// Perform operation
var result = _indexManager.BuildIndex(
    sourcePath,
    outputPath,
    chunkSize,
    chunkOverlap,
    progress =>
    {
        // Update progress
        if (progress % 10 == 0)
        {
            monitor.DisplaySummary();
        }
    }
);

// Final stats displayed automatically on disposal
```

### Building an Index

```csharp
var indexManager = serviceProvider.GetRequiredService<ISemanticIndexManager>();

var success = indexManager.BuildIndex(
    sourcePath: "./source",
    outputPath: "./semantic-index",
    chunkSize: 800,
    chunkOverlap: 200,
    progressCallback: progress => Console.WriteLine($"Progress: {progress}%")
);

if (success)
{
    Console.WriteLine("Index built successfully");
}
```

### Subscribing to Events

```csharp
var eventBus = serviceProvider.GetRequiredService<IEventBus>();

eventBus.Subscribe<IndexBuiltEvent>(evt =>
{
    Console.WriteLine($"Index built: {evt.IndexPath}");
    Console.WriteLine($"Entries: {evt.EntryCount}, Vectors: {evt.VectorCount}");
});
```

---

## Best Practices

### Code Organization

1. **Follow IDesign Method™**: Organize by volatility, not functionality
2. **Closed Architecture**: Components only call down one tier
3. **Dependency Injection**: Use constructor injection for all dependencies
4. **Interface Segregation**: Define focused interfaces for each component

### Error Handling

1. **Log Errors**: Use structured logging for all errors
2. **Graceful Degradation**: Handle errors without crashing
3. **User-Friendly Messages**: Display clear error messages to users
4. **Internal Logging**: Log detailed errors internally

### Performance

1. **Pre-allocate Collections**: Use capacity estimates
2. **Avoid Unnecessary Allocations**: Remove redundant `.ToList()` calls
3. **Monitor Memory**: Use `--monitor-memory` to verify optimizations
4. **Profile First**: Profile before optimizing

### Documentation

1. **XML Comments**: Document all public APIs
2. **Inline Comments**: Explain complex logic
3. **Examples**: Provide usage examples
4. **Remarks**: Include implementation details and rationale

---

## IDesign C# Coding Standard Compliance

The Documentation Toolkit codebase fully complies with the IDesign C# Coding Standard 3.1. See [IDesign C# Coding Standard Compliance](./IDESIGN-CSHARP-CODING-STANDARD-COMPLIANCE.md) for detailed compliance information.

### Key Compliance Points

- ✅ **Naming Conventions**: All interfaces start with "I", classes use PascalCase, private fields use underscore prefix
- ✅ **Service Boundaries**: Interface-based design, dependency injection, closed architecture
- ✅ **Error Handling**: Argument validation, exception documentation, structured logging
- ✅ **Code Organization**: Proper namespaces, separation of concerns, one class per file
- ✅ **Documentation**: XML comments, IDesign Method™ notes, inline comments

---

## References

### IDesign Standards (Primary)

- [IDesign C# Coding Standard 3.1](docs/IDesign%20C%23%20Coding%20Standard%203.1.pdf) - **Primary Coding Standard**
- [IDesign Method™ Version 2.5](docs/IDesign%20Method.pdf)
- [IDesign Design Standard](docs/IDesign%20Design%20Standard.pdf)
- [IDesign C# Coding Standard Compliance](./IDESIGN-CSHARP-CODING-STANDARD-COMPLIANCE.md)

### Style Guides

- [Red Hat Supplementary Style Guide](https://redhat-documentation.github.io/supplementary-style-guide/)
- [Red Hat Technical Writing Style Guide](https://stylepedia.net/)
- [Write the Docs Guide](https://www.writethedocs.org/guide/writing/docs-principles/)
- [Microsoft Style Guide](https://learn.microsoft.com/en-us/style-guide/welcome/)

### Technical Writing Resources

- [Awesome Technical Writing](https://github.com/BolajiAyodeji/awesome-technical-writing)

---

## Document History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2024 | Initial technical documentation |

---

**Last Updated**: 2024  
**Maintained By**: Documentation Toolkit Team
