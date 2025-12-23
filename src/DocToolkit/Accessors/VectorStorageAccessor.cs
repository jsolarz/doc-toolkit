using System.Text.Json;
using DocToolkit.Models;
using DocToolkit.Interfaces.Accessors;

namespace DocToolkit.Accessors;

/// <summary>
/// Accessor for vector storage operations.
/// Encapsulates storage volatility: storage format could change (file-based → database, binary → JSON).
/// </summary>
/// <remarks>
/// Component Type: Accessor (Storage Volatility)
/// Volatility: Storage technology and format
/// Pattern: Dumb CRUD operations - no business logic
/// Service Boundary: Called by Managers (SemanticIndexManager, SemanticSearchManager)
/// </remarks>
public class VectorStorageAccessor : IVectorStorageAccessor
{
    /// <summary>
    /// Saves vectors to binary file format.
    /// </summary>
    /// <param name="vectors">Array of embedding vectors to save</param>
    /// <param name="indexPath">Directory path where vectors will be saved</param>
    public void SaveVectors(float[][] vectors, string indexPath)
    {
        Directory.CreateDirectory(indexPath);

        var vectorsPath = Path.Combine(indexPath, "vectors.bin");
        
        using var fileStream = new FileStream(vectorsPath, FileMode.Create);
        using var writer = new BinaryWriter(fileStream);

        // Write count
        writer.Write(vectors.Length);
        
        // Write dimension
        if (vectors.Length > 0)
        {
            writer.Write(vectors[0].Length);
        }
        else
        {
            writer.Write(0);
        }

        // Write all vectors
        foreach (var vector in vectors)
        {
            foreach (var value in vector)
            {
                writer.Write(value);
            }
        }
    }

    /// <summary>
    /// Loads vectors from binary file format.
    /// </summary>
    /// <param name="indexPath">Directory path where vectors are stored</param>
    /// <returns>Array of embedding vectors</returns>
    /// <exception cref="FileNotFoundException">Thrown when vectors file not found</exception>
    public float[][] LoadVectors(string indexPath)
    {
        var vectorsPath = Path.Combine(indexPath, "vectors.bin");
        
        if (!File.Exists(vectorsPath))
        {
            throw new FileNotFoundException($"Vectors file not found: {vectorsPath}");
        }

        using var fileStream = new FileStream(vectorsPath, FileMode.Open);
        using var reader = new BinaryReader(fileStream);

        var count = reader.ReadInt32();
        var dimension = reader.ReadInt32();

        var vectors = new float[count][];

        for (int i = 0; i < count; i++)
        {
            vectors[i] = new float[dimension];
            for (int j = 0; j < dimension; j++)
            {
                vectors[i][j] = reader.ReadSingle();
            }
        }

        return vectors;
    }

    /// <summary>
    /// Saves index entries to JSON file format.
    /// </summary>
    /// <param name="entries">List of index entries to save</param>
    /// <param name="indexPath">Directory path where index will be saved</param>
    public void SaveIndex(List<IndexEntry> entries, string indexPath)
    {
        Directory.CreateDirectory(indexPath);

        var indexPathFile = Path.Combine(indexPath, "index.json");
        var json = JsonSerializer.Serialize(new { entries }, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(indexPathFile, json);
    }

    /// <summary>
    /// Loads index entries from JSON file format.
    /// </summary>
    /// <param name="indexPath">Directory path where index is stored</param>
    /// <returns>List of index entries</returns>
    /// <exception cref="FileNotFoundException">Thrown when index file not found</exception>
    public List<IndexEntry> LoadIndex(string indexPath)
    {
        var indexPathFile = Path.Combine(indexPath, "index.json");
        
        if (!File.Exists(indexPathFile))
        {
            throw new FileNotFoundException($"Index file not found: {indexPathFile}");
        }

        var json = File.ReadAllText(indexPathFile);
        var result = JsonSerializer.Deserialize<JsonElement>(json);

        if (result.TryGetProperty("entries", out var entriesElement))
        {
            return JsonSerializer.Deserialize<List<IndexEntry>>(entriesElement.GetRawText()) ?? new();
        }

        return new();
    }

    /// <summary>
    /// Checks if a semantic index exists at the specified path.
    /// </summary>
    /// <param name="indexPath">Directory path to check</param>
    /// <returns>True if both vectors.bin and index.json exist</returns>
    public bool IndexExists(string indexPath)
    {
        var vectorsPath = Path.Combine(indexPath, "vectors.bin");
        var indexPathFile = Path.Combine(indexPath, "index.json");
        return File.Exists(vectorsPath) && File.Exists(indexPathFile);
    }
}
