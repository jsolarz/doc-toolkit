namespace DocToolkit.Interfaces.Managers;

/// <summary>
/// Manager interface for semantic indexing workflow.
/// </summary>
public interface ISemanticIndexManager : IDisposable
{
    /// <summary>
    /// Builds a semantic index from source files.
    /// </summary>
    /// <param name="sourcePath">Source directory path</param>
    /// <param name="outputPath">Output directory path</param>
    /// <param name="chunkSize">Text chunk size in words</param>
    /// <param name="chunkOverlap">Chunk overlap in words</param>
    /// <param name="progressCallback">Progress callback (0-100)</param>
    /// <returns>True if successful</returns>
    bool BuildIndex(
        string sourcePath,
        string outputPath,
        int chunkSize,
        int chunkOverlap,
        Action<double>? progressCallback = null);
}
