using System.Text.Json;
using DocToolkit.ifx.Interfaces.IAccessors;
using DocToolkit.ifx.Models;
using Microsoft.Extensions.Logging;

namespace DocToolkit.Accessors;

/// <summary>
/// Accessor for vector storage operations.
/// Encapsulates storage volatility: storage format could change (file-based → database, binary → JSON).
/// </summary>
/// <remarks>
/// <para>
/// <strong>Component Type:</strong> Accessor (Storage Volatility)
/// </para>
/// <para>
/// <strong>Volatility Encapsulated:</strong> Storage technology and format. The storage mechanism
/// could change from file-based to database, the format could change from binary to JSON, or
/// the location could change from local filesystem to cloud storage. All these changes are
/// encapsulated within this accessor.
/// </para>
/// <para>
/// <strong>Design Pattern:</strong> Dumb CRUD operations - no business logic. The accessor only
/// knows "where" data is stored and "how" to access it. It does not contain any business rules
/// or processing logic. This follows the IDesign principle that Accessors are "dumb" - they
/// simply perform Create, Read, Update, Delete operations.
/// </para>
/// <para>
/// <strong>Service Boundary:</strong> Called by Managers (SemanticIndexManager, SemanticSearchManager).
/// Accessors are called by Managers, not by Engines. Engines receive data as parameters from Managers.
/// </para>
/// <para>
/// <strong>IDesign Method™ Compliance:</strong>
/// </para>
/// <list type="bullet">
/// <item>Encapsulates storage volatility (file format, location, technology)</item>
/// <item>Dumb CRUD pattern (no business logic)</item>
/// <item>Knows "where" data is stored</item>
/// <item>No business rules or processing logic</item>
/// <item>Called by Managers, not Engines</item>
/// </list>
/// <para>
/// <strong>Storage Format:</strong>
/// </para>
/// <list type="bullet">
/// <item>Vectors: Binary format (vectors.bin) - efficient for large arrays</item>
/// <item>Index: JSON format (index.json) - human-readable metadata</item>
/// </list>
/// </remarks>
public class VectorStorageAccessor : IVectorStorageAccessor
{
    /// <summary>
    /// Logger instance for logging storage operations and errors.
    /// </summary>
    private readonly ILogger<VectorStorageAccessor>? _logger;

    /// <summary>
    /// Initializes a new instance of the VectorStorageAccessor class.
    /// </summary>
    /// <param name="logger">
    /// Optional logger instance for logging storage operations, errors, and debug information.
    /// If null, no logging is performed.
    /// </param>
    /// <remarks>
    /// <para>
    /// IDesign C# Coding Standard: Constructor injection for dependencies.
    /// The logger is optional to allow the accessor to be used in scenarios where logging
    /// is not available or not needed.
    /// </para>
    /// </remarks>
    public VectorStorageAccessor(ILogger<VectorStorageAccessor>? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Saves vectors to binary file format.
    /// </summary>
    /// <param name="vectors">
    /// Array of embedding vectors to save. Must not be null. Each vector must have the same dimension.
    /// Empty arrays are allowed and will create an empty vectors file.
    /// </param>
    /// <param name="indexPath">
    /// Directory path where vectors will be saved. The directory will be created if it does not exist.
    /// The vectors will be saved as "vectors.bin" in this directory.
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>File Format:</strong>
    /// </para>
    /// <list type="number">
    /// <item>Int32: Count of vectors</item>
    /// <item>Int32: Dimension of each vector (0 if no vectors)</item>
    /// <item>Float[]: All vector values sequentially (vector0[0..n], vector1[0..n], ...)</item>
    /// </list>
    /// <para>
    /// <strong>IDesign Pattern:</strong>
    /// This is a "dumb" CRUD operation - it simply writes data to storage without any business logic.
    /// The accessor does not validate vector dimensions or perform any processing - it only stores data.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="vectors"/> or <paramref name="indexPath"/> is null.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when the directory cannot be created (permissions issue).
    /// </exception>
    /// <exception cref="IOException">
    /// Thrown when file I/O operations fail (disk full, file locked, etc.).
    /// </exception>
    public void SaveVectors(float[][] vectors, string indexPath)
    {
        // IDesign C# Coding Standard: Validate arguments
        if (vectors == null)
        {
            throw new ArgumentNullException(nameof(vectors));
        }

        if (string.IsNullOrWhiteSpace(indexPath))
        {
            throw new ArgumentNullException(nameof(indexPath));
        }

        try
        {
            // IDesign: Accessor knows "where" - creates directory if needed
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
            
            _logger?.LogDebug("Saved {VectorCount} vectors to {VectorsPath}", vectors.Length, vectorsPath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save vectors to {IndexPath}", indexPath);
            throw;
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
            _logger?.LogError("Vectors file not found: {VectorsPath}", vectorsPath);
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
            _logger?.LogError("Index file not found: {IndexPathFile}", indexPathFile);
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
