# Documentation Toolkit CLI

A beautiful, cross-platform CLI application for generating professional documentation.

## Features

- 🎨 Beautiful terminal UI with [Spectre.Console](https://spectreconsole.net/)
- 🚀 Fast and responsive
- 📦 Cross-platform (Windows, Linux, macOS)
- 🔍 Semantic search and indexing
- 📊 Knowledge graph generation
- 📝 Multiple document templates

## Installation

### Build from Source

```bash
git clone <repository-url>
cd doc-toolkit/src/DocToolkit
dotnet build
```

### Run Directly

```bash
cd src/DocToolkit
dotnet run -- <command>
```

### Create Global Tool (Optional)

```bash
dotnet pack
dotnet tool install -g --add-source ./bin/Debug DocToolkit
```

Then use `doc` from anywhere:
```bash
doc init MyProject
```

## Quick Start

1. **Initialize a project:**
   ```bash
   doc init MyProject
   ```

2. **Generate a document:**
   ```bash
   cd MyProject
   doc generate prd "User Management"
   ```

3. **Add source files:**
   ```bash
   # Add files to ./source/
   ```

4. **Build semantic index:**
   ```bash
   doc index
   ```

5. **Search:**
   ```bash
   doc search "customer requirements"
   ```

## Commands

### `doc init <name>`

Initialize a new documentation project.

**Example:**
```bash
doc init MyProject
```

Creates:
- Project directory structure
- Cursor IDE configuration
- Git repository
- README and .gitignore

### `doc generate <type> <name>`

Generate a document from a template.

**Types:**
- `prd` - Product Requirements Document
- `rfp` - Request for Proposal
- `tender` - Tender Response
- `sow` - Statement of Work
- `architecture` - Architecture Design
- `solution` - Solution Proposal
- `sla` - Service Level Agreement
- `spec` - Engineering Specification
- `api` - API Design
- `data` - Data Model
- `blog` - Blog Post
- `weekly-log` - Weekly Log Template

**Examples:**
```bash
doc generate prd "User Management"
doc gen sow "Cloud Migration"  # Short alias
```

**Options:**
- `-o, --output <path>` - Output directory (default: `./docs`)

### `doc index`

Build semantic index from source files.

**Options:**
- `-s, --source <path>` - Source directory (default: `./source`)
- `-o, --output <path>` - Index output directory (default: `./semantic-index`)
- `--chunk-size <number>` - Chunk size in words (default: 800)
- `--chunk-overlap <number>` - Chunk overlap in words (default: 200)

**Example:**
```bash
doc index --source ./my-source --chunk-size 1000
```

### `doc search <query>`

Search the semantic index.

**Options:**
- `-i, --index <path>` - Index directory (default: `./semantic-index`)
- `-k, --top-k <number>` - Number of results (default: 5)

**Example:**
```bash
doc search "customer requirements" --top-k 10
```

### `doc graph`

Build knowledge graph from source files.

**Options:**
- `-s, --source <path>` - Source directory (default: `./source`)
- `-o, --output <path>` - Output directory (default: `./knowledge-graph`)

**Example:**
```bash
doc graph --source ./source --output ./kg
```

### `doc summarize`

Summarize source files into a context document.

**Options:**
- `-s, --source <path>` - Source directory (default: `./source`)
- `-o, --output <path>` - Output file (default: `./context.md`)
- `--max-chars <number>` - Maximum characters per file (default: 5000)

**Example:**
```bash
doc summarize --output ./summary.md
```

### `doc validate`

Validate setup and dependencies.

**Example:**
```bash
doc validate
```

Checks:
- Python installation and version
- Required Python packages
- External tools (Poppler, Tesseract)
- Directory structure
- Templates availability

## Configuration

The CLI automatically detects:
- Templates directory (searches relative paths)
- Scripts directory (for Python integration)
- Project structure

## Troubleshooting

### Python Not Found

The CLI requires Python for semantic indexing and knowledge graph features. Install Python 3.10+ and ensure it's in your PATH.

### Missing Templates

If templates aren't found, ensure you're running from the toolkit root directory or that templates are in a relative path.

### Permission Errors

On Linux/macOS, you may need to make the executable:
```bash
chmod +x doc
```

## Development

### Project Structure

```
DocToolkit/
├── Commands/          # CLI command implementations
├── Services/          # Business logic services
├── Program.cs         # Entry point
└── DocToolkit.csproj  # Project file
```

### Adding New Commands

1. Create a command class in `Commands/`:
```csharp
public sealed class MyCommand : Command<MyCommand.Settings>
{
    public sealed class Settings : CommandSettings { }
    
    public override int Execute(CommandContext context, Settings settings)
    {
        // Implementation
        return 0;
    }
}
```

2. Register in `Program.cs`:
```csharp
config.AddCommand<MyCommand>("my-command")
    .WithDescription("Description here");
```

## License

MIT License - see [LICENSE](LICENSE) for details.
