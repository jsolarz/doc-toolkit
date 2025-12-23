namespace DocToolkit.Events;

/// <summary>
/// Event published when a summary document is created.
/// </summary>
public class SummaryCreatedEvent : BaseEvent
{
    /// <summary>
    /// Path to the summary output file.
    /// </summary>
    public string SummaryPath { get; set; } = string.Empty;

    /// <summary>
    /// Number of files summarized.
    /// </summary>
    public int FileCount { get; set; }

    /// <summary>
    /// Source directory that was summarized.
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;
}
