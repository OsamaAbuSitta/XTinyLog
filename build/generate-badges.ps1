# PowerShell script to generate coverage badges
param(
    [string]$CoverageFile = "TestResults/coverage.cobertura.xml",
    [string]$OutputPath = "assets/badges"
)

Write-Host "Generating coverage badges..." -ForegroundColor Green

if (-not (Test-Path $CoverageFile)) {
    Write-Host "Coverage file not found: $CoverageFile" -ForegroundColor Red
    Write-Host "Please run tests with coverage first using: ./build/run-coverage.ps1" -ForegroundColor Yellow
    exit 1
}

# Create output directory
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Parse coverage percentage from Cobertura XML
[xml]$coverage = Get-Content $CoverageFile
$lineRate = [math]::Round([double]$coverage.coverage.'line-rate' * 100, 1)
$branchRate = [math]::Round([double]$coverage.coverage.'branch-rate' * 100, 1)

Write-Host "Line Coverage: $lineRate%" -ForegroundColor Cyan
Write-Host "Branch Coverage: $branchRate%" -ForegroundColor Cyan

# Determine badge colors based on coverage percentage
function Get-BadgeColor($percentage) {
    if ($percentage -ge 90) { return "brightgreen" }
    elseif ($percentage -ge 80) { return "green" }
    elseif ($percentage -ge 70) { return "yellowgreen" }
    elseif ($percentage -ge 60) { return "yellow" }
    elseif ($percentage -ge 50) { return "orange" }
    else { return "red" }
}

$lineColor = Get-BadgeColor $lineRate
$branchColor = Get-BadgeColor $branchRate

# Generate badge URLs
$lineBadgeUrl = "https://img.shields.io/badge/coverage-$lineRate%25-$lineColor"
$branchBadgeUrl = "https://img.shields.io/badge/branch%20coverage-$branchRate%25-$branchColor"

# Download badges
try {
    Write-Host "Downloading line coverage badge..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri $lineBadgeUrl -OutFile "$OutputPath/coverage-line.svg"
    
    Write-Host "Downloading branch coverage badge..." -ForegroundColor Yellow
    Invoke-WebRequest -Uri $branchBadgeUrl -OutFile "$OutputPath/coverage-branch.svg"
    
    Write-Host "Badges generated successfully!" -ForegroundColor Green
    Write-Host "Line Coverage Badge: $OutputPath/coverage-line.svg" -ForegroundColor Cyan
    Write-Host "Branch Coverage Badge: $OutputPath/coverage-branch.svg" -ForegroundColor Cyan
    
    # Create markdown for README
    $markdown = @"
## Code Coverage

[![Line Coverage](./assets/badges/coverage-line.svg)](./TestResults/html/index.html)
[![Branch Coverage](./assets/badges/coverage-branch.svg)](./TestResults/html/index.html)

- **Line Coverage:** $lineRate%
- **Branch Coverage:** $branchRate%

"@
    
    $markdown | Out-File "$OutputPath/coverage-badges.md" -Encoding UTF8
    Write-Host "Markdown snippet saved to: $OutputPath/coverage-badges.md" -ForegroundColor Cyan
    
} catch {
    Write-Host "Failed to download badges: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}