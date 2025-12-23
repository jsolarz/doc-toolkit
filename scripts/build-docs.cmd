@echo off
echo ============================
echo Document Builder
echo ============================

echo Usage: build-docs.cmd type name
echo Example: build-docs.cmd prd "User Management"

if "%~1"=="" exit /b 1
if "%~2"=="" exit /b 1

generate-doc.cmd %1 "%2"
