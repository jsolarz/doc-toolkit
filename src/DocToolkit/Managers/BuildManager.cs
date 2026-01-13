using DocToolkit.ifx.Events;
using DocToolkit.ifx.Infrastructure;
using DocToolkit.ifx.Interfaces.IEngines;
using DocToolkit.ifx.Interfaces.IManagers;
using Microsoft.Extensions.Logging;

namespace DocToolkit.Managers;

/// <summary>
/// Manager for build workflow orchestration.
/// Encapsulates workflow volatility: build process could change (different steps, validation, deployment).
/// </summary>
public class BuildManager : IBuildManager
{
    private readonly IBuildEngine _buildEngine;
    private readonly ILinkResolver _linkResolver;
    private readonly INavigationGenerator _navigationGenerator;
    private readonly IEventBus _eventBus;
    private readonly ILogger<BuildManager>? _logger;

    public BuildManager(
        IBuildEngine buildEngine,
        ILinkResolver linkResolver,
        INavigationGenerator navigationGenerator,
        IEventBus eventBus,
        ILogger<BuildManager>? logger = null)
    {
        _buildEngine = buildEngine ?? throw new ArgumentNullException(nameof(buildEngine));
        _linkResolver = linkResolver ?? throw new ArgumentNullException(nameof(linkResolver));
        _navigationGenerator = navigationGenerator ?? throw new ArgumentNullException(nameof(navigationGenerator));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;
    }

    public bool BuildSite(string sourcePath, string outputPath, BuildManagerOptions? options = null, Action<double>? progressCallback = null)
    {
        if (!Directory.Exists(sourcePath))
        {
            _logger?.LogError("Source directory does not exist: {SourcePath}", sourcePath);
            return false;
        }

        options ??= new BuildManagerOptions();
        Directory.CreateDirectory(outputPath);

        var files = Directory.GetFiles(sourcePath, "*.md", SearchOption.AllDirectories).ToList();
        var totalFiles = files.Count;
        var errors = 0;

        if (options.ValidateLinks)
        {
            _logger?.LogInformation("Validating links in {FileCount} documents", totalFiles);
            var brokenLinks = new List<BrokenLink>();

            foreach (var file in files)
            {
                try
                {
                    var markdown = File.ReadAllText(file);
                    var broken = _linkResolver.ValidateLinks(markdown, file, sourcePath);
                    brokenLinks.AddRange(broken);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error validating links in {FilePath}", file);
                }
            }

            if (brokenLinks.Count > 0)
            {
                _logger?.LogWarning("Found {BrokenLinkCount} broken links", brokenLinks.Count);
                foreach (var broken in brokenLinks)
                {
                    _logger?.LogWarning("Broken link in {DocumentPath} line {LineNumber}: {LinkUrl}", 
                        broken.DocumentPath, broken.LineNumber, broken.LinkUrl);
                }
            }
        }

        progressCallback?.Invoke(25.0);

        _logger?.LogInformation("Building HTML from markdown files");
        var buildOptions = new BuildEngineOptions
        {
            GenerateAnchors = true,
            IncludeTableOfContents = true
        };

        var buildResult = _buildEngine.BuildSite(sourcePath, outputPath, buildOptions);
        errors = buildResult.Errors;

        progressCallback?.Invoke(60.0);

        if (options.GenerateNavigation)
        {
            _logger?.LogInformation("Generating navigation structure");
            var navigation = _navigationGenerator.GenerateNavigation(sourcePath);
            SaveNavigation(navigation, outputPath);
        }

        progressCallback?.Invoke(80.0);

        if (options.GenerateIndex)
        {
            _logger?.LogInformation("Generating documentation index");
            var index = _navigationGenerator.GenerateIndex(sourcePath);
            SaveIndex(index, outputPath);
        }

        progressCallback?.Invoke(100.0);

        _eventBus.Publish(new DocumentProcessedEvent
        {
            FilePath = outputPath,
            CharacterCount = buildResult.FilesProcessed
        });

        return errors == 0;
    }

    private void SaveNavigation(NavigationStructure navigation, string outputPath)
    {
        var navJson = System.Text.Json.JsonSerializer.Serialize(navigation, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(outputPath, "navigation.json"), navJson);
    }

    private void SaveIndex(DocumentationIndex index, string outputPath)
    {
        var indexJson = System.Text.Json.JsonSerializer.Serialize(index, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Path.Combine(outputPath, "index.json"), indexJson);
    }
}
