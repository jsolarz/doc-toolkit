# Validation script for Documentation Toolkit setup
# Checks Python installation, dependencies, and external tools

param(
    [switch]$Fix = $false
)

$errors = @()
$warnings = @()
$info = @()

Write-Host "🔍 Validating Documentation Toolkit Setup" -ForegroundColor Cyan
Write-Host ""

# Check Python installation
Write-Host "Checking Python installation..." -ForegroundColor Yellow
try {
    $pythonVersion = python --version 2>&1
    if ($LASTEXITCODE -eq 0 -or $pythonVersion -match "Python") {
        $info += "✓ Python found: $pythonVersion"
        Write-Host "  ✓ Python found: $pythonVersion" -ForegroundColor Green
        
        # Check Python version
        $versionMatch = $pythonVersion -match "Python (\d+)\.(\d+)"
        if ($versionMatch) {
            $major = [int]$matches[1]
            $minor = [int]$matches[2]
            if ($major -lt 3 -or ($major -eq 3 -and $minor -lt 10)) {
                $errors += "Python version must be 3.10 or higher (found $major.$minor)"
                Write-Host "  ❌ Python version must be 3.10 or higher" -ForegroundColor Red
            } else {
                Write-Host "  ✓ Python version is compatible" -ForegroundColor Green
            }
        }
    } else {
        throw "Python not found"
    }
} catch {
    $errors += "Python is not installed or not in PATH"
    Write-Host "  ❌ Python is not installed or not in PATH" -ForegroundColor Red
    Write-Host "     Install from: https://www.python.org/" -ForegroundColor Yellow
}

Write-Host ""

# Check Python packages
Write-Host "Checking Python packages..." -ForegroundColor Yellow
$requiredPackages = @{
    "sentence_transformers" = "sentence-transformers"
    "numpy" = "numpy"
    "sklearn" = "scikit-learn"
    "docx" = "python-docx"
    "pptx" = "python-pptx"
    "pypdf" = "pypdf"
    "PIL" = "Pillow"
    "pytesseract" = "pytesseract"
}

$missingPackages = @()
foreach ($package in $requiredPackages.Keys) {
    $packageName = $requiredPackages[$package]
    try {
        $checkScript = "import $package; print('OK')"
        $result = python -c $checkScript 2>&1
        if ($LASTEXITCODE -eq 0 -and $result -match "OK") {
            Write-Host "  ✓ $packageName" -ForegroundColor Green
        } else {
            throw "Not installed"
        }
    } catch {
        $missingPackages += $packageName
        Write-Host "  ❌ $packageName (missing)" -ForegroundColor Red
    }
}

if ($missingPackages.Count -gt 0) {
    $errors += "Missing Python packages: $($missingPackages -join ', ')"
    Write-Host ""
    Write-Host "  Install missing packages with:" -ForegroundColor Yellow
    Write-Host "    pip install $($missingPackages -join ' ')" -ForegroundColor Yellow
    Write-Host "  Or install all dependencies:" -ForegroundColor Yellow
    Write-Host "    pip install -r requirements.txt" -ForegroundColor Yellow
    
    if ($Fix) {
        Write-Host ""
        Write-Host "  Attempting to install missing packages..." -ForegroundColor Cyan
        foreach ($pkg in $missingPackages) {
            pip install $pkg
        }
    }
}

Write-Host ""

# Check external tools
Write-Host "Checking external tools..." -ForegroundColor Yellow

# Check Poppler (pdftotext)
try {
    $popplerCheck = pdftotext -v 2>&1
    if ($LASTEXITCODE -eq 0 -or $popplerCheck -match "pdftotext") {
        Write-Host "  ✓ Poppler (pdftotext) found" -ForegroundColor Green
    } else {
        throw "Not found"
    }
} catch {
    $warnings += "Poppler (pdftotext) not found - PDF text extraction may fail"
    Write-Host "  ⚠ Poppler (pdftotext) not found" -ForegroundColor Yellow
    Write-Host "     Install with: choco install poppler" -ForegroundColor Yellow
}

# Check Tesseract (optional)
try {
    $tesseractCheck = tesseract --version 2>&1
    if ($LASTEXITCODE -eq 0 -or $tesseractCheck -match "tesseract") {
        Write-Host "  ✓ Tesseract OCR found" -ForegroundColor Green
    } else {
        throw "Not found"
    }
} catch {
    $warnings += "Tesseract OCR not found - Image OCR will not work"
    Write-Host "  ⚠ Tesseract OCR not found (optional)" -ForegroundColor Yellow
    Write-Host "     Install with: choco install tesseract" -ForegroundColor Yellow
}

Write-Host ""

# Check directory structure
Write-Host "Checking directory structure..." -ForegroundColor Yellow
$requiredDirs = @("scripts", "templates", ".cursor")
foreach ($dir in $requiredDirs) {
    if (Test-Path $dir) {
        Write-Host "  ✓ $dir/" -ForegroundColor Green
    } else {
        $errors += "Required directory missing: $dir"
        Write-Host "  ❌ $dir/ (missing)" -ForegroundColor Red
    }
}

Write-Host ""

# Check templates
Write-Host "Checking templates..." -ForegroundColor Yellow
$requiredTemplates = @("prd.md", "rfp.md", "tender.md", "sow.md", "architecture.md", "solution.md")
foreach ($template in $requiredTemplates) {
    $templatePath = Join-Path "templates" $template
    if (Test-Path $templatePath) {
        Write-Host "  ✓ $template" -ForegroundColor Green
    } else {
        $warnings += "Template missing: $template"
        Write-Host "  ⚠ $template (missing)" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan

# Summary
if ($errors.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-Host "✅ All checks passed! Setup is complete." -ForegroundColor Green
    exit 0
} elseif ($errors.Count -eq 0) {
    Write-Host "⚠️  Setup complete with warnings:" -ForegroundColor Yellow
    foreach ($warning in $warnings) {
        Write-Host "   - $warning" -ForegroundColor Yellow
    }
    exit 0
} else {
    Write-Host "❌ Setup validation failed with errors:" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "   - $error" -ForegroundColor Red
    }
    if ($warnings.Count -gt 0) {
        Write-Host ""
        Write-Host "Warnings:" -ForegroundColor Yellow
        foreach ($warning in $warnings) {
            Write-Host "   - $warning" -ForegroundColor Yellow
        }
    }
    Write-Host ""
    Write-Host "Run with -Fix to attempt automatic fixes:" -ForegroundColor Cyan
    Write-Host "  .\validate-setup.ps1 -Fix" -ForegroundColor Cyan
    exit 1
}
