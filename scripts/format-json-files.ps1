# Format all JSON files with consistent indentation while preserving Unicode (emoji) characters

param(
    [string]$Path = "Game.Data\Data\Json"
)

$ErrorActionPreference = "Continue"
$filesProcessed = 0
$filesFormatted = 0
$filesSkipped = 0

Write-Host "Formatting JSON files in: $Path" -ForegroundColor Cyan
Write-Host ""

Get-ChildItem -Path $Path -Filter "*.json" -Recurse | ForEach-Object {
    $file = $_
    $filesProcessed++
    
    try {
        # Read with UTF-8 encoding (handles BOM automatically)
        $content = [System.IO.File]::ReadAllText($file.FullName, [System.Text.UTF8Encoding]::new($true))
        
        # Parse JSON
        $json = $content | ConvertFrom-Json -ErrorAction Stop
        
        # Convert back to formatted JSON
        $formatted = $json | ConvertTo-Json -Depth 100
        
        # Convert 4-space indentation to 2-space and fix spacing after colons
        $formatted = $formatted -replace '(?m)^(    )+', { '  ' * ($_.Value.Length / 4) }
        $formatted = $formatted -replace '":  ', '": '
        
        # Write back with UTF-8 encoding WITHOUT BOM (preserves emojis correctly)
        $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
        [System.IO.File]::WriteAllText($file.FullName, $formatted, $utf8NoBom)
        
        $filesFormatted++
        Write-Host "  OK $($file.Name)" -ForegroundColor Green
    }
    catch [System.ArgumentException] {
        # Handle invalid JSON or parse errors
        Write-Host "  SKIP $($file.Name) - Invalid JSON" -ForegroundColor Red
        $filesSkipped++
    }
    catch {
        Write-Host "  SKIP $($file.Name) - $($_.Exception.Message)" -ForegroundColor Red
        $filesSkipped++
    }
}

Write-Host ""
Write-Host "Files processed: $filesProcessed" -ForegroundColor White
Write-Host "Files formatted: $filesFormatted" -ForegroundColor Green
Write-Host "Files skipped:   $filesSkipped" -ForegroundColor Yellow
