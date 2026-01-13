using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DocToolkit.Accessors;
using DocToolkit.ifx.Models;

namespace DocToolkit.Tests.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class ProjectAccessorBenchmarks : IDisposable
{
    private readonly ProjectAccessor _accessor = new();
    private readonly string _testDir;

    public ProjectAccessorBenchmarks()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"benchmark_project_{Guid.NewGuid()}");
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Benchmark]
    public void CreateDirectories_CustomerFacing()
    {
        var projectPath = Path.Combine(_testDir, Guid.NewGuid().ToString());
        _accessor.CreateDirectories(projectPath, ProjectType.CustomerFacing);
    }

    [Benchmark]
    public void CreateDirectories_DeveloperFacing()
    {
        var projectPath = Path.Combine(_testDir, Guid.NewGuid().ToString());
        _accessor.CreateDirectories(projectPath, ProjectType.DeveloperFacing);
    }

    [Benchmark]
    public void CreateDirectories_Mixed()
    {
        var projectPath = Path.Combine(_testDir, Guid.NewGuid().ToString());
        _accessor.CreateDirectories(projectPath, ProjectType.Mixed);
    }

    [Benchmark]
    public void CreateCursorConfig()
    {
        var projectPath = Path.Combine(_testDir, Guid.NewGuid().ToString());
        Directory.CreateDirectory(projectPath);
        Directory.CreateDirectory(Path.Combine(projectPath, ".cursor"));
        _accessor.CreateCursorConfig(projectPath);
    }

    [Benchmark]
    public void CreateReadme()
    {
        var projectPath = Path.Combine(_testDir, Guid.NewGuid().ToString());
        Directory.CreateDirectory(projectPath);
        _accessor.CreateReadme(projectPath, ProjectType.Mixed);
    }

    public void Dispose()
    {
        Cleanup();
    }
}
