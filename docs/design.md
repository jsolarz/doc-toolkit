# Documentation Toolkit - System Design
*Following IDesign Method™ Version 2.5*

## Overview

The Documentation Toolkit is a reusable framework for generating professional documents and building static documentation sites following docs-as-code principles. It provides templates, automation scripts, opinionated organization, and automated build and deployment capabilities.

## Architecture (IDesign Method™)

### Volatility-Based Decomposition

The toolkit follows the IDesign Method™ principle of **volatility-based decomposition**. Components are organized based on what could change over time, not by functionality.

#### Volatilities Identified

1. **User Interface Volatility** (Client)
   - CLI commands could change (add new commands, change interfaces)
   - Future: Web UI, API, scheduled tasks

2. **Workflow Volatility** (Manager)
   - Orchestration logic could change (build workflow, structure creation)
   - Future: Different build strategies, deployment workflows

3. **Algorithm Volatility** (Engine)
   - Markdown rendering could change (different markdown processors)
   - Link resolution logic could change (different link formats)
   - Build process could change (different static site generators)

4. **Storage Volatility** (Accessor)
   - Build output format could change (HTML → other formats)
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

## Service Layer Architecture (IDesign Method™)

### Component Taxonomy

The architecture follows the IDesign Method™ component taxonomy:

| Component Type | Components | Volatility Encapsulated |
|---------------|------------|------------------------|
| **Client** | `InitCommand`, `GenerateCommand`, `BuildCommand`, `SuggestCommand`, `ValidateCommand`, `WebCommand`, `PublishCommand` | User interface volatility (CLI could become Web/API) |
| **Manager** | `BuildManager`, `StructureManager` | Workflow volatility (orchestration logic) |
| **Engine** | `BuildEngine`, `LinkResolver`, `NavigationGenerator`, `IndexGenerator`, `MetadataParser`, `TemplateSuggester`, `DocumentExtractionEngine` | Algorithm volatility (markdown rendering, link resolution, build process) |
| **Accessor** | `BuildAccessor`, `TemplateAccessor`, `ProjectAccessor` | Storage volatility (file system, storage technology) |

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
│  ┌──────────────┐ ┌──────────────┐                     │
│  │BuildManager │ │StructureMgr  │                     │
│  └──────────────┘ └──────────────┘                     │
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

**Note**: Engines and Accessors are at the same level. Managers call both, but Engines should not call Accessors directly (Engines receive data as parameters).

### Closed Architecture Pattern

- **Commands** can only call **Services** (one tier down)
- **Services** can only call **Models** or other **Services** in the same layer (for orchestration)
- **Models** are data structures only (no dependencies)
- No cross-tier calls (e.g., Commands cannot directly access Models)

### Call Chain Examples

**Use Case: Build Static Site**

```
BuildCommand (Client)
  ↓ (Service Boundary - Client → Manager)
BuildManager (Manager)
  ↓ (Service Boundary - Manager → Engine/Accessor)
MetadataParser (Engine) → BuildEngine (Engine) → LinkResolver (Engine) 
  → NavigationGenerator (Engine) → IndexGenerator (Engine) → BuildAccessor (Accessor)
  ↓ (Data Boundary)
Static HTML files in publish/web/
```

**Pattern**: Client → Manager → Engine/Accessor (avoiding "staircase" and "fork" patterns)

**Use Case: Initialize Project with Opinionated Structure**

```
InitCommand (Client)
  ↓ (Service Boundary)
StructureManager (Manager)
  ↓ (Service Boundary)
ProjectAccessor (Accessor) - Creates customer/developer/shared folders
  ↓ (Data Boundary)
Project structure on file system
```

**Pattern**: Manager orchestrates Accessor (create structure). Structure is based on project type selection.

**Use Case: Suggest Templates**

```
SuggestCommand (Client)
  ↓ (Service Boundary)
TemplateSuggester (Engine) - Analyzes project, suggests templates
  ↓ (Data Boundary)
Template suggestions with context
```

**Pattern**: Direct Engine call from Command (simple analysis, no orchestration needed).

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
- **Event Types**: `BuildCompletedEvent`, `DocumentProcessedEvent`, `LinkValidationCompletedEvent`

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
- Console logging for major events (BuildCompletedEvent, DocumentProcessedEvent, LinkValidationCompletedEvent)
- Optional cross-manager subscriptions can be enabled (e.g., auto-rebuild navigation when documents change)
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
- `InitCommand`: Project initialization with opinionated structure
- `GenerateCommand`: Document generation from templates
- `BuildCommand`: Static site generation (markdown → HTML)
- `SuggestCommand`: Template suggestions based on project state
- `ValidateCommand`: Setup validation
- `WebCommand`: Self-hosted web server for document viewing and sharing
- `PublishCommand`: Package documentation for deployment

**Service Boundaries**: Commands call Managers or Engines only (closed architecture)

### 2. Manager Layer (Orchestration)
**Purpose**: Encapsulate workflow volatility. Knows "when" to do things, not "how".

**Managers**:
- `BuildManager`: Orchestrates build workflow (parse metadata → compile → resolve links → generate nav → generate index → write output)
- `StructureManager`: Manages opinionated folder organization (creates customer/developer/shared structure)

**Service Boundaries**: Managers call Engines and Accessors. They orchestrate the workflow but don't contain business logic.

### 3. Engine Layer (Business Logic)
**Purpose**: Encapsulate algorithm volatility. Knows "how" to do things. Pure functions (no I/O).

**Engines**:
- `BuildEngine`: Encapsulates markdown → HTML compilation (Markdig could change to different processor)
- `LinkResolver`: Encapsulates link resolution logic (link format could change)
- `NavigationGenerator`: Encapsulates navigation generation (structure format could change)
- `IndexGenerator`: Encapsulates index page generation (index format could change)
- `MetadataParser`: Encapsulates YAML front matter parsing (metadata schema could change)
- `TemplateSuggester`: Encapsulates template suggestion logic (suggestion algorithm could change)
- `DocumentExtractionEngine`: Encapsulates extraction logic (format support could change)

**Service Boundaries**: Engines are "pure" - they accept data as parameters and return results. They should not call Accessors directly.

### 4. Accessor Layer (Resource Abstraction)
**Purpose**: Encapsulate storage volatility. Knows "where" data is stored. Dumb CRUD operations.

**Accessors**:
- `BuildAccessor`: Abstracts build output storage (output format could change, location could change)
- `TemplateAccessor`: Abstracts template storage (now uses embedded resources for self-contained deployment; could change to cloud/database)
- `ProjectAccessor`: Abstracts file system operations (could move to cloud storage)

**Service Boundaries**: Accessors are "dumb" - they perform CRUD operations only. No business logic.

### 5. Model Layer (Data)
**Purpose**: Data structures and contracts. Stable business concepts.

**Models**:
- `BuildOptions`: Build configuration (source dir, output dir, options)
- `DocumentMetadata`: Document metadata (title, category, status, date, tags)
- `NavigationStructure`: Navigation hierarchy (folders, files, relationships)
- `LinkValidationResult`: Link validation results (broken links, warnings)

**Data Boundary**: Models are pure data structures (no dependencies). Components should pass IDs or lightweight data, not fat entities.

### 6. Validation Service
**Purpose**: Setup validation (special case - utility service).

**Note**: `ValidationService` is a utility service that can be used by all layers. It doesn't fit the standard taxonomy but is needed for setup validation.

### 4. Legacy Script Layer
**Purpose**: PowerShell scripts for legacy support.

**Scripts**:
- `init-project.ps1`: Creates new project workspace
- `generate-doc.ps1`: Generates documents from templates
- `build-knowledge-graph.ps1`: Extracts entities (legacy)
- `summarize-source.ps1`: Creates context summaries (legacy)

**Note**: Scripts are being phased out in favor of C# services

## Data Models

### Document Metadata
```json
{
  "title": "Product Requirements Document",
  "category": "customer",
  "status": "draft",
  "date": "2024-01-15",
  "tags": ["prd", "requirements"]
}
```

### Navigation Structure
```json
{
  "folders": [
    {
      "name": "customer",
      "path": "docs/customer",
      "files": ["prd.md", "proposal.md"]
    }
  ]
}
```

## Workflows

### Document Generation Workflow
1. User selects document type
2. Template is copied from embedded resources
3. Document is generated in appropriate folder (customer/developer/shared)
4. User edits template with project-specific content
5. Document is ready for use

### Build Static Site Workflow
1. User runs `doc build`
2. BuildManager orchestrates the process:
   - MetadataParser extracts YAML front matter from all documents
   - BuildEngine compiles markdown to HTML using Markdig
   - LinkResolver resolves and validates all cross-references
   - NavigationGenerator creates navigation structure
   - IndexGenerator creates documentation index
3. BuildAccessor writes all output to `publish/web/`
4. Static site is ready for deployment

### Project Initialization Workflow
1. User runs `doc init MyProject`
2. User selects project type (customer-facing, developer-facing, mixed)
3. StructureManager creates opinionated folder structure
4. ProjectAccessor creates directories and configuration files
5. Git repository is initialized
6. CI/CD workflows are created
7. Project is ready for documentation

## Integration Points

### Cursor IDE Integration
- Rules files (`.mdc`) for document generation guidance
- Snippets for quick template insertion
- Configuration files linking to toolkit resources

### External Dependencies
- **.NET 9.0 SDK**: Core runtime
- **Markdig**: Markdown to HTML compilation (GitHub Flavored Markdown support)
- **Microsoft.Data.Sqlite**: Event persistence
- **DocumentFormat.OpenXml**: DOCX/PPTX text extraction (for source files)
- **UglyToad.PdfPig**: PDF text extraction (for source files)
- **Spectre.Console**: CLI user interface
- **YamlDotNet**: YAML front matter parsing (optional, can use simple regex)
- **Git**: Version control (for CI/CD workflows)

## Performance Considerations

- **Build Performance**: Parallel processing of markdown files where possible
- **Link Validation**: Incremental validation (only check changed files)
- **Navigation Generation**: Single pass through folder structure
- **Index Generation**: Efficient scanning with metadata caching
- **Temporary Files**: Created in system temp directory, cleaned up after use

## Security Considerations

- No network access required (all processing local)
- User-provided source files only
- No sensitive data in compiled output (only public documentation)
- Temporary files cleaned up after execution

## Self-Contained Deployment

The application is designed to be self-contained with minimal dependencies:

- **Embedded Templates**: All document templates are embedded as resources in the compiled assembly, eliminating the need for external template files
- **Minimal Dependencies**: Only essential NuGet packages are included
- **Portable**: The compiled executable can be distributed without requiring template folders or source code references
- **Subfolder Organization**: Generated documents can be organized into subfolders using the `--subfolder` option

### Template Access

Templates are accessed via embedded resources using `Assembly.GetManifestResourceStream()`. The `TemplateAccessor` automatically discovers and loads templates from the assembly, making the application fully self-contained.

## Web Interface

**Current State**: Self-hosted web server implemented

The toolkit includes a self-contained web interface for viewing and sharing documents:

- **Web Server**: ASP.NET Core minimal API with embedded static files
- **Document Viewer**: Modern, responsive web interface with markdown rendering
- **API Endpoints**: RESTful API for listing and retrieving documents
- **Self-Contained**: All web assets (HTML, CSS, JS) embedded as resources
- **Network Access**: Configurable host and port for team sharing

**Architecture**:
```
WebCommand (Client)
    ↓
ASP.NET Core WebApplication
    ↓
API Endpoints (/api/documents)
    ↓
File System (docs directory)
```

**Features**:
- Browse all generated documents
- View document content with markdown rendering
- Document metadata (type, size, last modified)
- Responsive design for mobile and desktop
- Real-time document list refresh

## Future Enhancements

- Support for additional markdown extensions
- Incremental builds (only rebuild changed files)
- Enhanced web UI features (search, filters, document editing)
- API for programmatic access
- Multi-language support
- PDF and CHM output formats
- Custom theme support for static sites

## Design Decisions

1. **Markdown Templates**: Easy to edit, version control friendly, widely supported
2. **Markdig for Rendering**: C# native, GitHub Flavored Markdown support, extensible
3. **Opinionated Organization**: Clear separation of customer/developer docs reduces confusion
4. **Static Site Generation**: Deployable anywhere, no server required, fast and simple
5. **YAML Front Matter**: Standard metadata format, easy to parse, widely supported
6. **GitHub Actions**: Industry standard, free for open source, easy to configure
7. **Local Processing**: Privacy-focused, no cloud dependencies
