using System.Diagnostics;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Exporters;
using Spectre.Console;

namespace DocToolkit.Tests.Infrastructure;

/// <summary>
/// Custom benchmark runner with Spectre.Console output.
/// </summary>
public static class BenchmarkRunner
{
    public static void RunBenchmarks<T>() where T : class
    {
        AnsiConsole.Write(new FigletText("Benchmarks").Color(Color.Blue));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]Running benchmarks for:[/] [cyan]{typeof(T).Name}[/]");
        AnsiConsole.WriteLine();

        var stopwatch = Stopwatch.StartNew();
        var summary = BenchmarkDotNet.Running.BenchmarkRunner.Run<T>();
        stopwatch.Stop();

        PrintSummary(summary, stopwatch.Elapsed);
    }

    public static void RunAllBenchmarks()
    {
        AnsiConsole.Write(new FigletText("Benchmarks").Color(Color.Blue));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Running all benchmarks...[/]");
        AnsiConsole.WriteLine();

        var stopwatch = Stopwatch.StartNew();
        
        var summaries = new List<Summary>();
        
        // Run each benchmark class
        summaries.Add(BenchmarkDotNet.Running.BenchmarkRunner.Run<DocToolkit.Tests.Benchmarks.DocumentExtractionBenchmarks>());
        summaries.Add(BenchmarkDotNet.Running.BenchmarkRunner.Run<DocToolkit.Tests.Benchmarks.ProjectAccessorBenchmarks>());
        
        stopwatch.Stop();

        PrintAllSummaries(summaries, stopwatch.Elapsed);
    }

    private static void PrintSummary(Summary summary, TimeSpan elapsed)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]Benchmark Results[/]").RuleStyle("blue"));

        if (!summary.Reports.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No benchmarks were executed.[/]");
            return;
        }

        // Create results table
        var table = new Table();
        table.AddColumn("Benchmark");
        table.AddColumn("Mean");
        table.AddColumn("Error");
        table.AddColumn("StdDev");
        table.AddColumn("Gen 0");
        table.AddColumn("Gen 1");
        table.AddColumn("Gen 2");
        table.AddColumn("Allocated");
        table.Border(TableBorder.Rounded);
        table.BorderColor(Color.Blue);

        foreach (var report in summary.Reports.OrderBy(r => r.BenchmarkCase.DisplayInfo))
        {
            var result = report.ResultStatistics;
            if (result == null) continue;

            var mean = FormatTime(result.Mean);
            var error = FormatTime(result.StandardError);
            var stdDev = FormatTime(result.StandardDeviation);
            
            var gen0 = report.GcStats.Gen0Collections > 0 ? report.GcStats.Gen0Collections.ToString() : "-";
            var gen1 = report.GcStats.Gen1Collections > 0 ? report.GcStats.Gen1Collections.ToString() : "-";
            var gen2 = report.GcStats.Gen2Collections > 0 ? report.GcStats.Gen2Collections.ToString() : "-";
            
            // Get allocated bytes from metrics (MemoryDiagnoser provides this)
            string allocated = "-";
            if (report.Metrics != null)
            {
                // Metrics is a dictionary where key is metric ID and value is Metric object
                var allocatedMetric = report.Metrics.FirstOrDefault(m => 
                    m.Key == "Allocated" || 
                    m.Key.Contains("Allocated") ||
                    m.Value.Descriptor.DisplayName.Contains("Allocated"));
                if (allocatedMetric.Value != null)
                {
                    var bytes = Convert.ToUInt64(allocatedMetric.Value.Value);
                    allocated = FormatBytes(bytes);
                }
            }

            table.AddRow(
                $"[cyan]{report.BenchmarkCase.DisplayInfo}[/]",
                $"[green]{mean}[/]",
                $"[dim]{error}[/]",
                $"[dim]{stdDev}[/]",
                gen0,
                gen1,
                gen2,
                allocated
            );
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"[dim]Total time: {elapsed.TotalSeconds:F2}s[/]");
    }

    private static void PrintAllSummaries(List<Summary> summaries, TimeSpan elapsed)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[bold]All Benchmark Results[/]").RuleStyle("blue"));

        foreach (var summary in summaries)
        {
            if (!summary.Reports.Any()) continue;

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold cyan]{summary.Title}[/]");
            AnsiConsole.WriteLine();

            var table = new Table();
            table.AddColumn("Benchmark");
            table.AddColumn("Mean");
            table.AddColumn("Error");
            table.AddColumn("StdDev");
            table.AddColumn("Allocated");
            table.Border(TableBorder.Rounded);
            table.BorderColor(Color.Blue);

            foreach (var report in summary.Reports.OrderBy(r => r.BenchmarkCase.DisplayInfo))
            {
                var result = report.ResultStatistics;
                if (result == null) continue;

                var mean = FormatTime(result.Mean);
                var error = FormatTime(result.StandardError);
                var stdDev = FormatTime(result.StandardDeviation);
                
                // Get allocated bytes from metrics (MemoryDiagnoser provides this)
                string allocated = "-";
                if (report.Metrics != null)
                {
                    // Metrics is a dictionary where key is metric ID and value is Metric object
                    var allocatedMetric = report.Metrics.FirstOrDefault(m => 
                        m.Key == "Allocated" || 
                        m.Key.Contains("Allocated") ||
                        m.Value.Descriptor.DisplayName.Contains("Allocated"));
                    if (allocatedMetric.Value != null)
                    {
                        var bytes = Convert.ToUInt64(allocatedMetric.Value.Value);
                        allocated = FormatBytes(bytes);
                    }
                }

                table.AddRow(
                    $"[cyan]{report.BenchmarkCase.DisplayInfo}[/]",
                    $"[green]{mean}[/]",
                    $"[dim]{error}[/]",
                    $"[dim]{stdDev}[/]",
                    allocated
                );
            }

            AnsiConsole.Write(table);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[dim]Total time: {elapsed.TotalSeconds:F2}s[/]");
    }

    private static string FormatTime(double nanoseconds)
    {
        if (nanoseconds < 1000)
            return $"{nanoseconds:F2} ns";
        if (nanoseconds < 1_000_000)
            return $"{nanoseconds / 1000:F2} μs";
        if (nanoseconds < 1_000_000_000)
            return $"{nanoseconds / 1_000_000:F2} ms";
        return $"{nanoseconds / 1_000_000_000:F2} s";
    }

    private static string FormatBytes(ulong bytes)
    {
        if (bytes == 0) return "-";
        if (bytes < 1024)
            return $"{bytes} B";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F2} KB";
        return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }
}
