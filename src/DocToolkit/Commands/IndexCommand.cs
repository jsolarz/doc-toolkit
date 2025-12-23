using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using DocToolkit.Managers;

namespace DocToolkit.Commands;

public sealed class IndexCommand : Command<IndexCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Source directory (default: ./source)")]
        [CommandOption("-s|--source")]
        [DefaultValue("./source")]
        public string SourcePath { get; init; } = "./source";

        [Description("Index output directory (default: ./semantic-index)")]
        [CommandOption("-o|--output")]
        [DefaultValue("./semantic-index")]
        public string OutputPath { get; init; } = "./semantic-index";

        [Description("Chunk size in words (default: 800)")]
        [CommandOption("--chunk-size")]
        [DefaultValue(800)]
        public int ChunkSize { get; init; } = 800;

        [Description("Chunk overlap in words (default: 200)")]
        [CommandOption("--chunk-overlap")]
        [DefaultValue(200)]
        public int ChunkOverlap { get; init; } = 200;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!Directory.Exists(settings.SourcePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Source directory not found: {settings.SourcePath}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[cyan]Building semantic index from:[/] {settings.SourcePath}");

        var progress = AnsiConsole.Progress();
        progress.Columns(
            new ProgressColumn[] {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
            }
        );

        bool result;
        using (var indexManager = new SemanticIndexManager())
        {
            result = progress.Start(ctx =>
            {
                var task = ctx.AddTask("[green]Processing files...[/]");
                return indexManager.BuildIndex(
                    settings.SourcePath,
                    settings.OutputPath,
                    settings.ChunkSize,
                    settings.ChunkOverlap,
                    progress => task.Increment(progress)
                );
            });
        }

        if (result)
        {
            var panel = new Panel(
                new Rows(
                    new Text($"[green]✓[/] Semantic index created at: [bold]{settings.OutputPath}[/]"),
                    new Text(""),
                    new Text("[dim]Files created:[/]"),
                    new Text($"  • {Path.Combine(settings.OutputPath, "vectors.bin")}"),
                    new Text($"  • {Path.Combine(settings.OutputPath, "index.json")}")
                ))
            {
                Header = new PanelHeader("Index Complete", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };

            AnsiConsole.Write(panel);
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Failed to build semantic index");
            return 1;
        }
    }
}
