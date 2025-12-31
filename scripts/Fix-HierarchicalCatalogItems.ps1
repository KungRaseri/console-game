# Fix hierarchical catalogs (organizations, world, quests) to have proper nested structure
# Old: { "component_types": [...] }
# New: { "component_types": { "category": { "items": [...] } } }

$ErrorActionPreference = "Stop"

$filesToFix = @(
    "c:\code\console-game\Game.Data\Data\Json\organizations\shops\catalog.json",
    "c:\code\console-game\Game.Data\Data\Json\organizations\factions\catalog.json",
    "c:\code\console-game\Game.Data\Data\Json\organizations\guilds\catalog.json",
    "c:\code\console-game\Game.Data\Data\Json\organizations\businesses\catalog.json",
    "c:\code\console-game\Game.Data\Data\Json\world\environments\catalog.json",
    "c:\code\console-game\Game.Data\Data\Json\world\regions\catalog.json",
    "c:\code\console-game\Game.Data\Data\Json\world\locations\towns\catalog.json",
    "c:\code\console-game\Game.Data\Data\Json\world\locations\dungeons\catalog.json",
    "c:\code\console-game\Game.Data\Data\Json\world\locations\wilderness\catalog.json"
)

Write-Host "Processing $($filesToFix.Count) hierarchical catalog files..." -ForegroundColor Cyan

foreach ($filePath in $filesToFix) {
    if (Test-Path $filePath) {
        Write-Host "`nProcessing: $filePath" -ForegroundColor Cyan
        
        # Read file
        $content = Get-Content $filePath -Raw
        $json = $content | ConvertFrom-Json
        
        # Get all *_types properties (excluding metadata)
        $typeProps = $json.PSObject.Properties | Where-Object { $_.Name -like "*_types" }
        
        $fixed = $false
        foreach ($prop in $typeProps) {
            $propName = $prop.Name
            $propValue = $prop.Value
            
            # Check if the property is a direct array (wrong structure)
            if ($propValue -is [System.Array]) {
                Write-Host "  X Found direct array: $propName" -ForegroundColor Yellow
                
                # Extract category name from property name (e.g., weapons_types -> weapons)
                $category = $propName -replace '_types$', ''
                
                Write-Host "  -> Wrapping in '$category' category with items array" -ForegroundColor Gray
                
                # Create new nested structure
                $newStructure = @{
                    $category = @{
                        items = $propValue
                    }
                }
                
                # Replace the property value
                $json.$propName = $newStructure
                $fixed = $true
            }
            else {
                Write-Host "  OK Property $propName is already an object" -ForegroundColor Green
            }
        }
        
        if ($fixed) {
            # Convert back to JSON and save
            $newContent = $json | ConvertTo-Json -Depth 100
            $newContent | Set-Content $filePath -Encoding UTF8
            Write-Host "  >> Fixed and saved!" -ForegroundColor Green
        }
        else {
            Write-Host "  OK No changes needed" -ForegroundColor Green
        }
    }
    else {
        Write-Host "  SKIP File not found: $filePath" -ForegroundColor Red
    }
}

Write-Host "`nDone! All hierarchical catalogs processed!" -ForegroundColor Green
