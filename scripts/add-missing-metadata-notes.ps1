# Add Missing Metadata and Notes to JSON Files
# This script automatically adds standardized metadata and notes sections
# to JSON files that are missing them.

param(
    [switch]$WhatIf,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$jsonPath = "Game.Data\Data\Json"
$today = Get-Date -Format "yyyy-MM-dd"
$filesModified = 0
$filesSkipped = 0

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "ADD MISSING METADATA & NOTES" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

if ($WhatIf) {
    Write-Host "DRY RUN MODE - No files will be modified`n" -ForegroundColor Yellow
}

# Helper function to detect file type
function Get-FileType {
    param($FileName, $Content)
    
    if ($FileName -eq "types.json") { return "catalog" }
    if ($FileName -eq "names.json") { return "generation" }
    if ($FileName -eq "prefixes.json") { return "prefix_modifier" }
    if ($FileName -eq "suffixes.json") { return "suffix_modifier" }
    if ($FileName -eq "traits.json") { return "trait_catalog" }
    if ($FileName -eq "effects.json") { return "effect_catalog" }
    if ($FileName -eq "colors.json") { return "reference_data" }
    
    return "reference_data"
}

# Helper function to get category from path
function Get-Category {
    param($RelativePath)
    
    if ($RelativePath -match "enemies\\(\w+)\\") { return $Matches[1] }
    if ($RelativePath -match "items\\(\w+)\\") { return $Matches[1] }
    if ($RelativePath -match "npcs\\(\w+)\\") { return $Matches[1] }
    if ($RelativePath -match "quests\\(\w+)\\") { return $Matches[1] }
    
    return "general"
}

# Helper function to generate metadata description
function Get-MetadataDescription {
    param($FileType, $Category, $FileName)
    
    switch ($FileType) {
        "catalog" { return "$Category type catalog with base stats and traits" }
        "generation" { return "Pattern-based name generation for $Category" }
        "prefix_modifier" { return "$Category prefix modifiers with stat bonuses/penalties" }
        "suffix_modifier" { return "$Category suffix modifiers with stat bonuses/penalties" }
        "trait_catalog" { return "$Category trait definitions and properties" }
        "effect_catalog" { return "$Category effect definitions and mechanics" }
        "reference_data" { return "Reference data components for $Category" }
        default { return "Data catalog for $Category" }
    }
}

# Helper function to generate notes
function Get-Notes {
    param($FileType, $Category)
    
    switch ($FileType) {
        "catalog" { 
            return @{
                usage = "Base catalog of $Category with inherent stats and traits. Each item has individual properties. Type-level traits apply to all items in category."
            }
        }
        "generation" { 
            return @{
                usage = "Pattern-based name generation for $Category. Pick components by rarityWeight and apply patterns to generate procedural names."
            }
        }
        "prefix_modifier" { 
            return @{
                usage = "Prefix modifiers applied before base item name. Provides stat bonuses/penalties when applied to $Category items."
            }
        }
        "suffix_modifier" { 
            return @{
                usage = "Suffix modifiers applied after base item name. Provides stat bonuses/penalties when applied to $Category items."
            }
        }
        "trait_catalog" { 
            return @{
                usage = "Trait definitions for $Category. Traits can be assigned to items to modify behavior and stats."
            }
        }
        "effect_catalog" { 
            return @{
                usage = "Effect definitions for $Category. Effects determine the mechanical impact when items are used."
            }
        }
        "reference_data" { 
            return @{
                usage = "Reference data components that can be used by other files for $Category generation or lookup."
            }
        }
        default { 
            return @{
                usage = "Data definitions for $Category category."
            }
        }
    }
}

# Helper function to count items
function Get-ItemCount {
    param($Content)
    
    if ($Content.PSObject.Properties.Name -contains "items") {
        return $Content.items.Count
    }
    
    # Count items in nested type structures
    $count = 0
    foreach ($prop in $Content.PSObject.Properties) {
        if ($prop.Name -match "_types$" -and $prop.Value -is [PSCustomObject]) {
            foreach ($typeProp in $prop.Value.PSObject.Properties) {
                if ($typeProp.Value.items) {
                    $count += $typeProp.Value.items.Count
                }
            }
        }
    }
    
    if ($count -gt 0) { return $count }
    return $null
}

# Process all JSON files
$jsonFiles = Get-ChildItem -Path $jsonPath -Recurse -Filter "*.json"

foreach ($file in $jsonFiles) {
    $relativePath = $file.FullName -replace [regex]::Escape($PWD.Path + '\'), ''
    $content = Get-Content $file.FullName -Raw | ConvertFrom-Json
    
    $needsMetadata = -not $content.metadata
    $needsNotes = -not $content.notes
    
    if (-not $needsMetadata -and -not $needsNotes) {
        $filesSkipped++
        if ($Verbose) {
            Write-Host "  SKIP: $relativePath (already has metadata and notes)" -ForegroundColor Gray
        }
        continue
    }
    
    $fileType = Get-FileType -FileName $file.Name -Content $content
    $category = Get-Category -RelativePath $relativePath
    $modified = $false
    
    Write-Host "  Processing: $relativePath" -ForegroundColor Yellow
    
    # Add metadata if missing
    if ($needsMetadata) {
        $description = Get-MetadataDescription -FileType $fileType -Category $category -FileName $file.Name
        
        $metadata = [ordered]@{
            description = $description
            version = "1.0"
            last_updated = $today
            type = $fileType
        }
        
        # Add item count if applicable
        $itemCount = Get-ItemCount -Content $content
        if ($itemCount) {
            $metadata["total_items"] = $itemCount
        }
        
        # Add metadata to content
        $content | Add-Member -NotePropertyName "metadata" -NotePropertyValue ([PSCustomObject]$metadata) -Force
        
        Write-Host "     Added metadata" -ForegroundColor Green
        $modified = $true
    }
    
    # Add notes if missing
    if ($needsNotes) {
        $notes = Get-Notes -FileType $fileType -Category $category
        
        # Add notes to content
        $content | Add-Member -NotePropertyName "notes" -NotePropertyValue ([PSCustomObject]$notes) -Force
        
        Write-Host "     Added notes" -ForegroundColor Green
        $modified = $true
    }
    
    # Save file if modified
    if ($modified -and -not $WhatIf) {
        # Reorder properties: put metadata first, notes last
        $orderedContent = [ordered]@{}
        
        # Add metadata first (if exists)
        if ($content.metadata) {
            $orderedContent["metadata"] = $content.metadata
        }
        
        # Add all other properties except notes
        foreach ($prop in $content.PSObject.Properties) {
            if ($prop.Name -ne "metadata" -and $prop.Name -ne "notes") {
                $orderedContent[$prop.Name] = $prop.Value
            }
        }
        
        # Add notes last (if exists)
        if ($content.notes) {
            $orderedContent["notes"] = $content.notes
        }
        
        # Save with proper formatting
        $json = [PSCustomObject]$orderedContent | ConvertTo-Json -Depth 20
        Set-Content -Path $file.FullName -Value $json -Encoding UTF8
        
        $filesModified++
    }
    elseif ($modified -and $WhatIf) {
        Write-Host "     WOULD MODIFY (dry run)" -ForegroundColor Cyan
        $filesModified++
    }
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Files Processed:  $($jsonFiles.Count)" -ForegroundColor White
Write-Host "Files Modified:   $filesModified" -ForegroundColor $(if ($filesModified -gt 0) { "Green" } else { "Gray" })
Write-Host "Files Skipped:    $filesSkipped" -ForegroundColor Gray

if ($WhatIf) {
    Write-Host "`n💡 Run without -WhatIf to apply changes`n" -ForegroundColor Yellow
}
else {
    Write-Host "`n✅ All files updated successfully!`n" -ForegroundColor Green
}
