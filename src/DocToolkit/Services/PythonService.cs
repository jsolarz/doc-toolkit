using System.Diagnostics;
using System.Text;
using DocToolkit.Models;

namespace DocToolkit.Services;

public class PythonService
{
    public bool IsPythonAvailable()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "--version",
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

    public bool CheckDependencies(string[] packages)
    {
        foreach (var package in packages)
        {
            var checkScript = $"import {package}; print('OK')";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"-c \"{checkScript}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                return false;
            }
        }

        return true;
    }

    public bool BuildSemanticIndex(string sourcePath, string outputPath, int chunkSize, int chunkOverlap, Action<double>? progressCallback = null)
    {
        // For now, call the existing Python script
        // In the future, this could be ported to C# using ML.NET or similar
        var scriptPath = FindScript("semantic-index.ps1");
        if (scriptPath == null)
        {
            // Fallback: create and run Python script inline
            return RunPythonScriptInline("semantic_index", sourcePath, outputPath, chunkSize, chunkOverlap);
        }

        // Call PowerShell script
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pwsh",
                Arguments = $"-File \"{scriptPath}\" -SourcePath \"{sourcePath}\" -IndexPath \"{outputPath}\" -ChunkSize {chunkSize} -ChunkOverlap {chunkOverlap}",
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

    public List<SearchResult>? SearchSemanticIndex(string query, string indexPath, int topK)
    {
        // This is handled directly in SearchCommand by calling PowerShell script
        // Could be ported to C# in the future
        return null;
    }

    public bool BuildKnowledgeGraph(string sourcePath, string outputPath, Action<double>? progressCallback = null)
    {
        var scriptPath = FindScript("build_kg.py");
        if (scriptPath == null)
        {
            return false;
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\"",
                WorkingDirectory = Directory.GetCurrentDirectory(),
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

    private string? FindScript(string scriptName)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var possiblePaths = new[]
        {
            Path.Combine(currentDir, "scripts", scriptName),
            Path.Combine(currentDir, "..", "scripts", scriptName),
            Path.Combine(currentDir, "..", "..", "scripts", scriptName),
            Path.Combine(currentDir, "..", "..", "..", "scripts", scriptName), // From src/DocToolkit to root
            Path.Combine(AppContext.BaseDirectory, "scripts", scriptName),
            Path.Combine(AppContext.BaseDirectory, "..", "scripts", scriptName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "scripts", scriptName),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "scripts", scriptName) // From src/DocToolkit/bin to root
        };

        return possiblePaths.FirstOrDefault(File.Exists);
    }

    private bool RunPythonScriptInline(string scriptName, params object[] args)
    {
        // This would generate Python code inline and execute it
        // For now, return false to indicate script-based approach needed
        return false;
    }
}

// Legacy SearchResult - kept for backward compatibility but not used in C# native implementation
// Use DocToolkit.Models.SearchResult instead
internal class LegacySearchResult
{
    public string File { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Chunk { get; set; } = string.Empty;
}
