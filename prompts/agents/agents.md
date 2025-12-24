# Agent Roles, Responsibilities, and Inter-Agent Communication

## Agent Identity

**Role**: AI Development Assistant  
**Primary Responsibility**: Assist with code development, documentation, and architecture following IDesign Method™ principles  
**Context**: Documentation Toolkit - C# CLI application for document generation and semantic indexing

## Development Workflow

### 1. Define the Goal and Requirements
- Clearly articulate the primary objective
- Keep the goal concise, general, and focused on end result or benefit
- Avoid technical jargon and unnecessary details
- Use simple, direct language

### 2. Provide High-Level System Design (IDesign Method™)
- Start with high-level one-line description of each component
- Apply IDesign Method™: Factor into services, document boundaries
- Show assembly allocation, process allocation, identity boundaries
- Use Markdown with headings and lists for easy parsing
- Document call chains through the architecture

### 3. Specify APIs, Utilities, and Integrations
- List external APIs, libraries, or utilities
- Give clear, structured definitions for tools or APIs
- Include endpoints, request/response formats, error codes
- Document service boundaries and external dependency access

### 4. Detail Task Steps and Instructions
- Break down overall task into clear, step-by-step instructions
- For each step, state expected input, processing, and output
- Include error handling and ambiguous input instructions
- Respect closed architecture - components only call down one tier

### 5. Set Response and Formatting Guidelines
- Specify output formatting (code blocks, JSON, bullet points)
- Define tone and formality level
- Use consistent formatting across responses

### 6. Include Test Cases and Validation Criteria
- Provide expected input/output pairs for test-driven development
- Write and run tests before implementing code
- Iterate until all tests pass
- Include example interactions for reference

### 7. Add Guardrails and Contextual Boundaries
- Stick to defined goal and avoid unrelated topics
- Preserve context across multi-step interactions
- Enforce closed architecture - no cross-tier calls

### 8. Use Examples and Templates
- Include sample prompts, expected outputs, or example conversations
- Reference templates from `/templates/` directory
- Follow document structure templates

### 9. Documentation Maintenance
- Always keep README.md and docs/design.md up to date when making changes
- Document architecture decisions using IDesign Method™ notations
- Update CHANGELOG.md with all changes
- Follow documentation location rules

## IDesign Method™ Principles

**CRITICAL**: All architecture and design work must follow the IDesign Method™ principles:

### Core Principle: Volatility-Based Decomposition
1. **Decompose by Volatility, Not Functionality**: Identify what could change (database, logic, workflow, UI) and encapsulate each volatility in a component
2. **The Vault Metaphor**: Changes should be contained within one component without rippling through the system

### Component Taxonomy
3. **Client**: Encapsulates UI volatility (CLI, Web, API, Scheduled Task)
4. **Manager**: Encapsulates workflow volatility (orchestration - knows "when")
5. **Engine**: Encapsulates algorithm volatility (business logic - knows "how", pure functions)
6. **Accessor**: Encapsulates storage volatility (resource abstraction - knows "where", dumb CRUD)

### Interaction Patterns
7. **Call Chain**: Client → Manager → Engine/Accessor (avoid "forks" and "staircases")
8. **Engines are Pure**: Accept data as parameters, no direct Accessor calls
9. **Accessors are Dumb**: CRUD operations only, no business logic
10. **Data Exchange**: Pass IDs, not fat entities. Use stable contracts.

### Architecture Documentation
11. **Closed Architecture Pattern**: Components can only call components in the tier immediately underneath
12. **Assembly Allocation**: Document which components belong to which assemblies
13. **Run-Time Process Allocation**: Document which services run in which processes
14. **Identity Management**: Document identity boundaries and security decisions
15. **Authentication & Authorization**: Mark authentication (solid bar) and authorization (patterned bar) at service boundaries
16. **Transaction Boundaries**: Document transaction scopes and flow
17. **Message Bus**: Use pub/sub for cross-component communication

## Service Layer Rules

When creating or modifying services:
- **One Service = One Responsibility**: Each service has a single, well-defined purpose
- **Service Boundaries**: All inter-service communication through well-defined interfaces
- **Closed Architecture**: Services only call services in the layer immediately below
- **Dependency Injection**: Services accept dependencies through constructor
- **Error Handling**: Handle errors at service boundaries
- **Event Bus**: Use event bus for cross-manager communication
- **Component Taxonomy**: Use IDesign Method™ taxonomy (Managers, Engines, Accessors, Clients)

## Architecture Documentation

When updating `docs/design.md`:
1. Include IDesign Method™ diagrams (assembly allocation, process allocation)
2. Document service boundaries and call chains
3. Show identity and security boundaries
4. Document transaction boundaries where applicable
5. Follow closed architecture pattern in all diagrams
6. Document event bus architecture and subscriptions
7. Document dependency injection configuration
8. Include current project structure (Managers/, Engines/, Accessors/, ifx/)

## File Management Rules

CRITICAL: Follow these rules for all file operations:

1. Do not generate unnecessary files. Only create files that are explicitly requested or required for the task.
2. Do not delete and recreate whole files. Use search_replace or similar tools to make targeted edits to existing files.
3. Use one CHANGELOG.md file in the root directory. Do not create multiple changelog files or version-specific changelogs.
4. Use one PATCH-*.md file per set of required changes. This file will be deleted after changes are applied. Do not create multiple patch files for the same change set.
5. Always update documentation and global files after changes:
   - Update README.md if project structure or usage changes
   - Update docs/design.md if architecture changes
   - Update CHANGELOG.md with all changes
   - Update relevant documentation files
6. Do not use emojis, icons, or horizontal rules (---) in documentation files. Keep formatting clean and professional.
7. Keep documentation clean and concise. Remove unnecessary content, avoid redundancy, and focus on essential information.

## Documentation Location Rules

CRITICAL: Follow these rules for documentation placement:

1. **Global Project Documentation**: Place in `docs/` folder
   - Architecture documents (docs/ARCHITECTURE-*.md)
   - Product requirements (docs/PRD-*.md)
   - Data models (docs/DATA-*.md)
   - Engineering specs (docs/SPEC-*.md)
   - Solution proposals (docs/SOLUTION-*.md)
   - Technical documentation (docs/TECHNICAL-DOCUMENTATION.md)
   - System design (docs/design.md)
   - Code standards (docs/CODE-DOCUMENTATION-STANDARDS.md)
   - Developer guides (docs/DEVELOPER-QUICK-REFERENCE.md)
   - Compliance documents (docs/IDESIGN-CSHARP-CODING-STANDARD-COMPLIANCE.md)

2. **Source Code Specific Documentation**: Place in `src/` folder
   - Application-specific docs: `src/DocToolkit/Docs/` or `src/DocToolkit/README.md`
   - Test-specific docs: `src/tests/DocToolkit.Tests/README.md` or `src/tests/DocToolkit.Tests/Docs/`
   - Component-specific docs: Co-located with source code (e.g., `src/DocToolkit/Engines/README.md`)

3. **Root Level Documentation**: Only essential project files
   - README.md (project overview)
   - CHANGELOG.md (project changelog)
   - ONBOARDING.md (project onboarding)
   - LICENSE (if applicable)

4. **Do NOT create documentation in**:
   - Multiple locations for the same content
   - Temporary or intermediary folders
   - Root directory (except essential files listed above)

## Inter-Agent Communication

When multiple agents or workflows are involved:
- Share context through structured documentation
- Use event bus for decoupled communication
- Document service boundaries explicitly
- Follow closed architecture pattern
- Maintain consistency across agents

## Quality Standards

- All code must follow IDesign Method™ principles
- All code must follow IDesign C# Coding Standard 3.1
- All public APIs must have XML documentation
- All changes must update relevant documentation
- All tests must pass before code is considered complete
