# PowerShell script to run tests with code coverage and generate reports
param(
    [string]$Configuration = "Debug",
    [string]$OutputPath = "TestResults",
    [int]$Threshold = 80
)

Write-Host "Running tests with code coverage..." -ForegroundColor Green

# Clean previous results
if (Test-Path $OutputPath) {
    Remove-Item -Recurse -Force $OutputPath
    Write-Host "Cleaned previous test results" -ForegroundColor Yellow
}

# Create output directory
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Run tests with coverage
Write-Host "Executing tests with Coverlet..." -ForegroundColor Cyan
dotnet test tests/XTinyLog.Tests/XTinyLog.Test.csproj `
    --configuration $Configuration `
    --collect:"XPlat Code Coverage" `
    --results-directory $OutputPath `
    --logger "trx;LogFileName=test-results.trx" `
    --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed or coverage threshold not met!" -ForegroundColor Red
    exit $LASTEXITCODE
}

# Find the coverage file
$coverageFiles = Get-ChildItem -Path $OutputPath -Filter "coverage.cobertura.xml" -Recurse
if ($coverageFiles.Count -eq 0) {
    # Try to find coverage files in subdirectories
    $coverageFiles = Get-ChildItem -Path $OutputPath -Filter "*.cobertura.xml" -Recurse
}

if ($coverageFiles.Count -eq 0) {
    Write-Host "No coverage files found!" -ForegroundColor Red
    exit 1
}

$coverageFile = $coverageFiles[0].FullName
Write-Host "Found coverage file: $coverageFile" -ForegroundColor Green

# Generate HTML report using ReportGenerator
Write-Host "Generating HTML coverage report..." -ForegroundColor Cyan
$reportPath = Join-Path $OutputPath "html"

dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.3.8 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ReportGenerator already installed or installation failed, continuing..." -ForegroundColor Yellow
}

reportgenerator `
    -reports:"$coverageFile" `
    -targetdir:"$reportPath" `
    -reporttypes:"Html;Badges;TextSummary" `
    -verbosity:Info `
    -title:"XTinyLog Code Coverage Report" `
    -tag:"$Configuration"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Coverage report generated successfully!" -ForegroundColor Green
    Write-Host "HTML Report: $reportPath/index.html" -ForegroundColor Cyan
    Write-Host "Badges: $reportPath/badge_*.svg" -ForegroundColor Cyan
    
    # Display summary
    $summaryFile = Join-Path $reportPath "Summary.txt"
    if (Test-Path $summaryFile) {
        Write-Host "`nCoverage Summary:" -ForegroundColor Yellow
        Get-Content $summaryFile | Write-Host
    }
} else {
    Write-Host "Failed to generate coverage report!" -ForegroundColor Red
    exit 1
}

Write-Host "`nCode coverage analysis completed successfully!" -ForegroundColor Green