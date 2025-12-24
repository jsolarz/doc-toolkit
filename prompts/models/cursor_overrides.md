# Cursor-Specific Overrides
## Cursor IDE Integration and Codebase Indexing

## Cursor IDE Features

### Codebase Indexing
- Cursor automatically indexes the codebase for semantic search
- Use @-symbol to reference files, functions, or symbols
- Codebase context is automatically included in conversations

### File References
- Use `@filename` to reference specific files
- Use `@foldername` to reference entire folders
- Use `@symbolname` to reference functions, classes, or variables

### Rule Files
- Rules are defined in `.cursor/rules/*.mdc` files
- Rules can be scoped by glob patterns
- Rules marked `alwaysApply: true` are always active
- Rules marked `alwaysApply: false` are context-specific

### Current Rule Structure
- `.cursor/rules/00-core-instructions.mdc` - Core document writing rules (always applies)
- `.cursor/rules/idesign-method.mdc` - IDesign Method™ guidelines (always applies)
- `.cursor/rules/architecture-design.mdc` - Architecture document rules (context-specific)
- `.cursor/rules/prd-standard.mdc` - PRD rules (context-specific)
- `.cursor/rules/rfp-tender-strategy.mdc` - RFP/Tender rules (context-specific)
- `.cursor/rules/sow-rules.mdc` - SOW rules (context-specific)

## Cursor-Specific Behaviors

### Code Completion
- Cursor provides AI-powered code completion
- Context from open files is automatically included
- Use `Ctrl+K` for inline edits
- Use `Ctrl+L` for chat

### Multi-File Editing
- Cursor supports editing multiple files simultaneously
- Use targeted edits (search_replace) rather than recreating files
- Maintain consistency across related files

### Documentation Generation
- Cursor can generate documentation from code
- Follow XML documentation standards for C#
- Include IDesign Method™ compliance notes in documentation

## Integration with Prompt System

When using Cursor:
- Reference files using @-symbol syntax
- Use codebase search for finding related code
- Leverage automatic context inclusion
- Follow rule file structure for organization
