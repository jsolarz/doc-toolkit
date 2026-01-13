namespace DocToolkit.ifx.Models;

/// <summary>
/// Project type enumeration for opinionated organization.
/// </summary>
public enum ProjectType
{
    /// <summary>
    /// Customer-facing documentation (PRDs, proposals, requirements)
    /// </summary>
    CustomerFacing,

    /// <summary>
    /// Developer-facing documentation (architecture, design, specs)
    /// </summary>
    DeveloperFacing,

    /// <summary>
    /// Mixed project with both customer and developer documentation
    /// </summary>
    Mixed
}
