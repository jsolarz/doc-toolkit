using System.Text.Json.Serialization;

namespace DocToolkit.Models;

public class GraphData
{
    [JsonPropertyName("nodes")]
    public GraphNodes Nodes { get; set; } = new();

    [JsonPropertyName("edges")]
    public List<GraphEdge> Edges { get; set; } = new();

    [JsonPropertyName("stats")]
    public GraphStats Stats { get; set; } = new();
}

public class GraphNodes
{
    [JsonPropertyName("file")]
    public List<GraphNode> File { get; set; } = new();

    [JsonPropertyName("entity")]
    public List<GraphNode> Entity { get; set; } = new();

    [JsonPropertyName("topic")]
    public List<GraphNode> Topic { get; set; } = new();
}

public class GraphNode
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("path")]
    public string? Path { get; set; }
}

public class GraphEdge
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("weight")]
    public int? Weight { get; set; }

    [JsonPropertyName("file")]
    public string? File { get; set; }
}

public class GraphStats
{
    [JsonPropertyName("num_files")]
    public int NumFiles { get; set; }

    [JsonPropertyName("num_entities")]
    public int NumEntities { get; set; }

    [JsonPropertyName("num_topics")]
    public int NumTopics { get; set; }
}
