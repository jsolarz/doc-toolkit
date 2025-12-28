# Changelog

All notable changes to the Documentation Toolkit project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2024

### Added

#### Self-Contained Deployment (Latest)
- **Embedded Templates**: All document templates are now embedded as resources in the compiled assembly
- **Self-Contained Application**: No external template folder references required - app works independently
- **Subfolder Organization**: `--subfolder` option for organizing generated documents into subfolders
- **Minimal Dependencies**: Optimized build output with embedded resources for portable deployment

#### Web Interface (Latest)
- **Self-Hosted Web Server**: `doc web` command to start a web server for viewing documents
- **Document Viewer**: Modern, responsive web interface with markdown rendering
- **Team Sharing**: Access documents from any device on your network
- **Embedded Web Assets**: All HTML, CSS, and JavaScript embedded as resources
- **RESTful API**: `/api/documents` endpoints for listing and retrieving documents
- **Configurable**: Custom port, host, and docs directory options
- **Modern Font Stacks**: System UI fonts for native OS rendering (PlanetScale-inspired)
- **Catppuccin Frappe Theme**: Beautiful light/dark theme with full color palette
- **Theme Toggle**: Persistent theme preference with smooth transitions
- **Improved Readability**: Optimized typography and spacing for technical content

#### Phase 1 Web Interface Improvements (Latest)
- **Direct Markdown Loading**: Raw markdown files served directly without preprocessing or third-party libraries
- **Client-Side Rendering**: Embedded markdown parser in JavaScript (no external dependencies)
- **Navigation Structure**: Auto-detected hierarchical navigation from folder structure with collapsible folders
- **Table of Contents**: Auto-generated TOC from markdown headers with anchor links and smooth scrolling
- **Search Functionality**: Full-text search with highlighting and relevance scoring
- **Enhanced API**: New endpoints for navigation structure, raw markdown content, TOC, and search
- **Zero Dependencies**: Removed Markdig dependency - fully self-contained with embedded parser

### Added

#### Core Functionality
- **CLI Application**: Beautiful C# command-line application built with Spectre.Console
- **Document Generation**: Generate documents from 11+ templates (PRD, RFP, Tender, SOW, Architecture, Solution, SLA, Spec, API, Data, Requirements Gathering)
- **Project Initialization**: `doc init` command to create new project workspaces with consistent structure
- **Semantic Indexing**: Build searchable indexes from document collections using ONNX embeddings
- **Semantic Search**: Natural language search with cosine similarity and top-K results
- **Knowledge Graph Generation**: Automatic extraction of entities, topics, and relationships
- **Document Summarization**: Generate context documents from source files
- **Setup Validation**: `doc validate` command to verify toolkit setup and dependencies

#### Architecture & Infrastructure
- **IDesign Method™ Architecture**: Volatility-based decomposition with Clients, Managers, Engines, Accessors
- **Dependency Injection**: Full DI container using Microsoft.Extensions.DependencyInjection
- **Event Bus**: In-memory pub/sub with SQLite persistence and automatic retry policies
- **Event Types**: `IndexBuiltEvent`, `GraphBuiltEvent`, `SummaryCreatedEvent`, `DocumentProcessedEvent`
- **Service Boundaries**: Clear separation with closed architecture pattern
- **Component Taxonomy**: Proper IDesign Method™ component organization

#### Engines (Algorithm Volatility)
- **EmbeddingEngine**: ONNX-based semantic embeddings (all-MiniLM-L6-v2 model)
- **DocumentExtractionEngine**: Text extraction from PDF, DOCX, PPTX, TXT, MD, CSV, JSON
- **TextChunkingEngine**: Configurable text chunking with overlap
- **SimilarityEngine**: Cosine similarity calculations
- **EntityExtractionEngine**: Entity and topic extraction from text
- **SummarizationEngine**: Text summarization capabilities

#### Accessors (Storage Volatility)
- **VectorStorageAccessor**: Binary vector storage with JSON metadata
- **TemplateAccessor**: Template management and document generation
- **ProjectAccessor**: Project workspace initialization and file system operations

#### Managers (Workflow Volatility)
- **SemanticIndexManager**: Orchestrates indexing workflow (extract → embed → store)
- **SemanticSearchManager**: Orchestrates search workflow (load → embed → search)
- **KnowledgeGraphManager**: Orchestrates graph generation workflow (extract → analyze → build)
- **SummarizeManager**: Orchestrates summarization workflow (extract → summarize → output)

#### Commands (Client Layer)
- **InitCommand**: Project initialization
- **GenerateCommand**: Document generation from templates
- **IndexCommand**: Semantic indexing with progress reporting
- **SearchCommand**: Semantic search with formatted results
- **GraphCommand**: Knowledge graph generation
- **SummarizeCommand**: Document summarization
- **ValidateCommand**: Setup validation

#### Data Models
- **IndexEntry**: Semantic index entry with file, path, chunk, and index
- **SearchResult**: Search result with file, path, score, and chunk
- **GraphData**: Knowledge graph structure with nodes, edges, and statistics
- **GraphNode**: Individual graph node (file, entity, or topic)
- **GraphEdge**: Relationship between graph nodes
- **GraphStats**: Graph statistics (file count, entity count, topic count)
- **BaseEvent**: Base class for all events
- **Event Types**: Specialized event classes for different operations

#### Testing
- **Unit Tests**: Comprehensive unit tests for all Engines, Accessors, and Managers
- **Integration Tests**: End-to-end tests for workflows
- **Benchmark Tests**: Performance benchmarks for critical operations
- **Test Infrastructure**: Custom Spectre.Console test reporter and benchmark runner
- **Test Coverage**: 80%+ code coverage target

#### Memory Optimization
- **Pre-allocated Collections**: Capacity estimates for lists to reduce allocations
- **Memory Monitoring**: Optional `--monitor-memory` flag for all operations
- **MemoryMonitor Class**: Real-time memory tracking with GC statistics
- **Optimized Event Payloads**: Removed large strings from events to reduce memory

#### Logging
- **Structured Logging**: Microsoft.Extensions.Logging integration
- **Error Logging**: Comprehensive error logging throughout application
- **Internal Operations**: Logging for debugging and monitoring
- **Console Output**: Spectre.Console for user-facing messages (preserved)

#### Documentation
- **Technical Documentation**: Comprehensive technical reference
- **Code Documentation**: XML comments following IDesign C# Coding Standard 3.1
- **Developer Guides**: Quick reference and onboarding documentation
- **Architecture Documentation**: IDesign Method™ architecture documentation
- **PRD, Architecture, Data, Spec, Solution Documents**: Complete project documentation

### Changed

#### Architecture Refactoring
- **Component Renaming**: Renamed all services to IDesign Method™ taxonomy:
  - `SemanticIndexService` → `SemanticIndexManager`
  - `SemanticSearchService` → `SemanticSearchManager`
  - `KnowledgeGraphService` → `KnowledgeGraphManager`
  - `SummarizeService` → `SummarizeManager`
  - `EmbeddingService` → `EmbeddingEngine`
  - `DocumentExtractionService` → `DocumentExtractionEngine`
  - `VectorStorageService` → `VectorStorageAccessor`
  - `TemplateService` → `TemplateAccessor`
  - `ProjectService` → `ProjectAccessor`

#### Project Structure
- **Infrastructure Folder**: Moved all infrastructure to `ifx/` folder
- **Component Organization**: Proper IDesign Method™ folder structure:
  - `Accessors/` - Storage volatility
  - `Engines/` - Algorithm volatility
  - `Managers/` - Workflow volatility
  - `ifx/Commands/` - UI volatility (Clients)
  - `ifx/Events/` - Event definitions
  - `ifx/Infrastructure/` - DI, Event Bus, etc.
  - `ifx/Interfaces/` - All interfaces
  - `ifx/Models/` - Data models

#### .NET Upgrade
- **Upgraded from .NET 8.0 to .NET 9.0**: Latest stable version with performance improvements
- **Package Updates**: All NuGet packages verified compatible with .NET 9.0
- **Target Framework**: Updated `TargetFramework` in all project files

#### Code Quality
- **IDesign C# Coding Standard 3.1 Compliance**: Full compliance with naming conventions, error handling, dependency injection
- **XML Documentation**: Comprehensive XML comments for all public APIs
- **Argument Validation**: All public methods validate arguments
- **Error Handling**: Proper exception handling at service boundaries
- **Code Organization**: One class per file, proper namespace structure

### Removed

- **Python Dependency**: Removed Python requirement (replaced with ONNX Runtime)
- **Legacy Services Folder**: Removed in favor of IDesign Method™ structure
- **Direct Service Instantiation**: Replaced with dependency injection
- **FluentAssertions**: Removed in favor of native xUnit assertions
- **Intermediary Documentation Files**: Consolidated into clean documentation structure

### Technical Details

#### Dependencies
- **.NET 9.0**: Runtime and framework
- **Spectre.Console**: Rich console output and CLI framework
- **Microsoft.ML.OnnxRuntime**: ONNX model inference for embeddings
- **Microsoft.Extensions.Logging**: Structured logging
- **Microsoft.Extensions.DependencyInjection**: Dependency injection container
- **Microsoft.Data.Sqlite**: SQLite database for event persistence
- **DocumentFormat.OpenXml**: DOCX/PPTX text extraction
- **UglyToad.PdfPig**: PDF text extraction
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework
- **BenchmarkDotNet**: Performance benchmarking

#### Performance
- **Document Generation**: < 2 seconds
- **Semantic Indexing**: < 30 seconds for 100 documents
- **Semantic Search**: < 200ms average query time
- **Knowledge Graph Generation**: < 15 seconds for 100 documents
- **Memory Usage**: < 200MB during indexing operations

#### File Format Support
- **PDF**: Text extraction using UglyToad.PdfPig
- **DOCX**: Text extraction using DocumentFormat.OpenXml
- **PPTX**: Text extraction using DocumentFormat.OpenXml
- **TXT, MD, CSV, JSON**: Native text file support

#### Event Bus
- **Persistence**: SQLite database at `%LocalAppData%\DocToolkit\events.db`
- **Retry Policy**: Automatic retry of failed events (max 3 retries, 5-minute intervals)
- **Event Subscriptions**: Centralized subscription management in `EventSubscriptions.ConfigureSubscriptions()`

#### Memory Optimization
- **Pre-allocation**: Collections pre-allocated with capacity estimates
- **Event Payloads**: Removed large strings from events
- **Batch Processing**: Files processed sequentially to manage memory
- **Memory Monitoring**: Real-time tracking with `MemoryMonitor` class

---

## Development Phases

### Phase 1: Core Functionality ✅
- Initial CLI application structure
- Document generation from templates
- Basic semantic indexing
- Simple search functionality

### Phase 2: Advanced Features ✅
- Knowledge graph generation
- Document summarization
- Event bus architecture
- Full dependency injection

### Phase 3: Quality & Performance ✅
- Comprehensive testing (unit, integration, benchmarks)
- Memory optimization
- Memory monitoring capabilities
- IDesign Method™ compliance

### Phase 4: Documentation ✅
- Technical documentation
- Code documentation standards
- Developer guides
- Comprehensive CHANGELOG
- Clean documentation structure (PRD, Architecture, Data, Spec, Solution)

---

## Coding Standards

### IDesign C# Coding Standard 3.1
- **Naming Conventions**: Interfaces with "I" prefix, PascalCase classes, underscore-prefixed private fields
- **Error Handling**: Argument validation in all public methods, proper exception types
- **Dependency Injection**: Constructor injection for all dependencies
- **Code Organization**: Proper namespace structure, one class per file
- **Documentation**: XML comments for all public APIs with IDesign Method™ notes

### Testing Standards
- **Unit Tests**: All Engines, Accessors, and Managers
- **Integration Tests**: End-to-end workflow tests
- **Benchmark Tests**: Performance regression detection
- **Test Coverage**: 80%+ target

---

## References

- [IDesign Method™ Version 2.5](docs/IDesign%20Method.pdf)
- [IDesign C# Coding Standard 3.1](docs/IDesign%20C%23%20Coding%20Standard%203.1.pdf)
- [Technical Documentation](docs/TECHNICAL-DOCUMENTATION.md)
- [Architecture Document](docs/ARCHITECTURE-Documentation-Toolkit.md)
- [PRD](docs/PRD-Documentation-Toolkit.md)
- [Data Model Document](docs/DATA-Documentation-Toolkit.md)
- [Engineering Specification](docs/SPEC-Documentation-Toolkit.md)
- [Solution Proposal](docs/SOLUTION-Documentation-Toolkit.md)

---

**Note**: This CHANGELOG consolidates all development steps from initial project creation through version 1.0.0, including architecture refactoring, .NET upgrades, testing implementation, memory optimization, and comprehensive documentation.
