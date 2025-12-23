# .NET 10 Breaking Changes Mitigation Document

## Executive Summary

This document provides a comprehensive analysis of .NET 10 breaking changes and their impact on the DocToolkit project, along with mitigation strategies.

**Reference**: [.NET 10 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)

## Impact Assessment

### High Priority (Immediate Review Required)

None identified for DocToolkit project.

### Medium Priority (Review Recommended)

#### 1. System.Text.Json Property Name Validation

**Breaking Change**: [System.Text.Json checks for property name conflicts](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0/serialization/10/property-name-validation)

**Type**: Behavioral change

**Impact**: Medium - Could cause serialization failures if property names conflict

**Affected Code**:
- `src/DocToolkit/Accessors/VectorStorageAccessor.cs` (lines 101-103, 125-129)
- `src/DocToolkit/Managers/KnowledgeGraphManager.cs` (line 240)
- `src/DocToolkit/Accessors/ProjectAccessor.cs` (line 58)
- `src/DocToolkit/ifx/Infrastructure/EventPersistence.cs` (line 72)

**Mitigation**:
1. Review all anonymous object serialization for property name conflicts
2. Use explicit classes instead of anonymous objects where possible
3. Test JSON serialization/deserialization thoroughly

**Action Items**:
- [x] Review `VectorStorageAccessor.SaveIndex` for property conflicts ✅ **VERIFIED SAFE** - Single property anonymous object `new { entries }` - no conflicts possible
- [x] Review `KnowledgeGraphManager.BuildGraph` for property conflicts ✅ **VERIFIED SAFE** - Uses explicit `GraphData` class with `JsonPropertyName` attributes - all property names are unique
- [x] Review `ProjectAccessor.CreateCursorConfig` for property conflicts ✅ **VERIFIED SAFE** - Anonymous object with unique property names: `version`, `rules`, `documentTypes` (nested properties: `sow`, `prd`, `rfp`, `tender`, `architecture`, `solution` all unique)
- [x] Review `EventPersistence.SaveEvent` for property conflicts ✅ **VERIFIED SAFE** - Serializes event objects that implement `IEvent` interface with explicit property names
- [x] Add unit tests for JSON serialization edge cases ✅ **EXISTS** - `VectorStorageAccessorTests.SaveIndex_And_LoadIndex_RoundTrip()` and `SaveIndex_WithEmptyList_HandlesGracefully()` verify JSON serialization works correctly

**Code Example** (Current - Safe):
```csharp
// VectorStorageAccessor.cs - Single property, safe
var json = JsonSerializer.Serialize(new { entries }, new JsonSerializerOptions
{
    WriteIndented = true
});

// KnowledgeGraphManager.cs - Explicit class with JsonPropertyName attributes, safe
var json = JsonSerializer.Serialize(graph, new JsonSerializerOptions
{
    WriteIndented = true
});

// ProjectAccessor.cs - Anonymous object with unique property names, safe
var cursorConfig = new
{
    rules = new
    {
        prd = new { template = "../../doc-toolkit/templates/prd.md" },
        rfp = new { template = "../../doc-toolkit/templates/rfp.md" },
        // ... all properties are unique
    }
};
```

**Recommendation**: All current JSON serialization code is safe:
- ✅ Single-property anonymous objects (no conflicts possible)
- ✅ Explicit classes with `JsonPropertyName` attributes (explicit naming)
- ✅ Anonymous objects with unique property names (verified)

### Low Priority (Monitor)

#### 2. Console JSON Logging Duplicate Messages

**Breaking Change**: [Message no longer duplicated in Console log output](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0/extensions/10.0/console-json-logging-duplicate-messages)

**Type**: Behavioral change

**Impact**: Low - We don't use JSON-formatted console logging

**Affected Code**: None (we use simple `Console.WriteLine`)

**Mitigation**: No action required. If JSON-formatted logging is added in the future, be aware that duplicate messages will no longer appear.

#### 3. Configuration Null Values Preserved

**Breaking Change**: [Null values preserved in configuration](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0/extensions/10.0/configuration-null-values-preserved)

**Type**: Behavioral change

**Impact**: Low - We don't use configuration with null values

**Affected Code**: None

**Mitigation**: No action required. If configuration is added in the future, be aware that null values are now preserved.

#### 4. System.Linq.AsyncEnumerable Included in Core Libraries

**Breaking Change**: [System.Linq.AsyncEnumerable included in core libraries](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0/core-libraries/10.0/asyncenumerable)

**Type**: Source incompatible

**Impact**: Low - We don't use AsyncEnumerable

**Affected Code**: None

**Mitigation**: No action required. If AsyncEnumerable is used in the future, remove any explicit package reference.

#### 5. GetKeyedService with AnyKey Fix

**Breaking Change**: [Fix issues in GetKeyedService() and GetKeyedServices() with AnyKey](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0/extensions/10.0/getkeyedservice-anykey)

**Type**: Behavioral change

**Impact**: Low - We don't use GetKeyedService with AnyKey

**Affected Code**: None

**Mitigation**: No action required.

## Not Applicable (N/A)

The following breaking changes are not applicable to DocToolkit:

### ASP.NET Core Changes
- ❌ Cookie authentication API endpoints - Not using ASP.NET Core
- ❌ WithOpenApi extension method - Not using OpenAPI
- ❌ Exception handler diagnostics - Not using ASP.NET Core exception handlers
- ❌ IActionContextAccessor - Not using ASP.NET Core
- ❌ IPNetwork and KnownNetworks - Not using forwarded headers
- ❌ Razor runtime compilation - Not using Razor
- ❌ WebHostBuilder - Not using ASP.NET Core hosting

### Entity Framework Core
- ❌ Not using EF Core

### Windows Forms / WPF
- ❌ Not using Windows Forms or WPF

### Cryptography
- ❌ Not using CompositeMLDsa, MLDsa, SlhDsa
- ❌ Not using CoseSigner
- ❌ Not using OpenSSL on macOS
- ❌ Not using X500DistinguishedName validation

### Networking
- ❌ Not using HTTP/3
- ❌ Not using HTTP streaming
- ❌ Not using long URIs

### Serialization
- ❌ Not using XmlSerializer

## Testing Strategy

### Pre-Upgrade Testing

1. **JSON Serialization Tests**
   - Test all JSON serialization paths
   - Verify no property name conflicts
   - Test with edge cases (null values, empty collections)

2. **Integration Tests**
   - Run full CLI workflow tests
   - Verify event bus functionality
   - Test SQLite operations

3. **Performance Tests**
   - Benchmark critical paths
   - Compare .NET 8 vs .NET 10 performance

### Post-Upgrade Testing

1. **Smoke Tests**
   ```bash
   # Test all commands
   dotnet run -- init TestProject
   dotnet run -- index
   dotnet run -- search "test"
   dotnet run -- graph
   dotnet run -- summarize
   ```

2. **Regression Tests**
   - Run full test suite
   - Verify all existing functionality works
   - Check for any new warnings or errors

3. **Performance Validation**
   - Compare benchmark results
   - Verify no performance regressions

## Migration Checklist

### Pre-Migration

- [x] Review all breaking changes ✅ **COMPLETED** - Comprehensive review in this document
- [x] Identify affected code paths ✅ **COMPLETED** - All affected code paths identified and verified safe
- [ ] Create backup of current project ⚠️ **MANUAL STEP** - User should create backup before upgrade
- [x] Document current behavior ✅ **COMPLETED** - Documented in this file and PATCH-DOTNET10-UPGRADE.md
- [ ] Run full test suite on .NET 8 ⚠️ **MANUAL STEP** - Should be run before upgrade (if .NET 8 SDK still available)

### Migration

- [x] Update TargetFramework to net10.0 ✅ **COMPLETED** - Both `DocToolkit.csproj` and `DocToolkit.Tests.csproj` updated
- [ ] Restore packages ⚠️ **PENDING VERIFICATION** - Requires manual `dotnet restore` command
- [ ] Build project ⚠️ **PENDING VERIFICATION** - Requires manual `dotnet build` command
- [ ] Fix any compilation errors ⚠️ **PENDING VERIFICATION** - No errors expected based on code review
- [ ] Review warnings ⚠️ **PENDING VERIFICATION** - Should be reviewed after build

### Post-Migration

- [ ] Run full test suite ⚠️ **PENDING VERIFICATION** - Requires manual `dotnet test` command
- [ ] Run integration tests ⚠️ **PENDING VERIFICATION** - Requires manual test execution
- [ ] Test all CLI commands ⚠️ **PENDING VERIFICATION** - Manual testing required: `init`, `index`, `search`, `graph`, `summarize`
- [x] Verify JSON serialization ✅ **VERIFIED** - Code review confirms all JSON serialization is safe; existing tests cover round-trip serialization
- [ ] Check event bus functionality ⚠️ **PENDING VERIFICATION** - Manual testing required
- [ ] Validate SQLite operations ⚠️ **PENDING VERIFICATION** - Manual testing required
- [x] Update documentation ✅ **COMPLETED** - README.md, docs/design.md, CHANGELOG.md updated
- [ ] Update CI/CD pipelines ⚠️ **MANUAL STEP** - Requires updating pipeline configuration files (if CI/CD exists)

## Risk Assessment

### Low Risk Areas

✅ **Console Logging**: Simple Console.WriteLine usage, no JSON formatting  
✅ **Dependency Injection**: Standard DI patterns, no GetKeyedService with AnyKey  
✅ **Configuration**: Not using configuration with null values  
✅ **AsyncEnumerable**: Not used  

### Medium Risk Areas

✅ **JSON Serialization**: ✅ **REVIEWED AND VERIFIED SAFE** - All JSON serialization code reviewed, no property name conflicts found
⚠️ **Package Compatibility**: ⚠️ **PENDING VERIFICATION** - All packages should be compatible (Microsoft.* packages are at 10.0.1), but requires build verification  

### High Risk Areas

None identified.

## Mitigation Strategies

### Strategy 1: Defensive JSON Serialization

Replace anonymous objects with explicit classes where possible:

**Before**:
```csharp
var json = JsonSerializer.Serialize(new { entries });
```

**After** (if conflicts possible):
```csharp
public class IndexData
{
    public List<IndexEntry> Entries { get; set; } = new();
}

var json = JsonSerializer.Serialize(new IndexData { Entries = entries });
```

### Strategy 2: Comprehensive Testing

Add specific tests for JSON serialization:

```csharp
[Fact]
public void SerializeIndex_WithConflictingPropertyNames_ThrowsException()
{
    // Test for property name conflicts
}
```

### Strategy 3: Gradual Migration

1. Upgrade test project first
2. Fix any issues
3. Upgrade main project
4. Monitor for runtime issues

## Rollback Plan

If critical issues are encountered:

1. **Immediate Rollback**:
   - Revert TargetFramework to `net8.0`
   - Restore packages: `dotnet restore`
   - Rebuild: `dotnet build`

2. **Investigation**:
   - Review error logs
   - Identify specific breaking change
   - Implement targeted fix
   - Re-attempt upgrade

3. **Alternative Approach**:
   - Stay on .NET 8 until issues are resolved
   - Or implement workarounds for specific breaking changes

## References

- [.NET 10 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0)
- [.NET 8 to .NET 10 Upgrade Guide](https://www.hrishidigital.com.au/blog/dotnet-8-to-dotnet-10-upgrade-guide/)
- [System.Text.Json Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0/serialization/10/property-name-validation)
- [Console Logging Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/10.0/extensions/10.0/console-json-logging-duplicate-messages)

## Conclusion

The DocToolkit project has **minimal risk** when upgrading to .NET 10. The primary area requiring attention is JSON serialization, which should be reviewed for property name conflicts. All other breaking changes are either not applicable or have low impact.

**Recommended Action**: Proceed with upgrade, focusing on JSON serialization review and comprehensive testing.
