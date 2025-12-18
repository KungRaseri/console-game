# Rename types.json to catalog.json
# This script renames all types.json files to catalog.json for consistency

[CmdletBinding(SupportsShouldProcess)]
param()

$ErrorActionPreference = "Stop"
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootPath = Split-Path -Parent $scriptPath
$dataPath = Join-Path $rootPath "Game.Shared\Data\Json"

Write-Host "=== Rename types.json to catalog.json ===" -ForegroundColor Cyan
Write-Host "Data Path: $dataPath" -ForegroundColor Gray
Write-Host ""

# Find all types.json files
$typesFiles = Get-ChildItem -Path $dataPath -Recurse -Filter "types.json" | 
    Where-Object { $_.FullName -notlike "*\.cbconfig.json" } |
    Sort-Object FullName -Unique

Write-Host "Found $($typesFiles.Count) types.json files to rename:" -ForegroundColor Yellow
Write-Host ""

$renamed = 0
$errors = 0

foreach ($file in $typesFiles) {
    $relativePath = $file.FullName.Replace("$dataPath\", "")
    $newName = "catalog.json"
    $newPath = Join-Path $file.DirectoryName $newName
    
    Write-Host "  $relativePath" -ForegroundColor White
    Write-Host "     -> $newName" -ForegroundColor Green
    
    try {
        if ($WhatIfPreference) {
            Write-Host "     [WHATIF] Would rename file" -ForegroundColor DarkGray
        } else {
            # Check if catalog.json already exists
            if (Test-Path $newPath) {
                Write-Host "     WARNING: catalog.json already exists, skipping!" -ForegroundColor Yellow
                $errors++
                continue
            }
            
            # Rename the file
            Rename-Item -Path $file.FullName -NewName $newName -Force
            Write-Host "     Renamed successfully" -ForegroundColor Green
            $renamed++
        }
    }
    catch {
        Write-Host "     ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $errors++
    }
    
    Write-Host ""
}

# Update .cbconfig.json files
Write-Host "=== Updating .cbconfig.json Files ===" -ForegroundColor Cyan
Write-Host ""

$cbconfigFiles = Get-ChildItem -Path $dataPath -Recurse -Filter ".cbconfig.json" |
    Sort-Object FullName -Unique

$configsUpdated = 0

foreach ($config in $cbconfigFiles) {
    $relativePath = $config.FullName.Replace("$dataPath\", "")
    
    try {
        $content = Get-Content $config.FullName -Raw | ConvertFrom-Json
        $updated = $false
        
        # Check if this config references types.json
        if ($content.files) {
            foreach ($fileEntry in $content.files) {
                if ($fileEntry.path -eq "types.json") {
                    Write-Host "  $relativePath" -ForegroundColor White
                    Write-Host "     Updating path: types.json to catalog.json" -ForegroundColor Yellow
                    
                    if ($WhatIfPreference) {
                        Write-Host "     [WHATIF] Would update config" -ForegroundColor DarkGray
                    } else {
                        $fileEntry.path = "catalog.json"
                        $updated = $true
                    }
                }
            }
        }
        
        if ($updated -and -not $WhatIfPreference) {
            $content | ConvertTo-Json -Depth 10 | Set-Content $config.FullName -Encoding UTF8
            Write-Host "     Updated successfully" -ForegroundColor Green
            $configsUpdated++
        }
    }
    catch {
        Write-Host "     ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $errors++
    }
    
    if ($updated -or $WhatIfPreference) {
        Write-Host ""
    }
}

# Summary
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host ""
if ($WhatIfPreference) {
    Write-Host "  [DRY RUN] Would rename: $($typesFiles.Count) files" -ForegroundColor Yellow
    Write-Host "  [DRY RUN] Would update configs: $configsUpdated files" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Run without -WhatIf to apply changes" -ForegroundColor Gray
} else {
    Write-Host "  Files renamed: $renamed" -ForegroundColor Green
    Write-Host "  Configs updated: $configsUpdated" -ForegroundColor Green
    if ($errors -gt 0) {
        Write-Host "  Errors: $errors" -ForegroundColor Red
    }
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Update code references from types.json to catalog.json" -ForegroundColor White
    Write-Host "  2. Update TypesEditor to CatalogEditor (or keep as-is)" -ForegroundColor White
    Write-Host "  3. Test ContentBuilder with renamed files" -ForegroundColor White
    Write-Host "  4. Commit changes to version control" -ForegroundColor White
}
Write-Host ""
