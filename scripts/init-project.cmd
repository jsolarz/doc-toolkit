@echo off
setlocal enabledelayedexpansion

if "%~1"=="" (
    echo Usage: init-project.cmd ProjectName
    exit /b 1
)

set PROJECT=%~1

echo Creating project: %PROJECT%

mkdir "%PROJECT%"
mkdir "%PROJECT%\docs"
mkdir "%PROJECT%\source"
mkdir "%PROJECT%\.cursor"
mkdir "%PROJECT%\semantic-index"
mkdir "%PROJECT%\knowledge-graph"

REM cursor.json
(
echo {
echo   "version": 1,
echo   "rules": [
echo     "../../doc-toolkit/cursor/rules.mdc",
echo     "../../doc-toolkit/cursor/sow-rules.mdc"
echo   ],
echo   "documentTypes": {
echo     "sow": { "template": "../../doc-toolkit/templates/sow.md" },
echo     "prd": { "template": "../../doc-toolkit/templates/prd.md" },
echo     "rfp": { "template": "../../doc-toolkit/templates/rfp.md" },
echo     "tender": { "template": "../../doc-toolkit/templates/tender.md" },
echo     "architecture": { "template": "../../doc-toolkit/templates/architecture.md" },
echo     "solution": { "template": "../../doc-toolkit/templates/solution.md" }
echo   }
echo }
) > "%PROJECT%\.cursor\cursor.json"

REM README
(
echo # %PROJECT%
echo
echo This project workspace was generated using the global Documentation Toolkit.
echo
echo ## Structure
echo - docs/
echo - source/
echo - semantic-index/
echo - knowledge-graph/
echo - .cursor/
echo
echo ## Commands
echo generate-doc.cmd sow "My SOW"
echo ..\doc-toolkit\scripts\semantic-index.ps1
echo ..\doc-toolkit\scripts\build-knowledge-graph.ps1
) > "%PROJECT%\README.md"

REM .gitignore
(
echo /docs/*
echo /source/*
echo /semantic-index/*
echo /knowledge-graph/*
echo !/docs/.keep
echo !/source/.keep
echo !/semantic-index/.keep
echo !/knowledge-graph/.keep
echo /.cursor/*
echo !/.cursor/cursor.json
) > "%PROJECT%\.gitignore"

cd "%PROJECT%"
git init >nul
git add . >nul
git commit -m "Initial project structure created by Documentation Toolkit" >nul
cd ..

echo Project %PROJECT% created and Git initialized.
