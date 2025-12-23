using System.Text;
using System.Text.Json;
using DocToolkit.Models;
using DocToolkit.Interfaces.Managers;
using DocToolkit.Interfaces.Engines;

namespace DocToolkit.Managers;

/// <summary>
/// Manager for knowledge graph generation workflow.
/// Encapsulates workflow volatility: graph generation orchestration could change.
/// </summary>
/// <remarks>
/// Component Type: Manager (Workflow Volatility)
/// Volatility: Graph generation workflow and orchestration
/// Pattern: Orchestrates Engines and Accessors - knows "when", not "how"
/// </remarks>
public class KnowledgeGraphManager : IKnowledgeGraphManager
{
    private readonly IDocumentExtractionEngine _extractor;
    private readonly IEntityExtractionEngine _extractionEngine;

    /// <summary>
    /// Initializes a new instance of the KnowledgeGraphManager.
    /// </summary>
    /// <param name="extractor">Document extraction engine</param>
    /// <param name="extractionEngine">Entity extraction engine</param>
    public KnowledgeGraphManager(
        IDocumentExtractionEngine extractor,
        IEntityExtractionEngine extractionEngine)
    {
        _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        _extractionEngine = extractionEngine ?? throw new ArgumentNullException(nameof(extractionEngine));
    }

    /// <summary>
    /// Builds a knowledge graph from source files.
    /// </summary>
    /// <param name="sourcePath">Source directory path</param>
    /// <param name="outputPath">Output directory path</param>
    /// <param name="progressCallback">Progress callback (0-100)</param>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// Service Boundary: Called by GraphCommand (Client)
    /// Orchestrates: DocumentExtractionEngine (Engine), EntityExtractionEngine (Engine)
    /// Authentication: None (local CLI tool)
    /// Authorization: None (local CLI tool)
    /// Transaction: None (file-based operations)
    /// </remarks>
    public bool BuildGraph(string sourcePath, string outputPath, Action<double>? progressCallback = null)
    {
        if (!Directory.Exists(sourcePath))
        {
            return false;
        }

        var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();
        var totalFiles = files.Count;
        var processedFiles = 0;

        var nodes = new GraphNodes();
        var edges = new List<GraphEdge>();
        var fileTexts = new Dictionary<string, string>();
        var entityGlobalCounts = new Dictionary<string, int>();
        var topicGlobalCounts = new Dictionary<string, int>();

        // Process each file
        foreach (var file in files)
        {
            try
            {
                // Orchestrate: Extract text (Engine)
                var text = _extractor.ExtractText(file);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                var fileId = $"file:{file}";
                fileTexts[fileId] = text;

                nodes.File.Add(new GraphNode
                {
                    Id = fileId,
                    Type = "file",
                    Name = Path.GetFileName(file),
                    Path = file
                });

                // Orchestrate: Extract entities and topics (Engine)
                var entities = _extractionEngine.ExtractEntities(text);
                var topics = _extractionEngine.ExtractTopics(text, 10);

                // Count entities and topics
                foreach (var entity in entities)
                {
                    entityGlobalCounts.TryGetValue(entity, out var count);
                    entityGlobalCounts[entity] = count + 1;
                }

                foreach (var topic in topics)
                {
                    topicGlobalCounts.TryGetValue(topic, out var count);
                    topicGlobalCounts[topic] = count + 1;
                }

                // Create entity nodes and edges
                foreach (var entity in entities.Distinct())
                {
                    var entityId = $"entity:{entity}";
                    if (!nodes.Entity.Any(e => e.Id == entityId))
                    {
                        nodes.Entity.Add(new GraphNode
                        {
                            Id = entityId,
                            Type = "entity",
                            Name = entity
                        });
                    }

                    edges.Add(new GraphEdge
                    {
                        Type = "FILE_CONTAINS_ENTITY",
                        From = fileId,
                        To = entityId,
                        Weight = entities.Count(e => e == entity)
                    });
                }

                // Create topic nodes and edges
                foreach (var topic in topics.Distinct())
                {
                    var topicId = $"topic:{topic}";
                    if (!nodes.Topic.Any(t => t.Id == topicId))
                    {
                        nodes.Topic.Add(new GraphNode
                        {
                            Id = topicId,
                            Type = "topic",
                            Name = topic
                        });
                    }

                    edges.Add(new GraphEdge
                    {
                        Type = "FILE_CONTAINS_TOPIC",
                        From = fileId,
                        To = topicId,
                        Weight = topics.Count(t => t == topic)
                    });
                }

                processedFiles++;
                progressCallback?.Invoke((double)processedFiles / totalFiles * 100);
            }
            catch
            {
                // Skip files that can't be processed
            }
        }

        // Build entity-entity and entity-topic co-occurrence edges
        foreach (var (fileId, text) in fileTexts)
        {
            var entities = _extractionEngine.ExtractEntities(text).Distinct().ToList();
            var topics = _extractionEngine.ExtractTopics(text, 10).Distinct().ToList();

            // Entity-entity relationships
            for (int i = 0; i < entities.Count; i++)
            {
                for (int j = i + 1; j < entities.Count; j++)
                {
                    edges.Add(new GraphEdge
                    {
                        Type = "ENTITY_RELATED_TO_ENTITY",
                        From = $"entity:{entities[i]}",
                        To = $"entity:{entities[j]}",
                        File = fileId
                    });
                }
            }

            // Entity-topic relationships
            foreach (var entity in entities)
            {
                foreach (var topic in topics)
                {
                    edges.Add(new GraphEdge
                    {
                        Type = "ENTITY_RELATED_TO_TOPIC",
                        From = $"entity:{entity}",
                        To = $"topic:{topic}",
                        File = fileId
                    });
                }
            }
        }

        // Create graph data
        var graph = new GraphData
        {
            Nodes = nodes,
            Edges = edges,
            Stats = new GraphStats
            {
                NumFiles = nodes.File.Count,
                NumEntities = nodes.Entity.Count,
                NumTopics = nodes.Topic.Count
            }
        };

        // Save outputs
        Directory.CreateDirectory(outputPath);
        SaveJson(graph, outputPath);
        SaveGraphviz(graph, outputPath);
        SaveMarkdown(graph, entityGlobalCounts, topicGlobalCounts, outputPath);

        return true;
    }

    private void SaveJson(GraphData graph, string outputPath)
    {
        var jsonPath = Path.Combine(outputPath, "graph.json");
        var json = JsonSerializer.Serialize(graph, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(jsonPath, json);
    }

    private void SaveGraphviz(GraphData graph, string outputPath)
    {
        var gvPath = Path.Combine(outputPath, "graph.gv");
        var sb = new StringBuilder();
        sb.AppendLine("graph G {");

        foreach (var node in graph.Nodes.File)
        {
            sb.AppendLine($"  \"{node.Id}\" [label=\"{node.Name}\" shape=box];");
        }

        foreach (var node in graph.Nodes.Entity)
        {
            sb.AppendLine($"  \"{node.Id}\" [label=\"{node.Name}\" shape=ellipse];");
        }

        foreach (var node in graph.Nodes.Topic)
        {
            sb.AppendLine($"  \"{node.Id}\" [label=\"{node.Name}\" shape=diamond];");
        }

        foreach (var edge in graph.Edges)
        {
            sb.AppendLine($"  \"{edge.From}\" -- \"{edge.To}\" [label=\"{edge.Type}\"];");
        }

        sb.AppendLine("}");
        File.WriteAllText(gvPath, sb.ToString());
    }

    private void SaveMarkdown(
        GraphData graph,
        Dictionary<string, int> entityCounts,
        Dictionary<string, int> topicCounts,
        string outputPath)
    {
        var mdPath = Path.Combine(outputPath, "graph.md");
        var sb = new StringBuilder();
        sb.AppendLine("# Knowledge Graph Summary");
        sb.AppendLine();
        sb.AppendLine($"- Files: {graph.Stats.NumFiles}");
        sb.AppendLine($"- Entities: {graph.Stats.NumEntities}");
        sb.AppendLine($"- Topics: {graph.Stats.NumTopics}");
        sb.AppendLine();

        sb.AppendLine("## Top Entities");
        foreach (var (entity, count) in entityCounts.OrderByDescending(kvp => kvp.Value).Take(20))
        {
            sb.AppendLine($"- {entity} ({count} mentions)");
        }

        sb.AppendLine();
        sb.AppendLine("## Top Topics");
        foreach (var (topic, count) in topicCounts.OrderByDescending(kvp => kvp.Value).Take(20))
        {
            sb.AppendLine($"- {topic} ({count} occurrences)");
        }

        File.WriteAllText(mdPath, sb.ToString());
    }
}
