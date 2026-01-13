using DocToolkit.ifx.Interfaces.IEngines;
using Microsoft.Extensions.Logging;

namespace DocToolkit.Engines;

/// <summary>
/// Engine for resolving and validating links in markdown documents.
/// Encapsulates algorithm volatility: link resolution logic could change (different link formats, validation rules).
/// </summary>
public class LinkResolver : ILinkResolver
{
    private readonly ILogger<LinkResolver>? _logger;

    public LinkResolver(ILogger<LinkResolver>? logger = null)
    {
        _logger = logger;
    }

    public List<ResolvedLink> ResolveLinks(string markdown, string documentPath, string basePath)
    {
        var links = new List<ResolvedLink>();
        var lines = markdown.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var matches = System.Text.RegularExpressions.Regex.Matches(line, @"\[([^\]]+)\]\(([^\)]+)\)");

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var linkText = match.Groups[1].Value;
                var linkUrl = match.Groups[2].Value;

                var resolved = new ResolvedLink
                {
                    OriginalText = linkText,
                    OriginalUrl = linkUrl,
                    LineNumber = i + 1
                };

                if (IsInternalLink(linkUrl))
                {
                    resolved.IsInternal = true;
                    resolved.ResolvedUrl = ResolveInternalLink(linkUrl, documentPath, basePath);
                    resolved.IsValid = File.Exists(resolved.ResolvedUrl) || Directory.Exists(Path.GetDirectoryName(resolved.ResolvedUrl));
                }
                else
                {
                    resolved.IsInternal = false;
                    resolved.ResolvedUrl = linkUrl;
                    resolved.IsValid = true;
                }

                links.Add(resolved);
            }
        }

        return links;
    }

    public List<BrokenLink> ValidateLinks(string markdown, string documentPath, string basePath)
    {
        var brokenLinks = new List<BrokenLink>();
        var resolvedLinks = ResolveLinks(markdown, documentPath, basePath);

        foreach (var link in resolvedLinks)
        {
            if (link.IsInternal && !link.IsValid)
            {
                brokenLinks.Add(new BrokenLink
                {
                    LinkText = link.OriginalText,
                    LinkUrl = link.OriginalUrl,
                    DocumentPath = documentPath,
                    LineNumber = link.LineNumber,
                    ErrorMessage = $"Target file does not exist: {link.ResolvedUrl}"
                });
            }
        }

        return brokenLinks;
    }

    public string ConvertLinksToHtml(string markdown, string documentPath, string basePath)
    {
        var lines = markdown.Split('\n');
        var result = new List<string>();

        foreach (var line in lines)
        {
            var convertedLine = System.Text.RegularExpressions.Regex.Replace(line, 
                @"\[([^\]]+)\]\(([^\)]+)\)",
                match =>
                {
                    var linkText = match.Groups[1].Value;
                    var linkUrl = match.Groups[2].Value;

                    if (IsInternalLink(linkUrl))
                    {
                        var resolved = ResolveInternalLink(linkUrl, documentPath, basePath);
                        var relativePath = Path.GetRelativePath(basePath, resolved).Replace('\\', '/');
                        return $"<a href=\"{relativePath}\">{linkText}</a>";
                    }

                    return $"<a href=\"{linkUrl}\" target=\"_blank\">{linkText}</a>";
                });

            result.Add(convertedLine);
        }

        return string.Join("\n", result);
    }

    private bool IsInternalLink(string url)
    {
        return !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
               !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) &&
               !url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) &&
               !url.StartsWith("#", StringComparison.OrdinalIgnoreCase);
    }

    private string ResolveInternalLink(string linkUrl, string documentPath, string basePath)
    {
        if (linkUrl.StartsWith("#"))
        {
            return documentPath;
        }

        var documentDir = Path.GetDirectoryName(documentPath) ?? basePath;
        var resolvedPath = Path.GetFullPath(Path.Combine(documentDir, linkUrl));

        if (!Path.HasExtension(resolvedPath))
        {
            var mdPath = resolvedPath + ".md";
            if (File.Exists(mdPath))
            {
                return mdPath;
            }
        }

        return resolvedPath;
    }
}
