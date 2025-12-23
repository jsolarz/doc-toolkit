using DocToolkit.ifx.Models;

namespace DocToolkit.ifx.Interfaces.IAccessors;

/// <summary>
/// Accessor interface for vector storage operations.
/// </summary>
public interface IVectorStorageAccessor
{
    /// <summary>
    /// Saves vectors to binary file format.
    /// </summary>
    /// <param name="vectors">Array of embedding vectors to save</param>
    /// <param name="indexPath">Directory path where vectors will be saved</param>
    void SaveVectors(float[][] vectors, string indexPath);

    /// <summary>
    /// Loads vectors from binary file format.
    /// </summary>
    /// <param name="indexPath">Directory path where vectors are stored</param>
    /// <returns>Array of embedding vectors</returns>
    float[][] LoadVectors(string indexPath);

    /// <summary>
    /// Saves index entries to JSON file format.
    /// </summary>
    /// <param name="entries">List of index entries to save</param>
    /// <param name="indexPath">Directory path where index will be saved</param>
    void SaveIndex(List<IndexEntry> entries, string indexPath);

    /// <summary>
    /// Loads index entries from JSON file format.
    /// </summary>
    /// <param name="indexPath">Directory path where index is stored</param>
    /// <returns>List of index entries</returns>
    List<IndexEntry> LoadIndex(string indexPath);

    /// <summary>
    /// Checks if a semantic index exists at the specified path.
    /// </summary>
    /// <param name="indexPath">Directory path to check</param>
    /// <returns>True if both vectors.bin and index.json exist</returns>
    bool IndexExists(string indexPath);
}
