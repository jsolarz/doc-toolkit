using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DocToolkit.ifx.Interfaces.IAccessors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

/// <summary>
/// Command to start a self-hosted web server for viewing and sharing documents.
/// </summary>
public sealed class WebCommand : Command<WebCommand.Settings>
{
    private readonly ITemplateAccessor _templateAccessor;

    /// <summary>
    /// Initializes a new instance of the WebCommand.
    /// </summary>
    /// <param name="templateAccessor">Template accessor</param>
    public WebCommand(ITemplateAccessor templateAccessor)
    {
        _templateAccessor = templateAccessor ?? throw new ArgumentNullException(nameof(templateAccessor));
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Port to run the web server on (default: 5000)")]
        [CommandOption("-p|--port")]
        [DefaultValue(5000)]
        public int Port { get; init; } = 5000;

        [Description("Host address (default: localhost, use 0.0.0.0 for all interfaces)")]
        [CommandOption("-h|--host")]
        [DefaultValue("localhost")]
        public string Host { get; init; } = "localhost";

        [Description("Directory containing documents (default: ./docs)")]
        [CommandOption("-d|--docs-dir")]
        [DefaultValue("./docs")]
        public string DocsDir { get; init; } = "./docs";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var docsPath = Path.GetFullPath(settings.DocsDir);

        if (!Directory.Exists(docsPath))
        {
            AnsiConsole.MarkupLine($"[yellow]Warning:[/] Docs directory does not exist: {docsPath}");
            AnsiConsole.MarkupLine("[dim]Creating directory...[/]");
            Directory.CreateDirectory(docsPath);
        }

        var url = $"http://{settings.Host}:{settings.Port}";

        AnsiConsole.Write(new Panel(
            new Rows(
                new Text($"[green]✓[/] Starting web server..."),
                new Text(""),
                new Text($"[dim]URL:[/] [bold cyan]{url}[/]"),
                new Text($"[dim]Docs:[/] {docsPath}"),
                new Text(""),
                new Text("[yellow]Press Ctrl+C to stop[/]")
            ))
        {
            Header = new PanelHeader("Document Viewer", Justify.Left),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green)
        });
        AnsiConsole.WriteLine();

        try
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls(url);

            var app = builder.Build();

            // Configure routes
            ConfigureRoutes(app, docsPath);

            // Start server
            AnsiConsole.MarkupLine($"[green]Server started![/] Open [cyan]{url}[/] in your browser.");
            AnsiConsole.MarkupLine("[dim]Press Ctrl+C to stop the server.[/]");
            AnsiConsole.WriteLine();

            app.Run();
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error starting web server:[/] {ex.Message}");
            return 1;
        }
    }

    private static void ConfigureRoutes(WebApplication app, string docsPath)
    {
        // API: List documents with navigation structure
        app.MapGet("/api/documents", () =>
        {
            var documents = GetDocuments(docsPath);
            var navigation = GetNavigationStructure(docsPath);
            return Results.Json(new
            {
                documents,
                navigation
            });
        });

        // API: Get document content (raw markdown, no preprocessing)
        app.MapGet("/api/documents/{*path}", (string path) =>
        {
            var fullPath = Path.Combine(docsPath, path);

            if (!File.Exists(fullPath) || !fullPath.StartsWith(Path.GetFullPath(docsPath)))
            {
                return Results.NotFound();
            }

            var markdown = File.ReadAllText(fullPath);
            var fileName = Path.GetFileName(fullPath);

            // Generate table of contents from headers (lightweight, no markdown parsing)
            var toc = GenerateTableOfContents(markdown);

            return Results.Json(new
            {
                name = fileName,
                path = path,
                content = markdown,
                toc = toc,
                lastModified = File.GetLastWriteTime(fullPath)
            });
        });

        // API: Search documents (full-text search)
        app.MapGet("/api/search", (string? q) =>
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Results.Json(new { results = Array.Empty<object>() });
            }

            var results = SearchDocuments(docsPath, q);
            return Results.Json(new { results, query = q });
        });

        // Serve static files (embedded resources)
        app.MapGet("/", async (HttpContext context) =>
        {
            var html = GetEmbeddedResource("web.index.html");
            context.Response.ContentType = "text/html; charset=utf-8";
            await context.Response.WriteAsync(html);
        });

        app.MapGet("/app.js", async (HttpContext context) =>
        {
            var js = GetEmbeddedResource("web.app.js");
            context.Response.ContentType = "application/javascript; charset=utf-8";
            await context.Response.WriteAsync(js);
        });

        app.MapGet("/app.css", async (HttpContext context) =>
        {
            var css = GetEmbeddedResource("web.app.css");
            context.Response.ContentType = "text/css; charset=utf-8";
            await context.Response.WriteAsync(css);
        });
    }

    private static List<DocumentInfo> GetDocuments(string docsPath)
    {
        var documents = new List<DocumentInfo>();

        if (!Directory.Exists(docsPath))
        {
            return documents;
        }

        var files = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(docsPath, file);
            var info = new FileInfo(file);

            documents.Add(new DocumentInfo
            {
                Name = Path.GetFileName(file),
                Path = relativePath.Replace('\\', '/'),
                Size = info.Length,
                LastModified = info.LastWriteTime,
                Type = GetDocumentType(Path.GetFileName(file))
            });
        }

        return documents.OrderByDescending(d => d.LastModified).ToList();
    }

    private static string GetDocumentType(string fileName)
    {
        var parts = fileName.Split('-');
        if (parts.Length >= 2)
        {
            return parts[1].ToUpperInvariant();
        }
        return "DOC";
    }

    private static string GetEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var fullResourceName = $"DocToolkit.web.{resourceName}";

        using var stream = assembly.GetManifestResourceStream(fullResourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource not found: {fullResourceName}");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static NavigationNode GetNavigationStructure(string docsPath)
    {
        var root = new NavigationNode { Name = "Documents", Path = "", Children = new List<NavigationNode>() };

        if (!Directory.Exists(docsPath))
        {
            return root;
        }

        var files = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories)
            .OrderBy(f => f)
            .ToList();

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(docsPath, file);
            var parts = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var currentNode = root;
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var isFile = i == parts.Length - 1;

                if (isFile)
                {
                    // Add file node
                    currentNode.Children.Add(new NavigationNode
                    {
                        Name = Path.GetFileNameWithoutExtension(part),
                        Path = relativePath.Replace('\\', '/'),
                        IsFile = true,
                        Type = GetDocumentType(part)
                    });
                }
                else
                {
                    // Find or create folder node
                    var folderNode = currentNode.Children.FirstOrDefault(n => n.Name == part && !n.IsFile);
                    if (folderNode == null)
                    {
                        folderNode = new NavigationNode
                        {
                            Name = part,
                            Path = string.Join("/", parts.Take(i + 1)),
                            IsFile = false,
                            Children = new List<NavigationNode>()
                        };
                        currentNode.Children.Add(folderNode);
                    }
                    currentNode = folderNode;
                }
            }
        }

        return root;
    }

    private static List<TocItem> GenerateTableOfContents(string markdown)
    {
        var toc = new List<TocItem>();
        var lines = markdown.Split('\n');
        var idCounter = new Dictionary<int, int>();

        foreach (var line in lines)
        {
            var headerMatch = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
            if (headerMatch.Success)
            {
                var level = headerMatch.Groups[1].Value.Length;
                var text = headerMatch.Groups[2].Value.Trim();

                // Generate anchor ID
                var anchorId = GenerateAnchorId(text, idCounter);

                toc.Add(new TocItem
                {
                    Level = level,
                    Text = text,
                    Anchor = anchorId
                });
            }
        }

        return toc;
    }

    private static string GenerateAnchorId(string text, Dictionary<int, int> idCounter)
    {
        // Convert to lowercase, replace spaces with hyphens, remove special chars
        var anchor = Regex.Replace(text.ToLowerInvariant(), @"[^\w\s-]", "")
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');

        // Handle duplicates
        if (idCounter.ContainsKey(anchor.GetHashCode()))
        {
            idCounter[anchor.GetHashCode()]++;
            anchor = $"{anchor}-{idCounter[anchor.GetHashCode()]}";
        }
        else
        {
            idCounter[anchor.GetHashCode()] = 0;
        }

        return anchor;
    }

    private static List<SearchResult> SearchDocuments(string docsPath, string query)
    {
        var results = new List<SearchResult>();
        var queryLower = query.ToLowerInvariant();
        var queryTerms = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!Directory.Exists(docsPath))
        {
            return results;
        }

        var files = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var contentLower = content.ToLowerInvariant();

            // Simple full-text search - count matching terms
            var matchCount = queryTerms.Count(term => contentLower.Contains(term));
            if (matchCount > 0)
            {
                var relativePath = Path.GetRelativePath(docsPath, file);
                var fileName = Path.GetFileName(file);

                // Extract context snippet (first 200 chars containing query)
                var snippet = ExtractSnippet(content, queryLower, 200);

                results.Add(new SearchResult
                {
                    Name = fileName,
                    Path = relativePath.Replace('\\', '/'),
                    Snippet = snippet,
                    Relevance = matchCount,
                    Type = GetDocumentType(fileName)
                });
            }
        }

        return results.OrderByDescending(r => r.Relevance).Take(20).ToList();
    }

    private static string ExtractSnippet(string content, string query, int maxLength)
    {
        var index = content.ToLowerInvariant().IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return content.Length > maxLength ? content.Substring(0, maxLength) + "..." : content;
        }

        var start = Math.Max(0, index - 50);
        var end = Math.Min(content.Length, index + query.Length + 150);
        var snippet = content.Substring(start, end - start);

        if (start > 0) snippet = "..." + snippet;
        if (end < content.Length) snippet = snippet + "...";

        return snippet;
    }

    private class DocumentInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    private class NavigationNode
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsFile { get; set; }
        public string Type { get; set; } = string.Empty;
        public List<NavigationNode> Children { get; set; } = new();
    }

    private class TocItem
    {
        public int Level { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Anchor { get; set; } = string.Empty;
    }

    private class SearchResult
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
        public int Relevance { get; set; }
        public string Type { get; set; } = string.Empty;
    }
}
