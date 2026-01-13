namespace DocToolkit.ifx.Interfaces.IEngines;

/// <summary>
/// Engine for resolving and validating links in markdown documents.
/// Encapsulates algorithm volatility: link resolution logic could change (different link formats, validation rules).
/// </summary>
public interface ILinkResolver
{
    /// <summary>
    /// Resolves all links in a markdown document.
    /// </summary>
    /// <param name="markdown">Markdown content</param>
    /// <param name="documentPath">Path to the document (for relative link resolution)</param>
    /// <param name="basePath">Base path for resolving relative links</param>
    /// <returns>List of resolved links</returns>
    List<ResolvedLink> ResolveLinks(string markdown, string documentPath, string basePath);

    /// <summary>
    /// Validates that all links in a document point to existing targets.
    /// </summary>
    /// <param name="markdown">Markdown content</param>
    /// <param name="documentPath">Path to the document</param>
    /// <param name="basePath">Base path for resolving relative links</param>
    /// <returns>List of broken links</returns>
    List<BrokenLink> ValidateLinks(string markdown, string documentPath, string basePath);

    /// <summary>
    /// Converts markdown links to HTML anchor links.
    /// </summary>
    /// <param name="markdown">Markdown content</param>
    /// <param name="documentPath">Path to the document</param>
    /// <param name="basePath">Base path for resolving relative links</param>
    /// <returns>Markdown with converted links</returns>
    string ConvertLinksToHtml(string markdown, string documentPath, string basePath);
}

/// <summary>
/// Represents a resolved link.
/// </summary>
public class ResolvedLink
{
    public string OriginalText { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public string ResolvedUrl { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public bool IsValid { get; set; }
    public int LineNumber { get; set; }
}

/// <summary>
/// Represents a broken link.
/// </summary>
public class BrokenLink
{
    public string LinkText { get; set; } = string.Empty;
    public string LinkUrl { get; set; } = string.Empty;
    public string DocumentPath { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
