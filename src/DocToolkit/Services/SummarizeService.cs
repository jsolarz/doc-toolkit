using System.Text;

namespace DocToolkit.Services;

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
public class SummarizeService
{
    private readonly DocumentExtractionService _extractor;

    /// <summary>
    /// Initializes a new instance of the SummarizeService.
    /// </summary>
    public SummarizeService()
    {
        _extractor = new DocumentExtractionService();
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
    /// Orchestrates: DocumentExtractionService (Engine)
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
                var content = ExtractText(file);
                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                if (content.Length > maxChars)
                {
                    content = content.Substring(0, maxChars);
                }

                var summary = SummarizeText(content);
                var topics = ExtractTopics(content);
                var entities = ExtractEntities(content);

                output.AppendLine($"## File: {Path.GetFileName(file)}");
                output.AppendLine($"**Path:** {file}");
                output.AppendLine($"**Size:** {new FileInfo(file).Length} bytes");
                output.AppendLine($"**Type:** {Path.GetExtension(file)}");
                output.AppendLine();
                output.AppendLine("### Summary");
                output.AppendLine(summary);
                output.AppendLine();
                output.AppendLine("### Key Topics");
                output.AppendLine(topics);
                output.AppendLine();
                output.AppendLine("### Named Entities");
                output.AppendLine(entities);
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

    private string ExtractText(string filePath)
    {
        return _extractor.ExtractText(filePath);
    }

    private string SummarizeText(string text)
    {
        if (text.Length <= 800)
        {
            return text;
        }

        var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
        var summaryCount = Math.Min(5, sentences.Length);
        return string.Join(" ", sentences.Take(summaryCount)) + ".";
    }

    private string ExtractTopics(string text)
    {
        var words = text.ToLower()
            .Split(new[] { ' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}' }, 
                StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 4)
            .GroupBy(w => w)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key);

        return string.Join(", ", words);
    }

    private string ExtractEntities(string text)
    {
        // Simple heuristic: capitalized multi-word phrases
        var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var entities = new List<string>();
        var stopWords = new HashSet<string> { "The", "This", "That", "And", "For", "With", "From", "Project", "Customer" };

        for (int i = 0; i < words.Length - 1; i++)
        {
            if (char.IsUpper(words[i][0]) && char.IsUpper(words[i + 1][0]))
            {
                var entity = $"{words[i]} {words[i + 1]}";
                if (!stopWords.Contains(words[i]) && !stopWords.Contains(words[i + 1]))
                {
                    entities.Add(entity);
                }
            }
        }

        return string.Join(", ", entities.Distinct().Take(20));
    }
}
