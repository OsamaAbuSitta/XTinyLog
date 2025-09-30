# PowerShell script to fix common StyleCop violations
param(
    [string]$Path = "src/XTinyLog"
)

Write-Host "Fixing StyleCop violations in $Path..." -ForegroundColor Green

# Get all C# files
$files = Get-ChildItem -Path $Path -Filter "*.cs" -Recurse

foreach ($file in $files) {
    Write-Host "Processing $($file.Name)..." -ForegroundColor Yellow
    
    # Read file content
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    
    # Fix SA1028: Remove trailing whitespace
    $content = $content -replace '[ \t]+\r?\n', "`n"
    $content = $content -replace '[ \t]+$', ''
    
    # Fix SA1518: Ensure file ends with single newline
    $content = $content.TrimEnd()
    $content += "`n"
    
    # Fix SA1629: Add periods to documentation comments
    $content = $content -replace '/// <summary>\s*([^<]+[^.])\s*</summary>', '/// <summary>$1.</summary>'
    $content = $content -replace '/// <param name="[^"]+">([^<]+[^.])\s*</param>', '/// <param name="$1">$1.</param>'
    $content = $content -replace '/// <returns>([^<]+[^.])\s*</returns>', '/// <returns>$1.</returns>'
    
    # Write back if changed
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "  Fixed violations in $($file.Name)" -ForegroundColor Cyan
    } else {
        Write-Host "  No changes needed for $($file.Name)" -ForegroundColor Gray
    }
}

Write-Host "StyleCop violation fixes completed!" -ForegroundColor Green