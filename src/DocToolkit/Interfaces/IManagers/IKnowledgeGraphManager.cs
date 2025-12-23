namespace DocToolkit.Interfaces.Managers;

/// <summary>
/// Manager interface for knowledge graph generation workflow.
/// </summary>
public interface IKnowledgeGraphManager
{
    /// <summary>
    /// Builds a knowledge graph from source files.
    /// </summary>
    /// <param name="sourcePath">Source directory path</param>
    /// <param name="outputPath">Output directory path</param>
    /// <param name="progressCallback">Progress callback (0-100)</param>
    /// <returns>True if successful</returns>
    bool BuildGraph(string sourcePath, string outputPath, Action<double>? progressCallback = null);
}
