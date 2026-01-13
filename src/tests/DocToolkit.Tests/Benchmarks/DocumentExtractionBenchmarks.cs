using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DocToolkit.Engines;

namespace DocToolkit.Tests.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class DocumentExtractionBenchmarks : IDisposable
{
    private readonly DocumentExtractionEngine _engine = new();
    private string _textFilePath = null!;
    private string _largeTextFilePath = null!;
    private readonly string _testDir;

    public DocumentExtractionBenchmarks()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"benchmark_extract_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    [GlobalSetup]
    public void Setup()
    {
        _textFilePath = Path.Combine(_testDir, "test.txt");
        File.WriteAllText(_textFilePath, "This is a test document with some content.");

        _largeTextFilePath = Path.Combine(_testDir, "large.txt");
        var largeContent = string.Join("\n", Enumerable.Range(1, 10000).Select(i => $"Line {i}: This is test content for benchmarking."));
        File.WriteAllText(_largeTextFilePath, largeContent);
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
    public string ExtractText_SmallFile()
    {
        return _engine.ExtractText(_textFilePath);
    }

    [Benchmark]
    public string ExtractText_LargeFile()
    {
        return _engine.ExtractText(_largeTextFilePath);
    }

    [Benchmark]
    public string ExtractText_NonExistentFile()
    {
        return _engine.ExtractText(Path.Combine(_testDir, "nonexistent.txt"));
    }

    public void Dispose()
    {
        Cleanup();
    }
}
