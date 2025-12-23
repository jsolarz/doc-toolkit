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
    void CreateDirectories(string projectName);

    /// <summary>
    /// Creates the Cursor IDE configuration file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    void CreateCursorConfig(string projectName);

    /// <summary>
    /// Creates a README file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    void CreateReadme(string projectName);

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
}
