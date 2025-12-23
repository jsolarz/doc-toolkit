using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DocToolkit.Engines;

namespace DocToolkit.Tests.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class TextChunkingBenchmarks
{
    private readonly TextChunkingEngine _engine = new();
    private string _largeText = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Generate a large text with 10,000 words
        var words = Enumerable.Range(1, 10000)
            .Select(i => $"word{i}")
            .ToArray();
        _largeText = string.Join(" ", words);
    }

    [Benchmark]
    public List<string> ChunkText_SmallChunks()
    {
        return _engine.ChunkText(_largeText, chunkSize: 100, chunkOverlap: 20);
    }

    [Benchmark]
    public List<string> ChunkText_MediumChunks()
    {
        return _engine.ChunkText(_largeText, chunkSize: 500, chunkOverlap: 100);
    }

    [Benchmark]
    public List<string> ChunkText_LargeChunks()
    {
        return _engine.ChunkText(_largeText, chunkSize: 1000, chunkOverlap: 200);
    }

    [Benchmark]
    public List<string> ChunkText_NoOverlap()
    {
        return _engine.ChunkText(_largeText, chunkSize: 500, chunkOverlap: 0);
    }
}
