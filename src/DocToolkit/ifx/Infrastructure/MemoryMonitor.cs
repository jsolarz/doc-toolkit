using System.Diagnostics;
using Spectre.Console;

namespace DocToolkit.ifx.Infrastructure;

/// <summary>
/// Memory monitoring utility for tracking memory usage during operations.
/// </summary>
public class MemoryMonitor : IDisposable
{
    private readonly bool _enabled;
    private readonly long _initialMemory;
    private readonly Stopwatch _stopwatch;
    private readonly string _operationName;

    /// <summary>
    /// Initializes a new instance of the MemoryMonitor.
    /// </summary>
    /// <param name="operationName">Name of the operation being monitored</param>
    /// <param name="enabled">Whether memory monitoring is enabled</param>
    public MemoryMonitor(string operationName, bool enabled = true)
    {
        _operationName = operationName;
        _enabled = enabled;
        _stopwatch = Stopwatch.StartNew();
        
        if (_enabled)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            _initialMemory = GC.GetTotalMemory(false);
        }
        else
        {
            _initialMemory = 0;
        }
    }

    /// <summary>
    /// Gets the current memory usage in bytes.
    /// </summary>
    public long CurrentMemory => GC.GetTotalMemory(false);

    /// <summary>
    /// Gets the memory delta since monitoring started.
    /// </summary>
    public long MemoryDelta => CurrentMemory - _initialMemory;

    /// <summary>
    /// Gets the elapsed time since monitoring started.
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Displays current memory statistics.
    /// </summary>
    public void DisplayStats(string? label = null)
    {
        if (!_enabled) return;

        var current = CurrentMemory;
        var delta = MemoryDelta;
        var elapsed = Elapsed;

        var stats = new Table();
        stats.Border(TableBorder.Minimal);
        stats.BorderColor(Color.DarkSlateGray3);
        stats.AddColumn(new TableColumn("[dim]Metric[/]").RightAligned());
        stats.AddColumn(new TableColumn("[dim]Value[/]").LeftAligned());

        var title = label ?? _operationName;
        stats.Title = new TableTitle($"[cyan]Memory Stats: {title}[/]");

        stats.AddRow("[bold]Current Memory[/]", FormatBytes(current));
        stats.AddRow("[bold]Memory Delta[/]", FormatBytes(delta, showSign: true));
        stats.AddRow("[bold]Elapsed Time[/]", $"{elapsed.TotalSeconds:F2}s");
        stats.AddRow("[bold]GC Gen 0[/]", $"{GC.CollectionCount(0)}");
        stats.AddRow("[bold]GC Gen 1[/]", $"{GC.CollectionCount(1)}");
        stats.AddRow("[bold]GC Gen 2[/]", $"{GC.CollectionCount(2)}");

        AnsiConsole.Write(stats);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Displays a compact memory summary.
    /// </summary>
    public void DisplaySummary()
    {
        if (!_enabled) return;

        var delta = MemoryDelta;
        var elapsed = Elapsed;
        var gen0 = GC.CollectionCount(0);
        var gen1 = GC.CollectionCount(1);
        var gen2 = GC.CollectionCount(2);

        var deltaColor = delta > 0 ? "yellow" : "green";
        var deltaText = FormatBytes(Math.Abs(delta), showSign: true);

        AnsiConsole.MarkupLine(
            $"[dim]Memory:[/] [{deltaColor}]{deltaText}[/] | " +
            $"[dim]Time:[/] [cyan]{elapsed.TotalSeconds:F2}s[/] | " +
            $"[dim]GC:[/] [yellow]{gen0}[/]/[yellow]{gen1}[/]/[yellow]{gen2}[/]"
        );
    }

    /// <summary>
    /// Formats bytes into a human-readable string.
    /// </summary>
    private static string FormatBytes(long bytes, bool showSign = false)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = Math.Abs(bytes);
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }

        var sign = showSign ? (bytes >= 0 ? "+" : "-") : "";
        return $"{sign}{len:F2} {sizes[order]}";
    }

    /// <summary>
    /// Forces a garbage collection and updates baseline.
    /// </summary>
    public void ForceGC()
    {
        if (!_enabled) return;
        
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        
        if (_enabled)
        {
            ForceGC();
            DisplayStats("Final");
        }
    }
}
