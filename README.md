# Documentation Toolkit

This toolkit provides a complete, reusable documentation framework for all projects.  
It centralizes rules, templates, scripts, and automation so you never duplicate work.

# Core Purpose

A project scaffolding and publishing tool for creating documentation projects following [docs-as-code](https://www.writethedocs.org/guide/docs-as-code/) principles from [Write the Docs](https://www.writethedocs.org/guide/).

Like `dotnet new` or Yeoman, but for documentation projects. 

1. **Initialize**: `doc init MyProject` - Creates complete project structure with folders, configs, Git repo
2. **Generate**: `doc generate prd "Name"` - Creates documents from templates
3. **Publish**: `doc publish web` - Compiles and packages documentation for deployment (web, PDF, CHM)
4. **Deploy**: Published output ready for Azure, AWS, Docker, GitHub Pages

Source repository stays private. Published output is public-ready.

Use this toolkit to generate:
- PRDs  
- RFPs  
- Tenders  
- SOWs  
- Architecture Docs  
- Solution Proposals  
- Engineering Specs  
- API Designs  
- Data Models  
- Blog Entries  
- Weekly Logs  

And to initialize new project workspaces with:
- A consistent folder structure  
- Git repository  
- Cursor IDE configuration  

---

## 📁 Toolkit Structure

```
/doc-toolkit/
    /src/
        /DocToolkit/          # C# CLI Application
            /Accessors/        # Storage volatility (Accessors)
            /Engines/          # Algorithm volatility (Engines)
            /Managers/         # Workflow volatility (Managers)
            /ifx/              # Infrastructure folder
                /Commands/     # CLI command implementations (Clients)
                /Events/       # Event definitions
                /Infrastructure/ # DI, Event Bus, etc.
                /Interfaces/   # All interfaces
                /Models/       # Data models
                Program.cs      # Application entry point
            DocToolkit.csproj

    /templates/               # Document templates (embedded as resources in build)
        prd.md
        rfp.md
        tender.md
        architecture.md
        solution.md
        sow.md
        sla.md
        spec.md
        api.md
        data.md
        blog.md
        weekly-log.md

    /scripts/                 # PowerShell/Python scripts (legacy)
        generate-doc.cmd
        generate-doc.ps1
        init-project.cmd
        init-project.ps1
        summarize-source.ps1
        semantic-index.ps1
        semantic-search.ps1
        build_kg.py
        build-knowledge-graph.ps1

    /.cursor/                 # Cursor IDE configuration
        /rules/
        cursor.json

    README.md
    requirements.txt
```

---

## 🚀 Core Capabilities

### CLI Application (Recommended)

The toolkit now includes a beautiful C# CLI application built with [Spectre.Console](https://spectreconsole.net/) for a modern, cross-platform experience, plus a self-hosted web interface for viewing and sharing documents.

#### Installation

Build the CLI application:
```bash
cd src/DocToolkit
dotnet build
```

Or run directly:
```bash
cd src/DocToolkit
dotnet run -- <command>
```

#### Commands

**1. Initialize a new project:**
```bash
doc init MyProject
```

**2. Generate a document:**
```bash
doc generate prd "User Management"
doc gen sow "Cloud Migration"  # Short alias
doc gen prd "API Design" --subfolder "architecture"  # Organize in subfolder
```

**3. Validate setup:**
```bash
doc validate
```

**4. Start web server to view and share documents:**
```bash
doc web
doc web --port 8080
doc web --host 0.0.0.0 --port 5000  # Accessible from network
doc web --docs-dir ./my-docs  # Custom docs directory
```

**5. Publish documentation for deployment:**
```bash
doc publish                    # Publish web interface
doc publish --format web       # Web interface (default)
doc publish --format all       # All formats (web, pdf, chm, single)
doc publish --target azure     # With Azure deployment config
doc publish --target docker    # With Docker configuration
```

#### Publishing System

The toolkit includes a comprehensive publishing system that:

- **Compiles documentation** into multiple formats (web, PDF, CHM, single-file)
- **Packages web interface** for static deployment (no server required)
- **Generates deployment configs** for Azure, AWS, Docker, GitHub Pages
- **Separates source from output** - keep repository private, publish output publicly

#### Docs-as-Code Philosophy

The toolkit follows [docs-as-code](https://www.doctave.com/docs-as-code) principles:

- **Version Control**: Git repository initialized automatically
- **Markdown-Based**: All templates in Markdown format
- **Automated Quality**: GitHub Actions workflows for quality checks
- **CI/CD Ready**: Automated linting and deployment workflows
- **Style Guides**: Microsoft Style Guide support (optional)
- **Collaboration**: PR-based workflow with automated checks
- **Workflow**: Edit → Review (PR) → Automated Checks → Publish

See [docs/DOCS-AS-CODE-ALIGNMENT.md](docs/DOCS-AS-CODE-ALIGNMENT.md) for complete alignment with industry best practices.

#### Web Interface

The web interface provides a beautiful, self-contained document viewer that allows you to:

- **Browse Documents**: View all generated documents in a sidebar list
- **View Content**: Click any document to view its full content with markdown rendering
- **Share with Team**: Access the web interface from any device on your network
- **Self-Contained**: All web assets are embedded in the application - no external dependencies
- **Real-time Updates**: Refresh the document list to see newly generated documents

**Features:**
- Modern, responsive design
- Markdown rendering with syntax highlighting
- Document metadata (type, size, last modified)
- Organized by document type
- Mobile-friendly interface

**Access:**
- Default: `http://localhost:5000`
- Network access: Use `--host 0.0.0.0` to allow access from other devices
- Custom port: Use `--port` to specify a different port

### Script-Based Usage (Legacy)

The original PowerShell and Python scripts are still available for advanced use cases.

#### 1. **Project Initialization**
Creates a new project workspace with:
- `/docs/`
- `/source/`
- `/.cursor/` (linked to toolkit rules/templates)
- `.gitignore`
- Git repository + initial commit
- README

**CLI:**
```bash
doc init MyProject
```

**Scripts:**
```powershell
.\scripts\init-project.ps1 MyProject
```

#### 2. **Document Generation**
Generate any document type using templates. Templates are embedded in the application for self-contained deployment.

**CLI:**
```bash
doc generate prd "User Management"
doc gen sow "Cloud Migration"
doc gen prd "API Design" --subfolder "architecture"  # Organize in docs/architecture/
doc gen sow "Project X" --output "./my-docs" --subfolder "sows"  # Custom output with subfolder
```

**Scripts:**
```bash
generate-doc.cmd sow "Cloud Migration"
.\generate-doc.ps1 -Type prd -Name "User Management"
```

---

## 🧠 Why This Toolkit Exists

- Centralizes all documentation rules  
- Ensures consistency across all projects  
- Eliminates duplication  
- Makes onboarding new team members trivial  
- Provides a scalable, professional documentation workflow  

---

## 🛠 Requirements

### CLI Application Requirements
- **.NET 9.0 SDK** or later
- **Poppler** (optional, for advanced PDF features)
- **Tesseract** (optional, for OCR)

### System Requirements (Scripts)
- **PowerShell 5+** (Windows) or **Bash** (Linux/Mac)
- **Python 3.10+**
- **Poppler** (for PDF text extraction)
- **Tesseract** (optional, for OCR)

### Installation

#### Windows
Install external tools via Chocolatey:
```powershell
choco install poppler
choco install tesseract
```

#### Linux (Ubuntu/Debian)
```bash
sudo apt-get update
sudo apt-get install poppler-utils tesseract-ocr
```

#### macOS
```bash
brew install poppler tesseract
```

### NuGet Packages

The CLI application uses the following NuGet packages (automatically restored):
- `Microsoft.Data.Sqlite` - For event persistence
- `Microsoft.Extensions.DependencyInjection` - For dependency injection
- `DocumentFormat.OpenXml` - For DOCX/PPTX text extraction
- `UglyToad.PdfPig` - For PDF text extraction
- `Spectre.Console` - For beautiful CLI interface

**Note**: Python is no longer required! The CLI is fully C# native with dependency injection and event bus.

### Validate Setup

**CLI:**
```bash
doc validate
```

**Scripts:**
```powershell
.\scripts\validate-setup.ps1
```

For automatic fixes (scripts only):
```powershell
.\scripts\validate-setup.ps1 -Fix
```

---

## 🔧 Troubleshooting

### Missing NuGet Packages
**Error**: `Missing required libraries`

**Solution**:
```bash
cd src/DocToolkit
dotnet restore
```

### PDF Text Extraction Fails
**Error**: `Unable to extract PDF text`

**Solution**:
1. Install Poppler: `choco install poppler` (Windows) or `apt-get install poppler-utils` (Linux)
2. Ensure `pdftotext` is in your PATH
3. Verify with: `pdftotext -v`

### Script Execution Policy (PowerShell)
**Error**: Script execution is disabled

**Solution**:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

---

## 📚 Documentation

### Project Documentation
- **[PRD](docs/PRD-Documentation-Toolkit.md)** - Product Requirements Document
- **[Architecture](docs/ARCHITECTURE-Documentation-Toolkit.md)** - Architecture Design Document
- **[Data Model](docs/DATA-Documentation-Toolkit.md)** - Data Model Document
- **[Engineering Spec](docs/SPEC-Documentation-Toolkit.md)** - Engineering Specification
- **[Solution Proposal](docs/SOLUTION-Documentation-Toolkit.md)** - Solution Proposal

### Technical Documentation
- **[Technical Documentation](docs/TECHNICAL-DOCUMENTATION.md)** - Complete technical reference
- **[System Design](docs/design.md)** - IDesign Method™ system design
- **[Code Documentation Standards](docs/CODE-DOCUMENTATION-STANDARDS.md)** - Documentation guidelines
- **[Developer Quick Reference](docs/DEVELOPER-QUICK-REFERENCE.md)** - Quick reference guide

### Standards & Compliance
- **[IDesign C# Coding Standard Compliance](docs/IDESIGN-CSHARP-CODING-STANDARD-COMPLIANCE.md)** - Coding standard compliance
- **[IDesign Method™ Guidelines](.cursor/rules/idesign-method.mdc)** - Architecture guidelines

---

## 📝 Version

Current version: **1.0.0**

See [CHANGELOG.md](CHANGELOG.md) for complete version history and all development steps.

---

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## 📄 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) for details.

---

## 🧩 Extending the Toolkit

You can add:
- New templates (add to `/templates/`)  
- New rulesets (add to `/.cursor/rules/`)  
- New snippets  
- New automation scripts (add to `/scripts/`)  

Just keep everything inside `/doc-toolkit/`.

---

## 🚀 Future Enhancements

The following features are planned for future releases:

### Semantic Intelligence Features

- **Semantic Indexing**: Build vector embeddings from source files for intelligent search
  - `doc index` - Build semantic index from source files
  - Support for multiple embedding models (ONNX-based)
  - Configurable chunking strategies

- **Semantic Search**: Query project knowledge using natural language
  - `doc search "query"` - Search across indexed content
  - Top-K result ranking
  - Similarity-based retrieval

- **Knowledge Graph Generation**: Extract and visualize relationships
  - `doc graph` - Build knowledge graph from source files
  - Entity and topic extraction
  - Relationship mapping
  - Graph visualization (JSON, Graphviz, Markdown)

- **Document Summarization**: Generate context summaries
  - `doc summarize` - Create summaries of source files
  - Extractive and abstractive summarization
  - Context-aware summaries

These features will require:
- ONNX model support (all-MiniLM-L6-v2.onnx or similar)
- Microsoft.ML.OnnxRuntime NuGet package
- Additional processing capabilities for embeddings and graph generation
