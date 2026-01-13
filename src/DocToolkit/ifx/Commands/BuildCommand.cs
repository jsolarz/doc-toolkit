using System.ComponentModel;
using DocToolkit.ifx.Interfaces.IManagers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

public sealed class BuildCommand : Command<BuildCommand.Settings>
{
    private readonly IBuildManager _buildManager;

    public BuildCommand(IBuildManager buildManager)
    {
        _buildManager = buildManager ?? throw new ArgumentNullException(nameof(buildManager));
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Source directory (default: ./docs)")]
        [CommandOption("-s|--source")]
        [DefaultValue("./docs")]
        public string Source { get; init; } = "./docs";

        [Description("Output directory (default: ./publish/web)")]
        [CommandOption("-o|--output")]
        [DefaultValue("./publish/web")]
        public string Output { get; init; } = "./publish/web";

        [Description("Skip link validation")]
        [CommandOption("--skip-validation")]
        [DefaultValue(false)]
        public bool SkipValidation { get; init; }

        [Description("Skip navigation generation")]
        [CommandOption("--skip-navigation")]
        [DefaultValue(false)]
        public bool SkipNavigation { get; init; }

        [Description("Skip index generation")]
        [CommandOption("--skip-index")]
        [DefaultValue(false)]
        public bool SkipIndex { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var sourcePath = Path.GetFullPath(settings.Source);
        var outputPath = Path.GetFullPath(settings.Output);

        if (!Directory.Exists(sourcePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Source directory does not exist: {sourcePath}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[bold cyan]Building static site...[/]");
        AnsiConsole.MarkupLine($"[dim]Source:[/] [cyan]{sourcePath}[/]");
        AnsiConsole.MarkupLine($"[dim]Output:[/] [cyan]{outputPath}[/]");
        AnsiConsole.WriteLine();

        var options = new BuildManagerOptions
        {
            ValidateLinks = !settings.SkipValidation,
            GenerateNavigation = !settings.SkipNavigation,
            GenerateIndex = !settings.SkipIndex
        };

        var progress = AnsiConsole.Progress();
        progress.Columns(
            new ProgressColumn[] {
                new SpinnerColumn(),
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn()
            }
        );

        var result = progress.Start(ctx =>
        {
            var task = ctx.AddTask("[green]Building site...[/]");
            return _buildManager.BuildSite(sourcePath, outputPath, options, progress =>
            {
                task.Increment(progress);
            });
        });

        if (result)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[bold green]Build Complete[/]").RuleStyle(Color.Green));
            AnsiConsole.MarkupLine($"[green]✓[/] Static site built successfully!");
            AnsiConsole.MarkupLine($"[dim]Output:[/] [cyan]{outputPath}[/]");
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Build failed. Check logs for details.");
            return 1;
        }
    }
}
