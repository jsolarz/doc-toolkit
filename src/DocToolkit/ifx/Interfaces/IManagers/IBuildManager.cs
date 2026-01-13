namespace DocToolkit.ifx.Interfaces.IManagers;

/// <summary>
/// Manager for build workflow orchestration.
/// Encapsulates workflow volatility: build process could change (different steps, validation, deployment).
/// </summary>
public interface IBuildManager
{
    /// <summary>
    /// Builds a static site from markdown files.
    /// </summary>
    /// <param name="sourcePath">Source directory containing markdown files</param>
    /// <param name="outputPath">Output directory for compiled site</param>
    /// <param name="options">Build options</param>
    /// <param name="progressCallback">Progress callback (0-100)</param>
    /// <returns>True if build succeeded</returns>
    bool BuildSite(string sourcePath, string outputPath, BuildManagerOptions? options = null, Action<double>? progressCallback = null);
}

/// <summary>
/// Build options for the build manager.
/// </summary>
public class BuildManagerOptions
{
    public bool ValidateLinks { get; set; } = true;
    public bool GenerateNavigation { get; set; } = true;
    public bool GenerateIndex { get; set; } = true;
    public bool MinifyHtml { get; set; } = false;
}
