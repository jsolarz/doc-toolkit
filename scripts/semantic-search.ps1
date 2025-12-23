param(
    [string]$Query,
    [string]$IndexPath = "./semantic-index",
    [int]$TopK = 5
)

if ($Query -eq "") {
    Write-Host "Usage: semantic-search.ps1 -Query 'your question'"
    exit 1
}

# Check if Python is installed
try {
    $pythonVersion = python --version 2>&1
    if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne $null) {
        throw "Python not found"
    }
} catch {
    Write-Host "❌ Python is not installed or not in PATH"
    Write-Host "Please install Python 3.10+ from https://www.python.org/"
    exit 1
}

# Check if required Python packages are installed
$requiredPackages = @("sentence_transformers", "numpy", "sklearn")
$missingPackages = @()

foreach ($package in $requiredPackages) {
    $checkScript = "import $package; print('OK')"
    $result = python -c $checkScript 2>&1
    if ($LASTEXITCODE -ne 0 -or $result -notmatch "OK") {
        $missingPackages += $package
    }
}

if ($missingPackages.Count -gt 0) {
    Write-Host "❌ Missing Python packages: $($missingPackages -join ', ')"
    Write-Host "Install with: pip install -r requirements.txt"
    exit 1
}

# Check if index exists
if (!(Test-Path "$IndexPath/vectors.npy") -or !(Test-Path "$IndexPath/index.json")) {
    Write-Host "❌ Semantic index not found at: $IndexPath"
    Write-Host "Run semantic-index.ps1 first to build the index"
    exit 1
}

$pythonScript = @"
import json, numpy as np
from sentence_transformers import SentenceTransformer
from sklearn.metrics.pairwise import cosine_similarity

import os
query = '$Query'
index_path = os.path.normpath(r'$IndexPath')
top_k = $TopK

model = SentenceTransformer('all-MiniLM-L6-v2')

vectors = np.load(index_path + "/vectors.npy")
with open(index_path + "/index.json", "r", encoding="utf-8") as f:
    entries = json.load(f)

q_emb = model.encode(query).reshape(1, -1)
scores = cosine_similarity(q_emb, vectors)[0]

top_idx = scores.argsort()[-top_k:][::-1]

print("Top results:")
for i in top_idx:
    e = entries[i]
    print("\n---")
    print("File:", e["file"])
    print("Path:", e["path"])
    print("Score:", scores[i])
    print("Chunk:", e["chunk"][:500], "...")
"@

# Write Python script to temp location
$tempDir = [System.IO.Path]::GetTempPath()
$pyFile = Join-Path $tempDir "semantic_search_$(Get-Date -Format 'yyyyMMddHHmmss').py"
$pythonScript | Out-File $pyFile -Encoding utf8

try {
    python $pyFile
    if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne $null) {
        Write-Host "❌ Python script failed with exit code: $LASTEXITCODE"
        exit 1
    }
} catch {
    Write-Host "❌ Error running Python script: $_"
    exit 1
} finally {
    # Clean up temporary file
    if (Test-Path $pyFile) {
        Remove-Item $pyFile -Force -ErrorAction SilentlyContinue
    }
}
