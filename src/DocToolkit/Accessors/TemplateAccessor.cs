using System.Reflection;
using DocToolkit.ifx.Interfaces.IAccessors;

namespace DocToolkit.Accessors;

/// <summary>
/// Accessor for document template operations.
/// Encapsulates storage volatility: template location could change (local → cloud, file → database).
/// Now uses embedded resources for self-contained deployment.
/// </summary>
/// <remarks>
/// Component Type: Accessor (Storage Volatility)
/// Volatility: Template storage location and format
/// Pattern: Dumb CRUD operations - no business logic
/// Service Boundary: Called by GenerateCommand (Client)
/// </remarks>
public class TemplateAccessor : ITemplateAccessor
{
    private readonly Assembly _assembly;
    private readonly Lazy<List<string>> _availableResourceNames;

    /// <summary>
    /// Initializes a new instance of the TemplateAccessor.
    /// </summary>
    public TemplateAccessor()
    {
        _assembly = Assembly.GetExecutingAssembly();
        _availableResourceNames = new Lazy<List<string>>(() =>
            _assembly.GetManifestResourceNames().ToList());
    }

    /// <summary>
    /// Finds the resource name for a template type.
    /// Searches for both "templates.{type}.md" and "{namespace}.templates.{type}.md" patterns.
    /// </summary>
    private string? FindResourceName(string type)
    {
        var resourceNames = _availableResourceNames.Value;
        var possibleNames = new[]
        {
            $"templates.{type}.md",
            $"{_assembly.GetName().Name}.templates.{type}.md"
        };

        return possibleNames.FirstOrDefault(name => resourceNames.Contains(name));
    }

    /// <summary>
    /// Checks if a template exists for the specified type.
    /// </summary>
    /// <param name="type">Template type (e.g., "prd", "sow", "rfp")</param>
    /// <returns>True if template resource exists</returns>
    public bool TemplateExists(string type)
    {
        return FindResourceName(type) != null;
    }

    /// <summary>
    /// Gets a list of available template types.
    /// </summary>
    /// <returns>List of template type names</returns>
    public List<string> GetAvailableTemplates()
    {
        var resourceNames = _availableResourceNames.Value;
        var templates = new List<string>();

        foreach (var resourceName in resourceNames)
        {
            // Match patterns: "templates.{name}.md" or "{namespace}.templates.{name}.md"
            var match = System.Text.RegularExpressions.Regex.Match(
                resourceName,
                @"(?:^|\.)templates\.([^\.]+)\.md$"
            );

            if (match.Success)
            {
                var templateName = match.Groups[1].Value;
                if (templateName != "master-template")
                {
                    templates.Add(templateName);
                }
            }
        }

        return templates.OrderBy(t => t).ToList();
    }

    /// <summary>
    /// Generates a document from a template.
    /// </summary>
    /// <param name="type">Template type</param>
    /// <param name="name">Document name</param>
    /// <param name="outputDir">Output directory path</param>
    /// <returns>Path to the generated document</returns>
    /// <exception cref="FileNotFoundException">Thrown when template not found</exception>
    public string GenerateDocument(string type, string name, string outputDir)
    {
        var resourceName = FindResourceName(type);

        if (resourceName == null)
        {
            throw new FileNotFoundException($"Template not found: {type}");
        }

        Directory.CreateDirectory(outputDir);

        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var sanitizedName = name.Replace(" ", "_");
        var outputPath = Path.Combine(outputDir, $"{date}-{type}-{sanitizedName}.md");

        // Read template from embedded resource
        using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Could not load template resource: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        var templateContent = reader.ReadToEnd();

        // Write to output file
        File.WriteAllText(outputPath, templateContent);

        return outputPath;
    }
}
