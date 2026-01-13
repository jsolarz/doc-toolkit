namespace DocToolkit.ifx.Interfaces.IEngines;

/// <summary>
/// Engine for building static sites from markdown files.
/// Encapsulates algorithm volatility: markdown compilation could change (different processors, rendering options).
/// </summary>
public interface IBuildEngine
{
    /// <summary>
    /// Compiles markdown content to HTML.
    /// </summary>
    /// <param name="markdown">Markdown content</param>
    /// <param name="options">Build options</param>
    /// <returns>Compiled HTML</returns>
    string CompileMarkdown(string markdown, BuildEngineOptions? options = null);

    /// <summary>
    /// Builds a static site from markdown files.
    /// </summary>
    /// <param name="sourcePath">Source directory containing markdown files</param>
    /// <param name="outputPath">Output directory for compiled HTML</param>
    /// <param name="options">Build options</param>
    /// <returns>Build result with statistics</returns>
    BuildResult BuildSite(string sourcePath, string outputPath, BuildEngineOptions? options = null);
}

/// <summary>
/// Build options for compilation.
/// </summary>
public class BuildEngineOptions
{
    public bool GenerateAnchors { get; set; } = true;
    public bool MinifyHtml { get; set; } = false;
    public bool IncludeTableOfContents { get; set; } = true;
}

/// <summary>
/// Result of a build operation.
/// </summary>
public class BuildResult
{
    public int FilesProcessed { get; set; }
    public int FilesSkipped { get; set; }
    public int Errors { get; set; }
    public List<string> ErrorMessages { get; set; } = new();
    public TimeSpan ElapsedTime { get; set; }
}
