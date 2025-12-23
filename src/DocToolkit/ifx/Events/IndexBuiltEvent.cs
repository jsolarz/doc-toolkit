namespace DocToolkit.ifx.Events;

/// <summary>
/// Event published when a semantic index is built.
/// </summary>
public class IndexBuiltEvent : BaseEvent
{
    /// <summary>
    /// Path to the index directory.
    /// </summary>
    public string IndexPath { get; set; } = string.Empty;

    /// <summary>
    /// Number of index entries created.
    /// </summary>
    public int EntryCount { get; set; }

    /// <summary>
    /// Number of vectors stored.
    /// </summary>
    public int VectorCount { get; set; }

    /// <summary>
    /// Source directory that was indexed.
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;
}
