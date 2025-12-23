
If you are an AI agent involved in the task, read this guide **VERY, VERY** carefully! Throughout development, you should always (1) start with a small and simple solution, (2) design at a high level (`docs/design.md`) before implementation following the IDesign Method™, and (3) frequently ask humans for feedback and clarification.

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
17. **Message Bus**: Use pub/sub for cross-component communication (future)

See `.cursor/rules/idesign-method.mdc` for detailed IDesign Method™ guidelines.

## Development Workflow

```
1. Define the Goal and Requirements
Clearly articulate the primary objective of the agent or coding task. 
Keep the goal concise, general, and focused on the end result or benefit for the user.
Avoid technical jargon and unnecessary details; use simple, direct language.

2. Provide High-Level System Design (IDesign Method™)
- Start with a high-level one-line description of each component or node in your system
- Apply IDesign Method™: Factor into services, document boundaries
- Show assembly allocation, process allocation, identity boundaries
- Use Markdown with headings and lists to organize sections for easy parsing by AI agents
- Document call chains through the architecture

3. Specify APIs, Utilities, and Integrations
List any external APIs, libraries, or utilities the agent should use, including authentication and data structures.
Give clear, structured definitions for any tools or APIs, including endpoints, request/response formats, and error codes.
Document service boundaries and how external dependencies are accessed.

4. Detail Task Steps and Instructions
Break down the overall task into clear, step-by-step instructions.
For each step, state the expected input, processing, and output.
Include instructions on how to handle errors and ambiguous input, with fallback prompts and clarification questions.
Respect closed architecture - components only call down one tier.

5. Set Response and Formatting Guidelines
Specify how the agent should format its outputs (e.g., code blocks in Markdown, JSON responses, bullet points for options).
Define the tone and formality level (e.g., concise, technical, or user-friendly).

6. Include Test Cases and Validation Criteria
Provide expected input/output pairs for test-driven development.
Ask the agent to write and run tests before implementing code, then iterate until all tests pass.
Optionally, include example conversations or sample interactions for reference.

7. Add Guardrails and Contextual Boundaries
Instruct the agent to stick to the defined goal and avoid unrelated topics.
Remind it to preserve context across multi-step interactions.
Enforce closed architecture - no cross-tier calls.

8. Use Examples and Templates
Where possible, include sample prompts, expected outputs, or example conversations to guide the agent's behavior.

9. Documentation is key
Always keep the README.md and Design.md files up to date when making changes to the code or the design of the app.
Document architecture decisions using IDesign Method™ notations (assembly allocation, process allocation, boundaries).
```

## Service Layer Rules

When creating or modifying services:
- **One Service = One Responsibility**: Each service has a single, well-defined purpose
- **Service Boundaries**: All inter-service communication through well-defined interfaces
- **Closed Architecture**: Services only call services in the layer immediately below
- **Dependency Injection**: Services accept dependencies through constructor
- **Error Handling**: Handle errors at service boundaries

## Architecture Documentation

When updating `docs/design.md`:
1. Include IDesign Method™ diagrams (assembly allocation, process allocation)
2. Document service boundaries and call chains
3. Show identity and security boundaries
4. Document transaction boundaries where applicable
5. Follow closed architecture pattern in all diagrams