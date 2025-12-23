# Refactoring Plan: IDesign Method™ Alignment

## Current State Analysis

### Component Classification

| Current Name | Type | Should Be | Status |
|-------------|------|-----------|--------|
| `SemanticIndexService` | Manager | `SemanticIndexManager` | ⚠️ Needs rename |
| `SemanticSearchService` | Manager | `SemanticSearchManager` | ⚠️ Needs rename |
| `KnowledgeGraphService` | Manager | `KnowledgeGraphManager` | ⚠️ Needs rename |
| `SummarizeService` | Manager | `SummarizeManager` | ⚠️ Needs rename + refactor |
| `EmbeddingService` | Engine | `EmbeddingEngine` | ⚠️ Needs rename |
| `DocumentExtractionService` | Engine | `DocumentExtractionEngine` | ⚠️ Needs rename |
| `VectorStorageService` | Accessor | `VectorStorageAccessor` | ⚠️ Needs rename |
| `TemplateService` | Accessor | `TemplateAccessor` | ⚠️ Needs rename |
| `ProjectService` | Accessor | `ProjectAccessor` | ⚠️ Needs rename |
| `ValidationService` | Utility | Keep as-is | ✅ OK |

### Manager-to-Manager Call Analysis

**Current State**: ✅ **No manager-to-manager calls detected**

All managers only call:
- Engines (DocumentExtractionService, EmbeddingService, Engines/*)
- Accessors (VectorStorageService, TemplateService, ProjectService)

### Business Logic in Managers

**Issue Found**: `SummarizeService` contains business logic that should be in Engines:
- `SummarizeText()` - Should be `SummarizationEngine`
- `ExtractTopics()` - Should use `EntityExtractionEngine` (already exists)
- `ExtractEntities()` - Should use `EntityExtractionEngine` (already exists)

## Refactoring Recommendations

### 1. Rename Components to IDesign Taxonomy

#### Managers (Workflow Volatility)
- `SemanticIndexService` → `SemanticIndexManager`
- `SemanticSearchService` → `SemanticSearchManager`
- `KnowledgeGraphService` → `KnowledgeGraphManager`
- `SummarizeService` → `SummarizeManager`

#### Engines (Algorithm Volatility)
- `EmbeddingService` → `EmbeddingEngine`
- `DocumentExtractionService` → `DocumentExtractionEngine`

#### Accessors (Storage Volatility)
- `VectorStorageService` → `VectorStorageAccessor`
- `TemplateService` → `TemplateAccessor`
- `ProjectService` → `ProjectAccessor`

### 2. Extract Business Logic from SummarizeManager

**Create New Engine**: `SummarizationEngine`
- Move `SummarizeText()` logic to engine
- Use existing `EntityExtractionEngine` for topics/entities

### 3. Merge Opportunities

**Analysis**: 
- `SummarizeManager` and `KnowledgeGraphManager` both process documents
- Both extract entities and topics
- Different outputs (summary document vs. graph)

**Recommendation**: **Keep Separate**
- Different use cases (summary document vs. knowledge graph)
- Different outputs (context.md vs. graph.json/graph.gv)
- Keep separate for clarity and single responsibility

**However**: Extract shared logic to Engines (already done with EntityExtractionEngine)

### 4. Decoupling Recommendations

#### A. Dependency Injection Container

**Current Problem**: Managers create dependencies directly (tight coupling)

**Solution**: Use dependency injection

```csharp
// Instead of:
public SemanticIndexManager()
{
    _extractor = new DocumentExtractionEngine();
    _embedding = new EmbeddingEngine();
    // ...
}

// Use:
public SemanticIndexManager(
    IDocumentExtractionEngine extractor,
    IEmbeddingEngine embedding,
    IVectorStorageAccessor storage,
    ITextChunkingEngine chunkingEngine)
{
    _extractor = extractor;
    _embedding = embedding;
    // ...
}
```

**Benefits**:
- Testability (can inject mocks)
- Flexibility (can swap implementations)
- Decoupling (managers don't know concrete types)

#### B. Event-Driven Architecture (Pub/Sub)

**Use Case**: When document processing completes, multiple systems might need to know:
- Index needs updating
- Graph needs updating
- Summary needs updating
- Analytics needs updating

**Current Problem**: Tight coupling - if we want to add a new feature, we modify managers

**Solution**: Event Bus Pattern

```csharp
// Event definitions
public class DocumentProcessedEvent
{
    public string FilePath { get; set; }
    public string ExtractedText { get; set; }
    public DateTime ProcessedAt { get; set; }
}

public class IndexBuiltEvent
{
    public string IndexPath { get; set; }
    public int EntryCount { get; set; }
}

// Event bus interface
public interface IEventBus
{
    void Publish<T>(T eventData) where T : class;
    void Subscribe<T>(Action<T> handler) where T : class;
}

// Usage in managers
public class SemanticIndexManager
{
    private readonly IEventBus _eventBus;
    
    public bool BuildIndex(...)
    {
        // ... build index ...
        
        // Publish event instead of direct calls
        _eventBus.Publish(new IndexBuiltEvent 
        { 
            IndexPath = outputPath,
            EntryCount = entries.Count 
        });
    }
}

// Other managers subscribe
public class KnowledgeGraphManager
{
    public KnowledgeGraphManager(IEventBus eventBus)
    {
        eventBus.Subscribe<IndexBuiltEvent>(OnIndexBuilt);
    }
    
    private void OnIndexBuilt(IndexBuiltEvent evt)
    {
        // Automatically rebuild graph when index is built
    }
}
```

**Benefits**:
- Zero coupling between managers
- Easy to add new subscribers
- Asynchronous processing possible
- Follows IDesign Method™ message bus pattern

#### C. Service Interfaces (Abstraction Layer)

**Create Interfaces for All Components**:

```csharp
// Manager interfaces
public interface ISemanticIndexManager
{
    bool BuildIndex(string sourcePath, string outputPath, int chunkSize, int chunkOverlap, Action<double>? progressCallback = null);
}

// Engine interfaces
public interface IDocumentExtractionEngine
{
    string ExtractText(string filePath);
}

public interface IEmbeddingEngine
{
    float[] GenerateEmbedding(string text);
}

// Accessor interfaces
public interface IVectorStorageAccessor
{
    void SaveVectors(float[][] vectors, string indexPath);
    float[][] LoadVectors(string indexPath);
    // ...
}
```

**Benefits**:
- Managers depend on abstractions, not concretions
- Easy to swap implementations
- Better testability

### 5. Recommended Architecture Pattern

```
┌─────────────────────────────────────────────────────────┐
│                    Client Layer                          │
│  (Commands)                                              │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│                    Manager Layer                         │
│  (Workflow Orchestration)                                │
│  ┌──────────────────┐  ┌──────────────────┐           │
│  │SemanticIndexMgr  │  │SemanticSearchMgr  │           │
│  └──────────────────┘  └──────────────────┘           │
│  ┌──────────────────┐  ┌──────────────────┐           │
│  │KnowledgeGraphMgr │  │SummarizeManager   │           │
│  └──────────────────┘  └──────────────────┘           │
└─────────────────────────────────────────────────────────┘
        ↓                    ↓                    ↓
┌──────────────┐  ┌──────────────┐  ┌──────────────┐
│ Engine Layer │  │ Accessor Layer│  │ Event Bus    │
│              │  │               │  │ (Pub/Sub)    │
└──────────────┘  └──────────────┘  └──────────────┘
```

**Key Points**:
- Managers call Engines and Accessors (not other Managers)
- Managers publish events to Event Bus
- Managers subscribe to events from Event Bus
- Zero direct manager-to-manager coupling

## Implementation Plan

### Phase 1: Rename Components (Breaking Changes)
1. Rename all Services to proper taxonomy (Manager/Engine/Accessor)
2. Update all Commands to use new names
3. Update all internal references

### Phase 2: Extract Business Logic
1. Create `SummarizationEngine` for text summarization
2. Update `SummarizeManager` to use `EntityExtractionEngine` and `SummarizationEngine`
3. Remove duplicate logic

### Phase 3: Add Interfaces
1. Create interfaces for all Managers, Engines, Accessors
2. Update implementations to use interfaces
3. Update constructors to accept interfaces

### Phase 4: Dependency Injection
1. Add DI container (Microsoft.Extensions.DependencyInjection)
2. Register all components in DI container
3. Update Commands to use DI

### Phase 5: Event Bus (Optional - Future)
1. Create event bus interface and implementation
2. Define event types
3. Update managers to publish/subscribe to events
4. Remove direct dependencies where possible

## Detailed Changes

### Files to Rename

**Managers**:
- `Services/SemanticIndexService.cs` → `Managers/SemanticIndexManager.cs`
- `Services/SemanticSearchService.cs` → `Managers/SemanticSearchManager.cs`
- `Services/KnowledgeGraphService.cs` → `Managers/KnowledgeGraphManager.cs`
- `Services/SummarizeService.cs` → `Managers/SummarizeManager.cs`

**Engines**:
- `Services/EmbeddingService.cs` → `Engines/EmbeddingEngine.cs`
- `Services/DocumentExtractionService.cs` → `Engines/DocumentExtractionEngine.cs`
- `Services/Engines/*` → Already in correct location ✅

**Accessors**:
- `Services/VectorStorageService.cs` → `Accessors/VectorStorageAccessor.cs`
- `Services/TemplateService.cs` → `Accessors/TemplateAccessor.cs`
- `Services/ProjectService.cs` → `Accessors/ProjectAccessor.cs`

### Files to Create

1. `Engines/SummarizationEngine.cs` - Text summarization algorithm
2. `Interfaces/IManagers/*.cs` - Manager interfaces
3. `Interfaces/IEngines/*.cs` - Engine interfaces
4. `Interfaces/IAccessors/*.cs` - Accessor interfaces
5. `Infrastructure/IEventBus.cs` - Event bus interface
6. `Infrastructure/InMemoryEventBus.cs` - Simple in-memory event bus implementation
7. `Infrastructure/ServiceConfiguration.cs` - DI container setup

### Files to Modify

- All Commands (update to use new names and DI)
- All Managers (add interfaces, use DI, publish events)
- Program.cs (setup DI container)

## Decoupling Strategy Summary

### Immediate (High Priority)
1. ✅ Rename to proper taxonomy
2. ✅ Extract business logic from SummarizeManager
3. ⚠️ Add interfaces for all components
4. ⚠️ Use dependency injection

### Short Term (Medium Priority)
5. ⚠️ Implement simple event bus for cross-manager communication
6. ⚠️ Move file I/O operations to Accessors (currently in Managers)

### Long Term (Low Priority)
7. ⚠️ Consider message queue for async processing
8. ⚠️ Add retry policies and circuit breakers
9. ⚠️ Add observability (logging, metrics, tracing)

## Benefits of Refactoring

1. **Clear Taxonomy**: Component names match IDesign Method™
2. **Zero Manager Coupling**: Managers don't call each other
3. **Testability**: All components can be mocked
4. **Flexibility**: Easy to swap implementations
5. **Extensibility**: Easy to add new features via events
6. **Maintainability**: Changes isolated to specific components
