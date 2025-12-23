# Patch: IDesign Method™ Refactoring Implementation

## Overview

This patch implements the refactoring to align with IDesign Method™ principles:
1. Rename all components to proper taxonomy (Manager/Engine/Accessor)
2. Extract business logic from Managers to Engines
3. Add interfaces for decoupling
4. Implement dependency injection
5. Add event bus for cross-manager communication

## Phase 1: Create New Engine

### File: `src/DocToolkit/Services/Engines/SummarizationEngine.cs`

```csharp
namespace DocToolkit.Services.Engines;

/// <summary>
/// Engine for text summarization operations.
/// Encapsulates algorithm volatility: summarization algorithm could change (sentence extraction, ML models).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Summarization algorithm
/// Pattern: Pure function - accepts text, returns summary (no I/O)
/// </remarks>
public class SummarizationEngine
{
    /// <summary>
    /// Summarizes text by extracting key sentences.
    /// </summary>
    /// <param name="text">Text to summarize</param>
    /// <param name="maxSentences">Maximum number of sentences in summary</param>
    /// <returns>Summarized text</returns>
    public string SummarizeText(string text, int maxSentences = 5)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (text.Length <= 800)
        {
            return text;
        }

        var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var summaryCount = Math.Min(maxSentences, sentences.Length);
        return string.Join(" ", sentences.Take(summaryCount)) + ".";
    }
}
```

## Phase 2: Create Interfaces

### File: `src/DocToolkit/Interfaces/IManagers/ISemanticIndexManager.cs`

```csharp
namespace DocToolkit.Interfaces.Managers;

public interface ISemanticIndexManager : IDisposable
{
    bool BuildIndex(
        string sourcePath,
        string outputPath,
        int chunkSize,
        int chunkOverlap,
        Action<double>? progressCallback = null);
}
```

### File: `src/DocToolkit/Interfaces/IManagers/ISemanticSearchManager.cs`

```csharp
using DocToolkit.Models;

namespace DocToolkit.Interfaces.Managers;

public interface ISemanticSearchManager : IDisposable
{
    List<SearchResult> Search(string query, string indexPath, int topK);
}
```

### File: `src/DocToolkit/Interfaces/IManagers/IKnowledgeGraphManager.cs`

```csharp
namespace DocToolkit.Interfaces.Managers;

public interface IKnowledgeGraphManager
{
    bool BuildGraph(string sourcePath, string outputPath, Action<double>? progressCallback = null);
}
```

### File: `src/DocToolkit/Interfaces/IManagers/ISummarizeManager.cs`

```csharp
namespace DocToolkit.Interfaces.Managers;

public interface ISummarizeManager
{
    bool SummarizeSource(string sourcePath, string outputFile, int maxChars, Action<double>? progressCallback = null);
}
```

### File: `src/DocToolkit/Interfaces/IEngines/IDocumentExtractionEngine.cs`

```csharp
namespace DocToolkit.Interfaces.Engines;

public interface IDocumentExtractionEngine
{
    string ExtractText(string filePath);
}
```

### File: `src/DocToolkit/Interfaces/IEngines/IEmbeddingEngine.cs`

```csharp
namespace DocToolkit.Interfaces.Engines;

public interface IEmbeddingEngine : IDisposable
{
    float[] GenerateEmbedding(string text);
}
```

### File: `src/DocToolkit/Interfaces/IEngines/ITextChunkingEngine.cs`

```csharp
namespace DocToolkit.Interfaces.Engines;

public interface ITextChunkingEngine
{
    List<string> ChunkText(string text, int chunkSize, int chunkOverlap);
}
```

### File: `src/DocToolkit/Interfaces/IEngines/ISimilarityEngine.cs`

```csharp
namespace DocToolkit.Interfaces.Engines;

public interface ISimilarityEngine
{
    double CosineSimilarity(float[] vectorA, float[] vectorB);
    List<(int index, double score)> FindTopSimilar(float[] queryVector, float[][] vectors, int topK);
}
```

### File: `src/DocToolkit/Interfaces/IEngines/IEntityExtractionEngine.cs`

```csharp
namespace DocToolkit.Interfaces.Engines;

public interface IEntityExtractionEngine
{
    List<string> ExtractEntities(string text);
    List<string> ExtractTopics(string text, int topN = 10);
}
```

### File: `src/DocToolkit/Interfaces/IEngines/ISummarizationEngine.cs`

```csharp
namespace DocToolkit.Interfaces.Engines;

public interface ISummarizationEngine
{
    string SummarizeText(string text, int maxSentences = 5);
}
```

### File: `src/DocToolkit/Interfaces/IAccessors/IVectorStorageAccessor.cs`

```csharp
using DocToolkit.Models;

namespace DocToolkit.Interfaces.Accessors;

public interface IVectorStorageAccessor
{
    void SaveVectors(float[][] vectors, string indexPath);
    float[][] LoadVectors(string indexPath);
    void SaveIndex(List<IndexEntry> entries, string indexPath);
    List<IndexEntry> LoadIndex(string indexPath);
    bool IndexExists(string indexPath);
}
```

### File: `src/DocToolkit/Interfaces/IAccessors/ITemplateAccessor.cs`

```csharp
namespace DocToolkit.Interfaces.Accessors;

public interface ITemplateAccessor
{
    bool TemplateExists(string type);
    List<string> GetAvailableTemplates();
    string GenerateDocument(string type, string name, string outputDir);
}
```

### File: `src/DocToolkit/Interfaces/IAccessors/IProjectAccessor.cs`

```csharp
namespace DocToolkit.Interfaces.Accessors;

public interface IProjectAccessor
{
    void CreateDirectories(string projectName);
    void CreateCursorConfig(string projectName);
    void CreateReadme(string projectName);
    void CreateGitIgnore(string projectName);
    void InitializeGit(string projectName);
}
```

## Phase 3: Event Bus Infrastructure

### File: `src/DocToolkit/Infrastructure/IEventBus.cs`

```csharp
namespace DocToolkit.Infrastructure;

/// <summary>
/// Event bus interface for pub/sub communication.
/// Follows IDesign Method™ message bus pattern for cross-component communication.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="eventData">Event data</param>
    void Publish<T>(T eventData) where T : class;

    /// <summary>
    /// Subscribes to events of a specific type.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="handler">Event handler</param>
    void Subscribe<T>(Action<T> handler) where T : class;

    /// <summary>
    /// Unsubscribes from events of a specific type.
    /// </summary>
    /// <typeparam name="T">Event type</typeparam>
    /// <param name="handler">Event handler to remove</param>
    void Unsubscribe<T>(Action<T> handler) where T : class;
}
```

### File: `src/DocToolkit/Infrastructure/InMemoryEventBus.cs`

```csharp
using System.Collections.Concurrent;

namespace DocToolkit.Infrastructure;

/// <summary>
/// Simple in-memory event bus implementation.
/// For production, consider using a message queue (RabbitMQ, Azure Service Bus, etc.).
/// </summary>
public class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();

    public void Publish<T>(T eventData) where T : class
    {
        if (eventData == null)
        {
            return;
        }

        var eventType = typeof(T);
        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<T>)handler)(eventData);
                }
                catch
                {
                    // Log error but don't stop other handlers
                    // In production, use proper logging
                }
            }
        }
    }

    public void Subscribe<T>(Action<T> handler) where T : class
    {
        var eventType = typeof(T);
        _subscribers.AddOrUpdate(
            eventType,
            new List<Delegate> { handler },
            (key, existing) =>
            {
                existing.Add(handler);
                return existing;
            });
    }

    public void Unsubscribe<T>(Action<T> handler) where T : class
    {
        var eventType = typeof(T);
        if (_subscribers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
        }
    }
}
```

### File: `src/DocToolkit/Events/DocumentProcessedEvent.cs`

```csharp
namespace DocToolkit.Events;

/// <summary>
/// Event published when a document is processed.
/// </summary>
public class DocumentProcessedEvent
{
    public string FilePath { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
}
```

### File: `src/DocToolkit/Events/IndexBuiltEvent.cs`

```csharp
namespace DocToolkit.Events;

/// <summary>
/// Event published when a semantic index is built.
/// </summary>
public class IndexBuiltEvent
{
    public string IndexPath { get; set; } = string.Empty;
    public int EntryCount { get; set; }
    public int VectorCount { get; set; }
    public DateTime BuiltAt { get; set; } = DateTime.UtcNow;
}
```

### File: `src/DocToolkit/Events/GraphBuiltEvent.cs`

```csharp
namespace DocToolkit.Events;

/// <summary>
/// Event published when a knowledge graph is built.
/// </summary>
public class GraphBuiltEvent
{
    public string GraphPath { get; set; } = string.Empty;
    public int FileCount { get; set; }
    public int EntityCount { get; set; }
    public int TopicCount { get; set; }
    public DateTime BuiltAt { get; set; } = DateTime.UtcNow;
}
```

## Phase 4: Dependency Injection Setup

### File: `src/DocToolkit/Infrastructure/ServiceConfiguration.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using DocToolkit.Interfaces.Managers;
using DocToolkit.Interfaces.Engines;
using DocToolkit.Interfaces.Accessors;
using DocToolkit.Infrastructure;

namespace DocToolkit.Infrastructure;

/// <summary>
/// Service configuration for dependency injection.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Configures all services in the DI container.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDocToolkitServices(this IServiceCollection services)
    {
        // Infrastructure
        services.AddSingleton<IEventBus, InMemoryEventBus>();

        // Engines
        services.AddSingleton<IDocumentExtractionEngine, DocumentExtractionEngine>();
        services.AddSingleton<IEmbeddingEngine, EmbeddingEngine>();
        services.AddSingleton<ITextChunkingEngine, TextChunkingEngine>();
        services.AddSingleton<ISimilarityEngine, SimilarityEngine>();
        services.AddSingleton<IEntityExtractionEngine, EntityExtractionEngine>();
        services.AddSingleton<ISummarizationEngine, SummarizationEngine>();

        // Accessors
        services.AddSingleton<IVectorStorageAccessor, VectorStorageAccessor>();
        services.AddSingleton<ITemplateAccessor, TemplateAccessor>();
        services.AddSingleton<IProjectAccessor, ProjectAccessor>();

        // Managers
        services.AddScoped<ISemanticIndexManager, SemanticIndexManager>();
        services.AddScoped<ISemanticSearchManager, SemanticSearchManager>();
        services.AddScoped<IKnowledgeGraphManager, KnowledgeGraphManager>();
        services.AddScoped<ISummarizeManager, SummarizeManager>();

        return services;
    }
}
```

## Phase 5: Rename and Refactor Components

### Rename Operations

**Managers** (Move to `Managers/` folder):
1. `Services/SemanticIndexService.cs` → `Managers/SemanticIndexManager.cs`
   - Rename class: `SemanticIndexService` → `SemanticIndexManager`
   - Implement: `ISemanticIndexManager`
   - Update constructor to use interfaces
   - Add event publishing

2. `Services/SemanticSearchService.cs` → `Managers/SemanticSearchManager.cs`
   - Rename class: `SemanticSearchService` → `SemanticSearchManager`
   - Implement: `ISemanticSearchManager`
   - Update constructor to use interfaces

3. `Services/KnowledgeGraphService.cs` → `Managers/KnowledgeGraphManager.cs`
   - Rename class: `KnowledgeGraphService` → `KnowledgeGraphManager`
   - Implement: `IKnowledgeGraphManager`
   - Update constructor to use interfaces
   - Add event publishing

4. `Services/SummarizeService.cs` → `Managers/SummarizeManager.cs`
   - Rename class: `SummarizeService` → `SummarizeManager`
   - Implement: `ISummarizeManager`
   - Remove `SummarizeText`, `ExtractTopics`, `ExtractEntities` methods
   - Use `SummarizationEngine` and `EntityExtractionEngine`
   - Update constructor to use interfaces

**Engines** (Move to `Engines/` folder):
1. `Services/EmbeddingService.cs` → `Engines/EmbeddingEngine.cs`
   - Rename class: `EmbeddingService` → `EmbeddingEngine`
   - Implement: `IEmbeddingEngine`

2. `Services/DocumentExtractionService.cs` → `Engines/DocumentExtractionEngine.cs`
   - Rename class: `DocumentExtractionService` → `DocumentExtractionEngine`
   - Implement: `IDocumentExtractionEngine`

3. `Services/Engines/TextChunkingEngine.cs` → Already in correct location
   - Add: `ITextChunkingEngine` implementation

4. `Services/Engines/SimilarityEngine.cs` → Already in correct location
   - Add: `ISimilarityEngine` implementation

5. `Services/Engines/EntityExtractionEngine.cs` → Already in correct location
   - Add: `IEntityExtractionEngine` implementation

**Accessors** (Move to `Accessors/` folder):
1. `Services/VectorStorageService.cs` → `Accessors/VectorStorageAccessor.cs`
   - Rename class: `VectorStorageService` → `VectorStorageAccessor`
   - Implement: `IVectorStorageAccessor`

2. `Services/TemplateService.cs` → `Accessors/TemplateAccessor.cs`
   - Rename class: `TemplateService` → `TemplateAccessor`
   - Implement: `ITemplateAccessor`

3. `Services/ProjectService.cs` → `Accessors/ProjectAccessor.cs`
   - Rename class: `ProjectService` → `ProjectAccessor`
   - Implement: `IProjectAccessor`

## Phase 6: Update Commands to Use DI

### Update `Program.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;
using DocToolkit.Commands;
using DocToolkit.Infrastructure;

var services = new ServiceCollection();
services.AddDocToolkitServices();

var serviceProvider = services.BuildServiceProvider();

var app = new CommandApp<CommandTypeResolver>(serviceProvider);

app.Configure(config =>
{
    config.SetApplicationName("doc");
    config.ValidateExamples();

    // Register commands...
});

// Show banner
AnsiConsole.Write(
    new FigletText("Doc Toolkit")
        .LeftJustified()
        .Color(Color.Cyan1));

AnsiConsole.MarkupLine("[dim]Professional documentation generation made beautiful[/]");
AnsiConsole.WriteLine();

return app.Run(args);
```

### Create `CommandTypeResolver.cs`

```csharp
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace DocToolkit;

public class CommandTypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    public CommandTypeResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? Resolve(Type? type)
    {
        if (type == null)
        {
            return null;
        }

        return _serviceProvider.GetService(type);
    }

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
```

### Update Commands to Use DI

**Example: `IndexCommand.cs`**

```csharp
public sealed class IndexCommand : Command<IndexCommand.Settings>
{
    private readonly ISemanticIndexManager _indexManager;

    public IndexCommand(ISemanticIndexManager indexManager)
    {
        _indexManager = indexManager;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        // Use _indexManager instead of creating new instance
        bool result;
        using (_indexManager as IDisposable)
        {
            result = progress.Start(ctx =>
            {
                var task = ctx.AddTask("[green]Processing files...[/]");
                return _indexManager.BuildIndex(
                    settings.SourcePath,
                    settings.OutputPath,
                    settings.ChunkSize,
                    settings.ChunkOverlap,
                    progress => task.Increment(progress)
                );
            });
        }
        // ...
    }
}
```

## Summary of Changes

### Files Created (17)
- 1 Engine: `SummarizationEngine.cs`
- 11 Interfaces: All manager, engine, and accessor interfaces
- 1 Event Bus: `IEventBus.cs`, `InMemoryEventBus.cs`
- 3 Events: `DocumentProcessedEvent.cs`, `IndexBuiltEvent.cs`, `GraphBuiltEvent.cs`
- 1 DI Setup: `ServiceConfiguration.cs`
- 1 Type Resolver: `CommandTypeResolver.cs`

### Files Renamed (9)
- 4 Managers: `*Service` → `*Manager` (move to `Managers/`)
- 2 Engines: `*Service` → `*Engine` (move to `Engines/`)
- 3 Accessors: `*Service` → `*Accessor` (move to `Accessors/`)

### Files Modified (7 Commands + Program.cs)
- All Commands: Update to use DI and new names
- Program.cs: Setup DI container

### Benefits
1. ✅ Clear IDesign Method™ taxonomy
2. ✅ Zero manager-to-manager coupling
3. ✅ Dependency injection for testability
4. ✅ Event bus for decoupling
5. ✅ Interfaces for abstraction
6. ✅ Business logic in Engines (not Managers)
