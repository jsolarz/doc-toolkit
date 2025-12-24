using System.Diagnostics;
using Spectre.Console;

namespace DocToolkit.ifx.Infrastructure;

/// <summary>
/// Memory monitoring utility for tracking memory usage during operations.
/// </summary>
/// <remarks>
/// <para>
/// The MemoryMonitor class provides real-time tracking of managed memory usage, garbage collection statistics,
/// and elapsed time during application operations. It is designed to help verify the effectiveness of memory
/// optimizations and identify potential memory leaks or excessive allocations.
/// </para>
/// <para>
/// When enabled, the monitor performs a forced garbage collection before starting to establish an accurate
/// baseline. This ensures that memory measurements reflect actual allocations during the operation rather
/// than pre-existing allocations.
/// </para>
/// <para>
/// The class implements IDisposable to automatically display final statistics when the monitoring period ends.
/// Use the <c>using</c> statement to ensure proper disposal.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using var monitor = new MemoryMonitor("Indexing", enabled: true);
/// monitor.DisplayStats("Initial");
/// 
/// // Perform operation...
/// 
/// monitor.DisplaySummary(); // Compact update during operation
/// // Dispose automatically displays final stats
/// </code>
/// </example>
public class MemoryMonitor : IDisposable
{
    /// <summary>
    /// Indicates whether memory monitoring is enabled.
    /// </summary>
    private readonly bool _enabled;

    /// <summary>
    /// Initial memory usage in bytes, captured after forced GC at construction.
    /// </summary>
    private readonly long _initialMemory;

    /// <summary>
    /// Stopwatch for tracking elapsed time since monitoring started.
    /// </summary>
    private readonly Stopwatch _stopwatch;

    /// <summary>
    /// Name of the operation being monitored, used in display titles.
    /// </summary>
    private readonly string _operationName;

    /// <summary>
    /// Initializes a new instance of the MemoryMonitor class.
    /// </summary>
    /// <param name="operationName">Name of the operation being monitored (e.g., "Indexing", "Search"). Used in display titles.</param>
    /// <param name="enabled">Whether memory monitoring is enabled. When false, all monitoring operations are no-ops.</param>
    /// <remarks>
    /// <para>
    /// When <paramref name="enabled"/> is true, the constructor performs a forced garbage collection
    /// to establish an accurate baseline. This involves:
    /// </para>
    /// <list type="number">
    /// <item>Forcing a full GC collection across all generations</item>
    /// <item>Waiting for pending finalizers to complete</item>
    /// <item>Performing a second full GC collection to ensure cleanup</item>
    /// <item>Capturing the total managed memory as the baseline</item>
    /// </list>
    /// <para>
    /// This baseline is used to calculate memory deltas throughout the monitoring period.
    /// </para>
    /// </remarks>
    public MemoryMonitor(string operationName, bool enabled = true)
    {
        _operationName = operationName ?? throw new ArgumentNullException(nameof(operationName));
        _enabled = enabled;
        _stopwatch = Stopwatch.StartNew();

        if (_enabled)
        {
            // Force a full garbage collection to establish an accurate baseline.
            // This ensures that memory measurements reflect actual allocations during
            // the operation rather than pre-existing allocations.
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
            // Second collection ensures all finalizable objects are collected
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            _initialMemory = GC.GetTotalMemory(false);
        }
        else
        {
            _initialMemory = 0;
        }
    }

    /// <summary>
    /// Gets the current total managed memory usage in bytes.
    /// </summary>
    /// <value>
    /// The current total managed memory allocated by the .NET runtime, measured using
    /// <see cref="GC.GetTotalMemory(bool)"/> with <c>false</c> to avoid triggering a collection.
    /// </value>
    /// <remarks>
    /// This property does not trigger a garbage collection. For accurate measurements,
    /// consider calling <see cref="ForceGC"/> before accessing this property.
    /// </remarks>
    public long CurrentMemory => GC.GetTotalMemory(false);

    /// <summary>
    /// Gets the memory delta (change) since monitoring started.
    /// </summary>
    /// <value>
    /// The difference between current memory and initial memory. Positive values indicate
    /// memory growth, negative values indicate memory freed.
    /// </value>
    /// <remarks>
    /// This value is calculated as <see cref="CurrentMemory"/> - <see cref="_initialMemory"/>.
    /// A positive delta indicates that the operation has allocated additional memory.
    /// </remarks>
    public long MemoryDelta => CurrentMemory - _initialMemory;

    /// <summary>
    /// Gets the elapsed time since monitoring started.
    /// </summary>
    /// <value>
    /// A <see cref="TimeSpan"/> representing the elapsed time since the MemoryMonitor
    /// was constructed and the stopwatch started.
    /// </value>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Displays a detailed table of current memory statistics.
    /// </summary>
    /// <param name="label">
    /// Optional label for the statistics table. If null, uses the operation name
    /// provided during construction. Used in the table title.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method displays a formatted table containing:
    /// </para>
    /// <list type="bullet">
    /// <item>Current Memory: Total managed memory currently allocated</item>
    /// <item>Memory Delta: Change in memory since monitoring started (with sign indicator)</item>
    /// <item>Elapsed Time: Time since monitoring started (in seconds)</item>
    /// <item>GC Gen 0: Number of generation 0 garbage collections</item>
    /// <item>GC Gen 1: Number of generation 1 garbage collections</item>
    /// <item>GC Gen 2: Number of generation 2 garbage collections</item>
    /// </list>
    /// <para>
    /// If monitoring is disabled, this method returns immediately without displaying anything.
    /// </para>
    /// </remarks>
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
    /// Displays a compact one-line memory summary suitable for progress updates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method displays a single line containing:
    /// </para>
    /// <list type="bullet">
    /// <item>Memory Delta: Formatted with color (yellow for positive, green for negative)</item>
    /// <item>Elapsed Time: In seconds with 2 decimal places</item>
    /// <item>GC Collections: Counts for Gen 0, Gen 1, and Gen 2 (separated by slashes)</item>
    /// </list>
    /// <para>
    /// The format is: <c>Memory: [+/-]X.XX MB | Time: X.XXs | GC: X/Y/Z</c>
    /// </para>
    /// <para>
    /// This method is designed to be called periodically during long-running operations
    /// to provide progress updates without cluttering the console with full tables.
    /// </para>
    /// <para>
    /// If monitoring is disabled, this method returns immediately without displaying anything.
    /// </para>
    /// </remarks>
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
    /// Formats a byte count into a human-readable string with appropriate unit (B, KB, MB, GB, TB).
    /// </summary>
    /// <param name="bytes">The number of bytes to format. Can be negative.</param>
    /// <param name="showSign">
    /// Whether to include a sign prefix (+ for positive, - for negative).
    /// Default is false (no sign).
    /// </param>
    /// <returns>
    /// A formatted string representing the byte count in the most appropriate unit,
    /// with 2 decimal places. Examples: "1.23 MB", "+45.67 KB", "-123.45 B"
    /// </returns>
    /// <remarks>
    /// <para>
    /// The method automatically selects the most appropriate unit by dividing by 1024
    /// until the value is less than 1024 or the maximum unit (TB) is reached.
    /// </para>
    /// <para>
    /// Units used: B (bytes), KB (kilobytes), MB (megabytes), GB (gigabytes), TB (terabytes).
    /// All conversions use base 1024 (binary) rather than base 1000 (decimal).
    /// </para>
    /// </remarks>
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
    /// Forces a full garbage collection across all generations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method performs a forced garbage collection to ensure accurate memory measurements.
    /// It follows the same pattern as the constructor:
    /// </para>
    /// <list type="number">
    /// <item>Forces a full GC collection across all generations</item>
    /// <item>Waits for pending finalizers to complete</item>
    /// <item>Performs a second full GC collection</item>
    /// </list>
    /// <para>
    /// Use this method when you need to measure memory after an operation completes,
    /// ensuring that all eligible objects are collected before measurement.
    /// </para>
    /// <para>
    /// Note: Forcing garbage collection can impact performance and should be used sparingly.
    /// The .NET runtime's automatic GC is generally sufficient for most scenarios.
    /// </para>
    /// <para>
    /// If monitoring is disabled, this method returns immediately without performing any operations.
    /// </para>
    /// </remarks>
    public void ForceGC()
    {
        if (!_enabled) return;

        // Force full collection across all generations
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        // Wait for finalizers to complete
        GC.WaitForPendingFinalizers();
        // Second collection ensures all finalizable objects are collected
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
    }

    /// <summary>
    /// Disposes the MemoryMonitor instance and displays final statistics.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When disposed, the MemoryMonitor:
    /// </para>
    /// <list type="number">
    /// <item>Stops the elapsed time stopwatch</item>
    /// <item>If enabled, forces a garbage collection for accurate final measurement</item>
    /// <item>If enabled, displays final statistics with the label "Final"</item>
    /// </list>
    /// <para>
    /// This method is automatically called when using the <c>using</c> statement pattern.
    /// It ensures that final statistics are always displayed, even if an exception occurs.
    /// </para>
    /// </remarks>
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
