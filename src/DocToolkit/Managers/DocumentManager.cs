using DocToolkit.ifx.Interfaces.IEngines;
using DocToolkit.ifx.Interfaces.IManagers;
using Microsoft.Extensions.Logging;

namespace DocToolkit.Managers;

/// <summary>
/// Manager for document management operations.
/// Encapsulates workflow volatility: document management workflow could change (different operations, filtering, sorting).
/// </summary>
public class DocumentManager : IDocumentManager
{
    private readonly INavigationGenerator _navigationGenerator;
    private readonly ILinkResolver _linkResolver;
    private readonly ILogger<DocumentManager>? _logger;

    public DocumentManager(
        INavigationGenerator navigationGenerator,
        ILinkResolver linkResolver,
        ILogger<DocumentManager>? logger = null)
    {
        _navigationGenerator = navigationGenerator ?? throw new ArgumentNullException(nameof(navigationGenerator));
        _linkResolver = linkResolver ?? throw new ArgumentNullException(nameof(linkResolver));
        _logger = logger;
    }

    public List<DocumentMetadata> ListDocuments(string documentsPath, string? filter = null)
    {
        if (!Directory.Exists(documentsPath))
        {
            _logger?.LogWarning("Documents directory does not exist: {DocumentsPath}", documentsPath);
            return new List<DocumentMetadata>();
        }

        var index = _navigationGenerator.GenerateIndex(documentsPath);
        var documents = index.Documents.Values.ToList();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            documents = FilterDocuments(documents, filter);
        }

        return documents.OrderBy(d => d.Path).ToList();
    }

    public DocumentInfo GetDocumentInfo(string documentPath, string basePath)
    {
        if (!File.Exists(documentPath))
        {
            throw new FileNotFoundException($"Document not found: {documentPath}");
        }

        var content = File.ReadAllText(documentPath);
        var fileInfo = new FileInfo(documentPath);
        var relativePath = Path.GetRelativePath(basePath, documentPath).Replace('\\', '/');

        var index = _navigationGenerator.GenerateIndex(Path.GetDirectoryName(documentPath) ?? basePath);
        var metadata = index.Documents.TryGetValue(relativePath, out var docMeta) 
            ? docMeta 
            : ExtractBasicMetadata(content, documentPath, relativePath);

        var info = new DocumentInfo
        {
            Metadata = metadata,
            WordCount = CountWords(content),
            LineCount = content.Split('\n').Length,
            CharacterCount = content.Length,
            Sections = ExtractSections(content),
            Links = ExtractLinks(content),
            SectionWordCounts = CountWordsBySection(content),
            CreatedDate = fileInfo.CreationTime,
            LastModified = fileInfo.LastWriteTime
        };

        return info;
    }

    private List<DocumentMetadata> FilterDocuments(List<DocumentMetadata> documents, string filter)
    {
        var filterLower = filter.ToLowerInvariant();
        return documents.Where(d =>
            d.Name.ToLowerInvariant().Contains(filterLower) ||
            d.Title?.ToLowerInvariant().Contains(filterLower) == true ||
            d.Type.ToLowerInvariant().Contains(filterLower) ||
            d.Category.ToLowerInvariant().Contains(filterLower) ||
            d.Path.ToLowerInvariant().Contains(filterLower)
        ).ToList();
    }

    private DocumentMetadata ExtractBasicMetadata(string content, string filePath, string relativePath)
    {
        var metadata = new DocumentMetadata
        {
            Name = Path.GetFileName(filePath),
            Path = relativePath,
            Type = GetDocumentType(Path.GetFileName(filePath)),
            Category = DetermineCategory(relativePath),
            Size = new FileInfo(filePath).Length,
            LastModified = new FileInfo(filePath).LastWriteTime
        };

        var titleMatch = System.Text.RegularExpressions.Regex.Match(content, @"^#\s+(.+)$", System.Text.RegularExpressions.RegexOptions.Multiline);
        if (titleMatch.Success)
        {
            metadata.Title = titleMatch.Groups[1].Value.Trim();
        }
        else
        {
            metadata.Title = Path.GetFileNameWithoutExtension(filePath);
        }

        var lines = content.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith("#"))
            {
                if (i + 1 < lines.Length && !string.IsNullOrWhiteSpace(lines[i + 1]))
                {
                    metadata.Description = lines[i + 1].Trim();
                    break;
                }
            }
        }

        return metadata;
    }

    private int CountWords(string content)
    {
        return System.Text.RegularExpressions.Regex.Matches(content, @"\b\w+\b").Count;
    }

    private List<string> ExtractSections(string content)
    {
        var sections = new List<string>();
        var lines = content.Split('\n');

        foreach (var line in lines)
        {
            var match = System.Text.RegularExpressions.Regex.Match(line, @"^#{1,6}\s+(.+)$");
            if (match.Success)
            {
                sections.Add(match.Groups[1].Value.Trim());
            }
        }

        return sections;
    }

    private List<string> ExtractLinks(string content)
    {
        var links = new List<string>();
        var matches = System.Text.RegularExpressions.Regex.Matches(content, @"\[([^\]]+)\]\(([^\)]+)\)");

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var linkText = match.Groups[1].Value;
            var linkUrl = match.Groups[2].Value;
            links.Add($"{linkText} -> {linkUrl}");
        }

        return links;
    }

    private Dictionary<string, int> CountWordsBySection(string content)
    {
        var sectionCounts = new Dictionary<string, int>();
        var lines = content.Split('\n');
        string? currentSection = null;
        var sectionContent = new List<string>();

        foreach (var line in lines)
        {
            var headerMatch = System.Text.RegularExpressions.Regex.Match(line, @"^#{1,6}\s+(.+)$");
            if (headerMatch.Success)
            {
                if (currentSection != null && sectionContent.Count > 0)
                {
                    var sectionText = string.Join("\n", sectionContent);
                    sectionCounts[currentSection] = CountWords(sectionText);
                }

                currentSection = headerMatch.Groups[1].Value.Trim();
                sectionContent.Clear();
            }
            else if (currentSection != null)
            {
                sectionContent.Add(line);
            }
        }

        if (currentSection != null && sectionContent.Count > 0)
        {
            var sectionText = string.Join("\n", sectionContent);
            sectionCounts[currentSection] = CountWords(sectionText);
        }

        return sectionCounts;
    }

    private string GetDocumentType(string fileName)
    {
        var parts = fileName.Split('-');
        if (parts.Length >= 2)
        {
            return parts[1].ToUpperInvariant();
        }
        return "DOC";
    }

    private string DetermineCategory(string relativePath)
    {
        if (relativePath.Contains("/customer/", StringComparison.OrdinalIgnoreCase))
        {
            return "customer";
        }
        if (relativePath.Contains("/developer/", StringComparison.OrdinalIgnoreCase))
        {
            return "developer";
        }
        if (relativePath.Contains("/shared/", StringComparison.OrdinalIgnoreCase))
        {
            return "shared";
        }
        return "other";
    }
}
