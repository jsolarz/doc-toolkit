
If you are an AI agent involved in the task, read this guide **VERY, VERY** carefully! Throughout development, you should always (1) start with a small and simple solution, (2) design at a high level (`docs/design.md`) before implementation following the IDesign Method™, and (3) frequently ask humans for feedback and clarification.

## Agent Definition

An agent is an LLM that autonomously uses tools in a loop. As underlying models become more capable, the level of autonomy scales—smarter models allow agents to independently navigate their information landscape and make decisions.

## Context Engineering for Agents

### The Challenge

Agents running in loops generate more and more data that *could* be relevant for the next turn of inference. This information must be cyclically refined. Context engineering is the art and science of curating what will go into the limited context window from that constantly evolving universe of possible information.

### Context Retrieval Strategies

**Pre-computed vs. Runtime Exploration**

- **Pre-computed (Indexing)**: Fast retrieval of pre-processed data, but risks stale information and complex maintenance
- **Runtime Exploration (Agentic Search)**: Agents use tools to navigate and retrieve information just-in-time, ensuring freshness but slower than pre-computed retrieval
- **Hybrid Strategy**: Retrieve some data up front for speed, pursue further autonomous exploration at agent's discretion

**Decision Boundary**

The right level of autonomy depends on the task:
- Dynamic content (codebases): Prefer runtime exploration with tools like glob and grep
- Static content (legal, finance): Hybrid strategy may be more effective
- As models improve, trend towards letting intelligent models act intelligently with less human curation

**RAG and Memory Optimization**

- Cache frequently used queries and results in vector stores
- Search existing results before generating new queries to avoid redundant LLM calls
- Reduces latency and costs while maintaining accuracy
- Creative context engineering through intelligent caching is a competitive advantage
- Optimize vector store maintenance and retrieval strategies

### Long-Horizon Task Management

For tasks spanning tens of minutes to multiple hours, agents require specialized techniques to work around context window limitations.

#### 1. Compaction

**What it is**: Taking a conversation nearing the context window limit, summarizing its contents, and reinitiating a new context window with the summary.

**Implementation**:
- Pass message history to model for summarization
- Preserve architectural decisions, unresolved bugs, implementation details
- Discard redundant tool outputs or messages
- Continue with compressed context plus most recently accessed files

**Best Practices**:
- Carefully tune compaction prompts on complex agent traces
- Start by maximizing recall, then iterate to improve precision
- Clear tool calls and results—once a tool has been called deep in history, the raw result is rarely needed again
- Tool result clearing is the safest, lightest-touch form of compaction

#### 2. Structured Note-Taking (Agentic Memory)

**What it is**: Agent regularly writes notes persisted to memory outside the context window. These notes get pulled back into context at later times.

**Implementation**:
- Use memory tool to create, read, update, and delete files in dedicated memory directory
- Maintain persistent memory with minimal overhead
- Track progress across complex tasks (e.g., TODO lists, NOTES.md files)
- Maintain critical context and dependencies that would otherwise be lost

**Benefits**:
- Enables coherence across summarization steps
- Supports long-horizon strategies impossible with context window alone
- Builds knowledge bases over time
- Maintains project state across sessions

**Example Patterns**:
- Create TODO lists to track progress
- Maintain NOTES.md files for project state
- **Use SESSION-LEDGER.md**: Mandatory file that tracks all AI interactions, decisions, and context across sessions (see `.cursor/rules/session-ledger.mdc`)
- Track objectives and achievements (e.g., "for the last 1,234 steps I've been training my Pokémon in Route 1")
- Develop maps of explored regions
- Remember key achievements and strategic notes

#### 3. Sub-Agent Architectures

**What it is**: Specialized sub-agents handle focused tasks with clean context windows. Main agent coordinates with high-level plan while sub-agents perform deep technical work.

**Pattern**:
- Main agent: High-level coordination and synthesis
- Sub-agents: Deep exploration using tens of thousands of tokens, return condensed summaries (1,000-2,000 tokens)
- Clear separation of concerns: detailed search context isolated within sub-agents

**Benefits**:
- Detailed search context remains isolated
- Lead agent focuses on synthesizing and analyzing results
- Substantial improvement over single-agent systems on complex research tasks

**When to Use**:
- Complex research and analysis where parallel exploration pays dividends
- Tasks requiring extensive exploration in multiple domains
- When detailed context would pollute the main agent's attention

### Choosing the Right Strategy

| Strategy | Best For | Characteristics |
|----------|----------|-----------------|
| **Compaction** | Tasks requiring extensive back-and-forth | Maintains conversational flow |
| **Note-Taking** | Iterative development with clear milestones | Tracks progress, maintains state |
| **Multi-Agent** | Complex research and analysis | Parallel exploration, synthesis |

## Agent Design Patterns

### Tool Design for Agents

**Principles**:
- Tools should be well-understood by LLMs with minimal overlap
- Self-contained, robust to error, extremely clear on intended use
- Input parameters should be descriptive, unambiguous
- Token-efficient returns
- Minimal viable set—if a human can't definitively choose which tool to use, an agent can't either

**Common Failure Modes**:
- Bloated tool sets covering too much functionality
- Ambiguous decision points about which tool to use
- Tools that encourage inefficient agent behaviors

### System Prompt Design for Agents

**Right Altitude**:
- Specific enough to guide behavior effectively
- Flexible enough to provide strong heuristics
- Avoid brittle if-else hardcoded logic
- Avoid vague, high-level guidance that assumes shared context

**Organization**:
- Distinct sections: `<background_information>`, `<instructions>`, `## Tool guidance`, `## Output description`
- Use XML tagging or Markdown headers to delineate sections
- Minimal set of information that fully outlines expected behavior

### Example Curation

**Best Practices**:
- Curate diverse, canonical examples that effectively portray expected behavior
- Examples are "pictures worth a thousand words"
- Avoid exhaustive edge case lists
- Focus on representative examples that demonstrate core patterns

## Context Management Best Practices

### General Principles

1. **Treat context as finite**: Every token depletes the attention budget
2. **Maximize signal-to-noise**: Include only high-signal tokens
3. **Dynamic retrieval**: Use tools to pull in context just-in-time
4. **Clear stale data**: Automatically remove outdated information
5. **Persist critical information**: Use memory tools for long-term knowledge
6. **Measure effectiveness**: Build evaluation pipelines to track context performance
7. **Detect context dilution**: Monitor for stale and irrelevant information accumulation

### For Long-Running Agents

1. **Monitor context usage**: Track remaining context window throughout conversation
2. **Implement compaction**: Summarize and compress when approaching limits
3. **Use structured notes**: Maintain persistent memory outside context window
4. **Consider sub-agents**: Isolate detailed exploration in specialized agents
5. **Clear tool results**: Remove redundant tool outputs from history

### For Multi-Turn Interactions

1. **Preserve critical decisions**: Keep architectural decisions and unresolved issues
2. **Discard redundant information**: Remove duplicate tool outputs and messages
3. **Maintain recent context**: Keep most recently accessed files in context
4. **Reference external memory**: Pull in notes and state from memory tool when needed

### State and Historical Context Management

**State Management Patterns**:
- Maintain access to previous states and historical context for revision phases
- Track subtasks, revisions, and past results from each agent in the workflow
- Pass state context based on optimization goals (speed, accuracy, cost)
- Context engineering requires iterative decision-making about what state to preserve

**Structured Outputs for State**:
- Use JSON schemas to structure state information consistently
- Define clear data structures for agent outputs that must be passed between components
- Reduce parsing errors and improve reliability of multi-agent workflows
- Structured outputs are critical when agents need to revise or iterate on previous work

**Query Augmentation**:
- Enhance queries before passing to tools (e.g., adding date ranges, domain context)
- Use dynamic context elements (date/time) to generate accurate queries
- Infer contextual information (like date ranges) from current state and user intent

## Advanced Context Engineering Topics

### Context Compression
- Techniques for compressing context while preserving critical information
- Balancing compression ratio with information fidelity
- When to compress vs. when to retrieve fresh context

### Context Safety
- Preventing context injection attacks
- Validating and sanitizing dynamic context elements
- Ensuring context integrity across agent interactions

### Evaluating Context Effectiveness
- Building evaluation pipelines to measure context performance
- Tracking metrics over time to detect context degradation
- A/B testing different context engineering strategies
- Measuring the impact of context changes on agent behavior

## References

- [Effective Context Engineering for AI Agents](https://www.anthropic.com/engineering/effective-context-engineering-for-ai-agents)
- [Claude Platform Context Windows Documentation](https://platform.claude.com/docs/en/build-with-claude/context-windows)
- [Claude Memory Tool Cookbook](https://github.com/anthropics/claude-cookbooks/blob/main/tool_use/memory_cookbook.ipynb)
- [Claude Skills Documentation](https://support.claude.com/en/articles/12512176-what-are-skills)
- [Context Engineering Guide](https://www.promptingguide.ai/guides/context-engineering-guide)

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
17. **Message Bus**: Use pub/sub for cross-component communication ✅ IMPLEMENTED

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
```

## Service Layer Rules

When creating or modifying services:
- **One Service = One Responsibility**: Each service has a single, well-defined purpose
- **Service Boundaries**: All inter-service communication through well-defined interfaces
- **Closed Architecture**: Services only call services in the layer immediately below
- **Dependency Injection**: Services accept dependencies through constructor ✅ IMPLEMENTED
- **Error Handling**: Handle errors at service boundaries
- **Event Bus**: Use event bus for cross-manager communication ✅ IMPLEMENTED (Phase 3)
- **Component Taxonomy**: Use IDesign Method™ taxonomy (Managers, Engines, Accessors, Clients) ✅ IMPLEMENTED

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