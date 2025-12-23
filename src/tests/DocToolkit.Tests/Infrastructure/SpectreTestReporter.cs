using System.Diagnostics;
using Spectre.Console;
using Xunit.Abstractions;

namespace DocToolkit.Tests.Infrastructure;

/// <summary>
/// Custom test reporter that uses Spectre.Console for beautiful test output.
/// </summary>
public class SpectreTestReporter : ITestOutputHelper
{
    private readonly List<TestResult> _results = new();
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private int _passed = 0;
    private int _failed = 0;
    private int _skipped = 0;

    public void WriteLine(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        AnsiConsole.MarkupLine($"[dim]{message}[/]");
    }

    public void WriteLine(string format, params object?[] args)
    {
        WriteLine(string.Format(format, args));
    }

    public void ReportTestStart(string testName, string? category = null)
    {
        AnsiConsole.MarkupLine($"[cyan]▶[/] [dim]{testName}[/]");
    }

    public void ReportTestPass(string testName, TimeSpan duration, string? category = null)
    {
        _passed++;
        _results.Add(new TestResult(testName, TestStatus.Passed, duration, category));
        AnsiConsole.MarkupLine($"[green]✓[/] [green]{testName}[/] [dim]({duration.TotalMilliseconds:F2}ms)[/]");
    }

    public void ReportTestFail(string testName, TimeSpan duration, string? errorMessage, string? category = null)
    {
        _failed++;
        _results.Add(new TestResult(testName, TestStatus.Failed, duration, category, errorMessage));
        AnsiConsole.MarkupLine($"[red]✗[/] [red]{testName}[/] [dim]({duration.TotalMilliseconds:F2}ms)[/]");
        if (!string.IsNullOrWhiteSpace(errorMessage))
        {
            AnsiConsole.MarkupLine($"[red]  {errorMessage}[/]");
        }
    }

    public void ReportTestSkip(string testName, string? reason, string? category = null)
    {
        _skipped++;
        _results.Add(new TestResult(testName, TestStatus.Skipped, TimeSpan.Zero, category, reason));
        AnsiConsole.MarkupLine($"[yellow]⊘[/] [yellow]{testName}[/] [dim](skipped)[/]");
        if (!string.IsNullOrWhiteSpace(reason))
        {
            AnsiConsole.MarkupLine($"[yellow]  Reason: {reason}[/]");
        }
    }

    public void PrintSummary()
    {
        _stopwatch.Stop();
        
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]Test Summary[/]").RuleStyle("blue"));

        var total = _passed + _failed + _skipped;
        var passRate = total > 0 ? (_passed * 100.0 / total) : 0;

        // Create summary table
        var table = new Table();
        table.AddColumn("Status");
        table.AddColumn("Count");
        table.AddColumn("Percentage");
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Blue);

        table.AddRow(
            "[green]Passed[/]",
            $"[green]{_passed}[/]",
            $"[green]{passRate:F1}%[/]"
        );

        if (_failed > 0)
        {
            table.AddRow(
                "[red]Failed[/]",
                $"[red]{_failed}[/]",
                $"[red]{(100.0 * _failed / total):F1}%[/]"
            );
        }

        if (_skipped > 0)
        {
            table.AddRow(
                "[yellow]Skipped[/]",
                $"[yellow]{_skipped}[/]",
                $"[yellow]{(100.0 * _skipped / total):F1}%[/]"
            );
        }

        table.AddRow(
            "[bold]Total[/]",
            $"[bold]{total}[/]",
            "[bold]100.0%[/]"
        );

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]Total time: {_stopwatch.Elapsed.TotalSeconds:F2}s[/]");

        // Show failed tests if any
        if (_failed > 0)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule("[bold red]Failed Tests[/]").RuleStyle("red"));
            
            var failedTests = _results.Where(r => r.Status == TestStatus.Failed).ToList();
            foreach (var test in failedTests)
            {
                AnsiConsole.MarkupLine($"[red]✗ {test.Name}[/]");
                if (!string.IsNullOrWhiteSpace(test.ErrorMessage))
                {
                    AnsiConsole.MarkupLine($"[dim]  {test.ErrorMessage}[/]");
                }
            }
        }
    }

    private enum TestStatus
    {
        Passed,
        Failed,
        Skipped
    }

    private record TestResult(string Name, TestStatus Status, TimeSpan Duration, string? Category, string? ErrorMessage = null);
}
