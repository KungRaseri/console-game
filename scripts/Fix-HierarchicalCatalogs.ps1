# Fix hierarchical catalog structure - move componentKeys/components to proper *_types root properties

$files = @(
    "Game.Data\Data\Json\organizations\businesses\catalog.json",
    "Game.Data\Data\Json\organizations\factions\catalog.json",
    "Game.Data\Data\Json\organizations\guilds\catalog.json",
    "Game.Data\Data\Json\organizations\shops\catalog.json",
    "Game.Data\Data\Json\social\dialogue\farewells\catalog.json",
    "Game.Data\Data\Json\social\dialogue\greetings\catalog.json",
    "Game.Data\Data\Json\social\dialogue\responses\catalog.json",
    "Game.Data\Data\Json\social\dialogue\styles\catalog.json",
    "Game.Data\Data\Json\world\environments\catalog.json",
    "Game.Data\Data\Json\world\locations\dungeons\catalog.json",
    "Game.Data\Data\Json\world\locations\towns\catalog.json",
    "Game.Data\Data\Json\world\locations\wilderness\catalog.json",
    "Game.Data\Data\Json\world\regions\catalog.json"
)

foreach ($file in $files) {
    Write-Host "Processing: $file" -ForegroundColor Cyan
    
    if (-not (Test-Path $file)) {
        Write-Host "  File not found, skipping" -ForegroundColor Yellow
        continue
    }
    
    $json = Get-Content $file -Raw | ConvertFrom-Json
    
    # Check if components exists at root
    if ($null -eq $json.components) {
        Write-Host "  No components found, skipping" -ForegroundColor Yellow
        continue
    }
    
    # Get component keys from either root or metadata
    $componentKeys = $null
    if ($null -ne $json.componentKeys) {
        $componentKeys = $json.componentKeys
    } elseif ($null -ne $json.metadata.componentKeys) {
        $componentKeys = $json.metadata.componentKeys
    } else {
        # If no componentKeys, use the property names from components
        $componentKeys = $json.components.PSObject.Properties.Name
    }
    
    Write-Host "  Component keys: $($componentKeys -join ', ')" -ForegroundColor Gray
    
    # Create new ordered hashtable for restructured JSON
    $newJson = [ordered]@{
        metadata = $json.metadata
    }
    
    # Remove componentKeys from metadata if present
    if ($newJson.metadata.PSObject.Properties['componentKeys']) {
        $newJson.metadata.PSObject.Properties.Remove('componentKeys')
    }
    
    # Move each component to root level with _types suffix
    foreach ($key in $componentKeys) {
        $typeName = "${key}_types"
        $newJson[$typeName] = $json.components.$key
        Write-Host "  Created: $typeName" -ForegroundColor Green
    }
    
    # Remove usage field if present (should be in metadata.notes)
    if ($json.PSObject.Properties['usage']) {
        Write-Host "  Removed root-level 'usage' field" -ForegroundColor Yellow
    }
    
    # Convert back to JSON and save
    $newJson | ConvertTo-Json -Depth 20 | Set-Content $file -NoNewline
    Write-Host "  Saved!" -ForegroundColor Green
}

Write-Host "`nAll files processed!" -ForegroundColor Green
