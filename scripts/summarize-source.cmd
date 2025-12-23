@echo off
setlocal enabledelayedexpansion

set SOURCE=source
set OUTPUT=context.md

echo # Project Context Summary > %OUTPUT%
echo. >> %OUTPUT%

if not exist %SOURCE% (
    echo Source folder not found: %SOURCE%
    exit /b 1
)

for %%F in (%SOURCE%\*) do (
    echo Processing: %%~nxF

    echo ## File: %%~nxF >> %OUTPUT%
    echo. >> %OUTPUT%
    echo **Summary:** >> %OUTPUT%

    type "%%F" | more +0 >> %OUTPUT%

    echo. >> %OUTPUT%
    echo --- >> %OUTPUT%
    echo. >> %OUTPUT%
)

echo context.md generated successfully.
