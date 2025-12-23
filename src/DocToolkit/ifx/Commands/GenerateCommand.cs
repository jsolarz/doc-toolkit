using System.ComponentModel;
using DocToolkit.ifx.Interfaces.IAccessors;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

public sealed class GenerateCommand : Command<GenerateCommand.Settings>
{
    private readonly ITemplateAccessor _templateAccessor;

    /// <summary>
    /// Initializes a new instance of the GenerateCommand.
    /// </summary>
    /// <param name="templateAccessor">Template accessor</param>
    public GenerateCommand(ITemplateAccessor templateAccessor)
    {
        _templateAccessor = templateAccessor ?? throw new ArgumentNullException(nameof(templateAccessor));
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Document type (prd, rfp, tender, sow, architecture, solution, sla, spec, api, data)")]
        [CommandArgument(0, "<type>")]
        public string Type { get; init; } = string.Empty;

        [Description("Document name")]
        [CommandArgument(1, "<name>")]
        public string Name { get; init; } = string.Empty;

        [Description("Output directory (default: ./docs)")]
        [CommandOption("-o|--output")]
        [DefaultValue("./docs")]
        public string Output { get; init; } = "./docs";
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Type) || string.IsNullOrWhiteSpace(settings.Name))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Both type and name are required");
            AnsiConsole.MarkupLine("[dim]Usage: doc generate <type> <name>[/]");
            return 1;
        }

        if (!_templateAccessor.TemplateExists(settings.Type))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Template '{settings.Type}' not found");
            
            var available = _templateAccessor.GetAvailableTemplates();
            var table = new Table();
            table.AddColumn("Available Templates");
            foreach (var template in available)
            {
                table.AddRow(template);
            }
            AnsiConsole.Write(table);
            return 1;
        }

        var outputPath = _templateAccessor.GenerateDocument(settings.Type, settings.Name, settings.Output);

        var panel = new Panel(
            new Rows(
                new Text($"[green]✓[/] Document created: [bold]{outputPath}[/]"),
                new Text(""),
                new Text($"[dim]Template:[/] {settings.Type}"),
                new Text($"[dim]Name:[/] {settings.Name}")
            ))
        {
            Header = new PanelHeader("Document Generated", Justify.Left),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green)
        };

        AnsiConsole.Write(panel);
        return 0;
    }
}
