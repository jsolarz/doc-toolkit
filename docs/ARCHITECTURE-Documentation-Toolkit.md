# Architecture Design Document
## Documentation Toolkit

**Version**: 1.0  
**Date**: 2024  
**Status**: Current

---

## 1. Executive Summary

The Documentation Toolkit is built on a service-oriented architecture following the IDesign Method™ principles. It uses volatility-based decomposition to organize components, ensuring maintainability and extensibility. The architecture consists of four primary layers: Clients (CLI commands), Managers (orchestration), Engines (business logic), and Accessors (storage abstraction). All components communicate through well-defined service boundaries with dependency injection and an event-driven architecture for decoupled cross-component communication. The toolkit provides a complete docs-as-code workflow with opinionated organization, static site generation, and automated deployment.

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
│  │         Build & Publishing Layer                 │  │
│  │  - Markdown Compilation (Markdig)                 │  │
│  │  - Link Resolution & Validation                   │  │
│  │  - Navigation Generation                          │  │
│  │  - Static Site Generation                        │  │
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
│                    User (CLI)                           │
└──────────────────────┬──────────────────────────────────┘
                       │
                       ↓
┌────────────────────────────────────────────────────────┐
│              Documentation Toolkit                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   Commands   │  │   Managers   │  │   Engines    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│  ┌──────────────┐  ┌──────────────┐                    │
│  │  Accessors   │  │  Event Bus   │                    │
│  └──────────────┘  └──────────────┘                    │
└────────────────────────────────────────────────────────┘
         │                    │                    │
         ↓                    ↓                    ↓
┌──────────────┐    ┌──────────────┐
│ File System  │    │  SQLite DB   │
│ (Templates,  │    │  (Events)    │
│  Markdown,   │    │              │
│  Compiled    │    │              │
│  HTML)       │    │              │
└──────────────┘    └──────────────┘
```

**External Dependencies**:
- File System: Templates, source markdown files, compiled static site
- SQLite Database: Event persistence (`%LocalAppData%\DocToolkit\events.db`)
- Git: Version control (for CI/CD workflows)

---

## 5. Component Architecture

### Component Taxonomy (IDesign Method™)

| Component Type | Components                                                                                                                                | Volatility Encapsulated                                                   |
| -------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| **Client**     | `InitCommand`, `GenerateCommand`, `BuildCommand`, `SuggestCommand`, `ValidateCommand`, `WebCommand`, `PublishCommand`                     | User interface volatility (CLI could become Web/API)                      |
| **Manager**    | `BuildManager`, `StructureManager`                                                                                                        | Workflow volatility (orchestration logic)                                 |
| **Engine**     | `BuildEngine`, `LinkResolver`, `NavigationGenerator`, `IndexGenerator`, `MetadataParser`, `TemplateSuggester`, `DocumentExtractionEngine` | Algorithm volatility (markdown rendering, link resolution, build process) |
| **Accessor**   | `BuildAccessor`, `TemplateAccessor`, `ProjectAccessor`                                                                                    | Storage volatility (file system, storage technology)                      |

### Static Architecture View

```
┌─────────────────────────────────────────────────────────┐
│                    Client Layer                          │
│  (UI Volatility - Initiation)                           │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ │
│  │ InitCmd  │ │ GenCmd   │ │ BuildCmd │ │SuggestCmd│ │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐              │
│  │WebCmd   │ │PublishCmd│ │ValidateCmd│              │
│  └──────────┘ └──────────┘ └──────────┘              │
└─────────────────────────────────────────────────────────┘
                        ↓ (Service Boundary)
┌─────────────────────────────────────────────────────────┐
│                    Manager Layer                         │
│  (Workflow Volatility - Orchestration)                  │
│  ┌──────────────┐ ┌──────────────┐                   │
│  │BuildManager │ │StructureMgr  │                   │
│  └──────────────┘ └──────────────┘                   │
└─────────────────────────────────────────────────────────┘
                        ↓ (Service Boundary)
┌─────────────────────────────────────────────────────────┐
│         Engine Layer          │      Accessor Layer     │
│  (Algorithm Volatility)       │  (Storage Volatility)   │
│  ┌──────────────┐             │  ┌──────────────┐      │
│  │BuildEngine   │             │  │BuildAccessor │      │
│  └──────────────┘             │  └──────────────┘      │
│  ┌──────────────┐             │  ┌──────────────┐      │
│  │LinkResolver  │             │  │TemplateAccessor│     │
│  └──────────────┘             │  └──────────────┘      │
│  ┌──────────────┐             │  ┌──────────────┐      │
│  │NavGenerator  │             │  │ProjectAccessor│     │
│  └──────────────┘             │  └──────────────┘      │
│  ┌──────────────┐                                      │
│  │IndexGenerator│                                      │
│  └──────────────┘                                      │
│  ┌──────────────┐                                      │
│  │MetadataParser│                                      │
│  └──────────────┘                                      │
│  ┌──────────────┐                                      │
│  │TemplateSuggest│                                     │
│  └──────────────┘                                      │
└─────────────────────────────────────────────────────────┘
                        ↓ (Data Boundary)
┌─────────────────────────────────────────────────────────┐
│                    Model Layer                           │
│  (Data Layer)                                           │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐  │
│  │BuildOptions  │ │DocumentMeta  │ │NavStructure  │  │
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

### Use Case: Build Static Site

```
BuildCommand (Client)
  ↓ (Service Boundary)
BuildManager (Manager)
  ↓
  ├─→ MetadataParser.ParseAll(docsDir) → List<DocumentMetadata>
  ├─→ BuildEngine.CompileAll(markdownFiles) → List<CompiledDocument>
  ├─→ LinkResolver.ResolveAll(compiledDocs) → List<ResolvedDocument>
  ├─→ LinkResolver.ValidateAll(compiledDocs) → LinkValidationResult
  ├─→ NavigationGenerator.Generate(folderStructure) → NavigationStructure
  ├─→ IndexGenerator.Generate(allDocuments) → IndexDocument
  └─→ BuildAccessor.WriteAll(outputDir, compiledDocs, nav, index)
       ↓
       File System (publish/web/*.html, navigation.json, index.html)
```

### Use Case: Initialize Project with Opinionated Structure

```
InitCommand (Client)
  ↓ (Service Boundary)
StructureManager (Manager)
  ↓
  ├─→ Prompt user for project type (customer/developer/mixed)
  ├─→ StructureManager.CreateStructure(projectType) → FolderStructure
  └─→ ProjectAccessor.CreateDirectories(folderStructure)
       ↓
       File System (docs/customer/, docs/developer/, docs/shared/)
```

### Use Case: Suggest Templates

```
SuggestCommand (Client)
  ↓ (Service Boundary - direct Engine call)
TemplateSuggester (Engine)
  ↓
  ├─→ Scan existing documents in docs/
  ├─→ Identify missing document types
  ├─→ Match templates to project type
  └─→ Generate suggestions with context
       ↓
       Display suggestions to user
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
- `BuildCompletedEvent`: Published when static site build completes
- `DocumentProcessedEvent`: Published for each document processed during build
- `LinkValidationCompletedEvent`: Published when link validation completes

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

- **Document Build**: 50-500 markdown files per project
- **Link Validation**: 100-1000 links per project
- **Navigation Generation**: 1 navigation structure per build

### Performance Targets

- Document generation: < 2 seconds
- Static site build: < 10 seconds for 100 markdown files
- Link validation: < 5 seconds for 100 documents
- Index generation: < 2 seconds
- Memory usage: < 200MB during build operations

### Scaling Strategies

- **Memory Optimization**: Pre-allocated collections, parallel processing where safe
- **Incremental Builds**: Only rebuild changed files (future enhancement)
- **Parallel Processing**: Process multiple markdown files concurrently
- **Link Validation**: Incremental validation (only check changed files)
- **Caching**: Cache parsed metadata and navigation structure

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
- Source documents are in Markdown format
- Users have Git installed (for CI/CD workflows)
- Users have sufficient disk space for compiled sites
- Local file system access available
- Users have GitHub repository (for CI/CD automation)

### Constraints

- Local file system only (no cloud storage)
- Single-user operation (no multi-user support)
- Markdown source format required (templates generate markdown)
- Static site generation only (no server-side processing)
- CI/CD requires GitHub (other platforms future enhancement)
- Memory constraints for large document collections

---

## 13. Risks & Mitigations

| Risk                                          | Impact | Probability | Mitigation                                            |
| --------------------------------------------- | ------ | ----------- | ----------------------------------------------------- |
| Broken links in documentation                 | Medium | Medium      | Link validation during build, clear error reporting   |
| Large document collections cause build issues | Medium | Low         | Incremental builds, parallel processing, optimization |
| Corrupted markdown files cause crashes        | Medium | Low         | Graceful error handling, skip corrupted files         |
| Invalid YAML front matter                     | Low    | Medium      | Clear error messages, validation, sensible defaults   |
| CI/CD workflow failures                       | Medium | Low         | Comprehensive error handling, clear failure messages  |
| Performance degradation with large sites      | Medium | Low         | Benchmark tests, optimization, incremental builds     |
| Event bus database corruption                 | Low    | Very Low    | SQLite transaction support, error recovery            |

---

## 14. Decision Log

| Date | Decision                       | Rationale                                       | Alternatives Considered           |
| ---- | ------------------------------ | ----------------------------------------------- | --------------------------------- |
| 2024 | IDesign Method™ architecture   | Volatility-based decomposition, maintainability | Traditional layered architecture  |
| 2024 | ONNX Runtime for embeddings    | C# native, no Python dependency                 | Python with sentence-transformers |
| 2024 | SQLite for event persistence   | Lightweight, embedded, no external dependencies | File-based, in-memory only        |
| 2024 | Binary vector storage          | Performance, compact format                     | JSON, CSV, NumPy format           |
| 2024 | Dependency injection           | Testability, loose coupling                     | Direct instantiation              |
| 2024 | Event bus architecture         | Decoupled communication                         | Direct method calls               |
| 2024 | Markdig for markdown rendering | C# native, GitHub Flavored Markdown support     | CommonMark.NET, custom parser     |
| 2024 | Opinionated organization       | Clear separation of customer/developer docs     | Flat structure, user-defined      |
| 2024 | Static site generation         | Deployable HTML without server                  | Server-side rendering, CMS        |
| 2024 | GitHub Actions for CI/CD       | Industry standard, free for open source         | Azure DevOps, GitLab CI, Jenkins  |

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
