param(
    [string]$SourcePath = "./source",
    [string]$OutputFile = "./context.md",
    [int]$MaxChars = 5000
)

Write-Host "🔍 Building project context from: $SourcePath"

# Check if source path exists
if (!(Test-Path $SourcePath)) {
    Write-Host "❌ Source folder not found: $SourcePath"
    exit 1
}

# Check if we can write to output location
$outputDir = Split-Path -Parent $OutputFile
if ($outputDir -and !(Test-Path $outputDir)) {
    try {
        New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
    } catch {
        Write-Host "❌ Cannot create output directory: $outputDir"
        Write-Host "Error: $_"
        exit 1
    }
}

# Header
@"
# Project Context Summary
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm")
Source Folder: $SourcePath

This file contains:
- Extracted text from all source materials
- Summaries
- Key topics
- Named entities
- Suggested tags

---
"@ | Out-File $OutputFile -Encoding utf8

# Helper: simple LLM-friendly summarizer
function Summarize-Text {
    param([string]$text)

    if ($text.Length -le 800) { return $text }

    $sentences = $text -split '(?<=[.!?])\s+'
    $summary = $sentences[0..([Math]::Min(5, $sentences.Count - 1))] -join " "

    return $summary
}

# Helper: extract key topics
function Extract-Topics {
    param([string]$text)

    $words = $text.ToLower() -split '\W+' | Group-Object | Sort-Object Count -Descending
    $keywords = $words | Where-Object { $_.Name.Length -gt 4 } | Select-Object -First 10

    return ($keywords.Name -join ", ")
}

# Helper: extract named entities (simple heuristic)
function Extract-Entities {
    param([string]$text)

    $entities = Select-String -InputObject $text -Pattern '\b[A-Z][a-z]+(?:\s[A-Z][a-z]+)*\b' -AllMatches |
        ForEach-Object { $_.Matches.Value } |
        Sort-Object -Unique

    return ($entities -join ", ")
}

# Loop through files
Get-ChildItem -Path $SourcePath -File | ForEach-Object {

    $file = $_.FullName
    $name = $_.Name
    $ext = $_.Extension.ToLower()

    Write-Host "📄 Processing: $name"

    $content = ""

    switch ($ext) {

        ".txt" { $content = Get-Content $file -Raw }
        ".md"  { $content = Get-Content $file -Raw }
        ".csv" { $content = Get-Content $file -Raw }
        ".json" { $content = Get-Content $file -Raw }

        ".docx" {
            try {
                $word = New-Object -ComObject Word.Application
                $doc = $word.Documents.Open($file)
                $content = $doc.Content.Text
                $doc.Close()
                $word.Quit()
            } catch {
                $content = "[Unable to extract DOCX text]"
            }
        }

        ".pdf" {
            try {
                $content = (pdftotext $file - | Out-String)
            } catch {
                $content = "[Unable to extract PDF text — install Poppler]"
            }
        }

        default {
            $content = "[Unsupported file type: $ext]"
        }
    }

    # Trim content
    if ($content.Length -gt $MaxChars) {
        $content = $content.Substring(0, $MaxChars)
    }

    # Generate metadata
    $summary = Summarize-Text $content
    $topics = Extract-Topics $content
    $entities = Extract-Entities $content

    # Append to context.md
    @"
## File: $name
**Path:** $file  
**Size:** $($_.Length) bytes  
**Type:** $ext  

### Summary
$summary

### Key Topics
$topics

### Named Entities
$entities

### Extracted Text
```
$content
```

---
"@ | Out-File $OutputFile -Append -Encoding utf8
}

Write-Host "✅ context.md generated successfully."
