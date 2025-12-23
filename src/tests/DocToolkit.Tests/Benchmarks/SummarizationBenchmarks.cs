using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using DocToolkit.Engines;

namespace DocToolkit.Tests.Benchmarks;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
public class SummarizationBenchmarks
{
    private readonly SummarizationEngine _engine = new();
    private string _shortText = null!;
    private string _mediumText = null!;
    private string _longText = null!;

    [GlobalSetup]
    public void Setup()
    {
        _shortText = "This is a short text with only a few sentences. It should not be summarized.";
        
        var mediumSentences = Enumerable.Range(1, 50)
            .Select(i => $"This is sentence number {i} in a medium-length text.")
            .ToArray();
        _mediumText = string.Join(" ", mediumSentences);

        var longSentences = Enumerable.Range(1, 200)
            .Select(i => $"This is sentence number {i} in a very long text that contains many sentences and should definitely be summarized.")
            .ToArray();
        _longText = string.Join(" ", longSentences);
    }

    [Benchmark]
    public string SummarizeText_Short()
    {
        return _engine.SummarizeText(_shortText);
    }

    [Benchmark]
    public string SummarizeText_Medium()
    {
        return _engine.SummarizeText(_mediumText, maxSentences: 5);
    }

    [Benchmark]
    public string SummarizeText_Long()
    {
        return _engine.SummarizeText(_longText, maxSentences: 5);
    }

    [Benchmark]
    public string SummarizeText_Long_Max10()
    {
        return _engine.SummarizeText(_longText, maxSentences: 10);
    }

    [Benchmark]
    public string SummarizeText_Long_Max20()
    {
        return _engine.SummarizeText(_longText, maxSentences: 20);
    }
}
