using DocToolkit.Tests.Infrastructure;
using Spectre.Console;

namespace DocToolkit.Tests;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0 || args[0].ToLowerInvariant() == "test")
        {
            return RunTests();
        }
        
        if (args[0].ToLowerInvariant() == "benchmark" || args[0].ToLowerInvariant() == "bench")
        {
            if (args.Length > 1)
            {
                return RunSpecificBenchmark(args[1]);
            }
            return RunAllBenchmarks();
        }

        ShowHelp();
        return 1;
    }

    private static int RunTests()
    {
        AnsiConsole.Write(new FigletText("Tests").Color(Color.Green));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Note:[/] For best results, run tests using: [cyan]dotnet test[/]");
        AnsiConsole.MarkupLine("[yellow]For detailed output:[/] [cyan]dotnet test --logger \"console;verbosity=detailed\"[/]");
        AnsiConsole.WriteLine();
        
        // Run tests and capture output
        var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "test --logger \"console;verbosity=normal\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        var outputLines = new List<string>();
        var errorLines = new List<string>();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                outputLines.Add(e.Data);
                FormatTestOutput(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                errorLines.Add(e.Data);
                AnsiConsole.MarkupLine($"[red]{e.Data}[/]");
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        // Print summary
        PrintTestSummary(outputLines);

        return process.ExitCode;
    }

    private static void FormatTestOutput(string line)
    {
        // Format xUnit test output
        if (line.Contains("Passed!") || line.Contains("✓"))
        {
            AnsiConsole.MarkupLine($"[green]✓[/] {line.Replace("Passed!", "").Trim()}");
        }
        else if (line.Contains("Failed!") || line.Contains("✗"))
        {
            AnsiConsole.MarkupLine($"[red]✗[/] {line.Replace("Failed!", "").Trim()}");
        }
        else if (line.Contains("Total tests:") || line.Contains("Test Run"))
        {
            AnsiConsole.MarkupLine($"[bold cyan]{line}[/]");
        }
        else if (line.Contains("Failed") || line.Contains("Error"))
        {
            AnsiConsole.MarkupLine($"[red]{line}[/]");
        }
        else
        {
            AnsiConsole.WriteLine(line);
        }
    }

    private static void PrintTestSummary(List<string> outputLines)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]Test Summary[/]").RuleStyle("green"));

        var totalLine = outputLines.FirstOrDefault(l => l.Contains("Total tests:"));
        var passedLine = outputLines.FirstOrDefault(l => l.Contains("Passed!"));
        var failedLine = outputLines.FirstOrDefault(l => l.Contains("Failed!"));

        if (totalLine != null || passedLine != null || failedLine != null)
        {
            var table = new Table();
            table.AddColumn("Metric");
            table.AddColumn("Value");
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Green);

            if (totalLine != null)
            {
                var match = System.Text.RegularExpressions.Regex.Match(totalLine, @"Total tests:\s*(\d+)");
                if (match.Success)
                {
                    table.AddRow("[bold]Total Tests[/]", $"[cyan]{match.Groups[1].Value}[/]");
                }
            }

            if (passedLine != null)
            {
                var match = System.Text.RegularExpressions.Regex.Match(passedLine, @"Passed!\s*(\d+)");
                if (match.Success)
                {
                    table.AddRow("[green]Passed[/]", $"[green]{match.Groups[1].Value}[/]");
                }
            }

            if (failedLine != null)
            {
                var match = System.Text.RegularExpressions.Regex.Match(failedLine, @"Failed!\s*(\d+)");
                if (match.Success)
                {
                    table.AddRow("[red]Failed[/]", $"[red]{match.Groups[1].Value}[/]");
                }
            }

            AnsiConsole.Write(table);
        }
    }

    private static int RunAllBenchmarks()
    {
        try
        {
            BenchmarkRunner.RunAllBenchmarks();
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private static int RunSpecificBenchmark(string benchmarkName)
    {
        try
        {
            var type = benchmarkName.ToLowerInvariant() switch
            {
                "textchunking" or "chunking" => typeof(DocToolkit.Tests.Benchmarks.TextChunkingBenchmarks),
                "similarity" => typeof(DocToolkit.Tests.Benchmarks.SimilarityBenchmarks),
                "entity" or "entityextraction" => typeof(DocToolkit.Tests.Benchmarks.EntityExtractionBenchmarks),
                "summarization" or "summarize" => typeof(DocToolkit.Tests.Benchmarks.SummarizationBenchmarks),
                _ => null
            };

            if (type == null)
            {
                AnsiConsole.MarkupLine($"[red]Unknown benchmark: {benchmarkName}[/]");
                ShowBenchmarkHelp();
                return 1;
            }

            var method = typeof(BenchmarkRunner).GetMethod(nameof(BenchmarkRunner.RunBenchmarks))!
                .MakeGenericMethod(type);
            method.Invoke(null, null);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    private static void ShowHelp()
    {
        AnsiConsole.Write(new FigletText("DocToolkit Tests").Color(Color.Blue));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Usage:[/]");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("  [cyan]dotnet run -- test[/]          Run all tests");
        AnsiConsole.MarkupLine("  [cyan]dotnet run -- benchmark[/]     Run all benchmarks");
        AnsiConsole.MarkupLine("  [cyan]dotnet run -- bench <name>[/]   Run specific benchmark");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Available Benchmarks:[/]");
        AnsiConsole.MarkupLine("  [cyan]textchunking[/]  - Text chunking performance");
        AnsiConsole.MarkupLine("  [cyan]similarity[/]   - Similarity calculation performance");
        AnsiConsole.MarkupLine("  [cyan]entity[/]       - Entity extraction performance");
        AnsiConsole.MarkupLine("  [cyan]summarization[/] - Summarization performance");
    }

    private static void ShowBenchmarkHelp()
    {
        AnsiConsole.MarkupLine("[bold]Available benchmarks:[/]");
        AnsiConsole.MarkupLine("  [cyan]textchunking[/]  - Text chunking performance");
        AnsiConsole.MarkupLine("  [cyan]similarity[/]   - Similarity calculation performance");
        AnsiConsole.MarkupLine("  [cyan]entity[/]       - Entity extraction performance");
        AnsiConsole.MarkupLine("  [cyan]summarization[/] - Summarization performance");
    }
}
