# Code Documentation Standards

**Version**: 1.0  
**Last Updated**: 2024

---

## Overview

This document defines the code documentation standards for the Documentation Toolkit project. These standards ensure consistency, clarity, and maintainability across the codebase.

### Style Guide References

This documentation follows best practices from:
- [IDesign C# Coding Standard 3.1](docs/IDesign%20C%23%20Coding%20Standard%203.1.pdf) - **Primary Standard**
- [IDesign Method™ Version 2.5](docs/IDesign%20Method.pdf)
- [IDesign Design Standard](docs/IDesign%20Design%20Standard.pdf)
- [Red Hat Supplementary Style Guide](https://redhat-documentation.github.io/supplementary-style-guide/)
- [Red Hat Technical Writing Style Guide](https://stylepedia.net/)
- [Write the Docs Guide](https://www.writethedocs.org/guide/writing/docs-principles/)
- [Microsoft Style Guide](https://learn.microsoft.com/en-us/style-guide/welcome/)
- [Awesome Technical Writing Resources](https://github.com/BolajiAyodeji/awesome-technical-writing)

### IDesign C# Coding Standard Compliance

**All code must comply with IDesign C# Coding Standard 3.1**. See [IDesign C# Coding Standard Compliance](./IDESIGN-CSHARP-CODING-STANDARD-COMPLIANCE.md) for detailed compliance requirements.

---

## XML Documentation Comments

### General Rules

1. **All public APIs must have XML documentation comments**
2. **Use complete sentences** with proper grammar and punctuation
3. **Be concise but comprehensive** - explain what, why, and when appropriate
4. **Include examples** for complex APIs
5. **Document parameters, return values, and exceptions**

### Class Documentation

```csharp
/// <summary>
/// Brief one-line description of the class purpose.
/// </summary>
/// <remarks>
/// <para>
/// Detailed explanation of the class, its responsibilities, and design decisions.
/// Include information about:
/// </para>
/// <list type="bullet">
/// <item>Component type (Client, Manager, Engine, Accessor)</item>
/// <item>Volatility encapsulated</item>
/// <item>Design patterns used</item>
/// <item>Thread safety considerations</item>
/// <item>Usage guidelines</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Usage example here
/// </code>
/// </example>
public class MyClass
{
}
```

### Method Documentation

```csharp
/// <summary>
/// Brief one-line description of what the method does.
/// </summary>
/// <param name="parameterName">
/// Description of the parameter, including constraints, valid ranges, or special values.
/// </param>
/// <returns>
/// Description of the return value, including what it represents and possible values.
/// </returns>
/// <exception cref="ExceptionType">
/// When this exception is thrown and why.
/// </exception>
/// <remarks>
/// <para>
/// Additional details about:
/// </para>
/// <list type="number">
/// <item>Implementation details</item>
/// <item>Performance considerations</item>
/// <item>Side effects</item>
/// <item>Usage guidelines</item>
/// </list>
/// </remarks>
public ReturnType MethodName(ParameterType parameterName)
{
}
```

### Property Documentation

```csharp
/// <summary>
/// Brief description of what the property represents.
/// </summary>
/// <value>
/// Detailed description of the property value, including:
/// <list type="bullet">
/// <item>Type and constraints</item>
/// <item>Default values</item>
/// <item>Valid ranges</item>
/// <item>Special values or nullability</item>
/// </list>
/// </value>
/// <remarks>
/// Additional information about the property, such as:
/// <list type="bullet">
/// <item>When it's set</item>
/// <item>Thread safety</item>
/// <item>Performance implications</item>
/// </list>
/// </remarks>
public PropertyType PropertyName { get; set; }
```

---

## Inline Comments

### When to Use Inline Comments

1. **Complex Logic**: Explain non-obvious algorithms or business rules
2. **Workarounds**: Document temporary solutions or known issues
3. **Performance Optimizations**: Explain why a specific approach was chosen
4. **Magic Numbers**: Explain the rationale behind specific values
5. **Non-Intuitive Code**: Clarify code that might be misunderstood

### Inline Comment Style

```csharp
// Good: Explains why, not what
// Pre-allocate collections with capacity estimate to reduce reallocations
// Estimate: average 5 chunks per file (conservative estimate)
var estimatedCapacity = Math.Max(1, totalFiles * 5);
var entries = new List<IndexEntry>(estimatedCapacity);

// Good: Explains complex logic
// Force a full garbage collection to establish an accurate baseline.
// This ensures that memory measurements reflect actual allocations during
// the operation rather than pre-existing allocations.
GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);

// Bad: States the obvious
// Create a new list
var list = new List<string>();
```

### Comment Formatting

- **Start with capital letter** (unless continuing a sentence)
- **End with period** for complete sentences
- **Use proper grammar** and spelling
- **Keep comments up to date** with code changes
- **Remove commented-out code** (use version control instead)

---

## Documentation Tags

### Required Tags

- `<summary>`: Brief description (required for all public APIs)
- `<param>`: Parameter descriptions (required for all parameters)
- `<returns>`: Return value description (required for methods with return values)
- `<exception>`: Exception documentation (required when exceptions are thrown)

### Optional but Recommended Tags

- `<remarks>`: Additional details, implementation notes, usage guidelines
- `<example>`: Code examples demonstrating usage
- `<value>`: Property value descriptions
- `<seealso>`: Related APIs or documentation
- `<typeparam>`: Generic type parameter descriptions

### IDesign Method™ Specific Tags

For components following IDesign Method™, include comprehensive documentation in `<remarks>`:

```csharp
/// <remarks>
/// <para>
/// <strong>Component Type:</strong> Manager (Workflow Volatility)
/// </para>
/// <para>
/// <strong>Volatility Encapsulated:</strong> Indexing workflow and orchestration. The sequence
/// of operations, the order of processing, or the workflow logic could change without affecting
/// the underlying Engines or Accessors.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Orchestrates Engines and Accessors - knows "when" to do things
/// but not "how" to do them. The Manager coordinates the workflow but delegates the actual
/// processing to Engines and storage to Accessors.
/// </para>
/// <para>
/// <strong>Service Boundary:</strong> Called by IndexCommand (Client). Managers are called by
/// Clients and orchestrate the workflow by calling Engines and Accessors.
/// </para>
/// <para>
/// <strong>Orchestrates:</strong>
/// </para>
/// <list type="bullet">
/// <item>DocumentExtractionEngine (Engine) - extracts text from documents</item>
/// <item>EmbeddingEngine (Engine) - generates semantic embeddings</item>
/// <item>TextChunkingEngine (Engine) - chunks text into segments</item>
/// <item>VectorStorageAccessor (Accessor) - stores vectors and index</item>
/// </list>
/// <para>
/// <strong>Authentication:</strong> None (local CLI tool)
/// </para>
/// <para>
/// <strong>Authorization:</strong> None (local CLI tool)
/// </para>
/// <para>
/// <strong>Transaction:</strong> None (file-based operations)
/// </para>
/// <para>
/// <strong>IDesign Method™ Compliance:</strong>
/// </para>
/// <list type="bullet">
/// <item>Encapsulates workflow volatility</item>
/// <item>Orchestrates Engines and Accessors</item>
/// <item>Knows "when" but not "how"</item>
/// <item>Publishes events for cross-manager communication</item>
/// <item>No business logic (delegates to Engines)</item>
/// </list>
/// </remarks>
```

**IDesign C# Coding Standard Requirements**:
- Document component type (Manager, Engine, Accessor, Client)
- Document volatility encapsulated
- Document design pattern used
- Document service boundaries
- Document dependencies and orchestration
- Document authentication/authorization (if applicable)
- Document transaction boundaries (if applicable)
- List IDesign Method™ compliance points

---

## Code Examples in Documentation

### Formatting Code Examples

Following the [Red Hat style guide for long code examples](https://redhat-documentation.github.io/supplementary-style-guide/#long-code-examples):

1. **Use code blocks** with language specification
2. **Include context** - show enough code to understand usage
3. **Add comments** in examples to explain key points
4. **Show error handling** when relevant
5. **Include expected output** when helpful

### Example Structure

```csharp
/// <example>
/// <para>
/// Basic usage example:
/// </para>
/// <code>
/// using var monitor = new MemoryMonitor("Indexing", enabled: true);
/// monitor.DisplayStats("Initial");
/// 
/// // Perform operation
/// _indexManager.BuildIndex(sourcePath, outputPath, chunkSize, chunkOverlap);
/// 
/// // Final stats displayed automatically on disposal
/// </code>
/// <para>
/// Example with progress updates:
/// </para>
/// <code>
/// using var monitor = new MemoryMonitor("Indexing", enabled: true);
/// 
/// _indexManager.BuildIndex(
///     sourcePath,
///     outputPath,
///     chunkSize,
///     chunkOverlap,
///     progress =>
///     {
///         if (progress % 10 == 0)
///         {
///             monitor.DisplaySummary();
///         }
///     }
/// );
/// </code>
/// </example>
```

---

## Language and Style Guidelines

### Writing Style

Following [Microsoft Style Guide](https://learn.microsoft.com/en-us/style-guide/welcome/) and [Write the Docs principles](https://www.writethedocs.org/guide/writing/docs-principles/):

1. **Use active voice**: "The method returns" not "A value is returned by the method"
2. **Be concise**: Remove unnecessary words
3. **Be specific**: Avoid vague terms like "sometimes" or "usually"
4. **Use present tense**: "Returns" not "Will return"
5. **Avoid contractions**: Use "does not" not "doesn't"

### Terminology

1. **Consistent naming**: Use the same terms throughout
2. **Define acronyms**: Define on first use (e.g., "GC (garbage collection)")
3. **Technical accuracy**: Use correct technical terms
4. **User perspective**: Write from the user's perspective

### Formatting

1. **Code references**: Use `<c>code</c>` for inline code
2. **Type references**: Use `<see cref="TypeName"/>` for type references
3. **Method references**: Use `<see cref="MethodName"/>` for method references
4. **Lists**: Use `<list>` tags for structured information
5. **Paragraphs**: Use `<para>` tags for multiple paragraphs

---

## Component-Specific Documentation

### Clients (Commands)

Document:
- Command purpose and usage
- Command-line options and their defaults
- Error conditions and exit codes
- Examples of usage

### Managers

Document:
- Workflow orchestration responsibilities
- Dependencies (Engines, Accessors)
- Event publishing
- Progress reporting
- Error handling strategy

### Engines

Document:
- Algorithm or business logic encapsulated
- Input/output contracts
- Performance characteristics
- Thread safety
- Pure function guarantees (if applicable)

### Accessors

Document:
- Resource abstraction (what is abstracted)
- Storage format or technology
- CRUD operations
- Error handling
- Thread safety

---

## IDesign C# Coding Standard Requirements

### Naming Conventions
- ✅ Interfaces start with "I" prefix
- ✅ Classes use PascalCase
- ✅ Methods use PascalCase
- ✅ Private fields use underscore prefix (`_fieldName`)
- ✅ Parameters use camelCase

### Error Handling
- ✅ All public methods validate arguments
- ✅ Use `ArgumentNullException` for null arguments
- ✅ Use `ArgumentException` for invalid values
- ✅ Include parameter name in exception messages
- ✅ Document all exceptions in XML comments

### Dependency Injection
- ✅ All dependencies injected through constructors
- ✅ No direct instantiation of dependencies
- ✅ Constructor validation using `ArgumentNullException`
- ✅ Dependencies stored in readonly fields

### Code Organization
- ✅ One class per file
- ✅ File name matches class name
- ✅ Namespaces follow component taxonomy
- ✅ Separation of concerns (Managers, Engines, Accessors, Clients)

## Documentation Review Checklist

Before submitting code, ensure:

- [ ] All public APIs have XML documentation comments
- [ ] All parameters are documented
- [ ] Return values are documented
- [ ] Exceptions are documented
- [ ] Complex logic has inline comments
- [ ] Code examples are included where helpful
- [ ] IDesign Method™ components include comprehensive architecture notes
- [ ] IDesign C# Coding Standard compliance verified
- [ ] Argument validation implemented
- [ ] Dependency injection used
- [ ] Terminology is consistent
- [ ] Grammar and spelling are correct
- [ ] Documentation matches implementation

---

## Examples

### Complete Class Documentation

See `src/DocToolkit/ifx/Infrastructure/MemoryMonitor.cs` for a complete example of comprehensive documentation following these standards.

### Complete Command Documentation

See `src/DocToolkit/ifx/Commands/IndexCommand.cs` for a complete example of command documentation.

---

## References

- [Red Hat Supplementary Style Guide](https://redhat-documentation.github.io/supplementary-style-guide/)
- [Red Hat Technical Writing Style Guide](https://stylepedia.net/)
- [Write the Docs Guide](https://www.writethedocs.org/guide/writing/docs-principles/)
- [Microsoft Style Guide](https://learn.microsoft.com/en-us/style-guide/welcome/)
- [Awesome Technical Writing](https://github.com/BolajiAyodeji/awesome-technical-writing)

---

**Last Updated**: 2024  
**Maintained By**: Documentation Toolkit Team
