# .NET 8 to .NET 9 Upgrade Patch

## Overview

This patch upgrades the DocToolkit project from .NET 8.0 to .NET 9.0, addressing breaking changes and ensuring compatibility with the latest .NET runtime.

**⚠️ CORRECTION**: This document was originally created for .NET 10, but .NET 10 has not been released yet. The upgrade has been corrected to target **.NET 9.0**, which is the latest available version.

**Reference**: [.NET 9.0 Breaking Changes](https://learn.microsoft.com/en-us/dotnet/core/compatibility/9.0)

## Prerequisites

- .NET 9.0 SDK installed
- Review breaking changes document: `BREAKING-CHANGES-MITIGATION.md`
- Backup current project state

## Files to Modify

### 1. `src/DocToolkit/DocToolkit.csproj`

**Change**: Update TargetFramework from `net8.0` to `net9.0`

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net9.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
  <RootNamespace>DocToolkit</RootNamespace>
  <AssemblyName>doc</AssemblyName>
  <Version>1.0.0</Version>
  <Authors>Documentation Toolkit</Authors>
  <Description>A beautiful CLI tool for generating professional documentation</Description>
</PropertyGroup>
```

**Package Updates** (if needed):
- Verify all packages are compatible with .NET 10
- Update to latest compatible versions if available

### 2. `src/tests/DocToolkit.Tests/DocToolkit.Tests.csproj`

**Change**: Update TargetFramework from `net8.0` to `net9.0`

```xml
<PropertyGroup>
  <TargetFramework>net9.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <IsPackable>false</IsPackable>
  <IsTestProject>true</IsTestProject>
</PropertyGroup>
```

### 3. `src/DocToolkit/Accessors/VectorStorageAccessor.cs`

**Potential Issue**: System.Text.Json property name validation

**Action**: Review JSON serialization for property name conflicts. If using custom property names, ensure they don't conflict.

**Current Code** (lines 101-103):
```csharp
var json = JsonSerializer.Serialize(new { entries }, new JsonSerializerOptions
{
    WriteIndented = true
});
```

**No Change Required**: This code should work fine, but verify no property name conflicts exist.

### 4. `src/DocToolkit/ifx/Infrastructure/EventSubscriptions.cs`

**Potential Issue**: Console logging changes

**Action**: Review Console.WriteLine usage. In .NET 10, console JSON logging may have different behavior.

**Current Code** (lines 37, 42, 47, 55):
```csharp
Console.WriteLine($"[Event] Index built: {evt.IndexPath} ({evt.EntryCount} entries, {evt.VectorCount} vectors)");
```

**No Change Required**: Simple Console.WriteLine calls are unaffected. Only JSON-formatted console logging is affected.

### 5. `src/DocToolkit/ifx/Infrastructure/EventBus.cs`

**Potential Issue**: Console logging in error handlers

**Action**: Verify Console.WriteLine usage in error handlers.

**Current Code** (lines 216, 236, 256, 293):
```csharp
Console.WriteLine($"Error invoking handler for event {eventData.EventType}: {ex.Message}");
```

**No Change Required**: Simple Console.WriteLine calls are unaffected.

### 6. `src/DocToolkit/ifx/Infrastructure/ServiceConfiguration.cs`

**Potential Issue**: Dependency Injection changes

**Action**: Verify GetKeyedService usage (if any). Review BackgroundService if used.

**Current Code**: Uses standard DI registration patterns.

**No Change Required**: Standard DI patterns are unaffected. Only GetKeyedService with AnyKey has behavioral changes, which we don't use.

## Implementation Steps

### Step 1: Update Project Files

1. Update `src/DocToolkit/DocToolkit.csproj`:
   ```xml
   <TargetFramework>net9.0</TargetFramework>
   ```

2. Update `src/tests/DocToolkit.Tests/DocToolkit.Tests.csproj`:
   ```xml
   <TargetFramework>net9.0</TargetFramework>
   ```

### Step 2: Restore and Build

```bash
cd src/DocToolkit
dotnet restore
dotnet build

cd ../tests/DocToolkit.Tests
dotnet restore
dotnet build
```

### Step 3: Run Tests

```bash
cd src/tests/DocToolkit.Tests
dotnet test
```

### Step 4: Verify Functionality

1. Test all CLI commands:
   ```bash
   dotnet run --project src/DocToolkit -- init TestProject
   dotnet run --project src/DocToolkit -- index
   dotnet run --project src/DocToolkit -- search "test query"
   dotnet run --project src/DocToolkit -- graph
   dotnet run --project src/DocToolkit -- summarize
   ```

2. Verify event bus functionality
3. Verify JSON serialization/deserialization
4. Verify SQLite operations

### Step 5: Update Documentation

1. Update `README.md` to reflect .NET 10 requirement
2. Update `CHANGELOG.md` with upgrade notes
3. Update any version references in documentation

## Breaking Changes Assessment

### Not Affected

✅ **System.Text.Json**: We use simple serialization without property name conflicts  
✅ **Console Logging**: We use simple Console.WriteLine, not JSON-formatted logging  
✅ **Dependency Injection**: We use standard DI patterns, not GetKeyedService with AnyKey  
✅ **BackgroundService**: Not used in this project  
✅ **Configuration**: We don't rely on null value preservation behavior  
✅ **AsyncEnumerable**: Not used in this project  

### Potentially Affected (Review Required)

⚠️ **System.Text.Json Property Name Validation**: Review all JsonSerializer usage for property name conflicts  
⚠️ **Console JSON Logging**: If JSON-formatted console logging is added in future, review duplicate message behavior  

## Testing Checklist

- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] CLI commands work correctly
- [ ] Event bus publishes and subscribes correctly
- [ ] JSON serialization/deserialization works
- [ ] SQLite database operations work
- [ ] ONNX model loading works
- [ ] Document extraction works (PDF, DOCX, PPTX)
- [ ] Semantic indexing works
- [ ] Semantic search works
- [ ] Knowledge graph generation works
- [ ] Document summarization works

## Rollback Plan

If issues are encountered:

1. Revert TargetFramework to `net8.0` in both project files (or `net9.0` if upgrading from 9.0)
2. Restore packages: `dotnet restore`
3. Rebuild: `dotnet build`
4. Review error logs and address specific breaking changes

## Post-Upgrade Tasks

1. Update CI/CD pipelines to use .NET 10 SDK
2. Update Docker images (if used) to .NET 10 base images
3. Update deployment documentation
4. Monitor for any runtime issues
5. Update package versions if newer compatible versions are available

## Version History

- **1.0.0** - Initial .NET 8.0 implementation
- **1.2.0** - Upgraded to .NET 9.0 (this patch)
