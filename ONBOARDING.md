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

Use the toolkit scripts from inside the project folder.

### CMD
```
generate-doc.cmd sow "Cloud Migration"
```

### PowerShell
```
.\generate-doc.ps1 -Type prd -Name "User Management"
```

Documents appear in `/docs/`.

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

```
.\..\doc-toolkit\scripts\semantic-index.ps1
```

Outputs:
- `semantic-index/index.json`
- `semantic-index/vectors.npy`

---

## 🔍 Semantic Search

```
.\..\doc-toolkit\scripts\semantic-search.ps1 -Query "data migration"
```

---

## 🧩 Build Knowledge Graph

```
.\..\doc-toolkit\scripts\build-knowledge-graph.ps1
```

Outputs:
- `knowledge-graph/graph.json`
- `knowledge-graph/graph.md`
- `knowledge-graph/graph.gv`

---

## 🧠 Best Practices

- Keep `/source/` organized  
- Rebuild semantic index after adding new files  
- Rebuild knowledge graph regularly  
- Commit documents frequently  
- Use the Decision Log sections in templates  
- Keep architecture diagrams updated  
- Use the global rules for consistency  

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
