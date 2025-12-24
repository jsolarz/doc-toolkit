# OpenAI-Specific Overrides
## GPT Model Integration and Best Practices

## OpenAI Model Characteristics

### Code Generation
- GPT models excel at code generation and completion
- Use clear, specific prompts for best results
- Provide examples when possible
- Break complex tasks into smaller functions

### Context Window
- Be mindful of context window limitations
- Prioritize relevant context
- Use file references efficiently
- Summarize long contexts when needed

## OpenAI-Specific Best Practices

### Prompt Engineering
- Use clear, structured prompts
- Provide examples of desired output
- Specify format requirements explicitly
- Break complex tasks into steps

### Code Generation
- Request code following IDesign Method™ principles
- Include XML documentation comments
- Follow C# coding standards
- Use dependency injection patterns

### Documentation Generation
- Follow document templates and structure
- Include all required sections
- Use consistent terminology
- Validate requirements for clarity

## Integration with Prompt System

When using OpenAI models:
- Reference core rules from `prompts/core/prompt_spec.md`
- Follow style guide from `prompts/core/style_guide.md`
- Use agent workflows from `prompts/agents/agents.md`
- Adapt prompts to model capabilities
