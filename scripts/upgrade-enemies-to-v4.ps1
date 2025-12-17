# Enemy Data v4.0 Upgrade Script
# Upgrades enemy names.json from v3.0 to v4.0 and updates types.json metadata
# Version: 1.0
# Date: 2025-12-17

param(
    [switch]$WhatIf = $false
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Enemy categories to process
$enemyCategories = @(
    "beasts",
    "demons",
    "dragons",
    "elementals",
    "goblinoids",
    "humanoids",
    "insects",
    "orcs",
    "plants",
    "reptilians",
    "trolls",
    "undead",
    "vampires"
)

$basePath = "Game.Shared\Data\Json\enemies"
$processedCount = 0
$errorCount = 0

Write-Host "=== Enemy Data v4.0 Upgrade ===" -ForegroundColor Cyan
Write-Host "Mode: $(if ($WhatIf) { 'DRY RUN (no changes)' } else { 'LIVE (files will be modified)' })" -ForegroundColor Yellow
Write-Host ""

function Upgrade-EnemyData {
    param(
        [string]$Category,
        [bool]$DryRun
    )
    
    $categoryPath = Join-Path $basePath $Category
    $namesFile = Join-Path $categoryPath "names.json"
    $typesFile = Join-Path $categoryPath "types.json"
    
    Write-Host "Processing: $Category" -ForegroundColor Green
    
    # Check if files exist
    if (-not (Test-Path $namesFile)) {
        Write-Warning "  [X] names.json not found, skipping"
        return $false
    }
    
    if (-not (Test-Path $typesFile)) {
        Write-Warning "  [X] types.json not found, skipping"
        return $false
    }
    
    try {
        # Read JSON files
        $names = Get-Content $namesFile -Raw | ConvertFrom-Json
        $types = Get-Content $typesFile -Raw | ConvertFrom-Json
        
        Write-Host "  [OK] Loaded existing files" -ForegroundColor Gray
        
        # Check current version
        $currentVersion = $names.metadata.version
        Write-Host "  [INFO] Current version: $currentVersion" -ForegroundColor Gray
        
        # Upgrade names.json to v4.0
        $namesChanged = $false
        
        if ($names.metadata.version -ne "4.0") {
            $names.metadata.version = "4.0"
            $namesChanged = $true
        }
        
        # Add supports_traits if not present
        if (-not ($names.metadata.PSObject.Properties.Name -contains "supports_traits")) {
            $names.metadata | Add-Member -NotePropertyName "supports_traits" -NotePropertyValue $true -Force | Out-Null
            $namesChanged = $true
        }
        
        # Update last_updated
        if ($namesChanged) {
            $names.metadata.last_updated = Get-Date -Format "yyyy-MM-dd"
        }
        
        # Add v4.0 note if not present
        $v4Note = "Upgraded to v4.0 with supports_traits metadata"
        $notesArray = @($names.metadata.notes)
        if ($notesArray -notcontains $v4Note) {
            $notesArray += $v4Note
            $names.metadata.notes = $notesArray
            $namesChanged = $true
        }
        
        if ($namesChanged) {
            Write-Host "  [OK] Updated names.json to v4.0" -ForegroundColor Gray
        } else {
            Write-Host "  [INFO] names.json already at v4.0" -ForegroundColor Gray
        }
        
        # Update types.json metadata
        $typesChanged = $false
        
        # Change enemy_catalog to item_catalog for consistency
        if ($types.metadata.type -eq "enemy_catalog") {
            $types.metadata.type = "item_catalog"
            $typesChanged = $true
        }
        
        # Add usage field if not present
        if (-not ($types.metadata.PSObject.Properties.Name -contains "usage")) {
            $types.metadata | Add-Member -NotePropertyName "usage" -NotePropertyValue "Provides base enemy types for pattern generation" -Force | Out-Null
            $typesChanged = $true
        }
        
        # Update last_updated
        if ($typesChanged) {
            $types.metadata.last_updated = Get-Date -Format "yyyy-MM-dd"
        }
        
        if ($typesChanged) {
            Write-Host "  [OK] Updated types.json metadata" -ForegroundColor Gray
        } else {
            Write-Host "  [INFO] types.json already up to date" -ForegroundColor Gray
        }
        
        # Save files
        if (-not $DryRun) {
            if ($namesChanged) {
                $names | ConvertTo-Json -Depth 100 | Set-Content $namesFile -Encoding UTF8
                Write-Host "  [OK] Saved names.json" -ForegroundColor Green
            }
            
            if ($typesChanged) {
                $types | ConvertTo-Json -Depth 100 | Set-Content $typesFile -Encoding UTF8
                Write-Host "  [OK] Saved types.json" -ForegroundColor Green
            }
        } else {
            if ($namesChanged -or $typesChanged) {
                Write-Host "  [WARN] DRY RUN: Would save changes to files" -ForegroundColor Yellow
            }
        }
        
        Write-Host "  [OK] $Category completed successfully" -ForegroundColor Green
        Write-Host ""
        
        return $true
        
    } catch {
        Write-Error "  [X] Error processing $Category : $_"
        return $false
    }
}

# Process all enemy categories
foreach ($category in $enemyCategories) {
    $success = Upgrade-EnemyData -Category $category -DryRun $WhatIf
    
    if ($success) {
        $processedCount++
    } else {
        $errorCount++
    }
}

# Summary
Write-Host ""
Write-Host "=== Upgrade Summary ===" -ForegroundColor Cyan
Write-Host "  Processed: $processedCount / $($enemyCategories.Count)" -ForegroundColor $(if ($processedCount -eq $enemyCategories.Count) { "Green" } else { "Yellow" })

if ($errorCount -gt 0) {
    Write-Host "  Errors: $errorCount" -ForegroundColor Red
}

Write-Host ""
if ($WhatIf) {
    Write-Host "DRY RUN COMPLETE - No files were modified" -ForegroundColor Yellow
    Write-Host "Run without -WhatIf to apply changes" -ForegroundColor Yellow
} else {
    Write-Host "UPGRADE COMPLETE" -ForegroundColor Green
}
