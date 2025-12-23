using DocToolkit.Interfaces.Accessors;

namespace DocToolkit.Accessors;

/// <summary>
/// Accessor for document template operations.
/// Encapsulates storage volatility: template location could change (local → cloud, file → database).
/// </summary>
/// <remarks>
/// Component Type: Accessor (Storage Volatility)
/// Volatility: Template storage location and format
/// Pattern: Dumb CRUD operations - no business logic
/// Service Boundary: Called by GenerateCommand (Client)
/// </remarks>
public class TemplateAccessor : ITemplateAccessor
{
    private readonly string _templatesDir;

    /// <summary>
    /// Initializes a new instance of the TemplateAccessor.
    /// </summary>
    public TemplateAccessor()
    {
        // Try to find templates directory relative to executable or current directory
        // Look from src/DocToolkit up to root, then check root templates folder
        var currentDir = Directory.GetCurrentDirectory();
        var possiblePaths = new[]
        {
            Path.Combine(currentDir, "templates"),
            Path.Combine(currentDir, "..", "templates"),
            Path.Combine(currentDir, "..", "..", "templates"),
            Path.Combine(currentDir, "..", "..", "..", "templates"), // From src/DocToolkit/bin to root
            Path.Combine(AppContext.BaseDirectory, "templates"),
            Path.Combine(AppContext.BaseDirectory, "..", "templates"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "templates"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "templates") // From src/DocToolkit/bin to root
        };

        _templatesDir = possiblePaths.FirstOrDefault(Directory.Exists) ?? "templates";
    }

    /// <summary>
    /// Checks if a template exists for the specified type.
    /// </summary>
    /// <param name="type">Template type (e.g., "prd", "sow", "rfp")</param>
    /// <returns>True if template file exists</returns>
    public bool TemplateExists(string type)
    {
        var templatePath = Path.Combine(_templatesDir, $"{type}.md");
        return File.Exists(templatePath);
    }

    /// <summary>
    /// Gets a list of available template types.
    /// </summary>
    /// <returns>List of template type names</returns>
    public List<string> GetAvailableTemplates()
    {
        if (!Directory.Exists(_templatesDir))
            return new List<string>();

        return Directory.GetFiles(_templatesDir, "*.md")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Where(t => t != "master-template")
            .OrderBy(t => t)
            .ToList();
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
        var templatePath = Path.Combine(_templatesDir, $"{type}.md");
        
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template not found: {templatePath}");
        }

        Directory.CreateDirectory(outputDir);

        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var sanitizedName = name.Replace(" ", "_");
        var outputPath = Path.Combine(outputDir, $"{date}-{type}-{sanitizedName}.md");

        File.Copy(templatePath, outputPath, overwrite: true);

        return outputPath;
    }
}
