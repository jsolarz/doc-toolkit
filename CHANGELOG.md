# Changelog

All notable changes to this project will be documented in this file.

## [1.0.1] - 2024-12-23

### Added
- Event subscriptions for cross-manager communication
- Event logging/observability (console output for major events)
- EventSubscriptions configuration class for centralized subscription management
- ValidationService moved to Infrastructure folder

### Changed
- Moved all Engine classes from `Services/Engines/` to `Engines/` folder
- Updated all Engine namespaces from `DocToolkit.Services.Engines` to `DocToolkit.Engines`
- Removed legacy Services folder (all components now in proper IDesign folders)
- Event subscriptions now configured at application startup in Program.cs
- DocumentProcessedEvent now published during indexing workflow

### Architecture Improvements
- **Phase 4**: Event subscriptions and project cleanup ✅
- All components now in proper IDesign folders (Managers, Engines, Accessors, ifx/Infrastructure)
- Event-driven communication enabled between managers

## [1.0.0] - 2024-12-23

### Added
- C# CLI application with Spectre.Console for beautiful user interface
- Full dependency injection using Microsoft.Extensions.DependencyInjection
- Event bus with SQLite persistence and retry policies
- IDesign Method™ compliant architecture (Managers, Engines, Accessors)
- Semantic indexing using ONNX Runtime (C# native, no Python required)
- Knowledge graph generation
- Document summarization
- Project initialization with Git and Cursor configuration

### Changed
- **BREAKING**: Renamed all services to IDesign Method™ taxonomy:
  - `SemanticIndexService` → `SemanticIndexManager`
  - `SemanticSearchService` → `SemanticSearchManager`
  - `KnowledgeGraphService` → `KnowledgeGraphManager`
  - `SummarizeService` → `SummarizeManager`
  - `EmbeddingService` → `EmbeddingEngine`
  - `DocumentExtractionService` → `DocumentExtractionEngine`
  - `VectorStorageService` → `VectorStorageAccessor`
  - `TemplateService` → `TemplateAccessor`
  - `ProjectService` → `ProjectAccessor`
- Reorganized project structure: moved infrastructure to `ifx/` folder
- Extracted business logic from Managers into Engines:
  - `TextChunkingEngine` - Text chunking logic
  - `SimilarityEngine` - Similarity calculations
  - `EntityExtractionEngine` - Entity and topic extraction
  - `SummarizationEngine` - Text summarization
- All components now use constructor injection with interfaces
- Commands updated to use dependency injection

### Architecture Improvements
- **Phase 1**: Component renaming and interface creation ✅
- **Phase 2**: Dependency injection implementation ✅
- **Phase 3**: Event bus with persistence and retry policies ✅
- **Project Structure**: Clean separation with `ifx/` folder for infrastructure ✅

### Infrastructure
- Event Bus: In-memory pub/sub with SQLite persistence
- Event Persistence: All events saved to `%LocalAppData%\DocToolkit\events.db`
- Retry Policies: Automatic retry of failed events (max 3 retries, 5-minute intervals)
- Dependency Injection: Full DI container with service lifetimes

### Removed
- Python dependency for semantic processing (replaced with ONNX Runtime)
- Direct service instantiation (replaced with dependency injection)

## [0.1.0] - 2024-12-20

### Added
- Initial project structure
- PowerShell and Python scripts for document generation
- Semantic indexing using Python and sentence-transformers
- Knowledge graph generation
- Document templates (PRD, RFP, Tender, SOW, etc.)
- Cursor IDE configuration
