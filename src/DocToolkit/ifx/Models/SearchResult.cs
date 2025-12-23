namespace DocToolkit.Models;

public class SearchResult
{
    public string File { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Chunk { get; set; } = string.Empty;
}
