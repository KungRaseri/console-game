# Consolidate NPC Occupations Script
# Merges common.json, criminal.json, magical.json, noble.json into single occupations.json
# Version: 1.0
# Date: 2025-12-17

param(
    [switch]$WhatIf = $false
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Host "=== Consolidate NPC Occupations ===" -ForegroundColor Cyan
Write-Host "Mode: $(if ($WhatIf) { 'DRY RUN (no changes)' } else { 'LIVE (files will be modified)' })" -ForegroundColor Yellow
Write-Host ""

$basePath = "Game.Shared\Data\Json\npcs\occupations"
$outputFile = Join-Path $basePath "occupations.json"

$sourceFiles = @(
    @{ File = "common.json"; Component = "common" },
    @{ File = "criminal.json"; Component = "criminal" },
    @{ File = "magical.json"; Component = "magical" },
    @{ File = "noble.json"; Component = "noble" }
)

Write-Host "Processing: Consolidate occupation files" -ForegroundColor Green

# Check if output file already exists
if (Test-Path $outputFile) {
    Write-Warning "  [X] Output file already exists: $outputFile"
    Write-Host ""
    Write-Host "DRY RUN COMPLETE - Output file already exists" -ForegroundColor Yellow
    exit 1
}

try {
    # Create consolidated structure
    $consolidated = [PSCustomObject]@{
        metadata = [PSCustomObject]@{
            description = "NPC occupation catalog with weight-based rarity and trait bonuses"
            version = "4.0"
            last_updated = (Get-Date -Format "yyyy-MM-dd")
            type = "occupation_catalog"
            supports_traits = $true
            component_keys = @("common", "criminal", "magical", "noble")
            total_occupations = 0
            categories = @("merchants", "craftsmen", "professionals", "service", "nobility", "religious", "adventurers", "magical", "criminal", "common")
            usage = "Provides occupation templates for NPC generation with stat bonuses"
        }
        components = [PSCustomObject]@{}
    }
    
    $totalOccupations = 0
    
    # Load and merge each source file
    foreach ($source in $sourceFiles) {
        $filePath = Join-Path $basePath $source.File
        
        if (-not (Test-Path $filePath)) {
            Write-Warning "  [X] Source file not found: $filePath"
            continue
        }
        
        $sourceContent = Get-Content $filePath -Raw | ConvertFrom-Json
        
        # Get components from source file
        $sourceComponents = $sourceContent.components
        
        # Add all components to consolidated structure
        foreach ($componentName in $sourceComponents.PSObject.Properties.Name) {
            $consolidated.components | Add-Member -NotePropertyName $componentName -NotePropertyValue $sourceComponents.$componentName -Force | Out-Null
            $totalOccupations += $sourceComponents.$componentName.Count
        }
        
        Write-Host "  [OK] Loaded $($source.File) - $($source.Component) categories" -ForegroundColor Gray
    }
    
    # Update total count
    $consolidated.metadata.total_occupations = $totalOccupations
    
    Write-Host "  [OK] Merged $totalOccupations total occupations" -ForegroundColor Gray
    
    if (-not $WhatIf) {
        # Save consolidated file
        $consolidated | ConvertTo-Json -Depth 100 | Set-Content $outputFile -Encoding UTF8
        Write-Host "  [OK] Created $outputFile" -ForegroundColor Green
        
        # Delete source files
        foreach ($source in $sourceFiles) {
            $filePath = Join-Path $basePath $source.File
            if (Test-Path $filePath) {
                Remove-Item $filePath -Force
                Write-Host "  [OK] Deleted $($source.File)" -ForegroundColor Green
            }
        }
        
        # Update .cbconfig.json
        $configPath = Join-Path $basePath ".cbconfig.json"
        if (Test-Path $configPath) {
            $config = Get-Content $configPath -Raw | ConvertFrom-Json
            
            # Remove old file icons
            foreach ($source in $sourceFiles) {
                $fileNameNoExt = [System.IO.Path]::GetFileNameWithoutExtension($source.File)
                if ($config.fileIcons.PSObject.Properties.Name -contains $fileNameNoExt) {
                    $config.fileIcons.PSObject.Properties.Remove($fileNameNoExt)
                }
            }
            
            # Add new icon for consolidated file
            $config.fileIcons | Add-Member -NotePropertyName "occupations" -NotePropertyValue "BriefcaseOutline" -Force | Out-Null
            
            $config | ConvertTo-Json -Depth 100 | Set-Content $configPath -Encoding UTF8
            Write-Host "  [OK] Updated .cbconfig.json" -ForegroundColor Green
        }
    } else {
        Write-Host "  [WARN] DRY RUN: Would create $outputFile" -ForegroundColor Yellow
        Write-Host "  [WARN] DRY RUN: Would delete $($sourceFiles.Count) source files" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "=== Consolidation Summary ===" -ForegroundColor Cyan
    Write-Host "  Total occupations: $totalOccupations" -ForegroundColor Green
    Write-Host "  Component categories: $($consolidated.components.PSObject.Properties.Count)" -ForegroundColor Green
    Write-Host "  Files to delete: $($sourceFiles.Count)" -ForegroundColor Yellow
    
    Write-Host ""
    if ($WhatIf) {
        Write-Host "DRY RUN COMPLETE - No files were modified" -ForegroundColor Yellow
        Write-Host "Run without -WhatIf to apply changes" -ForegroundColor Yellow
    } else {
        Write-Host "CONSOLIDATION COMPLETE" -ForegroundColor Green
    }
    
} catch {
    Write-Error "  [X] Error: $_"
}
