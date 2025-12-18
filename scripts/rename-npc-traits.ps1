# Rename NPC Traits Files Script
# Renames conflicting traits.json files in NPCs directory
# Version: 1.0
# Date: 2025-12-17

param(
    [switch]$WhatIf = $false
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Host "=== Rename NPC Traits Files ===" -ForegroundColor Cyan
Write-Host "Mode: $(if ($WhatIf) { 'DRY RUN (no changes)' } else { 'LIVE (files will be renamed)' })" -ForegroundColor Yellow
Write-Host ""

$changes = @(
    @{
        Old = "Game.Shared\Data\Json\npcs\dialogue\traits.json"
        New = "Game.Shared\Data\Json\npcs\dialogue\dialogue_styles.json"
        ConfigPath = "Game.Shared\Data\Json\npcs\dialogue\.cbconfig.json"
        Description = "Rename dialogue/traits.json to dialogue_styles.json"
        MetadataChanges = @{
            description = "Dialogue style templates with personality and formality variations"
            type = "dialogue_style_catalog"
        }
    },
    @{
        Old = "Game.Shared\Data\Json\npcs\personalities\traits.json"
        New = "Game.Shared\Data\Json\npcs\personalities\personality_traits.json"
        ConfigPath = "Game.Shared\Data\Json\npcs\personalities\.cbconfig.json"
        Description = "Rename personalities/traits.json to personality_traits.json"
        MetadataChanges = @{
            description = "Personality trait catalog with weight-based rarity for NPC characterization"
            type = "personality_trait_catalog"
        }
    }
)

$successCount = 0
$errorCount = 0

foreach ($change in $changes) {
    Write-Host "Processing: $($change.Description)" -ForegroundColor Green
    
    if (-not (Test-Path $change.Old)) {
        Write-Warning "  [X] Source file not found: $($change.Old)"
        $errorCount++
        continue
    }
    
    if (Test-Path $change.New) {
        Write-Warning "  [X] Destination file already exists: $($change.New)"
        $errorCount++
        continue
    }
    
    try {
        # Read and update the file
        $content = Get-Content $change.Old -Raw | ConvertFrom-Json
        
        # Update metadata
        foreach ($key in $change.MetadataChanges.Keys) {
            $content.metadata.$key = $change.MetadataChanges[$key]
        }
        
        # Update version and timestamp
        $content.metadata.version = "4.0"
        $content.metadata.last_updated = Get-Date -Format "yyyy-MM-dd"
        
        # Add v4.0 fields if they don't exist
        if (-not ($content.metadata.PSObject.Properties.Name -contains "supports_traits")) {
            $content.metadata | Add-Member -NotePropertyName "supports_traits" -NotePropertyValue $true -Force | Out-Null
        }
        
        if (-not ($content.metadata.PSObject.Properties.Name -contains "usage")) {
            if ($change.New -like "*dialogue_styles*") {
                $usage = "Provides dialogue style templates for NPC conversations"
            } else {
                $usage = "Provides personality traits for NPC characterization"
            }
            $content.metadata | Add-Member -NotePropertyName "usage" -NotePropertyValue $usage -Force | Out-Null
        }
        
        Write-Host "  [OK] Loaded and updated metadata" -ForegroundColor Gray
        
        if (-not $WhatIf) {
            # Save to new location
            $content | ConvertTo-Json -Depth 100 | Set-Content $change.New -Encoding UTF8
            Write-Host "  [OK] Created $($change.New)" -ForegroundColor Green
            
            # Delete old file
            Remove-Item $change.Old -Force
            Write-Host "  [OK] Deleted $($change.Old)" -ForegroundColor Green
            
            # Update .cbconfig.json if it exists
            if (Test-Path $change.ConfigPath) {
                $config = Get-Content $change.ConfigPath -Raw | ConvertFrom-Json
                
                $oldFileName = Split-Path $change.Old -Leaf
                $oldFileNameNoExt = [System.IO.Path]::GetFileNameWithoutExtension($oldFileName)
                $newFileName = Split-Path $change.New -Leaf
                $newFileNameNoExt = [System.IO.Path]::GetFileNameWithoutExtension($newFileName)
                
                # Update fileIcons if present
                if ($config.fileIcons.PSObject.Properties.Name -contains $oldFileNameNoExt) {
                    $icon = $config.fileIcons.$oldFileNameNoExt
                    $config.fileIcons.PSObject.Properties.Remove($oldFileNameNoExt)
                    $config.fileIcons | Add-Member -NotePropertyName $newFileNameNoExt -NotePropertyValue $icon -Force | Out-Null
                    
                    $config | ConvertTo-Json -Depth 100 | Set-Content $change.ConfigPath -Encoding UTF8
                    Write-Host "  [OK] Updated .cbconfig.json" -ForegroundColor Green
                }
            }
        } else {
            Write-Host "  [WARN] DRY RUN: Would rename $($change.Old) to $($change.New)" -ForegroundColor Yellow
        }
        
        $successCount++
        Write-Host ""
        
    } catch {
        Write-Error "  [X] Error: $_"
        $errorCount++
    }
}

# Summary
Write-Host ""
Write-Host "=== Rename Summary ===" -ForegroundColor Cyan
Write-Host "  Processed: $successCount / $($changes.Count)" -ForegroundColor $(if ($successCount -eq $changes.Count) { "Green" } else { "Yellow" })

if ($errorCount -gt 0) {
    Write-Host "  Errors: $errorCount" -ForegroundColor Red
}

Write-Host ""
if ($WhatIf) {
    Write-Host "DRY RUN COMPLETE - No files were modified" -ForegroundColor Yellow
    Write-Host "Run without -WhatIf to apply changes" -ForegroundColor Yellow
} else {
    Write-Host "RENAME COMPLETE" -ForegroundColor Green
}
