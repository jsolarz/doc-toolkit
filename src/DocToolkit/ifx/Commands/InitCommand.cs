using System.ComponentModel;
using DocToolkit.ifx.Interfaces.IAccessors;
using DocToolkit.ifx.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

public sealed class InitCommand : Command<InitCommand.Settings>
{
    private readonly IProjectAccessor _projectAccessor;

    /// <summary>
    /// Initializes a new instance of the InitCommand.
    /// </summary>
    /// <param name="projectAccessor">Project accessor</param>
    public InitCommand(IProjectAccessor projectAccessor)
    {
        _projectAccessor = projectAccessor ?? throw new ArgumentNullException(nameof(projectAccessor));
    }

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

        // Prompt for project type
        var projectType = AnsiConsole.Prompt(
            new SelectionPrompt<ProjectType>()
                .Title("What type of documentation project is this?")
                .AddChoices(ProjectType.CustomerFacing, ProjectType.DeveloperFacing, ProjectType.Mixed)
                .UseConverter(type => type switch
                {
                    ProjectType.CustomerFacing => "Customer-Facing (PRDs, proposals, requirements)",
                    ProjectType.DeveloperFacing => "Developer-Facing (architecture, design, specs)",
                    ProjectType.Mixed => "Mixed (both customer and developer documentation)",
                    _ => type.ToString()
                }));

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold cyan]Initializing Project[/]").RuleStyle(Color.Cyan1));

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Arrow)
            .Start("Initializing project...", ctx =>
            {
                ctx.Status = "Creating directory structure";
                _projectAccessor.CreateDirectories(settings.Name, projectType);

                ctx.Status = "Setting up Cursor configuration";
                _projectAccessor.CreateCursorConfig(settings.Name);

                ctx.Status = "Creating configuration files";
                _projectAccessor.CreateConfigFiles(settings.Name);

                ctx.Status = "Creating README and documentation";
                _projectAccessor.CreateReadme(settings.Name, projectType);
                _projectAccessor.CreateOnboardingGuide(settings.Name, projectType);
                _projectAccessor.CreateDocIgnore(settings.Name);

                ctx.Status = "Setting up docs-as-code (CI/CD workflows)";
                _projectAccessor.CreateLintWorkflow(settings.Name);

                ctx.Status = "Creating .gitignore";
                _projectAccessor.CreateGitIgnore(settings.Name);

                ctx.Status = "Initializing Git repository";
                _projectAccessor.InitializeGit(settings.Name);
            });

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold green]Initialization Complete[/]").RuleStyle(Color.Green));

        // Create tree view of project structure
        var tree = new Tree($"[bold green]{settings.Name}/[/]");

        // Documentation directories
        var docsNode = tree.AddNode("[cyan]docs/[/]");
        if (projectType == ProjectType.CustomerFacing || projectType == ProjectType.Mixed)
        {
            docsNode.AddNode("[dim]customer/[/] [grey]- Customer-facing docs[/]");
        }
        if (projectType == ProjectType.DeveloperFacing || projectType == ProjectType.Mixed)
        {
            docsNode.AddNode("[dim]developer/[/] [grey]- Developer-facing docs[/]");
        }
        docsNode.AddNode("[dim]shared/[/] [grey]- Shared documentation[/]");

        // Publishing directories
        var publishNode = tree.AddNode("[cyan]publish/[/]");
        publishNode.AddNode("[dim]web/[/] [grey]- Static site[/]");
        publishNode.AddNode("[dim]pdf/[/] [grey]- PDF files[/]");
        publishNode.AddNode("[dim]chm/[/] [grey]- HTML Help[/]");
        publishNode.AddNode("[dim]single/[/] [grey]- Single file[/]");

        // Configuration
        var configNode = tree.AddNode("[cyan].doc-toolkit/[/]");
        configNode.AddNode("[dim]config.json[/]");
        configNode.AddNode("[dim]llm-config.json[/]");
        configNode.AddNode("[dim]publish-config.json[/]");

        // CI/CD
        var githubNode = tree.AddNode("[cyan].github/[/]");
        githubNode.AddNode("[dim]workflows/[/] [grey]- CI/CD workflows[/]");

        // Root files
        tree.AddNode("[cyan]README.md[/]");
        tree.AddNode("[cyan]ONBOARDING.md[/]");
        tree.AddNode("[cyan].gitignore[/]");
        tree.AddNode("[cyan].docignore[/]");
        tree.AddNode("[cyan].cursorrules[/]");

        AnsiConsole.WriteLine();
        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();

        var panel = new Panel(
            new Rows(
                new Text($"[green]✓[/] Project '[bold]{settings.Name}[/]' created successfully!"),
                new Text(""),
                new Text("[bold cyan]Next steps:[/]"),
                new Text($"  [dim]1.[/] [cyan]cd {settings.Name}[/]"),
                new Text($"  [dim]2.[/] Read [cyan]ONBOARDING.md[/] for walkthrough"),
                new Text($"  [dim]3.[/] [cyan]doc generate prd \"Product Name\"[/]"),
                new Text($"  [dim]4.[/] [cyan]doc lint[/] (check document quality)"),
                new Text($"  [dim]5.[/] [cyan]doc publish web[/] (when ready)"),
                new Text(""),
                new Text("[dim]Docs-as-code: Git, linting, and CI/CD configured[/]"),
                new Text("[dim]Run 'doc --help' for all commands[/]")
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
