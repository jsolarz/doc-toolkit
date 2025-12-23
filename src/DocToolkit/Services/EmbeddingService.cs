using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Text;
using System.Text.Json;

namespace DocToolkit.Services;

/// <summary>
/// Engine for semantic embedding generation using ONNX Runtime.
/// Encapsulates algorithm volatility: embedding model could change (different ONNX models, tokenization).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Embedding algorithm and model
/// Pattern: Pure function - accepts text, returns embedding vector (no I/O except model loading)
/// Service Boundary: Called by Managers (SemanticIndexService, SemanticSearchService)
/// </remarks>
public class EmbeddingService : IDisposable
{
    private InferenceSession? _session;
    private readonly string _modelPath;
    private readonly object _lockObject = new();
    private const int EmbeddingDimension = 384; // all-MiniLM-L6-v2 dimension

    /// <summary>
    /// Initializes a new instance of the EmbeddingService.
    /// </summary>
    /// <param name="modelPath">Optional path to the ONNX model file. If not provided, searches default locations.</param>
    public EmbeddingService(string? modelPath = null)
    {
        _modelPath = modelPath ?? FindModelPath();
    }

    private string FindModelPath()
    {
        // Look for model in various locations
        var currentDir = Directory.GetCurrentDirectory();
        var possiblePaths = new[]
        {
            Path.Combine(currentDir, "models", "all-MiniLM-L6-v2.onnx"),
            Path.Combine(currentDir, "..", "models", "all-MiniLM-L6-v2.onnx"),
            Path.Combine(currentDir, "..", "..", "models", "all-MiniLM-L6-v2.onnx"),
            Path.Combine(AppContext.BaseDirectory, "models", "all-MiniLM-L6-v2.onnx"),
            Path.Combine(AppContext.BaseDirectory, "..", "models", "all-MiniLM-L6-v2.onnx")
        };

        return possiblePaths.FirstOrDefault(File.Exists) ?? "models/all-MiniLM-L6-v2.onnx";
    }

    private InferenceSession GetSession()
    {
        if (_session == null)
        {
            lock (_lockObject)
            {
                if (_session == null)
                {
                    if (!File.Exists(_modelPath))
                    {
                        throw new FileNotFoundException(
                            $"ONNX model not found at: {_modelPath}\n" +
                            "Please download the all-MiniLM-L6-v2 ONNX model and place it in the models/ directory.\n" +
                            "You can convert it from PyTorch or download a pre-converted version from Hugging Face."
                        );
                    }

                    var options = new SessionOptions
                    {
                        GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
                    };

                    _session = new InferenceSession(_modelPath, options);
                }
            }
        }

        return _session;
    }

    /// <summary>
    /// Generates a semantic embedding vector for the given text.
    /// </summary>
    /// <param name="text">Text to generate embedding for</param>
    /// <returns>Embedding vector (384 dimensions for all-MiniLM-L6-v2)</returns>
    /// <remarks>
    /// Pure function: Accepts text, returns embedding. No I/O during execution.
    /// Model loading is lazy and cached.
    /// </remarks>
    public float[] GenerateEmbedding(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new float[EmbeddingDimension];
        }

        var session = GetSession();

        // Simple tokenization - for production, use proper tokenizer
        // Note: This is a simplified version. For accuracy, you'd need to:
        // 1. Load the tokenizer.json from the model
        // 2. Use proper tokenization (WordPiece/BPE)
        // 3. Handle special tokens
        
        // For now, use a simple word-based approach
        // In production, integrate with tokenizer library or use pre-tokenized input
        var tokens = SimpleTokenize(text);
        
        // Truncate/pad to model's expected input length (typically 512)
        var maxLength = 512;
        if (tokens.Length > maxLength)
        {
            tokens = tokens.Take(maxLength).ToArray();
        }
        else if (tokens.Length < maxLength)
        {
            var padded = new long[maxLength];
            Array.Copy(tokens, padded, tokens.Length);
            // Pad with zeros (or use proper padding token ID)
            tokens = padded;
        }

        // Create input tensor
        var inputTensor = new DenseTensor<long>(tokens, new[] { 1, tokens.Length });
        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputTensor)
        };

        // Run inference
        using var results = session.Run(inputs);
        var output = results.First().AsTensor<float>();

        // Extract embedding (typically mean pooling of token embeddings)
        // For all-MiniLM-L6-v2, the output is usually already pooled
        var embedding = new float[EmbeddingDimension];
        var outputArray = output.ToArray();

        // Handle different output shapes
        if (outputArray.Length == EmbeddingDimension)
        {
            // Already pooled
            Array.Copy(outputArray, embedding, EmbeddingDimension);
        }
        else if (outputArray.Length > EmbeddingDimension)
        {
            // Need to pool (mean pooling)
            var tokenCount = outputArray.Length / EmbeddingDimension;
            for (int i = 0; i < EmbeddingDimension; i++)
            {
                float sum = 0;
                for (int j = 0; j < tokenCount; j++)
                {
                    sum += outputArray[j * EmbeddingDimension + i];
                }
                embedding[i] = sum / tokenCount;
            }
        }

        // Normalize embedding
        Normalize(embedding);

        return embedding;
    }

    private long[] SimpleTokenize(string text)
    {
        // Simplified tokenization - maps words to token IDs
        // In production, use proper tokenizer
        var words = text.ToLower()
            .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}' },
                StringSplitOptions.RemoveEmptyEntries);

        // Simple hash-based tokenization (not ideal, but works for basic cases)
        // For production: Use proper tokenizer.json or tokenizer library
        return words
            .Select(w => (long)Math.Abs(w.GetHashCode() % 30522)) // Approximate vocab size
            .ToArray();
    }

    private void Normalize(float[] vector)
    {
        var norm = Math.Sqrt(vector.Sum(x => x * x));
        if (norm > 0)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = (float)(vector[i] / norm);
            }
        }
    }

    public float[][] GenerateEmbeddingsBatch(string[] texts, Action<double>? progressCallback = null)
    {
        var embeddings = new float[texts.Length][];
        var total = texts.Length;

        for (int i = 0; i < texts.Length; i++)
        {
            embeddings[i] = GenerateEmbedding(texts[i]);
            progressCallback?.Invoke((double)(i + 1) / total * 100);
        }

        return embeddings;
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
