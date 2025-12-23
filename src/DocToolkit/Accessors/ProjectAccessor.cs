using System.Diagnostics;
using System.Text.Json;
using DocToolkit.ifx.Interfaces.IAccessors;

namespace DocToolkit.Accessors;

/// <summary>
/// Accessor for project workspace operations.
/// Encapsulates storage volatility: file system operations could change (local → cloud storage).
/// </summary>
/// <remarks>
/// Component Type: Accessor (Storage Volatility)
/// Volatility: File system and storage technology
/// Pattern: Dumb CRUD operations - no business logic
/// Service Boundary: Called by InitCommand (Client)
/// </remarks>
public class ProjectAccessor : IProjectAccessor
{
    /// <summary>
    /// Creates the directory structure for a new project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void CreateDirectories(string projectName)
    {
        Directory.CreateDirectory(projectName);
        Directory.CreateDirectory(Path.Combine(projectName, "docs"));
        Directory.CreateDirectory(Path.Combine(projectName, "source"));
        Directory.CreateDirectory(Path.Combine(projectName, ".cursor"));
        Directory.CreateDirectory(Path.Combine(projectName, "semantic-index"));
        Directory.CreateDirectory(Path.Combine(projectName, "knowledge-graph"));
    }

    /// <summary>
    /// Creates the Cursor IDE configuration file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void CreateCursorConfig(string projectName)
    {
        var cursorConfig = new
        {
            version = 1,
            rules = new[]
            {
                "../../doc-toolkit/.cursor/rules/00-core-instructions.mdc",
                "../../doc-toolkit/.cursor/rules/sow-rules.mdc"
            },
            documentTypes = new
            {
                sow = new { template = "../../doc-toolkit/templates/sow.md" },
                prd = new { template = "../../doc-toolkit/templates/prd.md" },
                rfp = new { template = "../../doc-toolkit/templates/rfp.md" },
                tender = new { template = "../../doc-toolkit/templates/tender.md" },
                architecture = new { template = "../../doc-toolkit/templates/architecture.md" },
                solution = new { template = "../../doc-toolkit/templates/solution.md" }
            }
        };

        var json = JsonSerializer.Serialize(cursorConfig, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var cursorPath = Path.Combine(projectName, ".cursor", "cursor.json");
        File.WriteAllText(cursorPath, json);
    }

    /// <summary>
    /// Creates a README file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void CreateReadme(string projectName)
    {
        var readme = $@"# {projectName}

This project workspace was generated using the Documentation Toolkit.

## Structure
- docs/
- source/
- semantic-index/
- knowledge-graph/
- .cursor/

## Commands
doc generate <type> <name>
doc index
doc search <query>
doc graph
";

        var readmePath = Path.Combine(projectName, "README.md");
        File.WriteAllText(readmePath, readme);
    }

    /// <summary>
    /// Creates a .gitignore file for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void CreateGitIgnore(string projectName)
    {
        var gitignore = @"/docs/*
/source/*
/semantic-index/*
/knowledge-graph/*
!/docs/.keep
!/source/.keep
!/semantic-index/.keep
!/knowledge-graph/.keep
/.cursor/*
!/.cursor/cursor.json
";

        var gitignorePath = Path.Combine(projectName, ".gitignore");
        File.WriteAllText(gitignorePath, gitignore);
    }

    /// <summary>
    /// Initializes a Git repository for the project.
    /// </summary>
    /// <param name="projectName">Name of the project</param>
    public void InitializeGit(string projectName)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "init",
                WorkingDirectory = projectName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode == 0)
        {
            // Add files and commit
            var addProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "add .",
                    WorkingDirectory = projectName,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            addProcess.Start();
            addProcess.WaitForExit();

            var commitProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "commit -m \"Initial project structure created by Documentation Toolkit\"",
                    WorkingDirectory = projectName,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            commitProcess.Start();
            commitProcess.WaitForExit();
        }
    }
}
