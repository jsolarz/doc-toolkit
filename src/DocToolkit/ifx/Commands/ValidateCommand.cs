using Spectre.Console;
using Spectre.Console.Cli;
using DocToolkit.Infrastructure;

namespace DocToolkit.Commands;

public sealed class ValidateCommand : Command
{
    public override int Execute(CommandContext context)
    {
        AnsiConsole.MarkupLine("[cyan]Validating Documentation Toolkit Setup[/]");
        AnsiConsole.WriteLine();

        var validationService = new ValidationService();
        var result = validationService.Validate();

        // Check ONNX model
        AnsiConsole.MarkupLine("[yellow]Checking embedding model...[/]");
        var modelAvailable = validationService.CheckOnnxModelAvailable();
        if (modelAvailable)
        {
            AnsiConsole.MarkupLine("  [green]✓[/] ONNX model found");
        }
        else
        {
            AnsiConsole.MarkupLine("  [yellow]⚠[/] ONNX model not found");
            AnsiConsole.MarkupLine("     [dim]Download all-MiniLM-L6-v2.onnx and place in models/ directory[/]");
        }

        AnsiConsole.WriteLine();

        // Check document libraries
        AnsiConsole.MarkupLine("[yellow]Checking document libraries...[/]");
        var libraries = new Dictionary<string, bool>
        {
            { "DocumentFormat.OpenXml", validationService.CheckDocumentLibraryAvailable("DocumentFormat.OpenXml") },
            { "UglyToad.PdfPig", validationService.CheckDocumentLibraryAvailable("UglyToad.PdfPig") },
            { "Microsoft.ML.OnnxRuntime", validationService.CheckDocumentLibraryAvailable("Microsoft.ML.OnnxRuntime") }
        };

        foreach (var (library, available) in libraries)
        {
            if (available)
            {
                AnsiConsole.MarkupLine($"  [green]✓[/] {library}");
            }
            else
            {
                AnsiConsole.MarkupLine($"  [red]❌[/] {library} (missing)");
            }
        }

        AnsiConsole.WriteLine();

        // Check external tools
        AnsiConsole.MarkupLine("[yellow]Checking external tools...[/]");
        var popplerAvailable = validationService.IsToolAvailablePublic("pdftotext");
        var tesseractAvailable = validationService.IsToolAvailablePublic("tesseract");

        if (popplerAvailable)
        {
            AnsiConsole.MarkupLine("  [green]✓[/] Poppler (pdftotext) found");
        }
        else
        {
            AnsiConsole.MarkupLine("  [yellow]⚠[/] Poppler (pdftotext) not found");
        }

        if (tesseractAvailable)
        {
            AnsiConsole.MarkupLine("  [green]✓[/] Tesseract OCR found");
        }
        else
        {
            AnsiConsole.MarkupLine("  [yellow]⚠[/] Tesseract OCR not found (optional)");
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule());

        if (result.Success)
        {
            var panel = new Panel(
                new Rows(
                    new Text("[green]✓[/] All checks passed! Setup is complete.")
                ))
            {
                Header = new PanelHeader("Validation Complete", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };

            AnsiConsole.Write(panel);
            return 0;
        }
        else
        {
            var errors = new List<Text>();
            errors.Add(new Text("[red]❌ Setup validation failed:[/]"));
            errors.Add(Text.Empty);

            foreach (var error in result.Errors)
            {
                errors.Add(new Text($"[red]  • {error}[/]"));
            }

            if (result.Warnings.Count > 0)
            {
                errors.Add(Text.Empty);
                errors.Add(new Text("[yellow]Warnings:[/]"));
                foreach (var warning in result.Warnings)
                {
                    errors.Add(new Text($"[yellow]  • {warning}[/]"));
                }
            }

            var panel = new Panel(new Rows(errors))
            {
                Header = new PanelHeader("Validation Failed", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Red)
            };

            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[dim]Install missing packages with: pip install -r requirements.txt[/]");
            return 1;
        }
    }
}
