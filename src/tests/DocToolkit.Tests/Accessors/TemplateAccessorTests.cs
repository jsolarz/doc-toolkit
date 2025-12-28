using DocToolkit.Accessors;
using Xunit;

namespace DocToolkit.Tests.Accessors;

public class TemplateAccessorTests
{
    private readonly TemplateAccessor _accessor;
    private readonly string _testOutputDir;

    public TemplateAccessorTests()
    {
        _accessor = new TemplateAccessor();
        _testOutputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testOutputDir);
    }

    [Fact]
    public void TemplateExists_WithValidTemplate_ReturnsTrue()
    {
        // Act
        var result = _accessor.TemplateExists("prd");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TemplateExists_WithInvalidTemplate_ReturnsFalse()
    {
        // Act
        var result = _accessor.TemplateExists("nonexistent-template");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TemplateExists_WithMultipleTemplates_ReturnsTrueForAll()
    {
        // Arrange
        var commonTemplates = new[] { "prd", "sow", "rfp", "tender", "architecture", "solution" };

        // Act & Assert
        foreach (var template in commonTemplates)
        {
            var exists = _accessor.TemplateExists(template);
            Assert.True(exists, $"Template '{template}' should exist");
        }
    }

    [Fact]
    public void GetAvailableTemplates_ReturnsNonEmptyList()
    {
        // Act
        var templates = _accessor.GetAvailableTemplates();

        // Assert
        Assert.NotEmpty(templates);
        Assert.All(templates, t => Assert.False(string.IsNullOrWhiteSpace(t)));
    }

    [Fact]
    public void GetAvailableTemplates_ExcludesMasterTemplate()
    {
        // Act
        var templates = _accessor.GetAvailableTemplates();

        // Assert
        Assert.DoesNotContain("master-template", templates);
    }

    [Fact]
    public void GetAvailableTemplates_ReturnsCommonTemplates()
    {
        // Act
        var templates = _accessor.GetAvailableTemplates();

        // Assert
        var commonTemplates = new[] { "prd", "sow", "rfp", "tender", "architecture", "solution" };
        foreach (var template in commonTemplates)
        {
            Assert.Contains(template, templates);
        }
    }

    [Fact]
    public void GetAvailableTemplates_ReturnsSortedList()
    {
        // Act
        var templates = _accessor.GetAvailableTemplates();

        // Assert
        var sorted = templates.OrderBy(t => t).ToList();
        Assert.Equal(sorted, templates);
    }

    [Fact]
    public void GenerateDocument_WithValidTemplate_CreatesDocument()
    {
        // Arrange
        var type = "prd";
        var name = "Test Document";
        var expectedFileName = $"{DateTime.Now:yyyy-MM-dd}-{type}-Test_Document.md";

        // Act
        var outputPath = _accessor.GenerateDocument(type, name, _testOutputDir);

        // Assert
        Assert.True(File.Exists(outputPath), "Generated document should exist");
        Assert.Equal(Path.Combine(_testOutputDir, expectedFileName), outputPath);
        var content = File.ReadAllText(outputPath);
        Assert.NotEmpty(content);
    }

    [Fact]
    public void GenerateDocument_WithValidTemplate_ContainsTemplateContent()
    {
        // Arrange
        var type = "prd";
        var name = "Test PRD";

        // Act
        var outputPath = _accessor.GenerateDocument(type, name, _testOutputDir);
        var content = File.ReadAllText(outputPath);

        // Assert
        Assert.Contains("#", content); // Should contain markdown headers
        Assert.NotEmpty(content.Trim());
    }

    [Fact]
    public void GenerateDocument_WithInvalidTemplate_ThrowsFileNotFoundException()
    {
        // Arrange
        var type = "nonexistent-template";
        var name = "Test Document";

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() =>
            _accessor.GenerateDocument(type, name, _testOutputDir));
        Assert.Contains(type, exception.Message);
    }

    [Fact]
    public void GenerateDocument_WithSubfolder_CreatesDirectory()
    {
        // Arrange
        var type = "sow";
        var name = "Test SOW";
        var subfolder = Path.Combine(_testOutputDir, "subfolder");

        // Act
        var outputPath = _accessor.GenerateDocument(type, name, subfolder);

        // Assert
        Assert.True(Directory.Exists(subfolder), "Subfolder should be created");
        Assert.True(File.Exists(outputPath), "Document should be created in subfolder");
    }

    [Fact]
    public void GenerateDocument_WithSpacesInName_SanitizesFileName()
    {
        // Arrange
        var type = "prd";
        var name = "My Test Document With Spaces";

        // Act
        var outputPath = _accessor.GenerateDocument(type, name, _testOutputDir);

        // Assert
        var fileName = Path.GetFileName(outputPath);
        Assert.Contains("My_Test_Document_With_Spaces", fileName);
        Assert.DoesNotContain(" ", fileName);
    }

    [Fact]
    public void GenerateDocument_WithMultipleCalls_CreatesMultipleDocuments()
    {
        // Arrange
        var type = "prd";
        var names = new[] { "Document 1", "Document 2", "Document 3" };

        // Act
        var outputPaths = names.Select(name =>
            _accessor.GenerateDocument(type, name, _testOutputDir)).ToList();

        // Assert
        Assert.Equal(3, outputPaths.Count);
        Assert.All(outputPaths, path => Assert.True(File.Exists(path)));
        Assert.Equal(outputPaths.Distinct().Count(), outputPaths.Count); // All paths should be unique
    }

    [Fact]
    public void GenerateDocument_OverwritesExistingFile()
    {
        // Arrange
        var type = "prd";
        var name = "Test Document";
        var firstPath = _accessor.GenerateDocument(type, name, _testOutputDir);
        var firstContent = File.ReadAllText(firstPath);

        // Modify the file to verify it gets overwritten
        File.WriteAllText(firstPath, "Modified content");

        // Act
        var secondPath = _accessor.GenerateDocument(type, name, _testOutputDir);
        var secondContent = File.ReadAllText(secondPath);

        // Assert
        Assert.Equal(firstPath, secondPath);
        Assert.NotEqual("Modified content", secondContent);
        Assert.Equal(firstContent, secondContent);
    }

    [Fact]
    public void GenerateDocument_WithDifferentTypes_GeneratesDifferentContent()
    {
        // Arrange
        var types = new[] { "prd", "sow", "rfp" };
        var name = "Test Document";

        // Act
        var contents = types.Select(type =>
        {
            var path = _accessor.GenerateDocument(type, name, _testOutputDir);
            return File.ReadAllText(path);
        }).ToList();

        // Assert
        Assert.Equal(3, contents.Count);
        // Each template should have unique content (at least different headers)
        var uniqueContents = contents.Distinct().ToList();
        Assert.True(uniqueContents.Count >= 2, "Different templates should produce different content");
    }
}
