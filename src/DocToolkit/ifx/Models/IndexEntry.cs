using System.Text.Json.Serialization;

namespace DocToolkit.ifx.Models;

public class IndexEntry
{
    [JsonPropertyName("file")]
    public string File { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("chunk")]
    public string Chunk { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public int Index { get; set; }
}
