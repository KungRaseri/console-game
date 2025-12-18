# Consolidate Quest Templates Script
# Merges fetch.json, kill.json, escort.json, delivery.json, investigate.json into single quest_templates.json
# Version: 1.0
# Date: 2025-12-17

param(
    [switch]$WhatIf = $false
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Host "=== Consolidate Quest Templates ===" -ForegroundColor Cyan
Write-Host "Mode: $(if ($WhatIf) { 'DRY RUN (no changes)' } else { 'LIVE (files will be modified)' })" -ForegroundColor Yellow
Write-Host ""

$basePath = "Game.Shared\Data\Json\quests\templates"
$outputFile = Join-Path $basePath "quest_templates.json"

$sourceFiles = @(
    @{ File = "fetch.json"; QuestType = "fetch" },
    @{ File = "kill.json"; QuestType = "kill" },
    @{ File = "escort.json"; QuestType = "escort" },
    @{ File = "delivery.json"; QuestType = "delivery" },
    @{ File = "investigate.json"; QuestType = "investigate" }
)

Write-Host "Processing: Consolidate quest template files" -ForegroundColor Green

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
            description = "Quest template catalog with various quest types and difficulty levels"
            version = "4.0"
            last_updated = (Get-Date -Format "yyyy-MM-dd")
            type = "quest_template_catalog"
            quest_types = @("fetch", "kill", "escort", "delivery", "investigate")
            total_templates = 0
            difficulty_levels = @("easy", "medium", "hard", "epic")
            usage = "Provides quest templates for procedural quest generation"
        }
        components = [PSCustomObject]@{}
    }
    
    $totalTemplates = 0
    $allCategories = @()
    
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
        
        # Add quest type as top-level component
        $typeComponents = [PSCustomObject]@{}
        
        # Add all difficulty categories under quest type
        foreach ($categoryName in $sourceComponents.PSObject.Properties.Name) {
            $typeComponents | Add-Member -NotePropertyName $categoryName -NotePropertyValue $sourceComponents.$categoryName -Force | Out-Null
            $totalTemplates += $sourceComponents.$categoryName.Count
            
            if ($allCategories -notcontains $categoryName) {
                $allCategories += $categoryName
            }
        }
        
        $consolidated.components | Add-Member -NotePropertyName $source.QuestType -NotePropertyValue $typeComponents -Force | Out-Null
        
        Write-Host "  [OK] Loaded $($source.File) - $($source.QuestType) templates" -ForegroundColor Gray
    }
    
    # Update metadata
    $consolidated.metadata.total_templates = $totalTemplates
    $consolidated.metadata | Add-Member -NotePropertyName "categories" -NotePropertyValue $allCategories -Force | Out-Null
    
    Write-Host "  [OK] Merged $totalTemplates total quest templates" -ForegroundColor Gray
    
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
            $config.fileIcons | Add-Member -NotePropertyName "quest_templates" -NotePropertyValue "FileDocumentOutline" -Force | Out-Null
            
            $config | ConvertTo-Json -Depth 100 | Set-Content $configPath -Encoding UTF8
            Write-Host "  [OK] Updated .cbconfig.json" -ForegroundColor Green
        }
    } else {
        Write-Host "  [WARN] DRY RUN: Would create $outputFile" -ForegroundColor Yellow
        Write-Host "  [WARN] DRY RUN: Would delete $($sourceFiles.Count) source files" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "=== Consolidation Summary ===" -ForegroundColor Cyan
    Write-Host "  Total quest templates: $totalTemplates" -ForegroundColor Green
    Write-Host "  Quest types: $($consolidated.components.PSObject.Properties.Count)" -ForegroundColor Green
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
