using DocToolkit.Models;

namespace DocToolkit.Interfaces.Managers;

/// <summary>
/// Manager interface for semantic search workflow.
/// </summary>
public interface ISemanticSearchManager : IDisposable
{
    /// <summary>
    /// Searches the semantic index for similar content.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="indexPath">Index directory path</param>
    /// <param name="topK">Number of top results to return</param>
    /// <returns>List of search results sorted by similarity score</returns>
    List<SearchResult> Search(string query, string indexPath, int topK);
}
