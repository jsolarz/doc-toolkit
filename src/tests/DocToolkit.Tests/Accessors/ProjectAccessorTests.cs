using DocToolkit.Accessors;
using DocToolkit.ifx.Models;
using Xunit;

namespace DocToolkit.Tests.Accessors;

public class ProjectAccessorTests : IDisposable
{
    private readonly ProjectAccessor _accessor;
    private readonly string _testDir;

    public ProjectAccessorTests()
    {
        _accessor = new ProjectAccessor();
        _testDir = Path.Combine(Path.GetTempPath(), $"test_project_{Guid.NewGuid()}");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void CreateDirectories_WithCustomerFacing_CreatesCustomerFolder()
    {
        // Act
        _accessor.CreateDirectories(_testDir, ProjectType.CustomerFacing);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(_testDir, "docs", "customer")));
        Assert.False(Directory.Exists(Path.Combine(_testDir, "docs", "developer")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, "docs", "shared")));
    }

    [Fact]
    public void CreateDirectories_WithDeveloperFacing_CreatesDeveloperFolder()
    {
        // Act
        _accessor.CreateDirectories(_testDir, ProjectType.DeveloperFacing);

        // Assert
        Assert.False(Directory.Exists(Path.Combine(_testDir, "docs", "customer")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, "docs", "developer")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, "docs", "shared")));
    }

    [Fact]
    public void CreateDirectories_WithMixed_CreatesBothFolders()
    {
        // Act
        _accessor.CreateDirectories(_testDir, ProjectType.Mixed);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(_testDir, "docs", "customer")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, "docs", "developer")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, "docs", "shared")));
    }

    [Fact]
    public void CreateDirectories_CreatesAllRequiredDirectories()
    {
        // Act
        _accessor.CreateDirectories(_testDir, ProjectType.Mixed);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(_testDir, "docs")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, ".cursor")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, ".doc-toolkit")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, "publish", "web")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, "publish", "pdf")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, "publish", "chm")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, "publish", "single")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, "templates")));
        Assert.True(Directory.Exists(Path.Combine(_testDir, ".github", "workflows")));
    }

    [Fact]
    public void CreateCursorConfig_CreatesConfigFile()
    {
        // Arrange
        Directory.CreateDirectory(_testDir);
        Directory.CreateDirectory(Path.Combine(_testDir, ".cursor"));

        // Act
        _accessor.CreateCursorConfig(_testDir);

        // Assert
        var configPath = Path.Combine(_testDir, ".cursor", "cursor.json");
        Assert.True(File.Exists(configPath));
        var content = File.ReadAllText(configPath);
        Assert.Contains("version", content);
        Assert.Contains("rules", content);
    }

    [Fact]
    public void CreateReadme_CreatesReadmeFile()
    {
        // Arrange
        Directory.CreateDirectory(_testDir);

        // Act
        _accessor.CreateReadme(_testDir, ProjectType.Mixed);

        // Assert
        var readmePath = Path.Combine(_testDir, "README.md");
        Assert.True(File.Exists(readmePath));
        var content = File.ReadAllText(readmePath);
        Assert.Contains(_testDir, content);
        Assert.Contains("customer", content);
        Assert.Contains("developer", content);
    }

    [Fact]
    public void CreateReadme_WithCustomerFacing_IncludesCustomerSection()
    {
        // Arrange
        Directory.CreateDirectory(_testDir);

        // Act
        _accessor.CreateReadme(_testDir, ProjectType.CustomerFacing);

        // Assert
        var readmePath = Path.Combine(_testDir, "README.md");
        var content = File.ReadAllText(readmePath);
        Assert.Contains("customer", content);
        Assert.DoesNotContain("developer", content);
    }

    [Fact]
    public void CreateReadme_WithDeveloperFacing_IncludesDeveloperSection()
    {
        // Arrange
        Directory.CreateDirectory(_testDir);

        // Act
        _accessor.CreateReadme(_testDir, ProjectType.DeveloperFacing);

        // Assert
        var readmePath = Path.Combine(_testDir, "README.md");
        var content = File.ReadAllText(readmePath);
        Assert.DoesNotContain("customer", content);
        Assert.Contains("developer", content);
    }
}
