using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DocToolkit.Engines;

namespace DocToolkit.Tests.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
public class EntityExtractionBenchmarks
{
    private readonly EntityExtractionEngine _engine = new();
    private string _largeText = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Generate text with many entities
        var entities = new[]
        {
            "Project Manager", "Customer Requirements", "System Architect",
            "Business Analyst", "Technical Lead", "Product Owner",
            "Development Team", "Quality Assurance", "User Experience"
        };

        var sentences = Enumerable.Range(0, 1000)
            .Select(i => $"The {entities[i % entities.Length]} discussed the {entities[(i + 1) % entities.Length]} with the {entities[(i + 2) % entities.Length]}.")
            .ToArray();

        _largeText = string.Join(" ", sentences);
    }

    [Benchmark]
    public List<string> ExtractEntities()
    {
        return _engine.ExtractEntities(_largeText);
    }

    [Benchmark]
    public List<string> ExtractTopics_Top10()
    {
        return _engine.ExtractTopics(_largeText, topN: 10);
    }

    [Benchmark]
    public List<string> ExtractTopics_Top50()
    {
        return _engine.ExtractTopics(_largeText, topN: 50);
    }

    [Benchmark]
    public List<string> ExtractTopics_Top100()
    {
        return _engine.ExtractTopics(_largeText, topN: 100);
    }
}
