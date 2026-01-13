using System.ComponentModel;
using System.Text.Json;
using DocToolkit.ifx.Interfaces.IAccessors;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

/// <summary>
/// Command to publish documentation in various formats for deployment.
/// </summary>
public sealed class PublishCommand : Command<PublishCommand.Settings>
{
    private readonly ITemplateAccessor _templateAccessor;

    /// <summary>
    /// Initializes a new instance of the PublishCommand.
    /// </summary>
    /// <param name="templateAccessor">Template accessor</param>
    public PublishCommand(ITemplateAccessor templateAccessor)
    {
        _templateAccessor = templateAccessor ?? throw new ArgumentNullException(nameof(templateAccessor));
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Output format: web, pdf, chm, single, or all (default: web)")]
        [CommandOption("-f|--format")]
        [DefaultValue("web")]
        public string Format { get; init; } = "web";

        [Description("Output directory (default: ./publish)")]
        [CommandOption("-o|--output")]
        [DefaultValue("./publish")]
        public string Output { get; init; } = "./publish";

        [Description("Deployment target: azure, aws, docker, github-pages (optional)")]
        [CommandOption("-t|--target")]
        public string? Target { get; init; }

        [Description("Include source markdown files in published output")]
        [CommandOption("--include-source")]
        [DefaultValue(false)]
        public bool IncludeSource { get; init; }

        [Description("Minify web assets for production")]
        [CommandOption("--minify")]
        [DefaultValue(true)]
        public bool Minify { get; init; } = true;

        [Description("Source documentation directory (default: ./docs)")]
        [CommandOption("-d|--docs-dir")]
        [DefaultValue("./docs")]
        public string DocsDir { get; init; } = "./docs";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var docsPath = Path.GetFullPath(settings.DocsDir);
        var outputPath = Path.GetFullPath(settings.Output);

        if (!Directory.Exists(docsPath))
        {
            AnsiConsole.MarkupLine($"[yellow]Warning:[/] Docs directory does not exist: {docsPath}");
            AnsiConsole.MarkupLine("[dim]Creating directory...[/]");
            Directory.CreateDirectory(docsPath);
        }

        AnsiConsole.MarkupLine($"[bold cyan]Publishing documentation...[/]");
        AnsiConsole.MarkupLine($"[dim]Source:[/] [cyan]{docsPath}[/]");
        AnsiConsole.MarkupLine($"[dim]Output:[/] [cyan]{outputPath}[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule($"[bold cyan]Publishing: {settings.Format.ToUpperInvariant()}[/]").RuleStyle(Color.Cyan1));

        try
        {
            switch (settings.Format.ToLowerInvariant())
            {
                case "web":
                    PublishWeb(docsPath, outputPath, settings);
                    break;
                case "pdf":
                    AnsiConsole.MarkupLine("[yellow]PDF publishing not yet implemented[/]");
                    AnsiConsole.MarkupLine("[dim]This will be available in a future update[/]");
                    return 0;
                case "chm":
                    AnsiConsole.MarkupLine("[yellow]CHM publishing not yet implemented[/]");
                    AnsiConsole.MarkupLine("[dim]This will be available in a future update[/]");
                    return 0;
                case "single":
                    AnsiConsole.MarkupLine("[yellow]Single-file compilation not yet implemented[/]");
                    AnsiConsole.MarkupLine("[dim]This will be available in a future update[/]");
                    return 0;
                case "all":
                    PublishWeb(docsPath, outputPath, settings);
                    AnsiConsole.MarkupLine("[dim]Other formats (PDF, CHM, single) coming soon...[/]");
                    break;
                default:
                    AnsiConsole.MarkupLine($"[red]Error:[/] Unknown format: {settings.Format}");
                    AnsiConsole.MarkupLine("[dim]Supported formats: web, pdf, chm, single, all[/]");
                    return 1;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[bold green]Publishing Complete[/]").RuleStyle(Color.Green));
            AnsiConsole.MarkupLine($"[green]✓[/] Publishing completed!");

            if (settings.Format == "web" || settings.Format == "all")
            {
                AnsiConsole.MarkupLine($"[dim]Web interface ready in:[/] [cyan]{Path.Combine(outputPath, "web")}[/]");
                if (!string.IsNullOrEmpty(settings.Target))
                {
                    AnsiConsole.MarkupLine($"[dim]Deployment target:[/] [bold cyan]{settings.Target}[/]");
                    AnsiConsole.MarkupLine("[dim]Run 'doc deploy --target " + settings.Target + "' to deploy[/]");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShowLinks);
            return 1;
        }
    }

    private void PublishWeb(string docsPath, string outputPath, Settings settings)
    {
        var webOutputPath = Path.Combine(outputPath, "web");
        var webDocsPath = Path.Combine(webOutputPath, "docs");

        AnsiConsole.Status()
            .Spinner(Spinner.Known.BouncingBar)
            .Start("Publishing web interface...", ctx =>
            {
                ctx.Status = "Creating output directories";
                Directory.CreateDirectory(webOutputPath);
                Directory.CreateDirectory(webDocsPath);

                ctx.Status = "Copying web interface files";
                CopyWebInterfaceFiles(webOutputPath);

                ctx.Status = "Copying documentation files";
                CopyDocumentationFiles(docsPath, webDocsPath, settings.IncludeSource);

                ctx.Status = "Generating navigation and document index";
                GenerateNavigationFile(webDocsPath, webOutputPath);
                GenerateDocumentIndex(webDocsPath, webOutputPath);

                if (!string.IsNullOrEmpty(settings.Target))
                {
                    ctx.Status = $"Creating {settings.Target} deployment configuration";
                    CreateDeploymentConfig(webOutputPath, settings.Target, settings);
                }
            });

        // Create tree view of published structure
        AnsiConsole.WriteLine();
        var tree = new Tree($"[bold green]{Path.GetFileName(webOutputPath)}/[/]");

        // Web interface files
        var webNode = tree.AddNode("[cyan]Web Interface[/]");
        if (File.Exists(Path.Combine(webOutputPath, "index.html")))
        {
            webNode.AddNode("[dim]index.html[/]");
        }
        if (File.Exists(Path.Combine(webOutputPath, "app.js")))
        {
            webNode.AddNode("[dim]app.js[/]");
        }
        if (File.Exists(Path.Combine(webOutputPath, "app.css")))
        {
            webNode.AddNode("[dim]app.css[/]");
        }

        // Documentation files
        var docsNode = tree.AddNode("[cyan]docs/[/]");
        if (Directory.Exists(webDocsPath))
        {
            var docFiles = Directory.GetFiles(webDocsPath, "*.md", SearchOption.AllDirectories);
            var fileCount = docFiles.Length;
            docsNode.AddNode($"[dim]{fileCount} markdown file(s)[/]");
        }

        // Generated files
        var genNode = tree.AddNode("[cyan]Generated Files[/]");
        if (File.Exists(Path.Combine(webOutputPath, "navigation.json")))
        {
            genNode.AddNode("[dim]navigation.json[/] [grey]- Document structure[/]");
        }
        if (File.Exists(Path.Combine(webOutputPath, "search-index.json")))
        {
            genNode.AddNode("[dim]search-index.json[/] [grey]- Search index[/]");
        }
        if (File.Exists(Path.Combine(webOutputPath, "data.json")))
        {
            genNode.AddNode("[dim]data.json[/] [grey]- Static data[/]");
        }

        // Deployment config
        if (!string.IsNullOrEmpty(settings.Target))
        {
            var deployNode = tree.AddNode("[cyan]Deployment[/]");
            deployNode.AddNode($"[dim]{settings.Target}[/] [grey]- Configuration[/]");
        }

        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();

        var publishPath = new TextPath(webOutputPath)
            .RootStyle(new Style(Color.Green))
            .SeparatorStyle(new Style(Color.Grey))
            .StemStyle(new Style(Color.Yellow))
            .LeafStyle(new Style(Color.White, decoration: Decoration.Bold));

        AnsiConsole.MarkupLine("[green]✓[/] Web interface published to:");
        AnsiConsole.Write(publishPath);
        AnsiConsole.WriteLine();
    }

    private void CopyWebInterfaceFiles(string outputPath)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();

        // Copy HTML and modify for static deployment
        var htmlContent = GetEmbeddedResource(assembly, resourceNames, "web.index.html");
        if (!string.IsNullOrEmpty(htmlContent))
        {
            // Modify HTML for static deployment (remove API calls, use data.json)
            htmlContent = htmlContent.Replace("/api/documents", "./docs");
            File.WriteAllText(Path.Combine(outputPath, "index.html"), htmlContent);
        }

        // Copy CSS
        CopyEmbeddedResource(assembly, resourceNames, "web.app.css", Path.Combine(outputPath, "app.css"));

        // Copy and modify JavaScript for static deployment
        var jsContent = GetEmbeddedResource(assembly, resourceNames, "web.app.js");
        if (!string.IsNullOrEmpty(jsContent))
        {
            // Modify JavaScript to load from data.json instead of API
            jsContent = ModifyJavaScriptForStatic(jsContent);
            File.WriteAllText(Path.Combine(outputPath, "app.js"), jsContent);
        }
    }

    private string GetEmbeddedResource(System.Reflection.Assembly assembly, string[] resourceNames, string resourceKey)
    {
        var resourceName = FindResourceName(assembly, resourceNames, resourceKey);
        if (resourceName == null)
        {
            return string.Empty;
        }

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            return string.Empty;
        }

        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private string? FindResourceName(System.Reflection.Assembly assembly, string[] resourceNames, string resourceKey)
    {
        // Try exact match first
        var exactMatch = resourceNames.FirstOrDefault(name =>
            name.EndsWith(resourceKey, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
        {
            return exactMatch;
        }

        // Try with namespace
        var withNamespace = $"DocToolkit.{resourceKey}";
        var namespaceMatch = resourceNames.FirstOrDefault(name =>
            name.EndsWith(withNamespace, StringComparison.OrdinalIgnoreCase) ||
            name.Contains(withNamespace, StringComparison.OrdinalIgnoreCase));

        if (namespaceMatch != null)
        {
            return namespaceMatch;
        }

        // Try partial match
        var partialKey = resourceKey.Replace(".", "");
        return resourceNames.FirstOrDefault(name =>
            name.Contains(partialKey, StringComparison.OrdinalIgnoreCase));
    }

    private string ModifyJavaScriptForStatic(string jsContent)
    {
        // For static deployment, we need to:
        // 1. Load navigation from data.json
        // 2. Load documents from documents.json or fetch markdown directly

        // Replace API calls with static data loading
        jsContent = jsContent.Replace(
            "const response = await fetch('/api/documents');",
            "const response = await fetch('./data.json');"
        );

        jsContent = jsContent.Replace(
            "const response = await fetch(`/api/documents/${encodedPath}`",
            "const response = await fetch(`./docs/${encodedPath}`"
        );

        // Add method to load from documents.json cache
        var staticLoader = @"
    // Static deployment: Load document from documents.json or fetch markdown
    async loadDocumentStatic(path) {
        try {
            // Try to load from documents.json first (faster, includes TOC)
            const docsResponse = await fetch('./documents.json');
            if (docsResponse.ok) {
                const docsIndex = await docsResponse.json();
                if (docsIndex[path]) {
                    return docsIndex[path];
                }
            }
            
            // Fall back to fetching markdown directly
            const response = await fetch(`./docs/${path}`);
            if (!response.ok) throw new Error('Document not found');
            const content = await response.text();
            
            // Generate TOC client-side
            const toc = this.generateTOCFromMarkdown(content);
            
            return {
                name: path.split('/').pop(),
                path: path,
                content: content,
                toc: toc,
                lastModified: new Date().toISOString()
            };
        } catch (error) {
            throw new Error(`Failed to load document: ${error.message}`);
        }
    }
    
    generateTOCFromMarkdown(markdown) {
        const toc = [];
        const lines = markdown.split('\n');
        const idCounter = {};
        
        for (const line of lines) {
            const match = line.match(/^(#{1,6})\s+(.+)$/);
            if (match) {
                const level = match[1].length;
                const text = match[2].trim();
                if (text) {
                    const anchor = this.generateAnchorId(text, idCounter);
                    toc.push({ level, text, anchor });
                }
            }
        }
        return toc;
    }
    
    generateAnchorId(text, idCounter) {
        if (!text) return 'header';
        let anchor = text.toLowerCase()
            .replace(/[^\\w\\s-]/g, '')
            .replace(/\\s+/g, '-')
            .replace(/-+/g, '-')
            .replace(/^-+|-+$/g, '') || 'header';
        
        if (idCounter[anchor] !== undefined) {
            idCounter[anchor]++;
            anchor = `${anchor}-${idCounter[anchor]}`;
        } else {
            idCounter[anchor] = 0;
        }
        return anchor;
    }
";

        // Insert static loader methods
        var insertPoint = jsContent.LastIndexOf("    async loadDocument(path)");
        if (insertPoint > 0)
        {
            var methodStart = jsContent.LastIndexOf("    ", insertPoint);
            jsContent = jsContent.Insert(methodStart, staticLoader);

            // Modify loadDocument to try static first
            jsContent = jsContent.Replace(
                "    async loadDocument(path) {",
                @"    async loadDocument(path) {
        // Try static loading first (for published sites)
        try {
            const doc = await this.loadDocumentStatic(path);
            this.currentDocument = doc;
            this.renderDocument(doc);
            return;
        } catch (staticError) {
            // Fall through to API loading (for local development)
        }
        
        // API loading (local development server)"
            );
        }

        return jsContent;
    }

    private void GenerateDocumentIndex(string docsPath, string outputPath)
    {
        if (!Directory.Exists(docsPath))
        {
            return;
        }

        var files = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories);
        var documentIndex = new Dictionary<string, object>();

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(docsPath, file).Replace('\\', '/');
            var content = File.ReadAllText(file);
            var fileName = Path.GetFileName(file);
            var fileInfo = new FileInfo(file);

            // Generate TOC from markdown
            var toc = GenerateTableOfContents(content);

            documentIndex[relativePath] = new
            {
                name = fileName,
                path = relativePath,
                content = content,
                toc = toc,
                lastModified = fileInfo.LastWriteTime
            };
        }

        var indexJson = JsonSerializer.Serialize(documentIndex, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(outputPath, "documents.json"), indexJson);
    }

    private List<object> GenerateTableOfContents(string markdown)
    {
        var toc = new List<object>();
        var lines = markdown.Split('\n');
        var idCounter = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var headerMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(#{1,6})\s+(.+)$");
            if (headerMatch.Success)
            {
                var level = headerMatch.Groups[1].Value.Length;
                var text = headerMatch.Groups[2].Value.Trim();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    var anchorId = GenerateAnchorId(text, idCounter);
                    toc.Add(new
                    {
                        level,
                        text,
                        anchor = anchorId
                    });
                }
            }
        }

        return toc;
    }

    private string GenerateAnchorId(string text, Dictionary<string, int> idCounter)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "header";
        }

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

    private void CopyEmbeddedResource(System.Reflection.Assembly assembly, string[] resourceNames, string resourceKey, string outputPath)
    {
        var content = GetEmbeddedResource(assembly, resourceNames, resourceKey);
        if (!string.IsNullOrEmpty(content))
        {
            File.WriteAllText(outputPath, content);
        }
    }

    private void CopyDocumentationFiles(string sourcePath, string destPath, bool includeSource)
    {
        if (!Directory.Exists(sourcePath))
        {
            return;
        }

        var files = Directory.GetFiles(sourcePath, "*.md", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(sourcePath, file);
            var destFile = Path.Combine(destPath, relativePath);
            var destDir = Path.GetDirectoryName(destFile);

            if (destDir != null)
            {
                Directory.CreateDirectory(destDir);
                File.Copy(file, destFile, overwrite: true);
            }
        }
    }

    private void GenerateNavigationFile(string docsPath, string outputPath)
    {
        if (!Directory.Exists(docsPath))
        {
            return;
        }

        var files = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories)
            .OrderBy(f => f)
            .ToList();

        var documents = files.Select(file =>
        {
            var relativePath = Path.GetRelativePath(docsPath, file).Replace('\\', '/');
            var info = new FileInfo(file);
            return new
            {
                name = Path.GetFileName(file),
                path = relativePath,
                size = info.Length,
                lastModified = info.LastWriteTime,
                type = GetDocumentType(Path.GetFileName(file))
            };
        }).ToList();

        var navigation = BuildNavigationStructure(docsPath, files);

        var data = new
        {
            documents,
            navigation
        };

        var navJson = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(outputPath, "data.json"), navJson);
    }

    private object BuildNavigationStructure(string docsPath, List<string> files)
    {
        var root = new NavigationNode { Name = "Documents", Path = "", IsFile = false, Children = new List<NavigationNode>() };

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(docsPath, file).Replace('\\', '/');
            var parts = relativePath.Split('/');

            var currentNode = root;
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];
                var isFile = i == parts.Length - 1;

                if (isFile)
                {
                    currentNode.Children.Add(new NavigationNode
                    {
                        Name = Path.GetFileNameWithoutExtension(part),
                        Path = relativePath,
                        IsFile = true,
                        Type = GetDocumentType(part)
                    });
                }
                else
                {
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

    private string GetDocumentType(string fileName)
    {
        var parts = fileName.Split('-');
        if (parts.Length >= 2)
        {
            return parts[1].ToUpperInvariant();
        }
        return "DOC";
    }

    private class NavigationNode
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public bool IsFile { get; set; }
        public string Type { get; set; } = string.Empty;
        public List<NavigationNode> Children { get; set; } = new();
    }

    private void CreateDeploymentConfig(string outputPath, string target, Settings settings)
    {
        var deployDir = Path.Combine(outputPath, "deploy");
        Directory.CreateDirectory(deployDir);

        switch (target.ToLowerInvariant())
        {
            case "azure":
            case "azure-static-webapp":
                CreateAzureStaticWebAppConfig(deployDir, settings);
                break;
            case "aws":
            case "aws-lambda":
                CreateAwsLambdaConfig(deployDir, settings);
                break;
            case "docker":
                CreateDockerConfig(deployDir, settings);
                break;
            case "github-pages":
            case "github":
                CreateGitHubPagesConfig(deployDir, settings);
                break;
        }
    }

    private void CreateAzureStaticWebAppConfig(string deployDir, Settings settings)
    {
        var config = new
        {
            name = "azure-static-webapp",
            output_location = ".",
            app_location = ".",
            api_location = "",
            build = new
            {
                app = "",
                api = ""
            }
        };

        var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(deployDir, "azure-static-webapp.json"), configJson);

        // Create GitHub Actions workflow
        var workflow = @"name: Deploy to Azure Static Web Apps

on:
  push:
    branches:
      - main
  workflow_dispatch:

jobs:
  build_and_deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Deploy to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: ""upload""
          app_location: "".""
          output_location: "".""
";

        var workflowsDir = Path.Combine(deployDir, ".github", "workflows");
        Directory.CreateDirectory(workflowsDir);
        File.WriteAllText(Path.Combine(workflowsDir, "azure-static-webapp.yml"), workflow);
    }

    private void CreateAwsLambdaConfig(string deployDir, Settings settings)
    {
        var config = new
        {
            name = "aws-lambda",
            runtime = "nodejs18.x",
            handler = "index.handler",
            timeout = 30,
            memory = 128
        };

        var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(deployDir, "lambda-config.json"), configJson);
    }

    private void CreateDockerConfig(string deployDir, Settings settings)
    {
        var dockerfile = @"FROM nginx:alpine

COPY . /usr/share/nginx/html

EXPOSE 80

CMD [""nginx"", ""-g"", ""daemon off;""]
";

        File.WriteAllText(Path.Combine(deployDir, "Dockerfile"), dockerfile);

        var dockerIgnore = @"node_modules
.git
.github
*.md
!README.md
";

        File.WriteAllText(Path.Combine(deployDir, ".dockerignore"), dockerIgnore);
    }

    private void CreateGitHubPagesConfig(string deployDir, Settings settings)
    {
        var workflow = @"name: Deploy to GitHub Pages

on:
  push:
    branches:
      - main
  workflow_dispatch:

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Deploy to GitHub Pages
        uses: peaceiris/actions-gh-pages@v3
        with:
          github_token: ${{ secrets.GITHUB_TOKEN }}
          publish_dir: .
";

        var workflowsDir = Path.Combine(deployDir, ".github", "workflows");
        Directory.CreateDirectory(workflowsDir);
        File.WriteAllText(Path.Combine(workflowsDir, "github-pages.yml"), workflow);
    }
}
