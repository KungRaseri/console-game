# Restructure classes catalog from separate *_types to unified class_types structure
# Old: { "warrior_types": {...}, "rogue_types": {...}, ... }
# New: { "class_types": { "warrior": {...}, "rogue": {...}, ... } }

$ErrorActionPreference = "Stop"

$filePath = "c:\code\console-game\Game.Data\Data\Json\classes\catalog.json"

Write-Host "Restructuring classes catalog..." -ForegroundColor Cyan

# Read file
$content = Get-Content $filePath -Raw
$json = $content | ConvertFrom-Json

# Create new class_types structure
$classTypes = @{
    warrior = $json.warrior_types
    rogue = $json.rogue_types
    mage = $json.mage_types
    cleric = $json.cleric_types
    ranger = $json.ranger_types
}

# Remove old individual type properties
$json.PSObject.Properties.Remove('warrior_types')
$json.PSObject.Properties.Remove('rogue_types')
$json.PSObject.Properties.Remove('mage_types')
$json.PSObject.Properties.Remove('cleric_types')
$json.PSObject.Properties.Remove('ranger_types')

# Add new class_types property
$json | Add-Member -MemberType NoteProperty -Name 'class_types' -Value $classTypes

# Update componentKeys in metadata
$json.metadata.componentKeys = @("class_types")

Write-Host "  Restructured: warrior_types -> class_types.warrior" -ForegroundColor Green
Write-Host "  Restructured: rogue_types -> class_types.rogue" -ForegroundColor Green
Write-Host "  Restructured: mage_types -> class_types.mage" -ForegroundColor Green
Write-Host "  Restructured: cleric_types -> class_types.cleric" -ForegroundColor Green
Write-Host "  Restructured: ranger_types -> class_types.ranger" -ForegroundColor Green

# Convert back to JSON and save
$newContent = $json | ConvertTo-Json -Depth 100
$newContent | Set-Content $filePath -Encoding UTF8

Write-Host "`nDone! Classes catalog restructured successfully!" -ForegroundColor Green
