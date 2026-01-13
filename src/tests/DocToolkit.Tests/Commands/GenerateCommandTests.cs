using DocToolkit.ifx.Commands;
using DocToolkit.ifx.Interfaces.IAccessors;
using Moq;
using Spectre.Console.Cli;
using Xunit;

namespace DocToolkit.Tests.Commands;

public class GenerateCommandTests : IDisposable
{
    private readonly Mock<ITemplateAccessor> _mockTemplateAccessor;
    private readonly GenerateCommand _command;
    private readonly string _testDir;

    public GenerateCommandTests()
    {
        _mockTemplateAccessor = new Mock<ITemplateAccessor>();
        _command = new GenerateCommand(_mockTemplateAccessor.Object);
        _testDir = Path.Combine(Path.GetTempPath(), $"test_generate_{Guid.NewGuid()}");
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
    public void Constructor_WithNullTemplateAccessor_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GenerateCommand(null!));
    }

    [Fact]
    public void Execute_WithEmptyType_ReturnsError()
    {
        // Arrange
        var settings = new GenerateCommand.Settings
        {
            Type = string.Empty,
            Name = "TestDocument"
        };
        var context = new CommandContext(Array.Empty<string>(), null!, "generate", null);

        // Act
        var result = _command.Execute(context, settings);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_WithEmptyName_ReturnsError()
    {
        // Arrange
        var settings = new GenerateCommand.Settings
        {
            Type = "prd",
            Name = string.Empty
        };
        var context = new CommandContext(Array.Empty<string>(), null!, "generate", null);

        // Act
        var result = _command.Execute(context, settings);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_WithNonExistentTemplate_ReturnsError()
    {
        // Arrange
        _mockTemplateAccessor.Setup(x => x.TemplateExists(It.IsAny<string>()))
            .Returns(false);
        _mockTemplateAccessor.Setup(x => x.GetAvailableTemplates())
            .Returns(new List<string> { "prd", "rfp" });

        var settings = new GenerateCommand.Settings
        {
            Type = "nonexistent",
            Name = "TestDocument"
        };
        var context = new CommandContext(Array.Empty<string>(), null!, "generate", null);

        // Act
        var result = _command.Execute(context, settings);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Execute_WithValidTemplate_GeneratesDocument()
    {
        // Arrange
        _mockTemplateAccessor.Setup(x => x.TemplateExists("prd"))
            .Returns(true);
        var expectedPath = Path.Combine(_testDir, $"{DateTime.Now:yyyy-MM-dd}-prd-TestDocument.md");
        _mockTemplateAccessor.Setup(x => x.GenerateDocument("prd", "TestDocument", _testDir))
            .Returns(expectedPath);

        var settings = new GenerateCommand.Settings
        {
            Type = "prd",
            Name = "TestDocument",
            Output = _testDir
        };
        var context = new CommandContext(Array.Empty<string>(), null!, "generate", null);

        // Act
        var result = _command.Execute(context, settings);

        // Assert
        Assert.Equal(0, result);
        _mockTemplateAccessor.Verify(x => x.GenerateDocument("prd", "TestDocument", _testDir), Times.Once);
    }

    [Fact]
    public void Execute_WithSubfolder_CreatesSubfolder()
    {
        // Arrange
        _mockTemplateAccessor.Setup(x => x.TemplateExists("prd"))
            .Returns(true);
        var subfolderPath = Path.Combine(_testDir, "customer");
        var expectedPath = Path.Combine(subfolderPath, $"{DateTime.Now:yyyy-MM-dd}-prd-TestDocument.md");
        _mockTemplateAccessor.Setup(x => x.GenerateDocument("prd", "TestDocument", subfolderPath))
            .Returns(expectedPath);

        var settings = new GenerateCommand.Settings
        {
            Type = "prd",
            Name = "TestDocument",
            Output = _testDir,
            Subfolder = "customer"
        };
        var context = new CommandContext(Array.Empty<string>(), null!, "generate", null);

        // Act
        var result = _command.Execute(context, settings);

        // Assert
        Assert.Equal(0, result);
        _mockTemplateAccessor.Verify(x => x.GenerateDocument("prd", "TestDocument", subfolderPath), Times.Once);
    }
}
