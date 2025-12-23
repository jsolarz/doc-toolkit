using System.Diagnostics;
using System.Reflection;

namespace DocToolkit.ifx.Infrastructure;

/// <summary>
/// Service for validating setup and dependencies.
/// Utility service for validation operations.
/// </summary>
public class ValidationService
{
    /// <summary>
    /// Validation result.
    /// </summary>
    public class ValidationResult
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    /// <summary>
    /// Validates the setup.
    /// </summary>
    /// <returns>Validation result</returns>
    public ValidationResult Validate()
    {
        var result = new ValidationResult { Success = true };

        // Check ONNX model
        if (!CheckOnnxModelAvailable())
        {
            result.Warnings.Add("ONNX model not found. Download all-MiniLM-L6-v2.onnx and place in models/ directory");
        }

        // Check document libraries
        var libraries = new[] { "DocumentFormat.OpenXml", "UglyToad.PdfPig", "Microsoft.ML.OnnxRuntime" };
        foreach (var library in libraries)
        {
            if (!CheckDocumentLibraryAvailable(library))
            {
                result.Errors.Add($"{library} library not found");
                result.Success = false;
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if ONNX model is available.
    /// </summary>
    /// <returns>True if model is available</returns>
    public bool CheckOnnxModelAvailable()
    {
        var modelPaths = new[]
        {
            Path.Combine("models", "all-MiniLM-L6-v2.onnx"),
            Path.Combine(Directory.GetCurrentDirectory(), "models", "all-MiniLM-L6-v2.onnx"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models", "all-MiniLM-L6-v2.onnx")
        };

        return modelPaths.Any(File.Exists);
    }

    /// <summary>
    /// Checks if a document library is available.
    /// </summary>
    /// <param name="libraryName">Library name</param>
    /// <returns>True if library is available</returns>
    public bool CheckDocumentLibraryAvailable(string libraryName)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = assembly.GetReferencedAssemblies();
            return referencedAssemblies.Any(a => a.Name == libraryName);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if an external tool is available.
    /// </summary>
    /// <param name="toolName">Tool name</param>
    /// <returns>True if tool is available</returns>
    public bool IsToolAvailablePublic(string toolName)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = toolName,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(1000);
            return process.ExitCode == 0 || process.ExitCode == 1; // Some tools return 1 for --version
        }
        catch
        {
            return false;
        }
    }
}
