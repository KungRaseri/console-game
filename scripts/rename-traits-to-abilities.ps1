# Rename traits.json to abilities.json Script
# Renames all enemy traits.json files to abilities.json and updates metadata
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

Write-Host "=== Rename traits.json to abilities.json ===" -ForegroundColor Cyan
Write-Host "Mode: $(if ($WhatIf) { 'DRY RUN (no changes)' } else { 'LIVE (files will be renamed)' })" -ForegroundColor Yellow
Write-Host ""

function Rename-TraitsToAbilities {
    param(
        [string]$Category,
        [bool]$DryRun
    )
    
    $categoryPath = Join-Path $basePath $Category
    $traitsFile = Join-Path $categoryPath "traits.json"
    $abilitiesFile = Join-Path $categoryPath "abilities.json"
    $configFile = Join-Path $categoryPath ".cbconfig.json"
    
    Write-Host "Processing: $Category" -ForegroundColor Green
    
    # Check if traits.json exists
    if (-not (Test-Path $traitsFile)) {
        Write-Warning "  [X] traits.json not found, skipping"
        return $false
    }
    
    # Check if abilities.json already exists
    if (Test-Path $abilitiesFile) {
        Write-Warning "  [X] abilities.json already exists, skipping"
        return $false
    }
    
    try {
        # Read traits.json
        $content = Get-Content $traitsFile -Raw | ConvertFrom-Json
        
        Write-Host "  [OK] Loaded traits.json" -ForegroundColor Gray
        
        # Update metadata
        $content.metadata.description = $content.metadata.description -replace "trait definitions", "ability definitions"
        $content.metadata.description = $content.metadata.description -replace "traits", "abilities"
        $content.metadata.type = "ability_catalog"
        $content.metadata.lastUpdated = Get-Date -Format "yyyy-MM-dd"
        
        # Rename total_items to total_abilities if present
        if ($content.metadata.PSObject.Properties.Name -contains "total_items") {
            $totalCount = $content.metadata.total_items
            $content.metadata.PSObject.Properties.Remove("total_items")
            $content.metadata | Add-Member -NotePropertyName "total_abilities" -NotePropertyValue $totalCount -Force | Out-Null
        }
        
        Write-Host "  [OK] Updated metadata (trait_catalog -> ability_catalog)" -ForegroundColor Gray
        
        # Save as abilities.json
        if (-not $DryRun) {
            $content | ConvertTo-Json -Depth 100 | Set-Content $abilitiesFile -Encoding UTF8
            Write-Host "  [OK] Created abilities.json" -ForegroundColor Green
            
            # Delete old traits.json
            Remove-Item $traitsFile -Force
            Write-Host "  [OK] Deleted traits.json" -ForegroundColor Green
        } else {
            Write-Host "  [WARN] DRY RUN: Would create abilities.json and delete traits.json" -ForegroundColor Yellow
        }
        
        # Update .cbconfig.json if it exists
        if (Test-Path $configFile) {
            $config = Get-Content $configFile -Raw | ConvertFrom-Json
            
            # Check if fileIcons has traits entry
            if ($config.fileIcons.PSObject.Properties.Name -contains "traits") {
                $traitsIcon = $config.fileIcons.traits
                $config.fileIcons.PSObject.Properties.Remove("traits")
                $config.fileIcons | Add-Member -NotePropertyName "abilities" -NotePropertyValue $traitsIcon -Force | Out-Null
                
                if (-not $DryRun) {
                    $config | ConvertTo-Json -Depth 100 | Set-Content $configFile -Encoding UTF8
                    Write-Host "  [OK] Updated .cbconfig.json (traits -> abilities)" -ForegroundColor Green
                } else {
                    Write-Host "  [WARN] DRY RUN: Would update .cbconfig.json" -ForegroundColor Yellow
                }
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
    $success = Rename-TraitsToAbilities -Category $category -DryRun $WhatIf
    
    if ($success) {
        $processedCount++
    } else {
        $errorCount++
    }
}

# Summary
Write-Host ""
Write-Host "=== Rename Summary ===" -ForegroundColor Cyan
Write-Host "  Processed: $processedCount / $($enemyCategories.Count)" -ForegroundColor $(if ($processedCount -eq $enemyCategories.Count) { "Green" } else { "Yellow" })

if ($errorCount -gt 0) {
    Write-Host "  Errors: $errorCount" -ForegroundColor Red
}

Write-Host ""
if ($WhatIf) {
    Write-Host "DRY RUN COMPLETE - No files were modified" -ForegroundColor Yellow
    Write-Host "Run without -WhatIf to apply changes" -ForegroundColor Yellow
} else {
    Write-Host "RENAME COMPLETE" -ForegroundColor Green
    Write-Host "All traits.json files have been renamed to abilities.json" -ForegroundColor Green
}
