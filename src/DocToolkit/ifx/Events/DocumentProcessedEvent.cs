namespace DocToolkit.Events;

/// <summary>
/// Event published when a document is processed.
/// </summary>
public class DocumentProcessedEvent : BaseEvent
{
    /// <summary>
    /// Path to the processed file.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Extracted text content.
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// File extension/type.
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Number of characters extracted.
    /// </summary>
    public int CharacterCount { get; set; }
}
