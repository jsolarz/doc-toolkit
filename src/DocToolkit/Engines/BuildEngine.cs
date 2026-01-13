using DocToolkit.ifx.Interfaces.IEngines;
using Markdig;
using Microsoft.Extensions.Logging;

namespace DocToolkit.Engines;

/// <summary>
/// Engine for building static sites from markdown files.
/// Encapsulates algorithm volatility: markdown compilation could change (different processors, rendering options).
/// </summary>
public class BuildEngine : IBuildEngine
{
    private readonly ILogger<BuildEngine>? _logger;
    private readonly MarkdownPipeline _pipeline;

    public BuildEngine(ILogger<BuildEngine>? logger = null)
    {
        _logger = logger;
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseAutoIdentifiers()
            .UseAutoLinks()
            .UseEmphasisExtras()
            .UseListExtras()
            .UseTaskLists()
            .UseMediaLinks()
            .UsePipeTables()
            .UseGenericAttributes()
            .Build();
    }

    public string CompileMarkdown(string markdown, BuildEngineOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var html = Markdown.ToHtml(markdown, _pipeline);

        if (options?.GenerateAnchors == true)
        {
            html = AddAnchorLinks(html);
        }

        if (options?.IncludeTableOfContents == true)
        {
            html = AddTableOfContents(html, markdown);
        }

        return html;
    }

    public BuildResult BuildSite(string sourcePath, string outputPath, BuildEngineOptions? options = null)
    {
        var result = new BuildResult();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        if (!Directory.Exists(sourcePath))
        {
            result.Errors = 1;
            result.ErrorMessages.Add($"Source directory does not exist: {sourcePath}");
            return result;
        }

        Directory.CreateDirectory(outputPath);

        var files = Directory.GetFiles(sourcePath, "*.md", SearchOption.AllDirectories).ToList();

        foreach (var file in files)
        {
            try
            {
                var relativePath = Path.GetRelativePath(sourcePath, file);
                var outputFile = Path.ChangeExtension(Path.Combine(outputPath, relativePath), ".html");
                var outputDir = Path.GetDirectoryName(outputFile);

                if (outputDir != null)
                {
                    Directory.CreateDirectory(outputDir);
                }

                var markdown = File.ReadAllText(file);
                var html = CompileMarkdown(markdown, options);

                File.WriteAllText(outputFile, html);
                result.FilesProcessed++;
            }
            catch (Exception ex)
            {
                result.Errors++;
                result.ErrorMessages.Add($"Error processing {file}: {ex.Message}");
                _logger?.LogWarning(ex, "Error building file: {FilePath}", file);
            }
        }

        stopwatch.Stop();
        result.ElapsedTime = stopwatch.Elapsed;

        return result;
    }

    private string AddAnchorLinks(string html)
    {
        return html;
    }

    private string AddTableOfContents(string html, string markdown)
    {
        var toc = ExtractTableOfContents(markdown);
        if (toc.Count == 0)
        {
            return html;
        }

        var tocHtml = GenerateTocHtml(toc);
        var insertIndex = html.IndexOf("</h1>", StringComparison.OrdinalIgnoreCase);
        if (insertIndex > 0)
        {
            return html.Insert(insertIndex + 5, tocHtml);
        }

        return tocHtml + html;
    }

    private List<TocEntry> ExtractTableOfContents(string markdown)
    {
        var toc = new List<TocEntry>();
        var lines = markdown.Split('\n');
        var idCounter = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var match = System.Text.RegularExpressions.Regex.Match(line, @"^(#{1,6})\s+(.+)$");
            if (match.Success)
            {
                var level = match.Groups[1].Value.Length;
                var text = match.Groups[2].Value.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var anchor = GenerateAnchorId(text, idCounter);
                    toc.Add(new TocEntry { Level = level, Text = text, Anchor = anchor });
                }
            }
        }

        return toc;
    }

    private string GenerateAnchorId(string text, Dictionary<string, int> idCounter)
    {
        var anchor = System.Text.RegularExpressions.Regex.Replace(text.ToLowerInvariant(), @"[^\w\s-]", "")
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');

        if (string.IsNullOrEmpty(anchor))
        {
            anchor = "header";
        }

        if (idCounter.TryGetValue(anchor, out var count))
        {
            idCounter[anchor] = count + 1;
            anchor = $"{anchor}-{count + 1}";
        }
        else
        {
            idCounter[anchor] = 0;
        }

        return anchor;
    }

    private string GenerateTocHtml(List<TocEntry> toc)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<nav class=\"table-of-contents\">");
        sb.AppendLine("  <h2>Table of Contents</h2>");
        sb.AppendLine("  <ul>");

        foreach (var entry in toc)
        {
            var indent = new string(' ', entry.Level * 2);
            sb.AppendLine($"{indent}<li><a href=\"#{entry.Anchor}\">{entry.Text}</a></li>");
        }

        sb.AppendLine("  </ul>");
        sb.AppendLine("</nav>");
        return sb.ToString();
    }

    private class TocEntry
    {
        public int Level { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Anchor { get; set; } = string.Empty;
    }
}
