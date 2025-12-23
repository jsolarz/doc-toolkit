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
            /Commands/         # CLI command implementations
            /Services/         # Business logic services
            DocToolkit.csproj
            Program.cs

    /templates/               # Document templates
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

The toolkit now includes a beautiful C# CLI application built with [Spectre.Console](https://spectreconsole.net/) for a modern, cross-platform experience.

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
Generate any document type using templates:

**CLI:**
```bash
doc generate prd "User Management"
doc gen sow "Cloud Migration"
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
- **.NET 8.0 SDK** or later
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
- `DocumentFormat.OpenXml` - For DOCX/PPTX text extraction
- `UglyToad.PdfPig` - For PDF text extraction

**Note**: Python is no longer required! The CLI is fully C# native.

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

## 📝 Version

Current version: **1.0.0**

See [CHANGELOG.md](CHANGELOG.md) for version history and changes.

---

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## 📄 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) for details.

---

## 🧩 Extending the Toolkit

You can add:
- New templates  
- New rulesets  
- New snippets  
- New automation scripts  
- New semantic models  
- New graph extractors  

Just keep everything inside `/doc-toolkit/`.
