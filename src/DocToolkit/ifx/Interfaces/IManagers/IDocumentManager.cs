using DocToolkit.ifx.Interfaces.IEngines;

namespace DocToolkit.ifx.Interfaces.IManagers;

/// <summary>
/// Manager for document management operations.
/// Encapsulates workflow volatility: document management workflow could change (different operations, filtering, sorting).
/// </summary>
public interface IDocumentManager
{
    /// <summary>
    /// Lists all documents in a directory with metadata.
    /// </summary>
    /// <param name="documentsPath">Path to documents directory</param>
    /// <param name="filter">Optional filter (type, category, or search term)</param>
    /// <returns>List of document metadata</returns>
    List<DocumentMetadata> ListDocuments(string documentsPath, string? filter = null);

    /// <summary>
    /// Gets detailed information about a specific document.
    /// </summary>
    /// <param name="documentPath">Path to the document</param>
    /// <param name="basePath">Base path for resolving relative paths</param>
    /// <returns>Document information</returns>
    DocumentInfo GetDocumentInfo(string documentPath, string basePath);
}

/// <summary>
/// Detailed document information.
/// </summary>
public class DocumentInfo
{
    public DocumentMetadata Metadata { get; set; } = new();
    public int WordCount { get; set; }
    public int LineCount { get; set; }
    public int CharacterCount { get; set; }
    public List<string> Sections { get; set; } = new();
    public List<string> Links { get; set; } = new();
    public Dictionary<string, int> SectionWordCounts { get; set; } = new();
    public DateTime? CreatedDate { get; set; }
    public DateTime LastModified { get; set; }
}
