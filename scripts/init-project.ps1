param(
    [Parameter(Mandatory=$true)]
    [string]$Name
)

$project = $Name

Write-Host "Creating project: $project"

New-Item -ItemType Directory -Path $project -Force | Out-Null
New-Item -ItemType Directory -Path "$project/docs" -Force | Out-Null
New-Item -ItemType Directory -Path "$project/source" -Force | Out-Null
New-Item -ItemType Directory -Path "$project/.cursor" -Force | Out-Null
New-Item -ItemType Directory -Path "$project/semantic-index" -Force | Out-Null
New-Item -ItemType Directory -Path "$project/knowledge-graph" -Force | Out-Null

# cursor.json
@"
{
  "version": 1,
  "rules": [
    "../../doc-toolkit/cursor/rules.mdc",
    "../../doc-toolkit/cursor/sow-rules.mdc"
  ],
  "documentTypes": {
    "sow": { "template": "../../doc-toolkit/templates/sow.md" },
    "prd": { "template": "../../doc-toolkit/templates/prd.md" },
    "rfp": { "template": "../../doc-toolkit/templates/rfp.md" },
    "tender": { "template": "../../doc-toolkit/templates/tender.md" },
    "architecture": { "template": "../../doc-toolkit/templates/architecture.md" },
    "solution": { "template": "../../doc-toolkit/templates/solution.md" }
  }
}
"@ | Out-File "$project/.cursor/cursor.json" -Encoding utf8

# README
@"
# $project

This project workspace was generated using the global Documentation Toolkit.

## Structure
- docs/
- source/
- semantic-index/
- knowledge-graph/
- .cursor/

## Commands
.\generate-doc.ps1 -Type sow -Name "My SOW"
..\doc-toolkit\scripts\semantic-index.ps1
..\doc-toolkit\scripts\build-knowledge-graph.ps1
"@ | Out-File "$project/README.md" -Encoding utf8

# .gitignore
@"
/docs/*
/source/*
/semantic-index/*
/knowledge-graph/*
!/docs/.keep
!/source/.keep
!/semantic-index/.keep
!/knowledge-graph/.keep
/.cursor/*
!/.cursor/cursor.json
"@ | Out-File "$project/.gitignore" -Encoding utf8