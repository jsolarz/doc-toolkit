using DocToolkit.Engines;
using DocToolkit.ifx.Events;
using DocToolkit.ifx.Infrastructure;
using DocToolkit.Managers;
using FluentAssertions;
using Xunit;

namespace DocToolkit.Tests.Integration;

public class SemanticIndexingIntegrationTests : IDisposable
{
    private readonly string _testSourceDir;
    private readonly string _testOutputDir;
    // Note: Full integration test would require ONNX model
    // private readonly SemanticIndexManager _manager;
    private readonly TextChunkingEngine _chunkingEngine;
    private readonly SimilarityEngine _similarityEngine;

    public SemanticIndexingIntegrationTests()
    {
        _testSourceDir = Path.Combine(Path.GetTempPath(), $"test_source_{Guid.NewGuid()}");
        _testOutputDir = Path.Combine(Path.GetTempPath(), $"test_output_{Guid.NewGuid()}");
        
        Directory.CreateDirectory(_testSourceDir);
        Directory.CreateDirectory(_testOutputDir);

        // Note: This integration test uses real engines but mocks EmbeddingEngine
        // since it requires ONNX model. For full integration, you'd need the model.
        _chunkingEngine = new TextChunkingEngine();
        _similarityEngine = new SimilarityEngine();
        
        // We'll create a simplified version that doesn't require ONNX for basic testing
        // In a real scenario, you'd use the actual EmbeddingEngine with a test model
    }

    [Fact(Skip = "Requires ONNX model - use for full integration testing")]
    public void BuildIndex_EndToEnd_WithRealEngines()
    {
        // This test would require:
        // 1. ONNX model file
        // 2. Real EmbeddingEngine
        // 3. Real DocumentExtractionEngine
        // 4. Full integration test
    }

    [Fact]
    public void TextChunking_And_Similarity_Integration()
    {
        // Arrange
        var text = "This is a test document with multiple sentences. " +
                   "Each sentence contains different words. " +
                   "We want to test chunking and similarity together.";

        // Act
        var chunks = _chunkingEngine.ChunkText(text, chunkSize: 10, chunkOverlap: 2);
        var queryVector = new float[] { 0.1f, 0.2f, 0.3f };
        var chunkVectors = chunks.Select(_ => new float[] { 0.1f, 0.2f, 0.3f }).ToArray();
        var results = _similarityEngine.FindTopSimilar(queryVector, chunkVectors, topK: 3);

        // Assert
        chunks.Should().NotBeEmpty();
        results.Should().HaveCountLessThanOrEqualTo(3);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testSourceDir))
            Directory.Delete(_testSourceDir, true);
        if (Directory.Exists(_testOutputDir))
            Directory.Delete(_testOutputDir, true);
    }
}
