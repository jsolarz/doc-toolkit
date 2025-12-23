namespace DocToolkit.ifx.Interfaces.IAccessors;

/// <summary>
/// Accessor interface for document template operations.
/// </summary>
public interface ITemplateAccessor
{
    /// <summary>
    /// Checks if a template exists for the specified type.
    /// </summary>
    /// <param name="type">Template type (e.g., "prd", "sow", "rfp")</param>
    /// <returns>True if template file exists</returns>
    bool TemplateExists(string type);

    /// <summary>
    /// Gets a list of available template types.
    /// </summary>
    /// <returns>List of template type names</returns>
    List<string> GetAvailableTemplates();

    /// <summary>
    /// Generates a document from a template.
    /// </summary>
    /// <param name="type">Template type</param>
    /// <param name="name">Document name</param>
    /// <param name="outputDir">Output directory path</param>
    /// <returns>Path to the generated document</returns>
    string GenerateDocument(string type, string name, string outputDir);
}
