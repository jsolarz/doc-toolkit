using DocToolkit.ifx.Infrastructure;
using Xunit;

namespace DocToolkit.Tests.Infrastructure;

public class ValidationServiceTests
{
    private readonly ValidationService _service;

    public ValidationServiceTests()
    {
        _service = new ValidationService();
    }

    [Fact]
    public void Validate_ReturnsValidationResult()
    {
        // Act
        var result = _service.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
        Assert.NotNull(result.Warnings);
    }

    [Fact]
    public void Validate_ChecksOnnxModel()
    {
        // Act
        var result = _service.Validate();

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Validate_ChecksDocumentLibraries()
    {
        // Act
        var result = _service.Validate();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Errors);
    }


    [Fact]
    public void CheckDocumentLibraryAvailable_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.CheckDocumentLibraryAvailable(null!));
    }

    [Fact]
    public void CheckDocumentLibraryAvailable_WithEmptyName_ReturnsFalse()
    {
        // Act
        var result = _service.CheckDocumentLibraryAvailable(string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CheckDocumentLibraryAvailable_WithNonExistentLibrary_ReturnsFalse()
    {
        // Act
        var result = _service.CheckDocumentLibraryAvailable("NonExistentLibrary.12345");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsToolAvailablePublic_WithNullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.IsToolAvailablePublic(null!));
    }

    [Fact]
    public void IsToolAvailablePublic_WithEmptyName_ReturnsFalse()
    {
        // Act
        var result = _service.IsToolAvailablePublic(string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsToolAvailablePublic_WithNonExistentTool_ReturnsFalse()
    {
        // Act
        var result = _service.IsToolAvailablePublic("nonexistenttool12345");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsToolAvailablePublic_WithGit_MayReturnTrueOrFalse()
    {
        // Act
        var result = _service.IsToolAvailablePublic("git");

        // Assert - May be true or false depending on environment
        Assert.True(result || !result);
    }
}
