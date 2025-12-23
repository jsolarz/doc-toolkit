param(
    [string]$SourcePath = "./source",
    [string]$OutputDir = "./knowledge-graph"
)

Write-Host "🧠 Building knowledge graph from: $SourcePath"

# Check if Python is installed
try {
    $pythonVersion = python --version 2>&1
    if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne $null) {
        throw "Python not found"
    }
    Write-Host "✓ Python found: $pythonVersion"
} catch {
    Write-Host "❌ Python is not installed or not in PATH"
    Write-Host "Please install Python 3.10+ from https://www.python.org/"
    exit 1
}

# Check if required Python packages are installed
$requiredPackages = @{
    "docx" = "python-docx"
    "pptx" = "python-pptx"
    "pypdf" = "pypdf"
}
$missingPackages = @()

foreach ($package in $requiredPackages.Keys) {
    $packageName = $requiredPackages[$package]
    $checkScript = "import $package; print('OK')"
    $result = python -c $checkScript 2>&1
    if ($LASTEXITCODE -ne 0 -or $result -notmatch "OK") {
        $missingPackages += $packageName
    }
}

if ($missingPackages.Count -gt 0) {
    Write-Host "❌ Missing Python packages: $($missingPackages -join ', ')"
    Write-Host "Install with: pip install -r requirements.txt"
    exit 1
}

if (!(Test-Path $SourcePath)) {
    Write-Host "❌ Source folder not found: $SourcePath"
    exit 1
}

# Ensure output directory exists
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

# Get script directory - use more robust method
$scriptDir = if ($PSScriptRoot) { $PSScriptRoot } else { Split-Path -Parent $MyInvocation.MyCommand.Path }
$pythonScript = Join-Path $scriptDir "build_kg.py"

if (!(Test-Path $pythonScript)) {
    Write-Host "❌ Python script not found: $pythonScript"
    exit 1
}

# Run Python from project root so ./source is correct
try {
    python $pythonScript
    if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne $null) {
        Write-Host "❌ Python script failed with exit code: $LASTEXITCODE"
        exit 1
    }
    Write-Host "✅ Knowledge graph created in: $OutputDir"
    Write-Host "  - graph.json"
    Write-Host "  - graph.gv"
    Write-Host "  - graph.md"
} catch {
    Write-Host "❌ Error running Python script: $_"
    exit 1
}
