using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using DocToolkit.Models;
using DocToolkit.Managers;
using DocToolkit.Accessors;

namespace DocToolkit.Commands;

public sealed class SearchCommand : Command<SearchCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Search query")]
        [CommandArgument(0, "<query>")]
        public string Query { get; init; } = string.Empty;

        [Description("Index directory (default: ./semantic-index)")]
        [CommandOption("-i|--index")]
        [DefaultValue("./semantic-index")]
        public string IndexPath { get; init; } = "./semantic-index";

        [Description("Number of results (default: 5)")]
        [CommandOption("-k|--top-k")]
        [DefaultValue(5)]
        public int TopK { get; init; } = 5;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Query))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Search query is required");
            AnsiConsole.MarkupLine("[dim]Usage: doc search <query>[/]");
            return 1;
        }

        var storageAccessor = new VectorStorageAccessor();
        if (!storageAccessor.IndexExists(settings.IndexPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Semantic index not found at: {settings.IndexPath}");
            AnsiConsole.MarkupLine("[dim]Run 'doc index' first to build the index[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[cyan]Searching for:[/] [bold]{settings.Query}[/]");
        AnsiConsole.WriteLine();

        List<Models.SearchResult> results;
        try
        {
            using var searchManager = new SemanticSearchManager();
            results = searchManager.Search(settings.Query, settings.IndexPath, settings.TopK);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            if (ex.Message.Contains("ONNX model"))
            {
                AnsiConsole.MarkupLine("[dim]Please ensure the ONNX model is available in the models/ directory[/]");
            }
            return 1;
        }

        if (results.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No results found[/]");
            return 0;
        }

        var table = new Table();
        table.AddColumn("Rank");
        table.AddColumn("File");
        table.AddColumn("Score");
        table.AddColumn("Preview");

        for (int i = 0; i < results.Count; i++)
        {
            var result = results[i];
            var preview = result.Chunk.Length > 100
                ? result.Chunk.Substring(0, 100) + "..."
                : result.Chunk;

            table.AddRow(
                $"[bold]{(i + 1)}[/]",
                $"[cyan]{result.File}[/]",
                $"[green]{result.Score:F3}[/]",
                $"[dim]{preview}[/]"
            );
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
