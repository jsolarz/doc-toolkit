using System.Diagnostics;

namespace DocToolkit.Services;

public class ValidationService
{
    public ValidationResult Validate()
    {
        var result = new ValidationResult();

        // Check ONNX model
        if (!CheckOnnxModelAvailable())
        {
            result.Warnings.Add("ONNX embedding model not found - semantic indexing will not work");
            result.Warnings.Add("Download all-MiniLM-L6-v2.onnx and place in models/ directory");
        }

        // Check document libraries
        var requiredLibraries = new[] { "DocumentFormat.OpenXml", "UglyToad.PdfPig", "Microsoft.ML.OnnxRuntime" };
        var missingLibraries = new List<string>();

        foreach (var library in requiredLibraries)
        {
            if (!CheckDocumentLibraryAvailable(library))
            {
                missingLibraries.Add(library);
            }
        }

        if (missingLibraries.Count > 0)
        {
            result.Errors.Add($"Missing required libraries: {string.Join(", ", missingLibraries)}");
            result.Errors.Add("Run 'dotnet restore' to install NuGet packages");
        }

        // Check external tools (warnings, not errors - optional)
        if (!IsToolAvailable("pdftotext"))
        {
            result.Warnings.Add("Poppler (pdftotext) not found - Advanced PDF features may be limited");
        }

        if (!IsToolAvailable("tesseract"))
        {
            result.Warnings.Add("Tesseract OCR not found - Image OCR will not work (optional)");
        }

        result.Success = result.Errors.Count == 0;

        return result;
    }


    private bool IsToolAvailable(string toolName)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = toolName,
                    Arguments = toolName == "tesseract" ? "--version" : "-v",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    // Public methods for command use
    public bool CheckOnnxModelAvailable()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var possiblePaths = new[]
        {
            Path.Combine(currentDir, "models", "all-MiniLM-L6-v2.onnx"),
            Path.Combine(currentDir, "..", "models", "all-MiniLM-L6-v2.onnx"),
            Path.Combine(currentDir, "..", "..", "models", "all-MiniLM-L6-v2.onnx"),
            Path.Combine(AppContext.BaseDirectory, "models", "all-MiniLM-L6-v2.onnx"),
            Path.Combine(AppContext.BaseDirectory, "..", "models", "all-MiniLM-L6-v2.onnx")
        };

        return possiblePaths.Any(File.Exists);
    }

    public bool CheckDocumentLibraryAvailable(string libraryName)
    {
        try
        {
            var assembly = System.Reflection.Assembly.Load(libraryName);
            return assembly != null;
        }
        catch
        {
            return false;
        }
    }

    public bool IsToolAvailablePublic(string toolName)
    {
        return IsToolAvailable(toolName);
    }
}

public class ValidationResult
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
