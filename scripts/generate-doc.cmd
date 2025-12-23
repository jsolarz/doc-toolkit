@echo off
setlocal enabledelayedexpansion

REM ============================
REM generate-doc.cmd
REM Usage: generate-doc.cmd <type> "Document Name"
REM ============================

if "%~1"=="" (
    echo Usage: generate-doc.cmd ^<type^> "Document Name"
    echo Types: prd, rfp, tender, architecture, solution, sow, sla, spec, api, data
    exit /b 1
)

if "%~2"=="" (
    echo You must provide a document name.
    exit /b 1
)

set TYPE=%~1
set NAME=%~2
set DATE=%DATE:~10,4%-%DATE:~4,2%-%DATE:~7,2%

set TEMPLATE=templates\%TYPE%.md

if not exist "%TEMPLATE%" (
    echo Template not found: %TEMPLATE%
    exit /b 1
)

if not exist docs (
    mkdir docs
)

set OUTPUT=docs\%DATE%-%TYPE%-%NAME:.=_%.md

copy "%TEMPLATE%" "%OUTPUT%" >nul

echo Created: %OUTPUT%
