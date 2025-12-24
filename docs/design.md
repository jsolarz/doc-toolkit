# Documentation Toolkit - System Design
*Following IDesign Method™ Version 2.5*

## Overview

The Documentation Toolkit is a reusable framework for generating professional documents and building semantic knowledge bases for projects. It provides templates, automation scripts, and AI-powered indexing capabilities.

## Architecture (IDesign Method™)

### Volatility-Based Decomposition

The toolkit follows the IDesign Method™ principle of **volatility-based decomposition**. Components are organized based on what could change over time, not by functionality.

#### Volatilities Identified

1. **User Interface Volatility** (Client)
   - CLI commands could change (add new commands, change interfaces)
   - Future: Web UI, API, scheduled tasks

2. **Workflow Volatility** (Manager)
   - Orchestration logic could change (indexing workflow, search workflow)
   - Future: Different indexing strategies, search algorithms

3. **Algorithm Volatility** (Engine)
   - Embedding model could change (different ONNX models)
   - Extraction logic could change (new file formats, OCR improvements)

4. **Storage Volatility** (Accessor)
   - Vector storage format could change (file-based → database)
   - Template location could change (local → cloud)
   - File system operations could change (local → cloud storage)

### Service-Oriented Architecture

The toolkit follows a service-oriented architecture with clear boundaries and closed architecture pattern. Components are organized into the IDesign Method™ taxonomy: Clients, Managers, Engines, and Accessors.

### High-Level Components

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
│  │  - Vector Embeddings (Sentence Transformers)    │  │
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

## Service Layer Architecture (IDesign Method™)

### Component Taxonomy

The architecture follows the IDesign Method™ component taxonomy:

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
│  │SummarizeSvc  │                                      │
│  └──────────────┘                                      │
└─────────────────────────────────────────────────────────┘
                        ↓ (Service Boundary)
┌─────────────────────────────────────────────────────────┐
│         Engine Layer          │      Accessor Layer     │
│  (Algorithm Volatility)       │  (Storage Volatility)   │
│  ┌──────────────┐             │  ┌──────────────┐      │
│  │EmbeddingSvc  │             │  │VectorStorage │      │
│  └──────────────┘             │  └──────────────┘      │
│  ┌──────────────┐             │  ┌──────────────┐      │
│  │DocumentExtract│             │  │TemplateService│     │
│  └──────────────┘             │  └──────────────┘      │
│                               │  ┌──────────────┐      │
│                               │  │ProjectService│      │
│                               │  └──────────────┘      │
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

**Note**: Engines and Accessors are at the same level. Managers call both, but Engines should not call Accessors directly (Engines receive data as parameters).

### Closed Architecture Pattern

- **Commands** can only call **Services** (one tier down)
- **Services** can only call **Models** or other **Services** in the same layer (for orchestration)
- **Models** are data structures only (no dependencies)
- No cross-tier calls (e.g., Commands cannot directly access Models)

### Call Chain Examples

**Use Case: Build Semantic Index**

```
IndexCommand (Client)
  ↓ (Service Boundary - Client → Manager)
SemanticIndexManager (Manager)
  ↓ (Service Boundary - Manager → Engine/Accessor)
DocumentExtractionEngine (Engine) → EmbeddingEngine (Engine) → VectorStorageAccessor (Accessor)
  ↓ (Data Boundary)
IndexEntry (Model)
```

**Pattern**: Client → Manager → Engine/Accessor (avoiding "staircase" and "fork" patterns)

**Use Case: Semantic Search**

```
SearchCommand (Client)
  ↓ (Service Boundary)
SemanticSearchManager (Manager)
  ↓ (Service Boundary)
VectorStorageAccessor (Accessor) → EmbeddingEngine (Engine) → SimilarityEngine (Engine)
  ↓ (Data Boundary)
SearchResult (Model)
```

**Pattern**: Manager orchestrates Accessor (get data) and Engine (process data). Engine receives data as parameter (pure function).

## Project Structure

The project follows a clean folder structure with infrastructure separated from business logic:

```
src/DocToolkit/
├── Accessors/          # Storage volatility (Accessors)
├── Engines/            # Algorithm volatility (Engines)
├── Managers/            # Workflow volatility (Managers)
└── ifx/                # Infrastructure folder
    ├── Commands/        # UI volatility (Clients)
    ├── Events/          # Event definitions
    ├── Infrastructure/  # DI, Event Bus, etc.
    ├── Interfaces/      # All interfaces
    ├── Models/          # Data models
    └── Program.cs       # Application entry point
```

## Assembly Allocation

All components are in a single assembly: `DocToolkit.dll`

```
┌────────────────────────────────────┐
│      DocToolkit Assembly           │
│  ┌──────────────────────────────┐  │
│  │ Commands (Client Layer)      │  │
│  └──────────────────────────────┘  │
│  ┌──────────────────────────────┐  │
│  │ Managers (Orchestration)     │  │
│  └──────────────────────────────┘  │
│  ┌──────────────────────────────┐  │
│  │ Engines + Accessors          │  │
│  └──────────────────────────────┘  │
│  ┌──────────────────────────────┐  │
│  │ Models (Data Layer)          │  │
│  └──────────────────────────────┘  │
│  ┌──────────────────────────────┐  │
│  │ Infrastructure (DI, Events)  │  │
│  └──────────────────────────────┘  │
└────────────────────────────────────┘
```

## Run-Time Process Allocation

All services run in a single process (the CLI executable):

```
┌────────────────────────────────────┐
│      doc.exe Process               │
│  ┌──────────────────────────────┐  │
│  │ DocToolkit Assembly          │  │
│  │ (All Commands, Services,     │  │
│  │  Models)                     │  │
│  └──────────────────────────────┘  │
└────────────────────────────────────┘
```

**Note**: Currently single-process. Future enhancements may require process isolation for:
- Fault isolation (embedding service crashes don't affect document generation)
- Security isolation (validation service runs with restricted permissions)
- Scalability (multiple embedding workers)

## Identity Management

All services run under the same identity (the user running the CLI):

```
┌────────────────────────────────────┐
│   Identity: Current User           │
│  ┌──────────────────────────────┐  │
│  │ All Services                 │  │
│  │ (Same Identity)              │  │
│  └──────────────────────────────┘  │
└────────────────────────────────────┘
```

**Future Consideration**: If services are split into separate processes, identity boundaries may be needed for security isolation.

## Authentication and Authorization

**Current State**: No authentication/authorization required (local CLI tool)

**Future State** (if services become remote or multi-user):
- **Authentication**: Solid bar at service boundaries
- **Authorization**: Patterned bar at service boundaries
- Each service authenticates its immediate callers
- Authorization checks at service entry points

## Transaction Boundaries

**Current State**: No transactions (file-based operations)

**Future State** (if database is introduced):
- Transaction boundaries marked with box notation
- Document transaction roots and participating services
- Transaction flow across service boundaries

## Event Bus Architecture

**Current State**: Event Bus implemented with SQLite persistence

The toolkit includes a robust event bus for decoupled cross-component communication:

- **Event Bus**: In-memory pub/sub with SQLite persistence
- **Event Persistence**: All events saved to SQLite database (`events.db`)
- **Retry Policies**: Automatic retry of failed events (max 3 retries, 5-minute intervals)
- **Event Types**: `IndexBuiltEvent`, `GraphBuiltEvent`, `SummaryCreatedEvent`, `DocumentProcessedEvent`

**Architecture**:
```
Manager (Publisher)
    ↓
EventBus (Pub/Sub)
    ↓
EventPersistence (SQLite)
    ↓
Subscribers (Handlers)
```

**Benefits**:
- Zero coupling between managers
- Reliable event delivery (persisted to database)
- Automatic retry of failed events
- Follows IDesign Method™ message bus pattern

**Event Subscriptions**:
- Event subscriptions configured at application startup
- Console logging for major events (IndexBuiltEvent, GraphBuiltEvent, SummaryCreatedEvent, DocumentProcessedEvent)
- Optional cross-manager subscriptions can be enabled (e.g., auto-rebuild graph when index changes)
- All subscriptions managed centrally in `EventSubscriptions.ConfigureSubscriptions()`

## Dependency Injection

**Current State**: Full dependency injection implemented

All components use constructor injection with interfaces:

- **DI Container**: Microsoft.Extensions.DependencyInjection
- **Service Lifetimes**: 
  - Engines: Singleton (stateless or expensive to create)
  - Accessors: Singleton (stateless)
  - Managers: Scoped (may have state per operation)
- **Type Resolution**: Custom `CommandTypeRegistrar` for Spectre.Console.Cli integration

**Benefits**:
- Loose coupling (depend on interfaces, not concrete types)
- Testability (easy to inject mocks)
- Flexibility (swap implementations without code changes)

## Component Descriptions

### 1. Command Layer (Application)
**Purpose**: User interface and command orchestration.

**Components**:
- `InitCommand`: Project initialization
- `GenerateCommand`: Document generation
- `IndexCommand`: Semantic indexing
- `SearchCommand`: Semantic search
- `GraphCommand`: Knowledge graph generation
- `SummarizeCommand`: Document summarization
- `ValidateCommand`: Setup validation

**Service Boundaries**: Commands call Services only (closed architecture)

### 2. Manager Layer (Orchestration)
**Purpose**: Encapsulate workflow volatility. Knows "when" to do things, not "how".

**Managers**:
- `SemanticIndexManager`: Orchestrates indexing workflow (extract → embed → store)
- `SemanticSearchManager`: Orchestrates search workflow (load → embed → search)
- `KnowledgeGraphManager`: Orchestrates graph generation workflow (extract → analyze → build)
- `SummarizeManager`: Orchestrates summarization workflow (extract → summarize → output)

**Service Boundaries**: Managers call Engines and Accessors. They orchestrate the workflow but don't contain business logic.

### 3. Engine Layer (Business Logic)
**Purpose**: Encapsulate algorithm volatility. Knows "how" to do things. Pure functions (no I/O).

**Engines**:
- `EmbeddingEngine`: Encapsulates embedding algorithm (ONNX model could change)
- `DocumentExtractionEngine`: Encapsulates extraction logic (format support could change)
- `TextChunkingEngine`: Encapsulates text chunking strategy (size, overlap, method)
- `SimilarityEngine`: Encapsulates similarity calculation (cosine, euclidean, etc.)
- `EntityExtractionEngine`: Encapsulates entity/topic extraction (regex, NLP, ML models)
- `SummarizationEngine`: Encapsulates summarization strategy (extractive, abstractive)

**Service Boundaries**: Engines are "pure" - they accept data as parameters and return results. They should not call Accessors directly.

### 4. Accessor Layer (Resource Abstraction)
**Purpose**: Encapsulate storage volatility. Knows "where" data is stored. Dumb CRUD operations.

**Accessors**:
- `VectorStorageAccessor`: Abstracts vector storage (file format could change to database)
- `TemplateAccessor`: Abstracts template storage (location could change to cloud)
- `ProjectAccessor`: Abstracts file system operations (could move to cloud storage)

**Service Boundaries**: Accessors are "dumb" - they perform CRUD operations only. No business logic.

### 5. Model Layer (Data)
**Purpose**: Data structures and contracts. Stable business concepts.

**Models**:
- `IndexEntry`: Semantic index entry
- `SearchResult`: Search result
- `GraphData`: Knowledge graph structure

**Data Boundary**: Models are pure data structures (no dependencies). Components should pass IDs or lightweight data, not fat entities.

### 6. Validation Service
**Purpose**: Setup validation (special case - utility service).

**Note**: `ValidationService` is a utility service that can be used by all layers. It doesn't fit the standard taxonomy but is needed for setup validation.

### 4. Legacy Script Layer
**Purpose**: PowerShell scripts for legacy support.

**Scripts**:
- `init-project.ps1`: Creates new project workspace
- `generate-doc.ps1`: Generates documents from templates
- `semantic-index.ps1`: Builds vector embeddings (legacy)
- `semantic-search.ps1`: Queries semantic index (legacy)
- `build-knowledge-graph.ps1`: Extracts entities (legacy)
- `summarize-source.ps1`: Creates context summaries (legacy)

**Note**: Scripts are being phased out in favor of C# services

## Data Models

### Semantic Index Entry
```json
{
  "file": "document.pdf",
  "path": "/path/to/document.pdf",
  "chunk": "Text content chunk..."
}
```

### Knowledge Graph Node
```json
{
  "id": "entity:ProjectName",
  "type": "entity",
  "name": "ProjectName"
}
```

### Knowledge Graph Edge
```json
{
  "type": "FILE_CONTAINS_ENTITY",
  "from": "file:/path/to/file",
  "to": "entity:EntityName",
  "weight": 5
}
```

## Workflows

### Document Generation Workflow
1. User selects document type
2. Script copies template to docs/ directory
3. User edits template with project-specific content
4. Document is ready for use

### Semantic Indexing Workflow
1. Script scans source/ directory
2. Extracts text from supported file types
3. Chunks text into overlapping segments
4. Generates embeddings for each chunk
5. Saves vectors and metadata to semantic-index/

### Knowledge Graph Workflow
1. Script processes all source files
2. Extracts entities (capitalized phrases)
3. Extracts topics (frequent meaningful words)
4. Builds relationships (co-occurrence, containment)
5. Generates JSON, Graphviz, and Markdown outputs

## Integration Points

### Cursor IDE Integration
- Rules files (`.mdc`) for document generation guidance
- Snippets for quick template insertion
- Configuration files linking to toolkit resources

### External Dependencies
- **.NET 9.0 SDK**: Core runtime
- **Microsoft.ML.OnnxRuntime**: Semantic embeddings (ONNX models)
- **Microsoft.Data.Sqlite**: Event persistence
- **DocumentFormat.OpenXml**: DOCX/PPTX text extraction
- **UglyToad.PdfPig**: PDF text extraction
- **Spectre.Console**: CLI user interface
- **PowerShell 5+**: Script execution environment (legacy scripts)
- **Python 3.10+**: Legacy scripts only (optional)
- **Poppler**: PDF text extraction (optional, for advanced features)
- **Tesseract**: OCR for images (optional)

## Performance Considerations

- **Chunking Strategy**: 800 words with 200 word overlap
- **Embedding Model**: Lightweight (all-MiniLM-L6-v2) for speed
- **Batch Processing**: Files processed sequentially to manage memory
- **Temporary Files**: Created in system temp directory, cleaned up after use

## Security Considerations

- No network access required (all processing local)
- User-provided source files only
- No sensitive data stored in embeddings (only text chunks)
- Temporary files cleaned up after execution

## Future Enhancements

- Support for additional file formats
- Configurable embedding models
- Incremental indexing (update index without full rebuild)
- Web UI for document generation
- API for programmatic access
- Multi-language support

## Design Decisions

1. **Markdown Templates**: Easy to edit, version control friendly, widely supported
2. **PowerShell + Python**: PowerShell for orchestration, Python for heavy processing
3. **Sentence Transformers**: Balance between quality and performance
4. **Simple Entity Extraction**: Heuristic-based for no external dependencies
5. **Local Processing**: Privacy-focused, no cloud dependencies
