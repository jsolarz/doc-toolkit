using System.ComponentModel;
using DocToolkit.ifx.Interfaces.IEngines;
using DocToolkit.ifx.Interfaces.IManagers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

/// <summary>
/// Command to list documents with metadata.
/// </summary>
public sealed class ListCommand : Command<ListCommand.Settings>
{
    private readonly IDocumentManager _documentManager;

    public ListCommand(IDocumentManager documentManager)
    {
        _documentManager = documentManager ?? throw new ArgumentNullException(nameof(documentManager));
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Documents directory (default: ./docs)")]
        [CommandOption("-d|--dir")]
        [DefaultValue("./docs")]
        public string Directory { get; init; } = "./docs";

        [Description("Filter by type, category, or search term")]
        [CommandOption("-f|--filter")]
        public string? Filter { get; init; }

        [Description("Output format: table, json, or tree (default: table)")]
        [CommandOption("--format")]
        [DefaultValue("table")]
        public string Format { get; init; } = "table";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var docsPath = Path.GetFullPath(settings.Directory);

        if (!Directory.Exists(docsPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Directory does not exist: {docsPath}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[bold cyan]Listing documents...[/]");
        AnsiConsole.MarkupLine($"[dim]Directory:[/] [cyan]{docsPath}[/]");
        if (!string.IsNullOrEmpty(settings.Filter))
        {
            AnsiConsole.MarkupLine($"[dim]Filter:[/] [cyan]{settings.Filter}[/]");
        }
        AnsiConsole.WriteLine();

        try
        {
            var documents = _documentManager.ListDocuments(docsPath, settings.Filter);

            if (documents.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No documents found[/]");
                return 0;
            }

            switch (settings.Format.ToLowerInvariant())
            {
                case "table":
                    RenderTable(documents, docsPath);
                    break;
                case "json":
                    RenderJson(documents);
                    break;
                case "tree":
                    RenderTree(documents, docsPath);
                    break;
                default:
                    AnsiConsole.MarkupLine($"[red]Error:[/] Unknown format: {settings.Format}");
                    AnsiConsole.MarkupLine("[dim]Supported formats: table, json, tree[/]");
                    return 1;
            }

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[dim]Total: {documents.Count} document(s)[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShowLinks);
            return 1;
        }
    }

    private void RenderTable(List<DocumentMetadata> documents, string basePath)
    {
        var table = new Table();
        table.AddColumn("[bold]Name[/]");
        table.AddColumn("[bold]Type[/]");
        table.AddColumn("[bold]Category[/]");
        table.AddColumn("[bold]Size[/]");
        table.AddColumn("[bold]Modified[/]");

        foreach (var doc in documents)
        {
            var relativePath = Path.GetRelativePath(basePath, Path.Combine(basePath, doc.Path));
            var size = FormatFileSize(doc.Size);
            var modified = doc.LastModified.ToString("yyyy-MM-dd");

            table.AddRow(
                relativePath,
                $"[cyan]{doc.Type}[/]",
                $"[dim]{doc.Category}[/]",
                $"[dim]{size}[/]",
                $"[dim]{modified}[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    private void RenderJson(List<DocumentMetadata> documents)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(documents, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
        AnsiConsole.WriteLine(json);
    }

    private void RenderTree(List<DocumentMetadata> documents, string basePath)
    {
        var tree = new Tree("[bold cyan]Documents[/]");
        var grouped = documents.GroupBy(d => Path.GetDirectoryName(Path.Combine(basePath, d.Path)) ?? basePath);

        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            var folderName = Path.GetFileName(group.Key) ?? "Root";
            var folderNode = tree.AddNode($"[dim]{folderName}/[/]");

            foreach (var doc in group.OrderBy(d => d.Name))
            {
                var node = folderNode.AddNode($"[cyan]{doc.Name}[/]");
                if (!string.IsNullOrEmpty(doc.Title) && doc.Title != doc.Name)
                {
                    node.AddNode($"[dim]Title: {doc.Title}[/]");
                }
                node.AddNode($"[dim]Type: {doc.Type} | Category: {doc.Category}[/]");
            }
        }

        AnsiConsole.Write(tree);
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
