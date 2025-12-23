using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DocToolkit.Engines;

namespace DocToolkit.Tests.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class SimilarityBenchmarks
{
    private readonly SimilarityEngine _engine = new();
    private float[] _vector1 = null!;
    private float[] _vector2 = null!;
    private float[][] _vectors = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Create 384-dimensional vectors (typical embedding size)
        var random = new Random(42);
        _vector1 = Enumerable.Range(0, 384)
            .Select(_ => (float)random.NextDouble())
            .ToArray();
        
        _vector2 = Enumerable.Range(0, 384)
            .Select(_ => (float)random.NextDouble())
            .ToArray();

        // Create array of 1000 vectors
        _vectors = Enumerable.Range(0, 1000)
            .Select(_ => Enumerable.Range(0, 384)
                .Select(__ => (float)random.NextDouble())
                .ToArray())
            .ToArray();
    }

    [Benchmark]
    public double CosineSimilarity_Single()
    {
        return _engine.CosineSimilarity(_vector1, _vector2);
    }

    [Benchmark]
    public List<(int index, double score)> FindTopSimilar_Small()
    {
        return _engine.FindTopSimilar(_vector1, _vectors.Take(100).ToArray(), topK: 10);
    }

    [Benchmark]
    public List<(int index, double score)> FindTopSimilar_Medium()
    {
        return _engine.FindTopSimilar(_vector1, _vectors.Take(500).ToArray(), topK: 10);
    }

    [Benchmark]
    public List<(int index, double score)> FindTopSimilar_Large()
    {
        return _engine.FindTopSimilar(_vector1, _vectors, topK: 10);
    }

    [Benchmark]
    public List<(int index, double score)> FindTopSimilar_TopK100()
    {
        return _engine.FindTopSimilar(_vector1, _vectors, topK: 100);
    }
}
