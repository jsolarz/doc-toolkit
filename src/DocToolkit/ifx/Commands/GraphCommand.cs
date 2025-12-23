using System.ComponentModel;
using DocToolkit.ifx.Interfaces.IManagers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

public sealed class GraphCommand : Command<GraphCommand.Settings>
{
    private readonly IKnowledgeGraphManager _graphManager;

    /// <summary>
    /// Initializes a new instance of the GraphCommand.
    /// </summary>
    /// <param name="graphManager">Knowledge graph manager</param>
    public GraphCommand(IKnowledgeGraphManager graphManager)
    {
        _graphManager = graphManager ?? throw new ArgumentNullException(nameof(graphManager));
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Source directory (default: ./source)")]
        [CommandOption("-s|--source")]
        [DefaultValue("./source")]
        public string SourcePath { get; init; } = "./source";

        [Description("Output directory (default: ./knowledge-graph)")]
        [CommandOption("-o|--output")]
        [DefaultValue("./knowledge-graph")]
        public string OutputPath { get; init; } = "./knowledge-graph";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!Directory.Exists(settings.SourcePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Source directory not found: {settings.SourcePath}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[cyan]Building knowledge graph from:[/] {settings.SourcePath}");

        var progress = AnsiConsole.Progress();
        progress.Columns(
            new ProgressColumn[] {
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
            }
        );

        var result = progress.Start(ctx =>
        {
            var task = ctx.AddTask("[green]Processing files...[/]");
            return _graphManager.BuildGraph(
                settings.SourcePath,
                settings.OutputPath,
                progress => task.Increment(progress)
            );
        });

        if (result)
        {
            var panel = new Panel(
                new Rows(
                    new Text($"[green]✓[/] Knowledge graph created at: [bold]{settings.OutputPath}[/]"),
                    new Text(""),
                    new Text("[dim]Files created:[/]"),
                    new Text($"  • {Path.Combine(settings.OutputPath, "graph.json")}"),
                    new Text($"  • {Path.Combine(settings.OutputPath, "graph.gv")}"),
                    new Text($"  • {Path.Combine(settings.OutputPath, "graph.md")}")
                ))
            {
                Header = new PanelHeader("Graph Complete", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };

            AnsiConsole.Write(panel);
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Failed to build knowledge graph");
            return 1;
        }
    }
}
