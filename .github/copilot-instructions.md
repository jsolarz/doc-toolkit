# GitHub Copilot Instructions for Documentation Toolkit

This file provides context and guidelines for GitHub Copilot when working on this project.

## Project Overview

**Documentation Toolkit** - A C# (.NET 9.0) CLI application for generating professional documentation following docs-as-code principles.

## Architecture

This project follows the **IDesign Method™** with volatility-based decomposition:

- **Clients** (`ifx/Commands/`): CLI command implementations (UI volatility)
- **Managers** (`Managers/`): Orchestration logic (workflow volatility)
- **Engines** (`Engines/`): Business logic and algorithms (algorithm volatility)
- **Accessors** (`Accessors/`): Storage and resource abstraction (storage volatility)
- **Infrastructure** (`ifx/Infrastructure/`): DI, Event Bus, logging

## Coding Standards

- Follow **IDesign C# Coding Standard 3.1**
- Use dependency injection for all services
- One class per file
- XML documentation for all public APIs
- Argument validation in all public methods
- Error handling at service boundaries

## Naming Conventions

- **Namespaces**: Match directory structure (e.g., `DocToolkit.Accessors`)
- **Classes/Methods/Properties**: PascalCase
- **Parameters/Local variables**: camelCase
- **Private fields**: `_camelCase` with underscore prefix
- **Interfaces**: Prefix with 'I' (e.g., `IProjectAccessor`)

## Project Structure

```
src/DocToolkit/
├── Accessors/          # Storage volatility
├── Engines/            # Algorithm volatility
├── Managers/           # Workflow volatility
└── ifx/                # Infrastructure
    ├── Commands/        # CLI commands (Clients)
    ├── Events/         # Event definitions
    ├── Infrastructure/ # DI, Event Bus, etc.
    ├── Interfaces/     # All interfaces
    └── Models/         # Data models
```

## Key Patterns

### Dependency Injection
All services are registered in `ServiceConfiguration.cs` and injected via constructor.

### Event Bus
Use the event bus for cross-manager communication. Events are defined in `ifx/Events/`.

### Error Handling
Handle errors at service boundaries. Use exceptions for exceptional conditions.

### Testing
- Unit tests in `src/tests/DocToolkit.Tests/`
- Follow existing test patterns
- Use xUnit framework

## Versioning

This project uses **Semantic Versioning (SemVer 2.0.0)**:
- Format: `MAJOR.MINOR.PATCH` (e.g., `1.0.0`)
- Pre-release: `MAJOR.MINOR.PATCH-alpha.1`, `-beta.1`, `-rc.1`
- Version is stored in `VERSION` file and `DocToolkit.csproj`

## Documentation

- Technical docs in `docs/`
- Code documentation standards in `src/DocToolkit/Docs/`
- Follow `.cursor/rules/` for document generation

## Common Tasks

### Adding a New Command
1. Create command class in `ifx/Commands/`
2. Register in `Program.cs`
3. Add tests in `src/tests/DocToolkit.Tests/Commands/`

### Adding a New Service
1. Create interface in `ifx/Interfaces/`
2. Implement in appropriate folder (Manager/Engine/Accessor)
3. Register in `ServiceConfiguration.cs`
4. Add XML documentation
5. Add tests

### Updating Version
1. Update `VERSION` file
2. Update `DocToolkit.csproj` `<Version>` property
3. Update `CHANGELOG.md`

## References

- [IDesign Method™](.cursor/rules/idesign-method.mdc)
- [C# Coding Standards](src/DocToolkit/Docs/IDESIGN-CSHARP-CODING-STANDARD-COMPLIANCE.md)
- [Semantic Versioning](https://semver.org/)
