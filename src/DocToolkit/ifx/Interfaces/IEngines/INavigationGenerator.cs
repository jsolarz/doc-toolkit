namespace DocToolkit.ifx.Interfaces.IEngines;

/// <summary>
/// Engine for generating navigation structures from document collections.
/// Encapsulates algorithm volatility: navigation generation logic could change (different structures, formats).
/// </summary>
public interface INavigationGenerator
{
    /// <summary>
    /// Generates a navigation structure from a collection of documents.
    /// </summary>
    /// <param name="documentsPath">Path to documents directory</param>
    /// <returns>Navigation structure</returns>
    NavigationStructure GenerateNavigation(string documentsPath);

    /// <summary>
    /// Generates a documentation index from a collection of documents.
    /// </summary>
    /// <param name="documentsPath">Path to documents directory</param>
    /// <returns>Documentation index</returns>
    DocumentationIndex GenerateIndex(string documentsPath);
}

/// <summary>
/// Navigation structure for documents.
/// </summary>
public class NavigationStructure
{
    public List<NavigationNode> RootNodes { get; set; } = new();
    public int TotalDocuments { get; set; }
    public Dictionary<string, int> DocumentCountsByType { get; set; } = new();
}

/// <summary>
/// Navigation node in the structure.
/// </summary>
public class NavigationNode
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public bool IsFile { get; set; }
    public string Type { get; set; } = string.Empty;
    public List<NavigationNode> Children { get; set; } = new();
}

/// <summary>
/// Documentation index with metadata.
/// </summary>
public class DocumentationIndex
{
    public Dictionary<string, DocumentMetadata> Documents { get; set; } = new();
    public int TotalDocuments { get; set; }
    public Dictionary<string, int> CountsByCategory { get; set; } = new();
}

/// <summary>
/// Document metadata.
/// </summary>
public class DocumentMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime LastModified { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}
