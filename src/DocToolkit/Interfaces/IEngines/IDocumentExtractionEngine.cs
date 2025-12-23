namespace DocToolkit.Interfaces.Engines;

/// <summary>
/// Engine interface for text extraction from various document formats.
/// </summary>
public interface IDocumentExtractionEngine
{
    /// <summary>
    /// Extracts text from a document file.
    /// </summary>
    /// <param name="filePath">Path to the document file</param>
    /// <returns>Extracted text, or empty string if extraction fails</returns>
    string ExtractText(string filePath);
}
