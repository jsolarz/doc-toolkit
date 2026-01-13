using DocToolkit.ifx.Interfaces.IEngines;
using DocToolkit.ifx.Interfaces.IAccessors;
using Microsoft.Extensions.Logging;

namespace DocToolkit.Engines;

/// <summary>
/// Engine for validating document quality and compliance.
/// Encapsulates algorithm volatility: validation rules could change (different standards, stricter checks).
/// </summary>
public class DocumentValidator : IDocumentValidator
{
    private readonly ILinkResolver _linkResolver;
    private readonly ITemplateAccessor? _templateAccessor;
    private readonly ILogger<DocumentValidator>? _logger;

    public DocumentValidator(
        ILinkResolver linkResolver,
        ITemplateAccessor? templateAccessor = null,
        ILogger<DocumentValidator>? logger = null)
    {
        _linkResolver = linkResolver ?? throw new ArgumentNullException(nameof(linkResolver));
        _templateAccessor = templateAccessor;
        _logger = logger;
    }

    public ValidationResult ValidateDocument(string markdown, string documentPath, string basePath, string? templateType = null)
    {
        var result = new ValidationResult();

        var syntaxIssues = ValidateMarkdownSyntax(markdown);
        result.Issues.AddRange(syntaxIssues.Where(i => i.Severity == ValidationSeverity.Error));
        result.Warnings.AddRange(syntaxIssues.Where(i => i.Severity == ValidationSeverity.Warning || i.Severity == ValidationSeverity.Info));

        var brokenLinks = _linkResolver.ValidateLinks(markdown, documentPath, basePath);
        foreach (var brokenLink in brokenLinks)
        {
            result.Issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Message = $"Broken link: {brokenLink.LinkUrl}",
                LineNumber = brokenLink.LineNumber,
                Suggestion = $"Target does not exist: {brokenLink.ErrorMessage}"
            });
        }

        if (!string.IsNullOrEmpty(templateType) && _templateAccessor != null)
        {
            var complianceIssues = ValidateTemplateCompliance(markdown, templateType);
            result.Issues.AddRange(complianceIssues.Where(i => i.Severity == ValidationSeverity.Error));
            result.Warnings.AddRange(complianceIssues.Where(i => i.Severity == ValidationSeverity.Warning || i.Severity == ValidationSeverity.Info));
        }

        result.IsValid = result.ErrorCount == 0;
        return result;
    }

    public List<ValidationIssue> ValidateMarkdownSyntax(string markdown)
    {
        var issues = new List<ValidationIssue>();
        var lines = markdown.Split('\n');

        if (string.IsNullOrWhiteSpace(markdown))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Error,
                Message = "Document is empty",
                LineNumber = 0
            });
            return issues;
        }

        var hasTitle = false;
        var headerLevels = new List<int>();
        var listContext = new Stack<string>();

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var lineNumber = i + 1;

            if (i == 0 && line.StartsWith("# "))
            {
                hasTitle = true;
            }

            if (line.StartsWith("#"))
            {
                var headerMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(#{1,6})\s+(.+)$");
                if (headerMatch.Success)
                {
                    var level = headerMatch.Groups[1].Value.Length;
                    var text = headerMatch.Groups[2].Value.Trim();

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        issues.Add(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Warning,
                            Message = "Empty header",
                            LineNumber = lineNumber
                        });
                    }

                    if (headerLevels.Count > 0 && level > headerLevels.Last() + 1)
                    {
                        issues.Add(new ValidationIssue
                        {
                            Severity = ValidationSeverity.Warning,
                            Message = $"Header level jump from H{headerLevels.Last()} to H{level}",
                            LineNumber = lineNumber,
                            Suggestion = "Headers should increase by one level at a time"
                        });
                    }

                    headerLevels.Add(level);
                }
            }

            if (line.Trim().StartsWith("- ") || line.Trim().StartsWith("* ") || line.Trim().StartsWith("+ "))
            {
                var indent = line.Length - line.TrimStart().Length;
                if (listContext.Count > 0 && indent > listContext.Peek().Length + 2)
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = "Inconsistent list indentation",
                        LineNumber = lineNumber,
                        Suggestion = "Use consistent indentation (2 or 4 spaces)"
                    });
                }
            }

            if (line.Contains("  ") && line.Trim().Length > 0 && !line.StartsWith("    ") && !line.StartsWith("\t"))
            {
                var trailingSpaces = line.Length - line.TrimEnd().Length;
                if (trailingSpaces > 0)
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Info,
                        Message = "Trailing whitespace",
                        LineNumber = lineNumber,
                        Suggestion = "Remove trailing spaces"
                    });
                }
            }

            if (line.Length > 100 && !line.StartsWith("#") && !line.StartsWith(">"))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Info,
                    Message = "Line exceeds 100 characters",
                    LineNumber = lineNumber,
                    Suggestion = "Consider breaking into multiple lines for readability"
                });
            }
        }

        if (!hasTitle)
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Message = "Document missing H1 title",
                LineNumber = 1,
                Suggestion = "Add a title as the first line: # Title"
            });
        }

        return issues;
    }

    public List<ValidationIssue> ValidateTemplateCompliance(string markdown, string templateType)
    {
        var issues = new List<ValidationIssue>();

        if (_templateAccessor == null || !_templateAccessor.TemplateExists(templateType))
        {
            issues.Add(new ValidationIssue
            {
                Severity = ValidationSeverity.Warning,
                Message = $"Template '{templateType}' not found, skipping compliance check",
                LineNumber = 0
            });
            return issues;
        }

        var requiredSections = GetRequiredSections(templateType);
        var documentSections = ExtractSections(markdown);

        foreach (var requiredSection in requiredSections)
        {
            if (!documentSections.ContainsKey(requiredSection.Key))
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"Missing required section: {requiredSection.Value}",
                    LineNumber = 0,
                    Section = requiredSection.Value,
                    Suggestion = $"Add section: ## {requiredSection.Value}"
                });
            }
        }

        var sectionOrder = GetSectionOrder(templateType);
        if (sectionOrder.Count > 0)
        {
            var documentOrder = documentSections.Keys.ToList();
            for (int i = 0; i < sectionOrder.Count - 1; i++)
            {
                var currentSection = sectionOrder[i];
                var nextSection = sectionOrder[i + 1];

                var currentIndex = documentOrder.IndexOf(currentSection);
                var nextIndex = documentOrder.IndexOf(nextSection);

                if (currentIndex >= 0 && nextIndex >= 0 && currentIndex > nextIndex)
                {
                    issues.Add(new ValidationIssue
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = $"Section order issue: '{sectionOrder[i]}' should come before '{nextSection}'",
                        LineNumber = 0,
                        Suggestion = "Reorganize sections according to template structure"
                    });
                }
            }
        }

        return issues;
    }

    private Dictionary<string, string> ExtractSections(string markdown)
    {
        var sections = new Dictionary<string, string>();
        var lines = markdown.Split('\n');

        foreach (var line in lines)
        {
            var match = System.Text.RegularExpressions.Regex.Match(line, @"^#{1,6}\s+(.+)$");
            if (match.Success)
            {
                var sectionTitle = match.Groups[1].Value.Trim();
                var normalizedKey = NormalizeSectionName(sectionTitle);
                sections[normalizedKey] = sectionTitle;
            }
        }

        return sections;
    }

    private string NormalizeSectionName(string sectionName)
    {
        return sectionName.ToLowerInvariant()
            .Replace("&", "and")
            .Replace("/", " ")
            .Replace("-", " ")
            .Replace("_", " ")
            .Replace(".", "")
            .Trim();
    }

    private Dictionary<string, string> GetRequiredSections(string templateType)
    {
        var sections = new Dictionary<string, string>();

        switch (templateType.ToLowerInvariant())
        {
            case "prd":
                sections["executive summary"] = "Executive Summary";
                sections["purpose"] = "Purpose & Scope";
                sections["problem statement"] = "Problem Statement / Business Need";
                sections["objectives"] = "Objectives & Success Criteria";
                sections["stakeholders"] = "Stakeholders";
                sections["requirements"] = "Requirements / Specifications";
                sections["assumptions"] = "Assumptions & Constraints";
                sections["risks"] = "Risks & Mitigations";
                sections["timeline"] = "Timeline / Milestones";
                break;

            case "rfp":
            case "tender":
                sections["background"] = "Background & Context";
                sections["project goals"] = "Project Goals";
                sections["scope of work"] = "Scope of Work";
                sections["technical requirements"] = "Technical Requirements";
                sections["vendor qualifications"] = "Vendor Qualifications";
                sections["evaluation criteria"] = "Evaluation Criteria";
                sections["submission instructions"] = "Submission Instructions";
                break;

            case "sow":
                sections["project overview"] = "Project Overview";
                sections["scope"] = "Scope, Milestones & Deliverables";
                sections["assumptions"] = "Assumptions";
                sections["out of scope"] = "Out of Scope";
                break;

            case "architecture":
            case "solution":
                sections["architecture overview"] = "Architecture Overview";
                sections["system context"] = "System Context Diagram";
                sections["component architecture"] = "Component Architecture";
                sections["data flow"] = "Data Flow Diagrams";
                sections["integration"] = "Integration Architecture";
                sections["security"] = "Security Architecture";
                sections["deployment"] = "Deployment Architecture";
                sections["scalability"] = "Scalability & Performance";
                break;

            default:
                sections["executive summary"] = "Executive Summary";
                sections["purpose"] = "Purpose & Scope";
                break;
        }

        return sections;
    }

    private List<string> GetSectionOrder(string templateType)
    {
        var order = new List<string>();

        switch (templateType.ToLowerInvariant())
        {
            case "prd":
                order.AddRange(new[] { "executive summary", "purpose", "problem statement", "objectives", "stakeholders", "requirements", "assumptions", "risks", "timeline" });
                break;

            case "rfp":
            case "tender":
                order.AddRange(new[] { "background", "project goals", "scope of work", "technical requirements", "vendor qualifications", "evaluation criteria", "submission instructions" });
                break;

            case "sow":
                order.AddRange(new[] { "project overview", "scope", "assumptions", "out of scope" });
                break;
        }

        return order;
    }
}
