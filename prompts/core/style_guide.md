# Style Guide
## Naming Conventions, Documentation Tone, and Formatting Rules

## Naming Conventions

### C# Naming
- **Interfaces**: "I" prefix (e.g., `ISemanticIndexManager`, `ITextChunkingEngine`)
- **Classes**: PascalCase (e.g., `SemanticIndexManager`, `TextChunkingEngine`)
- **Methods**: PascalCase (e.g., `BuildIndex`, `ChunkText`, `CosineSimilarity`)
- **Private Fields**: Underscore prefix (e.g., `_indexManager`, `_logger`, `_embeddingEngine`)
- **Parameters**: camelCase (e.g., `sourcePath`, `chunkSize`, `topK`)
- **Local Variables**: camelCase (e.g., `fileCount`, `totalMemory`)
- **Constants**: PascalCase (e.g., `DefaultChunkSize`, `MaxRetries`)

### File Naming
- **C# Files**: Match class name (e.g., `SemanticIndexManager.cs`)
- **Documentation**: Descriptive names with hyphens (e.g., `ARCHITECTURE-Documentation-Toolkit.md`)
- **Templates**: Lowercase with hyphens (e.g., `prd.md`, `architecture.md`)

## Documentation Tone

### Voice and Style
- **Role**: Lead Solution Architect & Principal Product Manager
- **Voice**: Authoritative, precise, and concise
- **Style**: Active voice, avoid "fluff" words
- **Tone**: Professional, concise, objective
- **Formatting**: Markdown with clear H1-H3 hierarchy

### Writing Principles
- Use short, direct sentences
- Avoid jargon unless defined
- Use consistent terminology throughout document
- No marketing fluff unless explicitly requested
- Use present tense for system behavior
- Define all acronyms on first use

### Prohibited Content
- No marketing fluff unless explicitly requested
- No speculative claims without evidence
- No ambiguous requirements
- No mixing business, functional, and technical layers
- No emojis, icons, or horizontal rules (---) in documentation files

## Formatting Rules

### Headings
- Use H1 for document title only
- Use H2 for major sections
- Use H3/H4 for subsections
- Maintain consistent hierarchy

### Lists
- Use bullet points for unordered lists
- Use numbered lists for sequences or processes
- Keep lists concise and focused

### Tables
- Use tables for comparisons, matrices, and structured data
- Include clear column headers
- Align content appropriately

### Code Blocks
- Use language tags for syntax highlighting
- Include line numbers only when referencing existing code
- Keep code examples focused and relevant

### Text Formatting
- Use **bold** for emphasis on key terms
- Use `code` formatting for technical terms, file names, and code references
- Use *italic* sparingly for definitions or notes
- Avoid excessive formatting

## Document Structure

### Universal Document Sections
All documents must include these sections unless explicitly removed:
1. Executive Summary
2. Purpose & Scope
3. Problem Statement / Business Need
4. Objectives & Success Criteria
5. Stakeholders
6. Requirements / Specifications
7. Assumptions & Constraints
8. Risks & Mitigations
9. Timeline / Milestones
10. Appendices / References

### Document-Specific Sections

**PRD (Product Requirements Document)**:
- Product Overview
- User Personas
- User Stories (format: *As a [user], I want [feature], so that [value].*)
- Functional Requirements (with acceptance criteria)
- Non-Functional Requirements (performance, security, reliability, scalability, compliance)
- UX/UI Considerations
- Data Requirements
- Analytics & KPIs
- Release Criteria

**Architecture Design Document**:
- Architecture Overview
- System Context Diagram
- Component Architecture
- Data Flow Diagrams
- Integration Architecture
- Security Architecture
- Deployment Architecture
- Scalability & Performance
- Operational Considerations

**RFP (Request for Proposal)**:
- Background & Context
- Project Goals
- Scope of Work
- Technical Requirements (explicit and measurable)
- Vendor Qualifications
- Evaluation Criteria
- Submission Instructions

**Tender Response**:
- Executive Summary
- Understanding of Requirements (restate requirements before answering)
- Proposed Solution
- Technical Approach
- Delivery Plan
- Team & Qualifications
- Pricing (if applicable)
- Compliance Matrix

**Solution Proposal**:
- Executive Summary
- Problem Understanding
- Proposed Solution
- Architecture Overview
- Implementation Plan
- Benefits & Value (quantifiable and qualitative)
- Risks & Mitigations
- Cost Estimate (if applicable)

**SOW (Statement of Work)**:
- Project Overview (Executive Summary, Requirements, Success Criteria, Assumptions, Customer Responsibilities, Out of Scope)
- Solution Architecture (diagram placeholder and text description)
- Scope, Milestones & Deliverables (each milestone must include description and deliverables)
- Cost Estimation (Cloud Platform, 3rd Party, Effort Estimation table with Milestone/Item/Effort columns, Disclaimer with 60-day validity and T&M billing note)

**SOW-Specific Rules**:
- Requirements must be written as clear, testable statements
- Success criteria must be measurable and map directly to requirements
- Assumptions must be explicit, realistic, and relevant
- Customer responsibilities must be actionable and unambiguous
- Out of scope must explicitly list exclusions (avoid vague statements)
- Deliverables must be concrete, measurable, and reviewable
- Effort estimation must use table format: Milestone, Item, Effort (days)

## Validation Rules

Before finalizing any document:
- Check for missing sections
- Validate requirements for clarity and testability
- Ensure diagrams (or diagram descriptions) are included
- Confirm alignment with business objectives
- Ensure no contradictions exist
- Verify consistent terminology throughout
- Remove redundancy and unnecessary content

## Decision Log Rules

Every document must include:
- Decisions made
- Alternatives considered
- Rationale
- Stakeholders involved

## Security & Compliance Rules

When applicable:
- Include data protection requirements
- Include access control models
- Include compliance standards (GDPR, ISO, SOC2, etc.)
- Document encryption, logging, and monitoring expectations
