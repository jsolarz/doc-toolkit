using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using DocToolkit.Accessors;

namespace DocToolkit.Commands;

public sealed class InitCommand : Command<InitCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("Project name")]
        [CommandArgument(0, "<name>")]
        public string Name { get; init; } = string.Empty;
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Name))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Project name is required");
            AnsiConsole.MarkupLine("[dim]Usage: doc init <name>[/]");
            return 1;
        }

        var projectAccessor = new ProjectAccessor();
        
        AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .Start("Initializing project...", ctx =>
            {
                ctx.Status = "Creating directory structure";
                projectAccessor.CreateDirectories(settings.Name);

                ctx.Status = "Setting up Cursor configuration";
                projectAccessor.CreateCursorConfig(settings.Name);

                ctx.Status = "Creating README";
                projectAccessor.CreateReadme(settings.Name);

                ctx.Status = "Creating .gitignore";
                projectAccessor.CreateGitIgnore(settings.Name);

                ctx.Status = "Initializing Git repository";
                projectAccessor.InitializeGit(settings.Name);
            });

        var panel = new Panel(
            new Rows(
                new Text($"[green]✓[/] Project '{settings.Name}' created successfully!"),
                new Text(""),
                new Text("[bold]Next steps:[/]"),
                new Text("  1. cd " + settings.Name),
                new Text("  2. doc generate <type> <name>"),
                new Text("  3. Add source files to ./source/")
            ))
        {
            Header = new PanelHeader("Project Initialized", Justify.Left),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green)
        };

        AnsiConsole.Write(panel);
        return 0;
    }
}
