param(
    [string]$SourcePath = "./source",
    [string]$IndexPath = "./semantic-index",
    [int]$ChunkSize = 800,
    [int]$ChunkOverlap = 200
)

Write-Host "🔍 Building semantic index from: $SourcePath"

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
    "sentence_transformers" = "sentence-transformers"
    "numpy" = "numpy"
    "docx" = "python-docx"
    "pptx" = "python-pptx"
    "pypdf" = "pypdf"
    "pytesseract" = "pytesseract"
    "PIL" = "Pillow"
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

# Ensure index folder exists
if (!(Test-Path $IndexPath)) {
    New-Item -ItemType Directory -Path $IndexPath | Out-Null
}

# Python script for embedding + vector store
$pythonScript = @"
import os, json, numpy as np
from sentence_transformers import SentenceTransformer
from docx import Document
from pptx import Presentation
from pypdf import PdfReader

import pytesseract
from PIL import Image

import os
source = os.path.normpath(r'$SourcePath')
index_path = os.path.normpath(r'$IndexPath')
chunk_size = $ChunkSize
chunk_overlap = $ChunkOverlap

model = SentenceTransformer('all-MiniLM-L6-v2')

def extract_text(path):
    ext = os.path.splitext(path)[1].lower()

    if ext in ['.txt', '.md', '.csv', '.json']:
        return open(path, 'r', encoding='utf-8', errors='ignore').read()

    if ext == '.docx':
        doc = Document(path)
        return "\n".join([p.text for p in doc.paragraphs])

    if ext == '.pdf':
        try:
            reader = PdfReader(path)
            return "\n".join([page.extract_text() or "" for page in reader.pages])
        except:
            return ""

    if ext == '.pptx':
        prs = Presentation(path)
        text = []
        for slide in prs.slides:
            for shape in slide.shapes:
                if hasattr(shape, "text"):
                    text.append(shape.text)
        return "\n".join(text)

    if ext in ['.png', '.jpg', '.jpeg']:
        try:
            return pytesseract.image_to_string(Image.open(path))
        except:
            return ""

    return ""

def chunk_text(text):
    words = text.split()
    chunks = []
    i = 0
    while i < len(words):
        chunk = " ".join(words[i:i+chunk_size])
        chunks.append(chunk)
        i += (chunk_size - chunk_overlap)
    return chunks

entries = []
vectors = []

for root, dirs, files in os.walk(source):
    for f in files:
        path = os.path.join(root, f)
        print("Processing:", f)

        text = extract_text(path)
        if not text.strip():
            continue

        chunks = chunk_text(text)

        for chunk in chunks:
            emb = model.encode(chunk)
            vectors.append(emb)
            entries.append({
                "file": f,
                "path": path,
                "chunk": chunk
            })

vectors = np.array(vectors)
np.save(os.path.join(index_path, "vectors.npy"), vectors)

with open(os.path.join(index_path, "index.json"), "w", encoding="utf-8") as f:
    json.dump(entries, f, indent=2)

print("Semantic index created.")
"@

# Write Python script to temp location
$tempDir = [System.IO.Path]::GetTempPath()
$pyFile = Join-Path $tempDir "semantic_index_$(Get-Date -Format 'yyyyMMddHHmmss').py"
$pythonScript | Out-File $pyFile -Encoding utf8

try {
    # Run Python
    python $pyFile
    if ($LASTEXITCODE -ne 0 -and $LASTEXITCODE -ne $null) {
        Write-Host "❌ Python script failed with exit code: $LASTEXITCODE"
        exit 1
    }
    Write-Host "✅ Semantic index created at: $IndexPath"
} catch {
    Write-Host "❌ Error running Python script: $_"
    exit 1
} finally {
    # Clean up temporary file
    if (Test-Path $pyFile) {
        Remove-Item $pyFile -Force -ErrorAction SilentlyContinue
    }
}
