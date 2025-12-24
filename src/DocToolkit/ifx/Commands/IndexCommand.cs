using System.ComponentModel;
using DocToolkit.ifx.Infrastructure;
using DocToolkit.ifx.Interfaces.IManagers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

public sealed class IndexCommand : Command<IndexCommand.Settings>
{
    private readonly ISemanticIndexManager _indexManager;

    /// <summary>
    /// Initializes a new instance of the IndexCommand.
    /// </summary>
    /// <param name="indexManager">Semantic index manager</param>
    public IndexCommand(ISemanticIndexManager indexManager)
    {
        _indexManager = indexManager ?? throw new ArgumentNullException(nameof(indexManager));
    }

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

        [Description("Monitor memory usage during indexing")]
        [CommandOption("--monitor-memory")]
        [DefaultValue(false)]
        public bool MonitorMemory { get; init; } = false;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!Directory.Exists(settings.SourcePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Source directory not found: {settings.SourcePath}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[cyan]Building semantic index from:[/] {settings.SourcePath}");

        using var memoryMonitor = new MemoryMonitor("Indexing", settings.MonitorMemory);
        
        if (settings.MonitorMemory)
        {
            memoryMonitor.DisplayStats("Initial");
        }

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
        result = progress.Start(ctx =>
        {
            var task = ctx.AddTask("[green]Processing files...[/]");
            return _indexManager.BuildIndex(
                    settings.SourcePath,
                    settings.OutputPath,
                    settings.ChunkSize,
                    settings.ChunkOverlap,
                    progress =>
                    {
                        task.Increment(progress);
                        if (settings.MonitorMemory && progress % 10 == 0)
                        {
                            // Update memory stats every 10% progress
                            memoryMonitor.DisplaySummary();
                        }
                    }
                );
        });

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
