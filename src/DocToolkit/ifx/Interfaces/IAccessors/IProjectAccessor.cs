using DocToolkit.ifx.Models;

namespace DocToolkit.ifx.Interfaces.IAccessors;

/// <summary>
/// Accessor interface for project workspace operations.
/// </summary>
public interface IProjectAccessor
{
    /// <summary>
    /// Creates the directory structure for a new project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="projectType">Type of project (determines folder structure)</param>
    void CreateDirectories(string projectName, ProjectType projectType);

    /// <summary>
    /// Creates the Cursor IDE configuration file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    void CreateCursorConfig(string projectName);

    /// <summary>
    /// Creates a README file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="projectType">Type of project (for structure explanation)</param>
    void CreateReadme(string projectName, ProjectType projectType);

    /// <summary>
    /// Creates a .gitignore file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    void CreateGitIgnore(string projectName);

    /// <summary>
    /// Initializes a Git repository for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    void InitializeGit(string projectName);

    /// <summary>
    /// Creates configuration files for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    void CreateConfigFiles(string projectName);

    /// <summary>
    /// Creates an onboarding/walkthrough guide.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    /// <param name="projectType">Type of project (for structure explanation)</param>
    void CreateOnboardingGuide(string projectName, ProjectType projectType);

    /// <summary>
    /// Creates a .docignore file to exclude files from publishing.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    void CreateDocIgnore(string projectName);

    /// <summary>
    /// Creates GitHub Actions workflow for documentation quality checks.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    void CreateLintWorkflow(string projectName);
}
