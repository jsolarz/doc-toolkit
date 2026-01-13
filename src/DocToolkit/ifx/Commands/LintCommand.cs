using System.ComponentModel;
using DocToolkit.ifx.Interfaces.IEngines;
using ValidationResult = DocToolkit.ifx.Interfaces.IEngines.ValidationResult;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

/// <summary>
/// Command to validate document quality and compliance.
/// </summary>
public sealed class LintCommand : Command<LintCommand.Settings>
{
    private readonly IDocumentValidator _documentValidator;

    public LintCommand(IDocumentValidator documentValidator)
    {
        _documentValidator = documentValidator ?? throw new ArgumentNullException(nameof(documentValidator));
    }

    public sealed class Settings : CommandSettings
    {
        [Description("Document or directory to validate (default: ./docs)")]
        [CommandArgument(0, "[path]")]
        [DefaultValue("./docs")]
        public string Path { get; init; } = "./docs";

        [Description("Template type for compliance checking (e.g., prd, sow, rfp)")]
        [CommandOption("-t|--template")]
        public string? Template { get; init; }

        [Description("Show warnings")]
        [CommandOption("-w|--warnings")]
        [DefaultValue(true)]
        public bool ShowWarnings { get; init; } = true;

        [Description("Exit with error code if issues found")]
        [CommandOption("--strict")]
        [DefaultValue(false)]
        public bool Strict { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var path = Path.GetFullPath(settings.Path);
        var basePath = Directory.Exists(path) ? path : Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;

        AnsiConsole.MarkupLine($"[bold cyan]Validating documents...[/]");
        AnsiConsole.MarkupLine($"[dim]Path:[/] [cyan]{path}[/]");
        if (!string.IsNullOrEmpty(settings.Template))
        {
            AnsiConsole.MarkupLine($"[dim]Template:[/] [cyan]{settings.Template}[/]");
        }
        AnsiConsole.WriteLine();

        var files = new List<string>();
        if (File.Exists(path))
        {
            files.Add(path);
        }
        else if (Directory.Exists(path))
        {
            files.AddRange(Directory.GetFiles(path, "*.md", SearchOption.AllDirectories));
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Path does not exist: {path}");
            return 1;
        }

        if (files.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No markdown files found[/]");
            return 0;
        }

        var allResults = new List<(string FilePath, ValidationResult Result)>();
        var totalErrors = 0;
        var totalWarnings = 0;

        foreach (var file in files)
        {
            try
            {
                var markdown = System.IO.File.ReadAllText(file);
                var result = _documentValidator.ValidateDocument(markdown, file, basePath, settings.Template);
                allResults.Add((FilePath: file, Result: result));
                totalErrors += result.ErrorCount;
                totalWarnings += result.WarningCount;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error reading {file}:[/] {ex.Message}");
                totalErrors++;
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold cyan]Validation Results[/]").RuleStyle(Color.Cyan1));

        foreach (var (filePath, result) in allResults)
        {
            var relativePath = Path.GetRelativePath(basePath, filePath);
            var hasIssues = result.ErrorCount > 0 || (settings.ShowWarnings && result.WarningCount > 0);

            if (hasIssues)
            {
                AnsiConsole.MarkupLine($"[bold]{relativePath}[/]");
                
                if (result.ErrorCount > 0)
                {
                    AnsiConsole.MarkupLine($"[red]  Errors: {result.ErrorCount}[/]");
                    foreach (var issue in result.Issues)
                    {
                        AnsiConsole.MarkupLine($"[red]    • Line {issue.LineNumber}: {issue.Message}[/]");
                        if (!string.IsNullOrEmpty(issue.Suggestion))
                        {
                            AnsiConsole.MarkupLine($"[dim]      → {issue.Suggestion}[/]");
                        }
                    }
                }

                if (settings.ShowWarnings && result.WarningCount > 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]  Warnings: {result.WarningCount}[/]");
                    foreach (var warning in result.Warnings)
                    {
                        AnsiConsole.MarkupLine($"[yellow]    • Line {warning.LineNumber}: {warning.Message}[/]");
                        if (!string.IsNullOrEmpty(warning.Suggestion))
                        {
                            AnsiConsole.MarkupLine($"[dim]      → {warning.Suggestion}[/]");
                        }
                    }
                }

                AnsiConsole.WriteLine();
            }
        }

        AnsiConsole.Write(new Rule().RuleStyle(Color.Grey));
        AnsiConsole.WriteLine();

        var summary = new Table();
        summary.AddColumn("Metric");
        summary.AddColumn("Count");
        summary.AddRow("[green]Files Validated[/]", files.Count.ToString());
        summary.AddRow("[red]Errors[/]", totalErrors.ToString());
        summary.AddRow("[yellow]Warnings[/]", totalWarnings.ToString());

        AnsiConsole.Write(summary);
        AnsiConsole.WriteLine();

        if (totalErrors == 0 && totalWarnings == 0)
        {
            AnsiConsole.Write(new Panel(
                new Text("[green]✓ All documents are valid![/]")
            )
            {
                Header = new PanelHeader("Validation Complete", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            });
            return 0;
        }
        else
        {
            var message = totalErrors > 0
                ? $"[red]✗ Validation failed with {totalErrors} error(s)[/]"
                : $"[yellow]⚠ Validation completed with {totalWarnings} warning(s)[/]";

            AnsiConsole.Write(new Panel(new Text(message))
            {
                Header = new PanelHeader("Validation Complete", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(totalErrors > 0 ? Color.Red : Color.Yellow)
            });

            return settings.Strict || totalErrors > 0 ? 1 : 0;
        }
    }
}
