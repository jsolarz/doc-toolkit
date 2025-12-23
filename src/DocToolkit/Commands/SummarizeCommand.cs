using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using DocToolkit.Managers;

namespace DocToolkit.Commands;

public sealed class SummarizeCommand : Command<SummarizeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Source directory (default: ./source)")]
        [CommandOption("-s|--source")]
        [DefaultValue("./source")]
        public string SourcePath { get; init; } = "./source";

        [Description("Output file (default: ./context.md)")]
        [CommandOption("-o|--output")]
        [DefaultValue("./context.md")]
        public string OutputFile { get; init; } = "./context.md";

        [Description("Maximum characters per file (default: 5000)")]
        [CommandOption("--max-chars")]
        [DefaultValue(5000)]
        public int MaxChars { get; init; } = 5000;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!Directory.Exists(settings.SourcePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Source directory not found: {settings.SourcePath}");
            return 1;
        }

        var summarizeManager = new SummarizeManager();

        AnsiConsole.MarkupLine($"[cyan]Summarizing source files from:[/] {settings.SourcePath}");

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
            return summarizeManager.SummarizeSource(
                settings.SourcePath,
                settings.OutputFile,
                settings.MaxChars,
                progress => task.Increment(progress)
            );
        });

        if (result)
        {
            var panel = new Panel(
                new Rows(
                    new Text($"[green]✓[/] Summary created: [bold]{settings.OutputFile}[/]")
                ))
            {
                Header = new PanelHeader("Summary Complete", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };

            AnsiConsole.Write(panel);
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Failed to create summary");
            return 1;
        }
    }
}
