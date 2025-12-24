# Architecture Design Document
## Documentation Toolkit

**Version**: 1.0  
**Date**: 2024  
**Status**: Current

---

## 1. Executive Summary

The Documentation Toolkit is built on a service-oriented architecture following the IDesign Method™ principles. It uses volatility-based decomposition to organize components, ensuring maintainability and extensibility. The architecture consists of four primary layers: Clients (CLI commands), Managers (orchestration), Engines (business logic), and Accessors (storage abstraction). All components communicate through well-defined service boundaries with dependency injection and an event-driven architecture for decoupled cross-component communication.

---

## 2. Purpose & Scope

This architecture document covers:
- System architecture and component organization
- Service boundaries and call chains
- Data flow and integration points
- Security and deployment considerations
- Scalability and performance characteristics

**Out of Scope**: Implementation details of individual algorithms, specific file format parsers, or third-party library internals.

---

## 3. Architecture Overview

The Documentation Toolkit follows the IDesign Method™ architecture pattern, which emphasizes **volatility-based decomposition** rather than functional decomposition. Components are organized based on what could change over time:

1. **User Interface Volatility** (Client Layer): CLI commands that could become Web UI or API
2. **Workflow Volatility** (Manager Layer): Orchestration logic that could change
3. **Algorithm Volatility** (Engine Layer): Business logic algorithms that could change
4. **Storage Volatility** (Accessor Layer): Storage technology and format that could change

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                   Documentation Toolkit                 │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Templates  │  │   Scripts    │  │    Rules     │  │
│  │   (Markdown) │  │ (PowerShell/ │  │  (Cursor)    │  │
│  │              │  │    Python)    │  │              │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│                                                           │
│  ┌──────────────────────────────────────────────────┐  │
│  │         Semantic Processing Layer                 │  │
│  │  - Text Extraction (PDF, DOCX, PPTX, Images)    │  │
│  │  - Vector Embeddings (ONNX Runtime)             │  │
│  │  - Knowledge Graph Generation                   │  │
│  └──────────────────────────────────────────────────┘  │
│                                                           │
│  ┌──────────────────────────────────────────────────┐  │
│  │         Project Workspace Generator               │  │
│  │  - Directory Structure                            │  │
│  │  - Git Initialization                             │  │
│  │  - Cursor Configuration                           │  │
│  └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

---

## 4. System Context Diagram

The Documentation Toolkit operates as a standalone CLI application with the following external dependencies:

```
┌─────────────────────────────────────────────────────────┐
│                    User (CLI)                            │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌─────────────────────────────────────────────────────────┐
│              Documentation Toolkit                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐ │
│  │   Commands   │  │   Managers   │  │   Engines    │ │
│  └──────────────┘  └──────────────┘  └──────────────┘ │
│  ┌──────────────┐  ┌──────────────┐                   │
│  │  Accessors   │  │  Event Bus   │                   │
│  └──────────────┘  └──────────────┘                   │
└─────────────────────────────────────────────────────────┘
         │                    │                    │
         ↓                    ↓                    ↓
┌──────────────┐    ┌──────────────┐    ┌──────────────┐
│ File System  │    │  SQLite DB   │    │  ONNX Model  │
│ (Templates,  │    │  (Events)    │    │  (Embeddings)│
│  Indexes,    │    │              │    │              │
│  Documents)  │    │              │    │              │
└──────────────┘    └──────────────┘    └──────────────┘
```

**External Dependencies**:
- File System: Templates, source documents, generated indexes
- SQLite Database: Event persistence (`%LocalAppData%\DocToolkit\events.db`)
- ONNX Model: Semantic embedding model (`all-MiniLM-L6-v2.onnx`)

---

## 5. Component Architecture

### Component Taxonomy (IDesign Method™)

| Component Type | Components | Volatility Encapsulated |
|---------------|------------|------------------------|
| **Client** | `InitCommand`, `GenerateCommand`, `IndexCommand`, `SearchCommand`, `GraphCommand`, `SummarizeCommand`, `ValidateCommand` | User interface volatility (CLI could become Web/API) |
| **Manager** | `SemanticIndexManager`, `SemanticSearchManager`, `KnowledgeGraphManager`, `SummarizeManager` | Workflow volatility (orchestration logic) |
| **Engine** | `EmbeddingEngine`, `DocumentExtractionEngine`, `TextChunkingEngine`, `SimilarityEngine`, `EntityExtractionEngine`, `SummarizationEngine` | Algorithm volatility (embedding models, extraction logic) |
| **Accessor** | `VectorStorageAccessor`, `TemplateAccessor`, `ProjectAccessor` | Storage volatility (file system, storage technology) |

### Static Architecture View

```
┌─────────────────────────────────────────────────────────┐
│                    Client Layer                          │
│  (UI Volatility - Initiation)                           │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ │
│  │ InitCmd  │ │ GenCmd   │ │ IndexCmd │ │SearchCmd │ │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐              │
│  │GraphCmd │ │Summarize │ │ValidateCmd│              │
│  └──────────┘ └──────────┘ └──────────┘              │
└─────────────────────────────────────────────────────────┘
                        ↓ (Service Boundary)
┌─────────────────────────────────────────────────────────┐
│                    Manager Layer                         │
│  (Workflow Volatility - Orchestration)                  │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐  │
│  │SemanticIndex │ │SemanticSearch │ │KnowledgeGraph │  │
│  └──────────────┘ └──────────────┘ └──────────────┘  │
│  ┌──────────────┐                                      │
│  │SummarizeMgr │                                      │
│  └──────────────┘                                      │
└─────────────────────────────────────────────────────────┘
                        ↓ (Service Boundary)
┌─────────────────────────────────────────────────────────┐
│         Engine Layer          │      Accessor Layer     │
│  (Algorithm Volatility)       │  (Storage Volatility)   │
│  ┌──────────────┐             │  ┌──────────────┐      │
│  │EmbeddingEng  │             │  │VectorStorage │      │
│  └──────────────┘             │  └──────────────┘      │
│  ┌──────────────┐             │  ┌──────────────┐      │
│  │DocumentExtract│             │  │TemplateAccessor│     │
│  └──────────────┘             │  └──────────────┘      │
│  ┌──────────────┐             │  ┌──────────────┐      │
│  │TextChunking │             │  │ProjectAccessor│     │
│  └──────────────┘             │  └──────────────┘      │
│  ┌──────────────┐                                      │
│  │SimilarityEng │                                      │
│  └──────────────┘                                      │
│  ┌──────────────┐                                      │
│  │EntityExtract │                                      │
│  └──────────────┘                                      │
│  ┌──────────────┐                                      │
│  │Summarization │                                      │
│  └──────────────┘                                      │
└─────────────────────────────────────────────────────────┘
                        ↓ (Data Boundary)
┌─────────────────────────────────────────────────────────┐
│                    Model Layer                           │
│  (Data Layer)                                           │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐  │
│  │ IndexEntry   │ │ SearchResult  │ │  GraphData    │  │
│  └──────────────┘ └──────────────┘ └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### Closed Architecture Pattern

- **Clients** can only call **Managers** (one tier down)
- **Managers** can call **Engines** and **Accessors** (one tier down)
- **Engines** are pure functions (accept data as parameters, no direct Accessor calls)
- **Accessors** are dumb CRUD operations (no business logic)
- **Models** are pure data structures (no dependencies)

---

## 6. Data Flow Diagrams

### Use Case: Build Semantic Index

```
IndexCommand (Client)
  ↓ (Service Boundary)
SemanticIndexManager (Manager)
  ↓
  ├─→ DocumentExtractionEngine.ExtractText(filePath) → string
  ├─→ TextChunkingEngine.ChunkText(text, chunkSize, chunkOverlap) → List<string>
  ├─→ EmbeddingEngine.GenerateEmbedding(chunk) → float[]
  └─→ VectorStorageAccessor.SaveVectors(vectors, entries, indexPath)
       ↓
       File System (vectors.bin, index.json)
```

### Use Case: Semantic Search

```
SearchCommand (Client)
  ↓ (Service Boundary)
SemanticSearchManager (Manager)
  ↓
  ├─→ VectorStorageAccessor.LoadVectors(indexPath) → float[][]
  ├─→ VectorStorageAccessor.LoadIndex(indexPath) → List<IndexEntry>
  ├─→ EmbeddingEngine.GenerateEmbedding(query) → float[]
  ├─→ SimilarityEngine.FindTopSimilar(queryVector, vectors, topK) → List<(index, score)>
  └─→ Format results → List<SearchResult>
       ↓
       Display to user
```

### Use Case: Knowledge Graph Generation

```
GraphCommand (Client)
  ↓ (Service Boundary)
KnowledgeGraphManager (Manager)
  ↓
  ├─→ DocumentExtractionEngine.ExtractText(filePath) → string (for each file)
  ├─→ EntityExtractionEngine.ExtractEntities(text) → List<string>
  ├─→ EntityExtractionEngine.ExtractTopics(text, topN) → List<string>
  └─→ Build graph structure → GraphData
       ↓
       File System (graph.json, graph.md, graph.gv)
```

---

## 7. Integration Architecture

### Event Bus Architecture

The toolkit uses an in-memory pub/sub event bus with SQLite persistence:

```
Manager (Publisher)
    ↓
EventBus (Pub/Sub)
    ↓
EventPersistence (SQLite)
    ↓
Subscribers (Handlers)
```

**Event Types**:
- `IndexBuiltEvent`: Published when semantic index is built
- `GraphBuiltEvent`: Published when knowledge graph is built
- `SummaryCreatedEvent`: Published when summary is created
- `DocumentProcessedEvent`: Published for each document processed during indexing

**Benefits**:
- Zero coupling between managers
- Reliable event delivery (persisted to database)
- Automatic retry of failed events (max 3 retries, 5-minute intervals)

### Dependency Injection

All components use constructor injection with interfaces:

- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Service Lifetimes**: 
  - Engines: Singleton (stateless or expensive to create)
  - Accessors: Singleton (stateless)
  - Managers: Scoped (may have state per operation)

---

## 8. Security Architecture

### Authentication & Authorization

**Current State**: No authentication/authorization required (local CLI tool)

**Future State** (if services become remote or multi-user):
- **Authentication**: Solid bar at service boundaries
- **Authorization**: Patterned bar at service boundaries
- Each service authenticates its immediate callers
- Authorization checks at service entry points

### Data Protection

- All processing local (no network access)
- User-provided source files only
- No sensitive data stored in embeddings (only text chunks)
- Temporary files cleaned up after execution
- Events persisted to local SQLite database only

### Logging

- Structured logging using Microsoft.Extensions.Logging
- Logs errors and internal operations
- No user data in logs
- Console output for user-facing messages (Spectre.Console)

---

## 9. Deployment Architecture

### Current Deployment

**Single Process**: All services run in a single process (CLI executable)

```
┌────────────────────────────────────┐
│      doc.exe Process               │
│  ┌──────────────────────────────┐  │
│  │ DocToolkit Assembly          │  │
│  │ (All Commands, Managers,     │  │
│  │  Engines, Accessors, Models) │  │
│  └──────────────────────────────┘  │
└────────────────────────────────────┘
```

### Future Enhancements

If process isolation is needed:
- **Fault Isolation**: Embedding service crashes don't affect document generation
- **Security Isolation**: Validation service runs with restricted permissions
- **Scalability**: Multiple embedding workers

---

## 10. Scalability & Performance

### Expected Load

- **Document Indexing**: 100-1000 documents per project
- **Search Queries**: 10-100 queries per session
- **Knowledge Graph**: 50-500 files per project

### Performance Targets

- Document generation: < 2 seconds
- Semantic indexing: < 30 seconds for 100 documents
- Semantic search: < 200ms average query time
- Knowledge graph generation: < 15 seconds for 100 documents
- Memory usage: < 200MB during indexing operations

### Scaling Strategies

- **Memory Optimization**: Pre-allocated collections, batch processing
- **Chunking Strategy**: Configurable chunk sizes (default: 800 words, 200 overlap)
- **Embedding Model**: Lightweight (all-MiniLM-L6-v2) for speed
- **Batch Processing**: Files processed sequentially to manage memory

---

## 11. Operational Considerations

### Monitoring

- **Memory Monitoring**: Optional `--monitor-memory` flag for all operations
- **Progress Reporting**: Real-time progress bars for long-running operations
- **Event Logging**: All events persisted to SQLite database

### Alerting

- **Error Handling**: Graceful error handling with clear messages
- **Validation**: Setup validation before operations
- **Event Retry**: Automatic retry of failed events

### Maintenance

- **Temporary Files**: Created in system temp directory, cleaned up after use
- **Event Database**: User-controlled retention (no automatic deletion)
- **Index Regeneration**: Indexes can be regenerated from source

### Disaster Recovery

- **Index Regeneration**: All indexes can be rebuilt from source documents
- **Event Persistence**: Events persisted to SQLite (survives crashes)
- **Template Backup**: Templates stored in version control

---

## 12. Assumptions & Constraints

### Assumptions

- Users have .NET 9.0 SDK installed
- Users have ONNX model file available
- Source documents are in supported formats
- Users have sufficient disk space for indexes
- Local file system access available

### Constraints

- Local file system only (no cloud storage)
- Single-user operation (no multi-user support)
- Limited to supported file formats (PDF, DOCX, PPTX, TXT, MD, CSV, JSON)
- ONNX model size (~90MB) required
- Memory constraints for large document collections

---

## 13. Risks & Mitigations

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| ONNX model not found | High | Medium | Clear error message with download instructions |
| Large document collections cause memory issues | Medium | Low | Memory monitoring, pre-allocation, batch processing |
| Corrupted files cause crashes | Medium | Low | Graceful error handling, skip corrupted files |
| Performance degradation with large indexes | Medium | Low | Benchmark tests, optimization, configurable chunk sizes |
| Event bus database corruption | Low | Very Low | SQLite transaction support, error recovery |

---

## 14. Decision Log

| Date | Decision | Rationale | Alternatives Considered |
|------|----------|-----------|------------------------|
| 2024 | IDesign Method™ architecture | Volatility-based decomposition, maintainability | Traditional layered architecture |
| 2024 | ONNX Runtime for embeddings | C# native, no Python dependency | Python with sentence-transformers |
| 2024 | SQLite for event persistence | Lightweight, embedded, no external dependencies | File-based, in-memory only |
| 2024 | Binary vector storage | Performance, compact format | JSON, CSV, NumPy format |
| 2024 | Dependency injection | Testability, loose coupling | Direct instantiation |
| 2024 | Event bus architecture | Decoupled communication | Direct method calls |

---

## 15. Appendices / References

### References
- [IDesign Method™ Version 2.5](docs/IDesign%20Method.pdf)
- [IDesign C# Coding Standard 3.1](docs/IDesign%20C%23%20Coding%20Standard%203.1.pdf)
- [System Design Document](docs/design.md)
- [Technical Documentation](docs/TECHNICAL-DOCUMENTATION.md)
- [PRD](docs/PRD-Documentation-Toolkit.md)

### Diagrams
- All architecture diagrams available in `docs/design.md`
- Component taxonomy documented in code XML comments
