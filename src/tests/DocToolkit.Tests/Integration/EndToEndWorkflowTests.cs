using DocToolkit.Accessors;
using DocToolkit.Engines;
using DocToolkit.ifx.Models;
using Xunit;

namespace DocToolkit.Tests.Integration;

public class EndToEndWorkflowTests : IDisposable
{
    private readonly string _testDir;

    public EndToEndWorkflowTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), $"test_e2e_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, true);
        }
    }

    [Fact]
    public void ProjectInitialization_WithValidProject_CreatesStructure()
    {
        // Arrange
        var projectAccessor = new ProjectAccessor();
        var projectName = Path.Combine(_testDir, "TestProject");

        // Act
        projectAccessor.CreateDirectories(projectName, ProjectType.Mixed);
        projectAccessor.CreateCursorConfig(projectName);
        projectAccessor.CreateReadme(projectName, ProjectType.Mixed);

        // Assert
        Assert.True(Directory.Exists(Path.Combine(projectName, "docs", "customer")));
        Assert.True(Directory.Exists(Path.Combine(projectName, "docs", "developer")));
        Assert.True(Directory.Exists(Path.Combine(projectName, "docs", "shared")));
        Assert.True(File.Exists(Path.Combine(projectName, ".cursor", "cursor.json")));
        Assert.True(File.Exists(Path.Combine(projectName, "README.md")));
    }

    [Fact]
    public void DocumentExtraction_WithTextFile_ExtractsContent()
    {
        // Arrange
        var extractor = new DocumentExtractionEngine();
        var testFile = Path.Combine(_testDir, "test.txt");
        var content = "This is test content for extraction.";
        File.WriteAllText(testFile, content);

        // Act
        var extracted = extractor.ExtractText(testFile);

        // Assert
        Assert.Equal(content, extracted);
    }
}
