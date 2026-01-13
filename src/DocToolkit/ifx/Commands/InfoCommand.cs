using System.ComponentModel;
using DocToolkit.ifx.Interfaces.IManagers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

/// <summary>
/// Command to show detailed information about a document.
/// </summary>
public sealed class InfoCommand : Command<InfoCommand.Settings>
{
    private readonly IDocumentManager _documentManager;

    public InfoCommand(IDocumentManager documentManager)
    {
        _documentManager = documentManager ?? throw new ArgumentNullException(nameof(documentManager));
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Document path (relative to base directory)")]
        [CommandArgument(0, "<path>")]
        public string Path { get; init; } = string.Empty;

        [Description("Base directory (default: ./docs)")]
        [CommandOption("-d|--dir")]
        [DefaultValue("./docs")]
        public string BaseDir { get; init; } = "./docs";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Path))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Document path is required");
            AnsiConsole.MarkupLine("[dim]Usage: doc info <path>[/]");
            return 1;
        }

        var basePath = Path.GetFullPath(settings.BaseDir);
        var documentPath = Path.GetFullPath(Path.Combine(basePath, settings.Path));

        if (!File.Exists(documentPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Document not found: {documentPath}");
            return 1;
        }

        try
        {
            var info = _documentManager.GetDocumentInfo(documentPath, basePath);

            AnsiConsole.MarkupLine($"[bold cyan]Document Information[/]");
            AnsiConsole.MarkupLine($"[dim]Path:[/] [cyan]{Path.GetRelativePath(basePath, documentPath)}[/]");
            AnsiConsole.WriteLine();

            var metadata = new Table();
            metadata.AddColumn("[bold]Property[/]");
            metadata.AddColumn("[bold]Value[/]");

            metadata.AddRow("[cyan]Name[/]", info.Metadata.Name);
            if (!string.IsNullOrEmpty(info.Metadata.Title))
            {
                metadata.AddRow("[cyan]Title[/]", info.Metadata.Title);
            }
            if (!string.IsNullOrEmpty(info.Metadata.Description))
            {
                metadata.AddRow("[cyan]Description[/]", info.Metadata.Description);
            }
            metadata.AddRow("[cyan]Type[/]", info.Metadata.Type);
            metadata.AddRow("[cyan]Category[/]", info.Metadata.Category);
            metadata.AddRow("[cyan]Size[/]", FormatFileSize(info.Metadata.Size));
            metadata.AddRow("[cyan]Created[/]", info.CreatedDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown");
            metadata.AddRow("[cyan]Modified[/]", info.LastModified.ToString("yyyy-MM-dd HH:mm:ss"));

            AnsiConsole.Write(metadata);
            AnsiConsole.WriteLine();

            var stats = new Table();
            stats.AddColumn("[bold]Statistic[/]");
            stats.AddColumn("[bold]Value[/]");

            stats.AddRow("[cyan]Words[/]", info.WordCount.ToString("N0"));
            stats.AddRow("[cyan]Lines[/]", info.LineCount.ToString("N0"));
            stats.AddRow("[cyan]Characters[/]", info.CharacterCount.ToString("N0"));
            stats.AddRow("[cyan]Sections[/]", info.Sections.Count.ToString());

            AnsiConsole.Write(stats);
            AnsiConsole.WriteLine();

            if (info.Sections.Count > 0)
            {
                AnsiConsole.MarkupLine("[bold cyan]Sections[/]");
                var sectionsTable = new Table();
                sectionsTable.AddColumn("[bold]Section[/]");
                sectionsTable.AddColumn("[bold]Words[/]");

                foreach (var section in info.Sections)
                {
                    var wordCount = info.SectionWordCounts.TryGetValue(section, out var count) ? count : 0;
                    sectionsTable.AddRow(section, wordCount.ToString("N0"));
                }

                AnsiConsole.Write(sectionsTable);
                AnsiConsole.WriteLine();
            }

            if (info.Links.Count > 0)
            {
                AnsiConsole.MarkupLine($"[bold cyan]Links ({info.Links.Count})[/]");
                foreach (var link in info.Links.Take(10))
                {
                    AnsiConsole.MarkupLine($"[dim]  • {link}[/]");
                }
                if (info.Links.Count > 10)
                {
                    AnsiConsole.MarkupLine($"[dim]  ... and {info.Links.Count - 10} more[/]");
                }
                AnsiConsole.WriteLine();
            }

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenPaths | ExceptionFormats.ShowLinks);
            return 1;
        }
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
