# Script to add basic XML documentation to C# files with CS1591 warnings
# This script adds standard documentation patterns for commands, queries, handlers, etc.

$projectPath = "C:\code\console-game\RealmEngine.Core\RealmEngine.Core.csproj"

# Build and get CS1591 warnings for RealmEngine.Core only
Write-Host "Building project to find CS1591 warnings..."
$warnings = dotnet build $projectPath 2>&1 | Select-String "CS1591" | Where-Object { $_ -match "RealmEngine\.Core\\" }

# Extract unique file paths
$files = $warnings | ForEach-Object {
    if ($_ -match '([A-Z]:\\[^:]+\.cs)\((\d+),(\d+)\):.*member ''([^'']+)''') {
        [PSCustomObject]@{
            File = $matches[1]
            Line = [int]$matches[2]
            Member = $matches[4]
        }
    }
} | Group-Object File | ForEach-Object { $_.Group[0] }

Write-Host "Found $($files.Count) files with CS1591 warnings"
Write-Host "Processing files..."

$processedCount = 0
$totalFixed = 0

foreach ($fileInfo in $files) {
    $filePath = $fileInfo.File
    
    if (-not (Test-Path $filePath)) {
        continue
    }
    
    $content = Get-Content $filePath -Raw
    $originalContent = $content
    $fixed = 0
    
    # Pattern 1: Public constructors without XML docs
    $content = $content -replace '(?<!///.*\r?\n)(\s+)(public\s+\w+\()', '$1/// <summary>$1/// Initializes a new instance of the class.$1/// </summary>$1$2'
    
    # Pattern 2: Public Handle methods in handlers
    if ($content -match 'IRequestHandler') {
        $content = $content -replace '(?<!///.*\r?\n)(\s+)(public\s+(?:async\s+)?Task<[^>]+>\s+Handle\([^)]+\))', '$1/// <summary>$1/// Handles the request and returns the result.$1/// </summary>$1$2'
    }
    
    # Pattern 3: Record properties (public get)
    $content = $content -replace '(?<!///.*\r?\n)(\s+)(public\s+(?:required\s+)?(?:[\w<>?]+)\s+(\w+)\s*\{\s*get;)', '$1/// <summary>$1/// Gets the $3.$1/// </summary>$1$2'
    
    # Pattern 4: Public methods
    $content = $content -replace '(?<!///.*\r?\n)(\s+)(public\s+(?:virtual\s+|override\s+|async\s+)?(?:[\w<>?]+)\s+(\w+)\()', '$1/// <summary>$1/// $3 method.$1/// </summary>$1$2'
    
    if ($content -ne $originalContent) {
        Set-Content $filePath $content -NoNewline
        $fixed = 1
        $totalFixed++
    }
    
    $processedCount++
    if ($processedCount % 10 -eq 0) {
        Write-Host "Processed $processedCount / $($files.Count) files, fixed $totalFixed files"
    }
}

Write-Host "`nCompleted! Processed $processedCount files, modified $totalFixed files"
Write-Host "`nRebuilding to check remaining warnings..."
$remainingWarnings = dotnet build $projectPath 2>&1 | Select-String "CS1591" | Where-Object { $_ -match "RealmEngine\.Core\\" }
$remainingCount = ($remainingWarnings | Measure-Object).Count
Write-Host "Remaining CS1591 warnings: $remainingCount"
