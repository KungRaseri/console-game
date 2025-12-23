# Enemy Data Consolidation Script
# Consolidates prefixes.json and suffixes.json into names.json for all enemy categories
# Version: 1.0
# Date: 2025-12-17

param(
    [switch]$WhatIf = $false
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
$VerbosePreference = "SilentlyContinue"
$DebugPreference = "SilentlyContinue"

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

Write-Host "=== Enemy Data Consolidation ===" -ForegroundColor Cyan
Write-Host "Mode: $(if ($WhatIf) { 'DRY RUN (no changes)' } else { 'LIVE (files will be modified)' })" -ForegroundColor Yellow
Write-Host ""

function Consolidate-EnemyData {
    param(
        [string]$Category,
        [bool]$DryRun
    )
    
    $categoryPath = Join-Path $basePath $Category
    $namesFile = Join-Path $categoryPath "names.json"
    $prefixesFile = Join-Path $categoryPath "prefixes.json"
    $suffixesFile = Join-Path $categoryPath "suffixes.json"
    $typesFile = Join-Path $categoryPath "types.json"
    
    Write-Host "Processing: $Category" -ForegroundColor Green
    
    # Check if files exist
    if (-not (Test-Path $namesFile)) {
        Write-Warning "  ✗ names.json not found, skipping"
        return $false
    }
    
    if (-not (Test-Path $prefixesFile)) {
        Write-Warning "  ✗ prefixes.json not found, skipping"
        return $false
    }
    
    if (-not (Test-Path $suffixesFile)) {
        Write-Warning "  ✗ suffixes.json not found, skipping"
        return $false
    }
    
    # Read JSON files
    try {
        $names = Get-Content $namesFile -Raw | ConvertFrom-Json
        $prefixes = Get-Content $prefixesFile -Raw | ConvertFrom-Json
        $suffixes = Get-Content $suffixesFile -Raw | ConvertFrom-Json
        
        Write-Host "  ✓ Loaded existing files" -ForegroundColor Gray
        
        # Transform prefix items (name → value)
        $prefixComponents = @()
        foreach ($item in $prefixes.items) {
            $component = @{
                value = $item.name
                rarityWeight = $item.rarityWeight
            }
            
            # Add traits if present
            if ($item.traits) {
                $component.traits = $item.traits
            }
            
            # Add description if present
            if ($item.description) {
                $component.description = $item.description
            }
            
            $prefixComponents += $component
        }
        
        Write-Host "  ✓ Transformed $($prefixComponents.Count) prefix components" -ForegroundColor Gray
        
        # Transform suffix items (name → value)
        $suffixComponents = @()
        foreach ($item in $suffixes.items) {
            $component = @{
                value = $item.name
                rarityWeight = $item.rarityWeight
            }
            
            # Add traits if present
            if ($item.traits) {
                $component.traits = $item.traits
            }
            
            # Add description if present
            if ($item.description) {
                $component.description = $item.description
            }
            
            $suffixComponents += $component
        }
        
        Write-Host "  ✓ Transformed $($suffixComponents.Count) suffix components" -ForegroundColor Gray
        
        # Add prefix and suffix to components
        $names.components | Add-Member -NotePropertyName "prefix" -NotePropertyValue $prefixComponents -Force | Out-Null
        $names.components | Add-Member -NotePropertyName "suffix" -NotePropertyValue $suffixComponents -Force | Out-Null
        
        # Update metadata
        $names.metadata.version = "4.0"
        $names.metadata | Add-Member -NotePropertyName "supportsTraits" -NotePropertyValue $true -Force | Out-Null
        
        # Update componentKeys
        if ($names.metadata.componentKeys -notcontains "prefix") {
            $names.metadata.componentKeys = @("prefix") + $names.metadata.componentKeys
        }
        if ($names.metadata.componentKeys -notcontains "suffix") {
            $names.metadata.componentKeys += "suffix"
        }
        
        # Update patternTokens
        if ($names.metadata.patternTokens -notcontains "prefix") {
            $names.metadata.patternTokens = @("prefix") + $names.metadata.patternTokens
        }
        if ($names.metadata.patternTokens -notcontains "suffix") {
            $names.metadata.patternTokens += "suffix"
        }
        
        # Update notes
        $newNotes = @(
            "Prefixes and suffixes merged from separate files in v4.0",
            "Traits are applied when components are selected in patterns",
            "Trait merging numbers take highest strings take last booleans use OR",
            "Emergent rarity calculated from combined component weights"
        )
        
        $existingNotes = @($names.metadata.notes)
        foreach ($note in $newNotes) {
            if ($existingNotes -notcontains $note) {
                $existingNotes += $note
            }
        }
        $names.metadata.notes = $existingNotes
        
        # Add new patterns with prefix/suffix
        $existingPatterns = @($names.patterns)
        $newPatterns = @(
            @{
                pattern = "{prefix} {base}"
                weight = 15
                example = "Wild Wolf"
            },
            @{
                pattern = "{base} {suffix}"
                weight = 15
                example = "Wolf the Fierce"
            },
            @{
                pattern = "{prefix} {base} {suffix}"
                weight = 5
                example = "Wild Wolf the Fierce"
            }
        )
        
        foreach ($pattern in $newPatterns) {
            $exists = $false
            foreach ($existingPattern in $existingPatterns) {
                if ($existingPattern.pattern -eq $pattern.pattern) {
                    $exists = $true
                    break
                }
            }
            if (-not $exists) {
                $existingPatterns += $pattern
            }
        }
        
        $names.patterns = $existingPatterns
        $names.metadata.totalPatterns = $existingPatterns.Count
        
        Write-Host "  ✓ Updated metadata and added patterns" -ForegroundColor Gray
        
        if (-not $DryRun) {
            # Write updated names.json
            $json = $names | ConvertTo-Json -Depth 100
            $json | Set-Content $namesFile -Encoding UTF8
            Write-Host "  ✓ Saved updated names.json" -ForegroundColor Gray
            
            # Update types.json metadata
            if (Test-Path $typesFile) {
                $types = Get-Content $typesFile -Raw | ConvertFrom-Json
                $types.metadata.type = "item_catalog"
                $types.metadata.usage = "Enemy type definitions with base stats - used as {base} token in pattern generation"
                
                # Add notes if missing
                if (-not $types.metadata.notes) {
                    $types.metadata | Add-Member -NotePropertyName "notes" -NotePropertyValue @(
                        "Type names are used as {base} token in name patterns",
                        "Stats define base values before modifiers are applied"
                    )
                }
                
                $typesJson = $types | ConvertTo-Json -Depth 100
                $typesJson | Set-Content $typesFile -Encoding UTF8
                Write-Host "  ✓ Updated types.json metadata" -ForegroundColor Gray
            }
            
            # Delete old files
            Remove-Item $prefixesFile -Force
            Remove-Item $suffixesFile -Force
            Write-Host "  ✓ Deleted prefixes.json and suffixes.json" -ForegroundColor Gray
        } else {
            Write-Host "  ⚠ DRY RUN: Would save names.json, update types.json, and delete prefix/suffix files" -ForegroundColor Yellow
        }
        
        Write-Host "  ✓ $Category completed successfully`n" -ForegroundColor Green
        return $true
        
    } catch {
        Write-Error "  ✗ Error processing $Category $_"
        return $false
    }
}

# Process each category
foreach ($category in $enemyCategories) {
    try {
        $success = Consolidate-EnemyData -Category $category -DryRun $WhatIf
        if ($success) {
            $processedCount++
        } else {
            $errorCount++
        }
    } catch {
        Write-Error "Failed to process $category $_"
        $errorCount++
    }
}

# Summary
Write-Host ""
Write-Host "=== Consolidation Summary ===" -ForegroundColor Cyan
Write-Host "  Processed: $processedCount / $($enemyCategories.Count)" -ForegroundColor Green
if ($errorCount -gt 0) {
    Write-Host "  Errors: $errorCount" -ForegroundColor Red
}

if ($WhatIf) {
    Write-Host ""
    Write-Host "DRY RUN COMPLETE - No files were modified" -ForegroundColor Yellow
    Write-Host "Run without -WhatIf to apply changes" -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "CONSOLIDATION COMPLETE!" -ForegroundColor Green
}
