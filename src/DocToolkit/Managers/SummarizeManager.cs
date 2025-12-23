using System.Text;
using DocToolkit.Interfaces.Managers;
using DocToolkit.Interfaces.Engines;

namespace DocToolkit.Managers;

/// <summary>
/// Manager for document summarization workflow.
/// Encapsulates workflow volatility: summarization orchestration could change.
/// </summary>
/// <remarks>
/// Component Type: Manager (Workflow Volatility)
/// Volatility: Summarization workflow and orchestration
/// Pattern: Orchestrates Engines and Accessors
/// Service Boundary: Called by SummarizeCommand (Client)
/// </remarks>
public class SummarizeManager : ISummarizeManager
{
    private readonly IDocumentExtractionEngine _extractor;
    private readonly ISummarizationEngine _summarizationEngine;
    private readonly IEntityExtractionEngine _entityExtractionEngine;

    /// <summary>
    /// Initializes a new instance of the SummarizeManager.
    /// </summary>
    /// <param name="extractor">Document extraction engine</param>
    /// <param name="summarizationEngine">Summarization engine</param>
    /// <param name="entityExtractionEngine">Entity extraction engine</param>
    public SummarizeManager(
        IDocumentExtractionEngine extractor,
        ISummarizationEngine summarizationEngine,
        IEntityExtractionEngine entityExtractionEngine)
    {
        _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        _summarizationEngine = summarizationEngine ?? throw new ArgumentNullException(nameof(summarizationEngine));
        _entityExtractionEngine = entityExtractionEngine ?? throw new ArgumentNullException(nameof(entityExtractionEngine));
    }

    /// <summary>
    /// Summarizes source files into a context document.
    /// </summary>
    /// <param name="sourcePath">Source directory path</param>
    /// <param name="outputFile">Output file path</param>
    /// <param name="maxChars">Maximum characters per file to include</param>
    /// <param name="progressCallback">Progress callback (0-100)</param>
    /// <returns>True if successful</returns>
    /// <remarks>
    /// Service Boundary: Called by SummarizeCommand (Client)
    /// Orchestrates: DocumentExtractionEngine (Engine), SummarizationEngine (Engine), 
    ///                EntityExtractionEngine (Engine)
    /// Authentication: None (local CLI tool)
    /// Authorization: None (local CLI tool)
    /// Transaction: None (file-based operations)
    /// </remarks>
    public bool SummarizeSource(string sourcePath, string outputFile, int maxChars, Action<double>? progressCallback = null)
    {
        if (!Directory.Exists(sourcePath))
        {
            return false;
        }

        var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories).ToList();
        var totalFiles = files.Count;
        var processed = 0;

        var output = new StringBuilder();
        output.AppendLine("# Project Context Summary");
        output.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
        output.AppendLine($"Source Folder: {sourcePath}");
        output.AppendLine();
        output.AppendLine("This file contains:");
        output.AppendLine("- Extracted text from all source materials");
        output.AppendLine("- Summaries");
        output.AppendLine("- Key topics");
        output.AppendLine("- Named entities");
        output.AppendLine();
        output.AppendLine("---");
        output.AppendLine();

        foreach (var file in files)
        {
            try
            {
                // Orchestrate: Extract text (Engine)
                var content = _extractor.ExtractText(file);
                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                if (content.Length > maxChars)
                {
                    content = content.Substring(0, maxChars);
                }

                // Orchestrate: Summarize text (Engine)
                var summary = _summarizationEngine.SummarizeText(content);
                
                // Orchestrate: Extract topics and entities (Engine)
                var topics = _entityExtractionEngine.ExtractTopics(content, 10);
                var entities = _entityExtractionEngine.ExtractEntities(content);

                output.AppendLine($"## File: {Path.GetFileName(file)}");
                output.AppendLine($"**Path:** {file}");
                output.AppendLine($"**Size:** {new FileInfo(file).Length} bytes");
                output.AppendLine($"**Type:** {Path.GetExtension(file)}");
                output.AppendLine();
                output.AppendLine("### Summary");
                output.AppendLine(summary);
                output.AppendLine();
                output.AppendLine("### Key Topics");
                output.AppendLine(string.Join(", ", topics));
                output.AppendLine();
                output.AppendLine("### Named Entities");
                output.AppendLine(string.Join(", ", entities.Take(20)));
                output.AppendLine();
                output.AppendLine("### Extracted Text");
                output.AppendLine("```");
                output.AppendLine(content);
                output.AppendLine("```");
                output.AppendLine();
                output.AppendLine("---");
                output.AppendLine();

                processed++;
                progressCallback?.Invoke((double)processed / totalFiles * 100);
            }
            catch
            {
                // Skip files that can't be processed
            }
        }

        var outputDir = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrEmpty(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        File.WriteAllText(outputFile, output.ToString());
        return true;
    }
}
