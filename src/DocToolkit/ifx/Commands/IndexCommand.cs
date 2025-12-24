using System.ComponentModel;
using DocToolkit.ifx.Infrastructure;
using DocToolkit.ifx.Interfaces.IManagers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DocToolkit.ifx.Commands;

/// <summary>
/// Command for building semantic indexes from source files.
/// </summary>
/// <remarks>
/// <para>
/// The IndexCommand encapsulates the UI volatility of indexing operations. It provides
/// a CLI interface for building semantic indexes, which involves extracting text from
/// documents, chunking the text, generating embeddings, and storing vectors.
/// </para>
/// <para>
/// Component Type: Client (UI Volatility)
/// Volatility: CLI interface and user interaction
/// Pattern: Initiates workflow by calling Manager - knows "what" user wants, not "how"
/// </para>
/// <para>
/// This command supports memory monitoring via the <c>--monitor-memory</c> flag to
/// track memory usage during indexing operations, useful for verifying optimization
/// effectiveness.
/// </para>
/// </remarks>
public sealed class IndexCommand : Command<IndexCommand.Settings>
{
    private readonly ISemanticIndexManager _indexManager;

    /// <summary>
    /// Initializes a new instance of the IndexCommand class.
    /// </summary>
    /// <param name="indexManager">
    /// The semantic index manager used to orchestrate the indexing workflow.
    /// Must not be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="indexManager"/> is null.
    /// </exception>
    public IndexCommand(ISemanticIndexManager indexManager)
    {
        _indexManager = indexManager ?? throw new ArgumentNullException(nameof(indexManager));
    }

    /// <summary>
    /// Settings for the index command, defining command-line options and their default values.
    /// </summary>
    public sealed class Settings : CommandSettings
    {
        /// <summary>
        /// Gets or sets the source directory containing files to index.
        /// </summary>
        /// <value>
        /// The path to the source directory. Default is <c>./source</c>.
        /// </value>
        [Description("Source directory (default: ./source)")]
        [CommandOption("-s|--source")]
        [DefaultValue("./source")]
        public string SourcePath { get; init; } = "./source";

        /// <summary>
        /// Gets or sets the output directory where the semantic index will be created.
        /// </summary>
        /// <value>
        /// The path to the output directory. Default is <c>./semantic-index</c>.
        /// The directory will be created if it does not exist.
        /// </value>
        [Description("Index output directory (default: ./semantic-index)")]
        [CommandOption("-o|--output")]
        [DefaultValue("./semantic-index")]
        public string OutputPath { get; init; } = "./semantic-index";

        /// <summary>
        /// Gets or sets the chunk size in words for text chunking.
        /// </summary>
        /// <value>
        /// The number of words per chunk. Default is <c>800</c>.
        /// Larger chunks provide more context but may exceed embedding model limits.
        /// </value>
        [Description("Chunk size in words (default: 800)")]
        [CommandOption("--chunk-size")]
        [DefaultValue(800)]
        public int ChunkSize { get; init; } = 800;

        /// <summary>
        /// Gets or sets the chunk overlap in words for text chunking.
        /// </summary>
        /// <value>
        /// The number of words to overlap between consecutive chunks. Default is <c>200</c>.
        /// Overlap helps maintain context across chunk boundaries.
        /// </value>
        [Description("Chunk overlap in words (default: 200)")]
        [CommandOption("--chunk-overlap")]
        [DefaultValue(200)]
        public int ChunkOverlap { get; init; } = 200;

        /// <summary>
        /// Gets or sets a value indicating whether to monitor memory usage during indexing.
        /// </summary>
        /// <value>
        /// <c>true</c> to enable memory monitoring; otherwise, <c>false</c>. Default is <c>false</c>.
        /// When enabled, displays memory statistics at start, during progress (every 10%), and at completion.
        /// </value>
        [Description("Monitor memory usage during indexing")]
        [CommandOption("--monitor-memory")]
        [DefaultValue(false)]
        public bool MonitorMemory { get; init; } = false;
    }

    /// <summary>
    /// Executes the index command to build a semantic index from source files.
    /// </summary>
    /// <param name="context">The command context provided by Spectre.Console.Cli.</param>
    /// <param name="settings">The command settings containing source path, output path, and other options.</param>
    /// <returns>
    /// Returns <c>0</c> if the index was built successfully; otherwise, returns <c>1</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method:
    /// </para>
    /// <list type="number">
    /// <item>Validates that the source directory exists</item>
    /// <item>Initializes memory monitoring if enabled</item>
    /// <item>Creates a progress display using Spectre.Console</item>
    /// <item>Calls the semantic index manager to build the index</item>
    /// <item>Displays success or error messages</item>
    /// </list>
    /// <para>
    /// If memory monitoring is enabled, statistics are displayed:
    /// </para>
    /// <list type="bullet">
    /// <item>At the start (initial baseline)</item>
    /// <item>Every 10% progress (compact summary)</item>
    /// <item>At completion (final statistics)</item>
    /// </list>
    /// </remarks>
    public override int Execute(CommandContext context, Settings settings)
    {
        // Validate source directory exists
        if (!Directory.Exists(settings.SourcePath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] Source directory not found: {settings.SourcePath}");
            return 1;
        }

        AnsiConsole.MarkupLine($"[cyan]Building semantic index from:[/] {settings.SourcePath}");

        // Initialize memory monitoring if enabled
        using var memoryMonitor = new MemoryMonitor("Indexing", settings.MonitorMemory);
        
        if (settings.MonitorMemory)
        {
            memoryMonitor.DisplayStats("Initial");
        }

        // Create progress display with multiple columns
        var progress = AnsiConsole.Progress();
        progress.Columns(
            new ProgressColumn[] {
                new TaskDescriptionColumn(),    // Task description text
                new ProgressBarColumn(),        // Visual progress bar
                new PercentageColumn(),         // Percentage complete
                new SpinnerColumn()            // Animated spinner
            }
        );

        bool result;
        // Start progress display and build index
        result = progress.Start(ctx =>
        {
            var task = ctx.AddTask("[green]Processing files...[/]");
            return _indexManager.BuildIndex(
                    settings.SourcePath,
                    settings.OutputPath,
                    settings.ChunkSize,
                    settings.ChunkOverlap,
                    progress =>
                    {
                        // Update progress bar
                        task.Increment(progress);
                        
                        // Display memory summary every 10% progress if monitoring is enabled
                        if (settings.MonitorMemory && progress % 10 == 0)
                        {
                            memoryMonitor.DisplaySummary();
                        }
                    }
                );
        });

        if (result)
        {
            var panel = new Panel(
                new Rows(
                    new Text($"[green]✓[/] Semantic index created at: [bold]{settings.OutputPath}[/]"),
                    new Text(""),
                    new Text("[dim]Files created:[/]"),
                    new Text($"  • {Path.Combine(settings.OutputPath, "vectors.bin")}"),
                    new Text($"  • {Path.Combine(settings.OutputPath, "index.json")}")
                ))
            {
                Header = new PanelHeader("Index Complete", Justify.Left),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.Green)
            };

            AnsiConsole.Write(panel);
            return 0;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Failed to build semantic index");
            return 1;
        }
    }
}
