# Developer Quick Reference

**Version**: 1.0  
**Last Updated**: 2024

---

## Quick Links

- [Technical Documentation](./TECHNICAL-DOCUMENTATION.md) - Complete technical reference
- [Code Documentation Standards](./CODE-DOCUMENTATION-STANDARDS.md) - Documentation guidelines
- [Architecture Design](./design.md) - System architecture
- [Memory Optimization](./MEMORY-OPTIMIZATION-RECOMMENDATIONS.md) - Memory optimization guide

---

## Code Structure

```
src/DocToolkit/
├── Accessors/          # Storage volatility (knows "where")
├── Engines/            # Algorithm volatility (knows "how")
├── Managers/           # Workflow volatility (knows "when")
└── ifx/                # Infrastructure
    ├── Commands/       # UI volatility (Clients)
    ├── Events/         # Event definitions
    ├── Infrastructure/ # DI, Event Bus, Memory Monitor
    ├── Interfaces/     # All interfaces
    └── Models/         # Data models
```

---

## Adding Documentation

### 1. XML Comments for Public APIs

```csharp
/// <summary>
/// Brief description of what the method/class does.
/// </summary>
/// <param name="paramName">Parameter description</param>
/// <returns>Return value description</returns>
/// <remarks>
/// Additional details, implementation notes, examples.
/// </remarks>
```

### 2. Inline Comments for Complex Logic

```csharp
// Pre-allocate collections with capacity estimate to reduce reallocations
// Estimate: average 5 chunks per file (conservative estimate)
var estimatedCapacity = Math.Max(1, totalFiles * 5);
var entries = new List<IndexEntry>(estimatedCapacity);
```

### 3. IDesign Method™ Notes

```csharp
/// <remarks>
/// Component Type: Manager (Workflow Volatility)
/// Volatility: Indexing workflow and orchestration
/// Pattern: Orchestrates Engines and Accessors - knows "when", not "how"
/// </remarks>
```

---

## Common Patterns

### Memory Monitoring

```csharp
using var monitor = new MemoryMonitor("OperationName", enabled: true);
monitor.DisplayStats("Initial");
// ... perform operation ...
// Final stats displayed automatically on disposal
```

### Event Publishing

```csharp
_eventBus.Publish(new IndexBuiltEvent
{
    IndexPath = outputPath,
    EntryCount = entries.Count,
    VectorCount = vectors.Count
});
```

### Progress Reporting

```csharp
progressCallback?.Invoke((double)processedFiles / totalFiles * 100);
```

---

## Style Guide Quick Reference

### Writing Style

- ✅ Use active voice: "The method returns"
- ✅ Be concise and specific
- ✅ Use present tense: "Returns" not "Will return"
- ✅ Avoid contractions: "does not" not "doesn't"

### Code Examples

- Include context
- Show error handling
- Add comments in examples
- Use proper formatting

---

## Documentation Checklist

Before committing:

- [ ] All public APIs have XML comments
- [ ] Parameters documented
- [ ] Return values documented
- [ ] Exceptions documented
- [ ] Complex logic has inline comments
- [ ] Examples included where helpful
- [ ] IDesign Method™ notes for components
- [ ] Grammar and spelling checked

---

## Resources

- [Technical Documentation](./TECHNICAL-DOCUMENTATION.md)
- [Code Documentation Standards](./CODE-DOCUMENTATION-STANDARDS.md)
- [Red Hat Style Guide](https://redhat-documentation.github.io/supplementary-style-guide/)
- [Microsoft Style Guide](https://learn.microsoft.com/en-us/style-guide/welcome/)

---

**Last Updated**: 2024
