using DocToolkit.ifx.Interfaces.IEngines;
using Microsoft.Extensions.Logging;

namespace DocToolkit.Engines;

/// <summary>
/// Engine for generating navigation structures from document collections.
/// Encapsulates algorithm volatility: navigation generation logic could change (different structures, formats).
/// </summary>
public class NavigationGenerator : INavigationGenerator
{
    private readonly ILogger<NavigationGenerator>? _logger;

    public NavigationGenerator(ILogger<NavigationGenerator>? logger = null)
    {
        _logger = logger;
    }

    public NavigationStructure GenerateNavigation(string documentsPath)
    {
        if (!Directory.Exists(documentsPath))
        {
            return new NavigationStructure();
        }

        var files = Directory.GetFiles(documentsPath, "*.md", SearchOption.AllDirectories)
            .OrderBy(f => f)
            .ToList();

        var rootNodes = new List<NavigationNode>();
        var documentCounts = new Dictionary<string, int>();

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(documentsPath, file).Replace('\\', '/');
            var parts = relativePath.Split('/');
            var docType = GetDocumentType(Path.GetFileName(file));

            if (!documentCounts.ContainsKey(docType))
            {
                documentCounts[docType] = 0;
            }
            documentCounts[docType]++;

            var currentNode = rootNodes;
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var isFile = i == parts.Length - 1;

                if (isFile)
                {
                    var fileNode = new NavigationNode
                    {
                        Name = Path.GetFileNameWithoutExtension(part),
                        Path = relativePath,
                        IsFile = true,
                        Type = docType
                    };
                    currentNode.Add(fileNode);
                }
                else
                {
                    var folderNode = currentNode.FirstOrDefault(n => n.Name == part && !n.IsFile);
                    if (folderNode == null)
                    {
                        folderNode = new NavigationNode
                        {
                            Name = part,
                            Path = string.Join("/", parts.Take(i + 1)),
                            IsFile = false,
                            Children = new List<NavigationNode>()
                        };
                        currentNode.Add(folderNode);
                    }
                    currentNode = folderNode.Children;
                }
            }
        }

        return new NavigationStructure
        {
            RootNodes = rootNodes,
            TotalDocuments = files.Count,
            DocumentCountsByType = documentCounts
        };
    }

    public DocumentationIndex GenerateIndex(string documentsPath)
    {
        if (!Directory.Exists(documentsPath))
        {
            return new DocumentationIndex();
        }

        var files = Directory.GetFiles(documentsPath, "*.md", SearchOption.AllDirectories);
        var documents = new Dictionary<string, DocumentMetadata>();
        var countsByCategory = new Dictionary<string, int>();

        foreach (var file in files)
        {
            try
            {
                var relativePath = Path.GetRelativePath(documentsPath, file).Replace('\\', '/');
                var fileInfo = new FileInfo(file);
                var content = File.ReadAllText(file);
                var metadata = ExtractMetadata(content, file, relativePath);

                documents[relativePath] = metadata;

                if (!countsByCategory.ContainsKey(metadata.Category))
                {
                    countsByCategory[metadata.Category] = 0;
                }
                countsByCategory[metadata.Category]++;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error processing file for index: {FilePath}", file);
            }
        }

        return new DocumentationIndex
        {
            Documents = documents,
            TotalDocuments = documents.Count,
            CountsByCategory = countsByCategory
        };
    }

    private DocumentMetadata ExtractMetadata(string content, string filePath, string relativePath)
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
