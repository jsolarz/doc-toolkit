param(
    [Parameter(Mandatory=$true)]
    [Alias("Type")]
    [string]$type,
    [Parameter(Mandatory=$true)]
    [Alias("Name")]
    [string]$name
)

$templatesDir = "templates"
$docsDir = "docs"
$date = Get-Date -Format "yyyy-MM-dd"

$template = "$templatesDir\$type.md"

if (!(Test-Path $template)) {
    Write-Host "Template not found: $template"
    exit 1
}

if (!(Test-Path $docsDir)) {
    New-Item -ItemType Directory -Path $docsDir | Out-Null
}

$sanitizedName = $name -replace " ", "_"
$output = "$docsDir\$date-$type-$sanitizedName.md"

Copy-Item $template $output

Write-Host "Created: $output"
