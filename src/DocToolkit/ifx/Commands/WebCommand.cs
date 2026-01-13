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
                new Text($"[dim]Docs:[/] [cyan]{docsPath}[/]"),
                new Text(""),
                new Text("[yellow]Press Ctrl+C to stop[/]")
            ))
        {
            Header = new PanelHeader("Document Viewer", Justify.Left),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green)
        });
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold cyan]Server Status[/]").RuleStyle(Color.Cyan1));

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
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShowLinks);
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
            if (string.IsNullOrWhiteSpace(path))
            {
                return Results.BadRequest("Path is required");
            }

            // Decode URL encoding and normalize path separators
            var decodedPath = Uri.UnescapeDataString(path);
            var normalizedPath = decodedPath.Replace('\\', '/').TrimStart('/');
            
            // Prevent directory traversal
            if (normalizedPath.Contains("..") || Path.IsPathRooted(normalizedPath))
            {
                return Results.BadRequest("Invalid path");
            }

            // Ensure it's a markdown file
            if (!normalizedPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                normalizedPath += ".md";
            }

            var fullPath = Path.GetFullPath(Path.Combine(docsPath, normalizedPath));
            var docsFullPath = Path.GetFullPath(docsPath);

            // Security check: ensure the resolved path is within docs directory
            if (!fullPath.StartsWith(docsFullPath, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
            {
                return Results.NotFound($"Document not found: {normalizedPath}");
            }

            try
            {
                var markdown = File.ReadAllText(fullPath);
                var fileName = Path.GetFileName(fullPath);
                var fileInfo = new FileInfo(fullPath);

                // Generate table of contents from headers (lightweight, no markdown parsing)
                var toc = GenerateTableOfContents(markdown);

                return Results.Json(new
                {
                    name = fileName,
                    path = normalizedPath,
                    content = markdown,
                    toc = toc,
                    lastModified = fileInfo.LastWriteTime
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error reading document: {ex.Message}");
            }
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
            try
            {
                var html = GetEmbeddedResource("index.html");
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.WriteAsync(html);
            }
            catch (FileNotFoundException ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Error loading web interface: {ex.Message}");
            }
        });

        app.MapGet("/app.js", async (HttpContext context) =>
        {
            try
            {
                var js = GetEmbeddedResource("app.js");
                context.Response.ContentType = "application/javascript; charset=utf-8";
                await context.Response.WriteAsync(js);
            }
            catch (FileNotFoundException ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Error loading JavaScript: {ex.Message}");
            }
        });

        app.MapGet("/app.css", async (HttpContext context) =>
        {
            try
            {
                var css = GetEmbeddedResource("app.css");
                context.Response.ContentType = "text/css; charset=utf-8";
                await context.Response.WriteAsync(css);
            }
            catch (FileNotFoundException ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Error loading CSS: {ex.Message}");
            }
        });

        // Handle direct markdown file access (e.g., /CODE-DOCUMENTATION-STANDARDS.md or /subfolder/file.md)
        // This must come after static file routes but handles .md files
        app.MapGet("/{*filePath}", (string filePath) =>
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Results.NotFound();
            }

            // Only handle .md files
            if (!filePath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                return Results.NotFound();
            }

            // Normalize path
            var normalizedPath = filePath.Replace('\\', '/').TrimStart('/');
            
            // Prevent directory traversal
            if (normalizedPath.Contains("..") || Path.IsPathRooted(normalizedPath))
            {
                return Results.BadRequest("Invalid path");
            }

            var fullPath = Path.GetFullPath(Path.Combine(docsPath, normalizedPath));
            var docsFullPath = Path.GetFullPath(docsPath);

            // Security check
            if (!fullPath.StartsWith(docsFullPath, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
            {
                return Results.NotFound();
            }

            // Redirect to API endpoint for proper JSON response
            var encodedPath = Uri.EscapeDataString(normalizedPath);
            return Results.Redirect($"/api/documents/{encodedPath}");
        });
    }

    private static List<DocumentInfo> GetDocuments(string docsPath)
    {
        var documents = new List<DocumentInfo>();

        if (!Directory.Exists(docsPath))
        {
            return documents;
        }

        try
        {
            var files = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
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
                catch (Exception ex)
                {
                    // Log but continue processing other files
                    System.Diagnostics.Debug.WriteLine($"Error processing file {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading documents directory: {ex.Message}");
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

    // Cache embedded resources for performance (they don't change at runtime)
    private static readonly Dictionary<string, string> _resourceCache = new();
    private static readonly object _resourceCacheLock = new();

    private static string GetEmbeddedResource(string fileName)
    {
        // Check cache first
        lock (_resourceCacheLock)
        {
            if (_resourceCache.TryGetValue(fileName, out var cached))
            {
                return cached;
            }
        }

        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();
        
        // Try both patterns: "web.{fileName}" and "{namespace}.web.{fileName}"
        var possibleNames = new[]
        {
            $"web.{fileName}",
            $"{assembly.GetName().Name}.web.{fileName}"
        };

        var resourceName = possibleNames.FirstOrDefault(name => resourceNames.Contains(name));
        if (resourceName == null)
        {
            throw new FileNotFoundException($"Embedded resource not found. Tried: {string.Join(", ", possibleNames)}");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Could not load embedded resource: {resourceName}");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = reader.ReadToEnd();

        // Cache the result
        lock (_resourceCacheLock)
        {
            _resourceCache[fileName] = content;
        }

        return content;
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
        
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return toc;
        }

        var lines = markdown.Split('\n');
        var idCounter = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var headerMatch = Regex.Match(line, @"^(#{1,6})\s+(.+)$");
            if (headerMatch.Success)
            {
                var level = headerMatch.Groups[1].Value.Length;
                var text = headerMatch.Groups[2].Value.Trim();

                if (!string.IsNullOrWhiteSpace(text))
                {
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
        }

        return toc;
    }

    private static string GenerateAnchorId(string text, Dictionary<string, int> idCounter)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "header";
        }

        // Convert to lowercase, replace spaces with hyphens, remove special chars
        var anchor = Regex.Replace(text.ToLowerInvariant(), @"[^\w\s-]", "")
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');

        if (string.IsNullOrEmpty(anchor))
        {
            anchor = "header";
        }

        // Handle duplicates using the actual anchor string as key (more reliable than hash)
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

    private static List<SearchResult> SearchDocuments(string docsPath, string query)
    {
        var results = new List<SearchResult>();

        if (string.IsNullOrWhiteSpace(query) || !Directory.Exists(docsPath))
        {
            return results;
        }

        var queryLower = query.ToLowerInvariant();
        var queryTerms = queryLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (queryTerms.Length == 0)
        {
            return results;
        }

        try
        {
            var files = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
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
                catch (Exception ex)
                {
                    // Log but continue processing other files
                    System.Diagnostics.Debug.WriteLine($"Error searching file {file}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error reading documents directory for search: {ex.Message}");
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
