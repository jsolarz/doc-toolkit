# Project Onboarding Guide

Welcome!  
This project workspace was generated using the global Documentation Toolkit.

---

## 📁 Project Structure

```
/docs/              → Generated documents  
/source/            → Raw materials, PDFs, specs, notes  
/semantic-index/    → Vector store (auto-generated)  
/knowledge-graph/   → Knowledge graph (auto-generated)  
/.cursor/           → Cursor config referencing the global toolkit  
README.md           → Project overview  
.gitignore          → Standard ignore rules  
```

---

## 🚀 Generating Documents

### CLI Application (Recommended)

Use the CLI application from the toolkit:

```bash
# Navigate to toolkit directory
cd path/to/doc-toolkit/src/DocToolkit

# Generate a document
dotnet run -- generate prd "User Management"
dotnet run -- generate sow "Cloud Migration"
```

Or if installed as a global tool:

```bash
doc generate prd "User Management"
doc gen sow "Cloud Migration"  # Short alias
```

Documents appear in `/docs/` with date-prefixed filenames (e.g., `2024-12-23-prd-User_Management.md`).

### Legacy Scripts (Alternative)

You can also use the legacy scripts:

**CMD:**
```
generate-doc.cmd sow "Cloud Migration"
```

**PowerShell:**
```
.\generate-doc.ps1 -Type prd -Name "User Management"
```

---

## 📚 Adding Source Material

Place all contextual files in:

```
/source/
```

Examples:
- PDFs  
- RFP documents  
- Customer emails  
- Screenshots  
- Architecture diagrams  
- Meeting notes  

---

## 🧠 Build Semantic Index

### CLI Application (Recommended)

```bash
# From toolkit directory
cd path/to/doc-toolkit/src/DocToolkit
dotnet run -- index

# Or with options
dotnet run -- index --source ./source --chunk-size 1000 --monitor-memory
```

Or if installed as a global tool:

```bash
doc index
doc index --source ./source --chunk-size 1000 --monitor-memory
```

Outputs:
- `semantic-index/vectors.bin` (binary vector storage)
- `semantic-index/index.json` (metadata)

### Legacy Scripts (Alternative)

```powershell
.\..\doc-toolkit\scripts\semantic-index.ps1
```

---

## 🔍 Semantic Search

### CLI Application (Recommended)

```bash
# From toolkit directory
cd path/to/doc-toolkit/src/DocToolkit
dotnet run -- search "data migration"

# Or with options
dotnet run -- search "data migration" --top-k 10 --monitor-memory
```

Or if installed as a global tool:

```bash
doc search "data migration"
doc search "data migration" --top-k 10 --monitor-memory
```

### Legacy Scripts (Alternative)

```powershell
.\..\doc-toolkit\scripts\semantic-search.ps1 -Query "data migration"
```

---

## 🧩 Build Knowledge Graph

### CLI Application (Recommended)

```bash
# From toolkit directory
cd path/to/doc-toolkit/src/DocToolkit
dotnet run -- graph

# Or with options
dotnet run -- graph --source ./source --output ./kg --monitor-memory
```

Or if installed as a global tool:

```bash
doc graph
doc graph --source ./source --output ./kg --monitor-memory
```

Outputs:
- `knowledge-graph/graph.json` (JSON format)
- `knowledge-graph/graph.md` (Markdown format)
- `knowledge-graph/graph.gv` (Graphviz format)

### Legacy Scripts (Alternative)

```powershell
.\..\doc-toolkit\scripts\build-knowledge-graph.ps1
```

---

## 📊 Memory Monitoring

The CLI application includes optional memory monitoring for all operations:

```bash
doc index --monitor-memory
doc search "query" --monitor-memory
doc graph --monitor-memory
doc summarize --monitor-memory
```

This displays real-time memory usage, delta from baseline, elapsed time, and GC statistics.

## 🧠 Best Practices

- Keep `/source/` organized  
- Rebuild semantic index after adding new files  
- Rebuild knowledge graph regularly  
- Commit documents frequently  
- Use the Decision Log sections in templates  
- Keep architecture diagrams updated  
- Use the global rules for consistency
- Monitor memory usage for large document collections  

---

## 🔗 Toolkit Reference

The project’s `.cursor/cursor.json` links directly to:

```
../../doc-toolkit/cursor/rules.mdc
../../doc-toolkit/cursor/sow-rules.mdc
../../doc-toolkit/templates/*.md
```

This ensures:
- No duplication  
- Automatic updates  
- Consistent formatting and structure  

---

Happy documenting!
