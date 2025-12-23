namespace DocToolkit.ifx.Events;

/// <summary>
/// Event published when a knowledge graph is built.
/// </summary>
public class GraphBuiltEvent : BaseEvent
{
    /// <summary>
    /// Path to the graph output directory.
    /// </summary>
    public string GraphPath { get; set; } = string.Empty;

    /// <summary>
    /// Number of files processed.
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Number of entities extracted.
    /// </summary>
    public int EntityCount { get; set; }

    /// <summary>
    /// Number of topics extracted.
    /// </summary>
    public int TopicCount { get; set; }

    /// <summary>
    /// Source directory that was processed.
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;
}
