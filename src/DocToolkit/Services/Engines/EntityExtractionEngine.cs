using System.Text.RegularExpressions;
using DocToolkit.Interfaces.Engines;

namespace DocToolkit.Services.Engines;

/// <summary>
/// Engine for entity and topic extraction from text.
/// Encapsulates algorithm volatility: extraction methods could change (regex, NLP, ML models).
/// </summary>
/// <remarks>
/// Component Type: Engine (Algorithm Volatility)
/// Volatility: Entity/topic extraction algorithm
/// Pattern: Pure function - accepts text, returns entities/topics (no I/O)
/// </remarks>
public class EntityExtractionEngine : IEntityExtractionEngine
{
    /// <summary>
    /// Extracts entities (capitalized multi-word phrases) from text.
    /// </summary>
    /// <param name="text">Text to extract entities from</param>
    /// <returns>List of extracted entities</returns>
    public List<string> ExtractEntities(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        // Simple heuristic: capitalized multi-word phrases
        var pattern = @"\b([A-Z][a-zA-Z0-9]+(?:\s+[A-Z][a-zA-Z0-9]+)*)\b";
        var matches = Regex.Matches(text, pattern);
        var stopWords = new HashSet<string> 
        { 
            "The", "This", "That", "And", "For", "With", "From", 
            "Project", "Customer", "System", "Service" 
        };

        return matches
            .Cast<Match>()
            .Select(m => m.Value.Trim())
            .Where(e => !stopWords.Contains(e) && e.Length > 2)
            .ToList();
    }

    /// <summary>
    /// Extracts topics (frequent meaningful words) from text.
    /// </summary>
    /// <param name="text">Text to extract topics from</param>
    /// <param name="topN">Number of top topics to return</param>
    /// <returns>List of extracted topics</returns>
    public List<string> ExtractTopics(string text, int topN = 10)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new List<string>();
        }

        var words = Regex.Matches(text.ToLower(), @"\b[a-zA-Z]{5,}\b")
            .Cast<Match>()
            .Select(m => m.Value);

        var stopWords = new HashSet<string> 
        { 
            "project", "requirements", "system", "solution", 
            "cloud", "customer", "service", "document", 
            "management", "application" 
        };

        return words
            .Where(w => !stopWords.Contains(w))
            .GroupBy(w => w)
            .OrderByDescending(g => g.Count())
            .Take(topN)
            .Select(g => g.Key)
            .ToList();
    }
}
