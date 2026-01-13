using System.Diagnostics;
using System.Text.Json;
using DocToolkit.ifx.Interfaces.IAccessors;
using DocToolkit.ifx.Models;

namespace DocToolkit.Accessors;

/// <summary>
/// Accessor for project workspace operations.
/// Encapsulates storage volatility: file system operations could change (local → cloud storage).
/// </summary>
/// <remarks>
/// Component Type: Accessor (Storage Volatility)
/// Volatility: File system and storage technology
/// Pattern: Dumb CRUD operations - no business logic
/// Service Boundary: Called by InitCommand (Client)
/// </remarks>
public class ProjectAccessor : IProjectAccessor
{
    /// <summary>
    /// Creates the directory structure for a new project with opinionated organization.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="projectType">Type of project (determines folder structure)</param>
    public void CreateDirectories(string projectName, ProjectType projectType)
    {
        // Root directories
        Directory.CreateDirectory(projectName);
        Directory.CreateDirectory(Path.Combine(projectName, "docs"));
        Directory.CreateDirectory(Path.Combine(projectName, ".cursor"));
        Directory.CreateDirectory(Path.Combine(projectName, ".doc-toolkit"));

        // Opinionated documentation organization based on project type
        switch (projectType)
        {
            case ProjectType.CustomerFacing:
                // Only customer-facing documentation
                Directory.CreateDirectory(Path.Combine(projectName, "docs", "customer"));
                break;

            case ProjectType.DeveloperFacing:
                // Only developer-facing documentation
                Directory.CreateDirectory(Path.Combine(projectName, "docs", "developer"));
                break;

            case ProjectType.Mixed:
                // Both customer and developer documentation
                Directory.CreateDirectory(Path.Combine(projectName, "docs", "customer"));
                Directory.CreateDirectory(Path.Combine(projectName, "docs", "developer"));
                break;
        }

        // Shared documentation (always created)
        Directory.CreateDirectory(Path.Combine(projectName, "docs", "shared"));

        // Publishing directories
        Directory.CreateDirectory(Path.Combine(projectName, "publish"));
        Directory.CreateDirectory(Path.Combine(projectName, "publish", "web"));
        Directory.CreateDirectory(Path.Combine(projectName, "publish", "pdf"));
        Directory.CreateDirectory(Path.Combine(projectName, "publish", "chm"));
        Directory.CreateDirectory(Path.Combine(projectName, "publish", "single"));

        // Deployment configurations
        Directory.CreateDirectory(Path.Combine(projectName, "deploy"));
        Directory.CreateDirectory(Path.Combine(projectName, "deploy", "azure-static-webapp"));
        Directory.CreateDirectory(Path.Combine(projectName, "deploy", "aws-lambda"));
        Directory.CreateDirectory(Path.Combine(projectName, "deploy", "docker"));
        Directory.CreateDirectory(Path.Combine(projectName, "deploy", "github-pages"));

        // Optional templates directory
        Directory.CreateDirectory(Path.Combine(projectName, "templates"));

        // Docs-as-code: CI/CD directories
        Directory.CreateDirectory(Path.Combine(projectName, ".github", "workflows"));
    }

    /// <summary>
    /// Creates the Cursor IDE configuration file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void CreateCursorConfig(string projectName)
    {
        var cursorConfig = new
        {
            version = 1,
            rules = new[]
            {
                "../../doc-toolkit/.cursor/rules/00-core-instructions.mdc",
                "../../doc-toolkit/.cursor/rules/sow-rules.mdc"
            },
            documentTypes = new
            {
                sow = new { template = "../../doc-toolkit/templates/sow.md" },
                prd = new { template = "../../doc-toolkit/templates/prd.md" },
                rfp = new { template = "../../doc-toolkit/templates/rfp.md" },
                tender = new { template = "../../doc-toolkit/templates/tender.md" },
                architecture = new { template = "../../doc-toolkit/templates/architecture.md" },
                solution = new { template = "../../doc-toolkit/templates/solution.md" }
            }
        };

        var json = JsonSerializer.Serialize(cursorConfig, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var cursorPath = Path.Combine(projectName, ".cursor", "cursor.json");
        File.WriteAllText(cursorPath, json);
    }

    /// <summary>
    /// Creates a README file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="projectType">Type of project (for structure explanation)</param>
    public void CreateReadme(string projectName, ProjectType projectType)
    {
        var customerSection = projectType == ProjectType.CustomerFacing || projectType == ProjectType.Mixed
            ? "- `docs/customer/` - Customer-facing documentation (PRDs, proposals, requirements)"
            : "";
        var developerSection = projectType == ProjectType.DeveloperFacing || projectType == ProjectType.Mixed
            ? "- `docs/developer/` - Developer-facing documentation (architecture, design, specs)"
            : "";

        var readme = $@"# {projectName}

This documentation project was generated using the Documentation Toolkit.

## Project Structure

### Documentation Organization

This project uses an opinionated folder structure to separate documentation by audience:

{customerSection}
{developerSection}
- `docs/shared/` - Shared documentation (glossary, decisions, changelog)

### Configuration
- `.cursor/` - Cursor IDE configuration
- `.doc-toolkit/` - Toolkit configuration files
- `templates/` - Project-specific document templates (optional)

### Published Output (Public-Ready)
- `publish/web/` - Compiled static site for deployment
- `publish/pdf/` - PDF documentation files
- `publish/chm/` - Compiled HTML Help files
- `publish/single/` - Single-file compilation

### Deployment Configurations
- `deploy/` - Deployment configurations for various platforms

## Quick Start

1. **Generate your first document:**
   ```bash
   doc generate prd ""Product Name""
   ```

2. **Build static site:**
   ```bash
   doc build
   ```

3. **View documentation locally:**
   ```bash
   doc web
   ```

4. **Publish documentation:**
   ```bash
   doc publish web
   ```

## Commands

- `doc generate <type> <name>` - Generate a document from a template
- `doc build` - Build static site from markdown files
- `doc suggest` - Get template suggestions based on project state
- `doc publish <format>` - Publish documentation (web, pdf, chm, single, all)
- `doc web` - Start local web server to view documents

## Publishing

The `publish/` directory contains compiled documentation ready for deployment:

- **Web**: Self-contained web interface for static hosting
- **PDF**: Professional PDF documents
- **CHM**: Windows Help files
- **Single**: Combined single-file documentation

See `ONBOARDING.md` for a detailed walkthrough.
";

        var readmePath = Path.Combine(projectName, "README.md");
        File.WriteAllText(readmePath, readme);
    }

    /// <summary>
    /// Creates a .gitignore file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void CreateGitIgnore(string projectName)
    {
        var gitignore = @"# Source files (private - keep structure but ignore content)
/docs/*
!/docs/.keep
/source/*
!/source/.keep

# Generated files
/semantic-index/*
!/semantic-index/.keep
/knowledge-graph/*
!/knowledge-graph/.keep

# Published output (can be committed if needed)
/publish/*
!/publish/.keep

# Configuration (keep structure, ignore secrets)
/.cursor/*
!/.cursor/cursor.json
/.doc-toolkit/llm-config.json
/.doc-toolkit/*-secrets.json

# System files
.DS_Store
Thumbs.db
*.swp
*.swo
*~

# IDE
.vs/
.idea/
*.user
*.suo
";

        var gitignorePath = Path.Combine(projectName, ".gitignore");
        File.WriteAllText(gitignorePath, gitignore);
    }

    /// <summary>
    /// Creates configuration files for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void CreateConfigFiles(string projectName)
    {
        var configDir = Path.Combine(projectName, ".doc-toolkit");

        // Main config.json
        var config = new
        {
            project = new
            {
                name = projectName,
                version = "1.0.0",
                description = "Documentation project"
            },
            paths = new
            {
                docs = "./docs",
                source = "./source",
                publish = "./publish"
            },
            publishing = new
            {
                defaultFormat = "web",
                defaultTarget = "azure",
                includeSource = false,
                minify = true
            }
        };

        var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(configDir, "config.json"), configJson);

        // LLM config template
        var llmConfig = new
        {
            provider = "openai",
            model = "gpt-4",
            apiKey = "${OPENAI_API_KEY}",
            temperature = 0.7,
            maxTokens = 2000,
            _comment = "Set OPENAI_API_KEY environment variable or update this file with your API key"
        };

        var llmConfigJson = JsonSerializer.Serialize(llmConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(configDir, "llm-config.json"), llmConfigJson);

        // Publish config
        var publishConfig = new
        {
            web = new
            {
                title = $"{projectName} Documentation",
                theme = "nord-dark",
                includeSearch = true,
                includeTOC = true
            },
            pdf = new
            {
                template = "default",
                pageSize = "A4",
                includeTOC = true,
                singleFile = false
            },
            chm = new
            {
                title = $"{projectName} Help",
                defaultTopic = "index.html"
            },
            deployment = new
            {
                azure = new
                {
                    resourceGroup = $"{projectName.ToLowerInvariant()}-docs",
                    staticWebApp = $"{projectName.ToLowerInvariant()}-docs"
                },
                aws = new
                {
                    bucket = $"{projectName.ToLowerInvariant()}-docs",
                    region = "us-east-1"
                }
            }
        };

        var publishConfigJson = JsonSerializer.Serialize(publishConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(configDir, "publish-config.json"), publishConfigJson);
    }

    /// <summary>
    /// Creates an onboarding/walkthrough guide.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="projectType">Type of project (for structure explanation)</param>
    public void CreateOnboardingGuide(string projectName, ProjectType projectType)
    {
        var customerDocs = projectType == ProjectType.CustomerFacing || projectType == ProjectType.Mixed
            ? "- `docs/customer/` - Customer-facing documentation (PRDs, proposals, requirements)"
            : "";
        var developerDocs = projectType == ProjectType.DeveloperFacing || projectType == ProjectType.Mixed
            ? "- `docs/developer/` - Developer-facing documentation (architecture, design, specs)"
            : "";

        var onboarding = $@"# Getting Started with {projectName}

Welcome! This guide will walk you through using the Documentation Toolkit to create, manage, and publish your documentation.

## Documentation Organization

This project uses an opinionated folder structure:

{customerDocs}
{developerDocs}
- `docs/shared/` - Shared documentation (glossary, decisions, changelog)

## Step 1: Generate Your First Document

Start by creating a document from a template:

\`\`\`bash
doc generate prd ""Product Requirements""
\`\`\`

This creates a new Product Requirements Document. Available document types:

- `prd` - Product Requirements Document
- `rfp` - Request for Proposal
- `tender` - Tender Response
- `sow` - Statement of Work
- `architecture` - Architecture Design
- `solution` - Solution Proposal
- `spec` - Engineering Specification
- `api` - API Design
- `data` - Data Model
- `blog` - Blog Post
- `weekly-log` - Weekly Log

## Step 2: Get Template Suggestions

Get suggestions for which templates to use based on your project:

\`\`\`bash
doc suggest
\`\`\`

This analyzes your existing documentation and suggests what's missing.

## Step 3: Build Static Site

Compile your markdown files into a static site:

\`\`\`bash
doc build
\`\`\`

This:
- Compiles markdown to HTML
- Resolves cross-references
- Validates links
- Generates navigation
- Creates documentation index

## Step 4: View Documentation Locally

Start the web interface to view and share your documents:

\`\`\`bash
doc web
\`\`\`

Then open http://localhost:5000 in your browser.

## Step 5: Publish Your Documentation

### Build and Deploy

The build process creates a static site in `publish/web/` that can be deployed to:
- GitHub Pages (automated via CI/CD)
- Azure Static Web Apps
- AWS S3 + CloudFront
- Docker container
- Any static hosting service

\`\`\`bash
doc build
\`\`\`

### CI/CD Automation

This project includes GitHub Actions workflows that automatically:
- Build documentation on push
- Validate links
- Deploy to GitHub Pages

## Project Structure

### Documentation
{customerDocs}
{developerDocs}
- `docs/shared/` - Shared documentation

### Configuration
- `.doc-toolkit/` - Configuration files
- `.cursor/` - IDE configuration

### Published Output (Public-Ready)
- `publish/web/` - Compiled static site
- `publish/pdf/` - PDF files
- `publish/chm/` - Windows Help files
- `publish/single/` - Single-file compilation

## Configuration

Edit `.doc-toolkit/config.json` to customize:
- Project metadata
- Paths
- Publishing defaults

Edit `.doc-toolkit/publish-config.json` to configure:
- Web interface settings
- PDF generation options
- Deployment targets

## Deployment

The `publish/web/` directory contains everything needed to deploy your documentation website. See deployment configurations in `deploy/` for:
- Azure Static Web Apps
- AWS Lambda
- Docker
- GitHub Pages

## Next Steps

1. Generate documents for your project
2. Organize them in logical folders
3. Build semantic index for search
4. Publish when ready to share

For more information, see the main README.md or run \`doc --help\`.
";

        var onboardingPath = Path.Combine(projectName, "ONBOARDING.md");
        File.WriteAllText(onboardingPath, onboarding);
    }

    /// <summary>
    /// Creates a .docignore file to exclude files from publishing.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void CreateDocIgnore(string projectName)
    {
        var docignore = @"# Files and patterns to exclude from publishing

# Draft documents
*-draft.md
*-wip.md
*-todo.md

# Internal notes
notes/
internal/
private/

# Temporary files
*.tmp
*.bak
*.swp

# Configuration files
config/
secrets/

# Large binary files
*.pdf
*.zip
*.exe
";

        var docignorePath = Path.Combine(projectName, ".docignore");
        File.WriteAllText(docignorePath, docignore);
    }

    /// <summary>
    /// Initializes a Git repository for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void InitializeGit(string projectName)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "init",
                WorkingDirectory = projectName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            // Add files and commit
            var addProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "add .",
                    WorkingDirectory = projectName,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            addProcess.Start();
            addProcess.WaitForExit();

            var commitProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "commit -m \"Initial project structure created by Documentation Toolkit\"",
                    WorkingDirectory = projectName,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            commitProcess.Start();
            commitProcess.WaitForExit();
        }
    }

    /// <summary>
    /// Creates GitHub Actions workflows for docs-as-code (per Doctave, freeCodeCamp references).
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void CreateLintWorkflow(string projectName)
    {
        var workflowsDir = Path.Combine(projectName, ".github", "workflows");

        // Link check workflow (critical per Doctave)
        var linkCheckWorkflow = @"name: Check Documentation Links

on:
  pull_request:
    branches: [ main ]
  push:
    branches: [ main ]

jobs:
  link-check:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout Code
        uses: actions/checkout@v3

      - name: Check Links
        run: |
          echo ""Checking documentation links...""
          # doc link-check (when implemented)
";

        File.WriteAllText(Path.Combine(workflowsDir, "docs-link-check.yml"), linkCheckWorkflow);

        // Preview environment workflow (per Doctave)
        var previewWorkflow = @"name: Preview Documentation

on:
  pull_request:
    branches: [ main ]

jobs:
  preview:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout Code
        uses: actions/checkout@v3

      - name: Build Preview
        run: |
          echo ""Building preview...""
          # doc publish web (when available)

      - name: Deploy Preview
        run: |
          echo ""Deploy preview to temporary URL""
          # Comment on PR with preview link
";

        File.WriteAllText(Path.Combine(workflowsDir, "docs-preview.yml"), previewWorkflow);

        // Deployment workflow (per freeCodeCamp pattern)
        var deployWorkflow = @"name: Deploy Documentation

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout Code
        uses: actions/checkout@v3

      - name: Build Documentation
        run: |
          echo ""Building documentation...""
          # doc publish web

      - name: Deploy to GitHub Pages
        uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: ./publish/web
";

        File.WriteAllText(Path.Combine(workflowsDir, "docs-deploy.yml"), deployWorkflow);
    }
}
