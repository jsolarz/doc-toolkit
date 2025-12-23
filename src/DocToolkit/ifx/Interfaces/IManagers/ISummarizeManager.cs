namespace DocToolkit.Interfaces.Managers;

/// <summary>
/// Manager interface for document summarization workflow.
/// </summary>
public interface ISummarizeManager
{
    /// <summary>
    /// Summarizes source files into a context document.
    /// </summary>
    /// <param name="sourcePath">Source directory path</param>
    /// <param name="outputFile">Output file path</param>
    /// <param name="maxChars">Maximum characters per file to include</param>
    /// <param name="progressCallback">Progress callback (0-100)</param>
    /// <returns>True if successful</returns>
    bool SummarizeSource(string sourcePath, string outputFile, int maxChars, Action<double>? progressCallback = null);
}
