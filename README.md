# Documentation Toolkit

This toolkit provides a complete, reusable documentation and knowledge‑engineering framework for all projects.  
It centralizes rules, templates, scripts, semantic indexing, and knowledge graph generation so you never duplicate work.

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
- Semantic index  
- Knowledge graph  
- Context extraction  

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

**Initialize a new project:**
```bash
doc init MyProject
```

**Generate a document:**
```bash
doc generate prd "User Management"
doc gen sow "Cloud Migration"  # Short alias
doc gen prd "API Design" --subfolder "architecture"  # Organize in subfolder
```

**Build semantic index:**
```bash
doc index
doc index --source ./my-source --chunk-size 1000
```

**Search the semantic index:**
```bash
doc search "customer requirements"
doc search "data migration" --top-k 10
```

**Build knowledge graph:**
```bash
doc graph
doc graph --source ./source --output ./kg
```

**Summarize source files:**
```bash
doc summarize
doc sum --output ./summary.md
```

**Validate setup:**
```bash
doc validate
```

**Start web server to view and share documents:**
```bash
doc web
doc web --port 8080
doc web --host 0.0.0.0 --port 5000  # Accessible from network
doc web --docs-dir ./my-docs  # Custom docs directory
```

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

#### 3. **Semantic Indexing**
Builds a vector store from `/source/`:

**CLI:**
```bash
doc index
```

**Scripts:**
```powershell
.\semantic-index.ps1
```

Outputs:
- `semantic-index/vectors.npy`
- `semantic-index/index.json`

#### 4. **Semantic Search**
Query your project knowledge:

**CLI:**
```bash
doc search "customer requirements"
```

**Scripts:**
```powershell
.\semantic-search.ps1 -Query "customer requirements"
```

#### 5. **Knowledge Graph**
Builds a graph of:
- Entities  
- Topics  
- Relationships  
- File connections  

**CLI:**
```bash
doc graph
```

**Scripts:**
```powershell
.\build-knowledge-graph.ps1
```

Outputs:
- `knowledge-graph/graph.json`
- `knowledge-graph/graph.md`
- `knowledge-graph/graph.gv`

---

## 🧠 Why This Toolkit Exists

- Centralizes all documentation rules  
- Ensures consistency across all projects  
- Eliminates duplication  
- Adds semantic intelligence to every project  
- Makes onboarding new team members trivial  
- Provides a scalable, professional documentation workflow  

---

## 🛠 Requirements

### CLI Application Requirements
- **.NET 9.0 SDK** or later
- **ONNX Model** (all-MiniLM-L6-v2.onnx, ~90MB) - See [README-MODEL.md](src/DocToolkit/README-MODEL.md)
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
- `Microsoft.ML.OnnxRuntime` - For semantic embeddings
- `Microsoft.Data.Sqlite` - For event persistence
- `Microsoft.Extensions.DependencyInjection` - For dependency injection
- `DocumentFormat.OpenXml` - For DOCX/PPTX text extraction
- `UglyToad.PdfPig` - For PDF text extraction
- `Spectre.Console` - For beautiful CLI interface

**Note**: Python is no longer required! The CLI is fully C# native with dependency injection and event bus.

### ONNX Model Setup

The semantic indexing feature requires an ONNX model file. See [README-MODEL.md](src/DocToolkit/README-MODEL.md) for setup instructions.

**Quick Setup**:
1. Download or convert `all-MiniLM-L6-v2.onnx` model
2. Create `models/` directory
3. Place model file in `models/all-MiniLM-L6-v2.onnx`

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

### ONNX Model Not Found
**Error**: `ONNX model not found at: models/all-MiniLM-L6-v2.onnx`

**Solution**:
1. Download the ONNX model (see [README-MODEL.md](src/DocToolkit/README-MODEL.md))
2. Create `models/` directory in project root or executable directory
3. Place `all-MiniLM-L6-v2.onnx` in the `models/` directory
4. Run `doc validate` to verify

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

### Semantic Index Not Found
**Error**: `Semantic index not found`

**Solution**:
1. Run `.\semantic-index.ps1` first to build the index
2. Ensure you have source files in the `./source/` directory

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
- New semantic models (update `EmbeddingEngine`)  
- New graph extractors (update `EntityExtractionEngine`)  

Just keep everything inside `/doc-toolkit/`.
