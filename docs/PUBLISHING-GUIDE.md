# Publishing Guide

## Overview

The Documentation Toolkit now supports comprehensive project scaffolding and publishing capabilities, transforming it into a complete documentation project management system.

## Project Initialization

### Enhanced `doc init` Command

The `doc init` command now creates a complete documentation project structure:

```bash
doc init MyProject
```

**Creates:**
- Complete folder structure (docs, source, publish, deploy)
- Configuration files (`.doc-toolkit/config.json`, `llm-config.json`, `publish-config.json`)
- Onboarding guide (`ONBOARDING.md`)
- Git repository with proper `.gitignore`
- Cursor IDE configuration

**Project Structure:**
```
MyProject/
├── .doc-toolkit/          # Configuration (private)
│   ├── config.json
│   ├── llm-config.json
│   └── publish-config.json
├── docs/                   # Source markdown (private)
├── source/                 # Source code for indexing
├── publish/                # Published output (public-ready)
│   ├── web/               # Web interface package
│   ├── pdf/               # PDF files
│   ├── chm/               # CHM files
│   └── single/            # Single-file compilation
├── deploy/                 # Deployment configs
├── ONBOARDING.md          # Walkthrough guide
└── README.md
```

## Publishing Documentation

### `doc publish` Command

Publish your documentation in various formats for deployment:

```bash
# Publish web interface (default)
doc publish

# Publish specific format
doc publish --format web
doc publish --format pdf
doc publish --format chm
doc publish --format single

# Publish all formats
doc publish --format all

# With deployment target
doc publish --format web --target azure
doc publish --format web --target aws
doc publish --format web --target docker
```

### Options

- `--format <format>`: Output format (web, pdf, chm, single, all)
- `--output <dir>`: Output directory (default: `./publish`)
- `--target <target>`: Deployment target (azure, aws, docker, github-pages)
- `--include-source`: Include source markdown in published output
- `--minify`: Minify web assets for production
- `--docs-dir <dir>`: Source documentation directory (default: `./docs`)

## Web Interface Publishing

### What It Does

The `doc publish web` command:

1. **Copies web interface files** (HTML, CSS, JavaScript)
2. **Copies documentation files** from `docs/` to `publish/web/docs/`
3. **Generates navigation structure** (`data.json`)
4. **Creates document index** (`documents.json`) with pre-processed TOC
5. **Modifies JavaScript** for static deployment (no server required)
6. **Creates deployment configurations** (if `--target` specified)

### Output Structure

```
publish/web/
├── index.html              # Main entry point
├── app.js                  # Application (modified for static)
├── app.css                 # Styles
├── data.json               # Navigation structure
├── documents.json          # Document index with TOC
├── docs/                   # Documentation files
│   ├── architecture/
│   └── requirements/
└── deploy/                 # Deployment configs (if target specified)
    ├── Dockerfile
    ├── azure-static-webapp.json
    └── .github/workflows/
```

### Static Deployment

The published web interface is **completely static** - no server required:

- Documents are loaded from `./docs/` directory
- Navigation loaded from `data.json`
- Document metadata from `documents.json`
- Works with any static hosting service

## Deployment Targets

### Azure Static Web Apps

```bash
doc publish --format web --target azure
```

**Creates:**
- `deploy/azure-static-webapp.json` - Azure configuration
- `.github/workflows/azure-static-webapp.yml` - GitHub Actions workflow

**Deploy:**
1. Push to GitHub
2. Connect Azure Static Web App to repository
3. Set `AZURE_STATIC_WEB_APPS_API_TOKEN` secret
4. Workflow automatically deploys on push

### AWS Lambda

```bash
doc publish --format web --target aws
```

**Creates:**
- `deploy/lambda-config.json` - Lambda configuration

**Deploy:**
- Package as ZIP file
- Upload to Lambda
- Configure API Gateway

### Docker

```bash
doc publish --format web --target docker
```

**Creates:**
- `deploy/Dockerfile` - Nginx-based container
- `deploy/.dockerignore`

**Deploy:**
```bash
cd publish/web/deploy
docker build -t myproject-docs .
docker run -p 8080:80 myproject-docs
```

### GitHub Pages

```bash
doc publish --format web --target github-pages
```

**Creates:**
- `.github/workflows/github-pages.yml` - GitHub Actions workflow

**Deploy:**
1. Push to GitHub
2. Enable GitHub Pages in repository settings
3. Workflow automatically deploys

## PDF Publishing

**Status:** Coming soon

Will support:
- Single PDF with all documents
- Separate PDF per document
- Custom PDF templates
- Table of contents generation

## CHM Publishing

**Status:** Coming soon

Will support:
- Windows Help file generation
- Table of contents
- Index generation
- Search functionality

## Single-File Compilation

**Status:** Coming soon

Will support:
- Combined markdown file
- Combined HTML file
- Unified table of contents
- Man-page style output

## Configuration

### Project Configuration (`.doc-toolkit/config.json`)

```json
{
  "project": {
    "name": "MyProject",
    "version": "1.0.0",
    "description": "Project documentation"
  },
  "paths": {
    "docs": "./docs",
    "source": "./source",
    "publish": "./publish"
  },
  "publishing": {
    "defaultFormat": "web",
    "defaultTarget": "azure",
    "includeSource": false,
    "minify": true
  }
}
```

### Publishing Configuration (`.doc-toolkit/publish-config.json`)

```json
{
  "web": {
    "title": "MyProject Documentation",
    "theme": "nord-dark",
    "includeSearch": true,
    "includeTOC": true
  },
  "pdf": {
    "template": "default",
    "pageSize": "A4",
    "includeTOC": true,
    "singleFile": false
  },
  "deployment": {
    "azure": {
      "resourceGroup": "myproject-docs",
      "staticWebApp": "myproject-docs"
    }
  }
}
```

## Workflow

### Development Workflow

1. **Initialize project:**
   ```bash
   doc init MyProject
   cd MyProject
   ```

2. **Generate documents:**
   ```bash
   doc generate prd "Product Requirements"
   doc generate architecture "System Design" --subfolder "design"
   ```

3. **Build semantic index:**
   ```bash
   doc index
   ```

4. **View locally:**
   ```bash
   doc web
   ```

### Publishing Workflow

1. **Publish web interface:**
   ```bash
   doc publish web --target azure
   ```

2. **Review published output:**
   ```bash
   cd publish/web
   # Test locally or deploy
   ```

3. **Deploy:**
   - Push to Git repository
   - CI/CD pipeline deploys automatically
   - Or manually deploy to hosting service

## Separation of Concerns

### Private Repository (Source)
- `docs/` - Markdown source files
- `source/` - Source code
- `.doc-toolkit/` - Configuration (may contain secrets)
- Semantic index data
- Knowledge graph data

### Public Output (Published)
- `publish/web/` - Web interface (can be public)
- `publish/pdf/` - PDF files (can be public)
- `publish/chm/` - CHM files (can be public)
- Deployment configurations (without secrets)

## Best Practices

1. **Keep source private**: Don't commit sensitive information in `docs/`
2. **Publish selectively**: Use `.docignore` to exclude draft documents
3. **Version control**: Tag published versions
4. **CI/CD**: Automate publishing and deployment
5. **Testing**: Test published output before deploying

## Next Steps

1. **PDF/CHM support**: Coming in future updates
2. **Advanced deployment**: More deployment targets
3. **Custom templates**: Project-specific templates
4. **Plugin system**: Extensible format support
