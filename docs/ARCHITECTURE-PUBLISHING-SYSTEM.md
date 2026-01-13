# Architecture: Documentation Publishing System

## Overview

The Documentation Toolkit will be enhanced to support comprehensive project scaffolding and publishing capabilities, transforming it into a complete documentation project management system similar to Yeoman or `dotnet new`.

## Core Concepts

### Project Lifecycle

1. **Initialize**: Create project structure with all necessary files
2. **Develop**: Write documentation using templates and tools
3. **Compile**: Generate output formats (PDF, CHM, single file, etc.)
4. **Publish**: Package and deploy documentation website

### Separation of Concerns

- **Source Repository**: Private, contains markdown source files, templates, configs
- **Published Output**: Public-ready, contains compiled documentation and web interface
- **Web Interface**: Self-contained, deployable package referencing docs

## Enhanced Project Structure

```
MyProject/
├── .doc-toolkit/              # Toolkit configuration (private)
│   ├── config.json           # Project configuration
│   ├── llm-config.json       # LLM model settings
│   └── publish-config.json   # Publishing settings
│
├── docs/                     # Source markdown files (private)
│   ├── architecture/
│   ├── requirements/
│   └── api/
│
├── source/                   # Source code for semantic indexing
│
├── templates/                # Project-specific templates (optional)
│
├── .cursor/                  # Cursor IDE configuration
│   └── cursor.json
│
├── semantic-index/           # Generated semantic index
│
├── knowledge-graph/          # Generated knowledge graph
│
├── publish/                  # Published output (can be public)
│   ├── web/                  # Packaged web interface
│   │   ├── index.html
│   │   ├── app.js
│   │   ├── app.css
│   │   └── docs/             # Compiled documentation
│   ├── pdf/                  # PDF outputs
│   ├── chm/                  # CHM outputs
│   └── single/               # Single-file compilation
│
├── deploy/                   # Deployment configurations
│   ├── azure-static-webapp/
│   ├── aws-lambda/
│   ├── docker/
│   └── github-pages/
│
├── README.md                 # Project README
├── ONBOARDING.md             # Walkthrough guide
├── .gitignore
└── .docignore                # Files to exclude from publishing
```

## Commands

### Enhanced `doc init <name>`

Creates a complete documentation project with:
- Full folder structure
- Configuration files
- LLM model settings
- Publishing configuration
- Walkthrough guide
- Example documents

**Options:**
- `--template <name>`: Use a project template
- `--llm-provider <provider>`: Pre-configure LLM (OpenAI, Anthropic, etc.)
- `--publish-target <target>`: Pre-configure publishing (azure, aws, docker, etc.)

### New `doc publish`

Compiles and packages documentation for deployment.

**Subcommands:**
- `doc publish web` - Package web interface
- `doc publish pdf` - Generate PDF files
- `doc publish chm` - Generate CHM files
- `doc publish single` - Create single-file compilation
- `doc publish all` - Generate all formats

**Options:**
- `--output <dir>`: Output directory (default: `./publish`)
- `--format <format>`: Output format (web, pdf, chm, single, all)
- `--target <target>`: Deployment target (azure, aws, docker, github-pages)
- `--include-source`: Include source markdown in published output
- `--minify`: Minify web assets for production

### New `doc deploy`

Deploy published documentation to configured target.

**Options:**
- `--target <target>`: Deployment target
- `--dry-run`: Preview deployment without executing
- `--config <file>`: Use custom deployment configuration

## Publishing Workflow

### 1. Web Interface Publishing

**Process:**
1. Copy web interface files (HTML, CSS, JS) to `publish/web/`
2. Copy documentation files from `docs/` to `publish/web/docs/`
3. Generate navigation structure
4. Minify assets (optional)
5. Create deployment package

**Output Structure:**
```
publish/web/
├── index.html              # Main entry point
├── app.js                  # Application logic
├── app.css                 # Styles
├── docs/                   # Documentation files
│   ├── architecture/
│   └── requirements/
└── deploy/                  # Deployment-specific files
    ├── azure-static-webapp.json
    ├── Dockerfile
    └── lambda-function.zip
```

### 2. PDF Compilation

**Process:**
1. Collect all markdown files from `docs/`
2. Convert to PDF using markdown-to-PDF engine
3. Generate table of contents
4. Apply styling/templates
5. Output to `publish/pdf/`

**Options:**
- Single PDF with all documents
- Separate PDF per document
- Custom PDF templates

### 3. CHM Compilation

**Process:**
1. Generate CHM project file (.hhp)
2. Create table of contents (.hhc)
3. Create index (.hhk)
4. Compile using HTML Help Workshop or alternative
5. Output to `publish/chm/`

### 4. Single File Compilation

**Process:**
1. Combine all markdown files
2. Generate unified table of contents
3. Create single markdown or HTML file
4. Output to `publish/single/`

## Deployment Targets

### Azure Static Web Apps

**Configuration:**
- `azure-static-webapp.json` configuration
- GitHub Actions workflow
- Build configuration

**Package:**
- Static files ready for Azure deployment
- No server-side code required

### AWS Lambda

**Configuration:**
- Lambda function handler
- API Gateway configuration
- Serverless framework config

**Package:**
- ZIP file with Lambda function
- Web interface embedded
- Documentation files included

### Docker Container

**Configuration:**
- Dockerfile
- nginx configuration
- Health check endpoint

**Package:**
- Docker image with web server
- Documentation files embedded
- Ready to deploy to any container platform

### GitHub Pages

**Configuration:**
- GitHub Actions workflow
- Jekyll configuration (if needed)

**Package:**
- Static files compatible with GitHub Pages
- Automatic deployment on push

## Configuration Files

### `.doc-toolkit/config.json`

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

### `.doc-toolkit/llm-config.json`

```json
{
  "provider": "openai",
  "model": "gpt-4",
  "apiKey": "${OPENAI_API_KEY}",
  "temperature": 0.7,
  "maxTokens": 2000
}
```

### `.doc-toolkit/publish-config.json`

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
  "chm": {
    "title": "MyProject Help",
    "defaultTopic": "index.html"
  },
  "deployment": {
    "azure": {
      "resourceGroup": "myproject-docs",
      "staticWebApp": "myproject-docs"
    },
    "aws": {
      "bucket": "myproject-docs",
      "region": "us-east-1"
    }
  }
}
```

## Implementation Plan

### Phase 1: Enhanced Project Structure
1. Update `InitCommand` to create comprehensive structure
2. Create configuration file templates
3. Add walkthrough/onboarding guide
4. Create LLM configuration support

### Phase 2: Publishing Infrastructure
1. Create `PublishCommand` with subcommands
2. Implement web interface packaging
3. Add PDF compilation (using library like PuppeteerSharp or similar)
4. Add CHM compilation support
5. Implement single-file compilation

### Phase 3: Deployment Support
1. Create deployment configurations for each target
2. Implement deployment command
3. Add deployment validation
4. Create deployment scripts/templates

### Phase 4: Advanced Features
1. Custom templates support
2. Plugin system for custom formats
3. CI/CD integration
4. Version management

## Technical Considerations

### PDF Generation
- Use PuppeteerSharp or similar for HTML-to-PDF
- Support custom PDF templates
- Handle large documents efficiently

### CHM Generation
- Use HTML Help Workshop API or alternative
- Generate proper navigation structure
- Support search functionality

### Web Interface Packaging
- Embed documentation files
- Generate static navigation
- Optimize for CDN deployment
- Support offline mode

### Security
- Never include API keys in published output
- Sanitize paths and file names
- Validate deployment configurations
- Support environment variables for secrets

## File Organization

### Source Files (Private)
- Markdown source files
- Templates
- Configuration files with secrets
- Semantic index data
- Knowledge graph data

### Published Files (Public-Ready)
- Compiled documentation
- Web interface assets
- Deployment configurations (without secrets)
- Static navigation files

## Benefits

1. **Clear Separation**: Source repository stays private, published output is public-ready
2. **Multiple Formats**: Support various output formats for different use cases
3. **Easy Deployment**: One command to package and deploy
4. **Flexible**: Support multiple deployment targets
5. **Professional**: Like man pages, structured and navigable
6. **Self-Contained**: Published web interface is completely standalone
