namespace DocToolkit.ifx.Interfaces.IEngines;

/// <summary>
/// Engine for validating document quality and compliance.
/// Encapsulates algorithm volatility: validation rules could change (different standards, stricter checks).
/// </summary>
public interface IDocumentValidator
{
    /// <summary>
    /// Validates a markdown document for quality and compliance.
    /// </summary>
    /// <param name="markdown">Markdown content</param>
    /// <param name="documentPath">Path to the document</param>
    /// <param name="basePath">Base path for resolving relative links</param>
    /// <param name="templateType">Optional template type for compliance checking</param>
    /// <returns>Validation result with issues and warnings</returns>
    ValidationResult ValidateDocument(string markdown, string documentPath, string basePath, string? templateType = null);

    /// <summary>
    /// Validates markdown syntax.
    /// </summary>
    /// <param name="markdown">Markdown content</param>
    /// <returns>List of syntax issues</returns>
    List<ValidationIssue> ValidateMarkdownSyntax(string markdown);

    /// <summary>
    /// Validates document structure against a template.
    /// </summary>
    /// <param name="markdown">Markdown content</param>
    /// <param name="templateType">Template type to validate against</param>
    /// <returns>List of compliance issues</returns>
    List<ValidationIssue> ValidateTemplateCompliance(string markdown, string templateType);
}

/// <summary>
/// Result of document validation.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationIssue> Issues { get; set; } = new();
    public List<ValidationIssue> Warnings { get; set; } = new();
    public int ErrorCount => Issues.Count;
    public int WarningCount => Warnings.Count;
}

/// <summary>
/// Represents a validation issue.
/// </summary>
public class ValidationIssue
{
    public ValidationSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string? Section { get; set; }
    public string? Suggestion { get; set; }
}

/// <summary>
/// Severity level of a validation issue.
/// </summary>
public enum ValidationSeverity
{
    Error,
    Warning,
    Info
}
