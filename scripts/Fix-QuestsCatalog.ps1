# Fix quests catalog to have proper nested structure
# Old: { "templates_types": { "fetch": { "easy_fetch": [...] } } }
# New: { "templates_types": { "fetch": { "easy_fetch": { "items": [...] } } } }

$ErrorActionPreference = "Stop"

$filePath = "c:\code\console-game\Game.Data\Data\Json\quests\catalog.json"

Write-Host "Processing quests catalog..." -ForegroundColor Cyan

if (Test-Path $filePath) {
    # Read file
    $content = Get-Content $filePath -Raw
    $json = $content | ConvertFrom-Json
    
    # Get templates_types
    if ($json.PSObject.Properties['templates_types']) {
        $templatesTypes = $json.templates_types
        
        # Iterate through each quest type (fetch, kill, escort, etc.)
        foreach ($questTypeProp in $templatesTypes.PSObject.Properties) {
            $questTypeName = $questTypeProp.Name
            $questTypeValue = $questTypeProp.Value
            
            Write-Host "`nProcessing quest type: $questTypeName" -ForegroundColor Yellow
            
            # Iterate through each difficulty level (easy_fetch, medium_fetch, etc.)
            foreach ($difficultyProp in $questTypeValue.PSObject.Properties) {
                $difficultyName = $difficultyProp.Name
                $difficultyValue = $difficultyProp.Value
                
                # Check if it's a direct array (needs fixing)
                if ($difficultyValue -is [System.Array]) {
                    Write-Host "  X $difficultyName is a direct array" -ForegroundColor Red
                    Write-Host "    -> Wrapping in items array" -ForegroundColor Gray
                    
                    # Wrap in items array
                    $questTypeValue.$difficultyName = @{
                        items = $difficultyValue
                    }
                }
                else {
                    Write-Host "  OK $difficultyName already has correct structure" -ForegroundColor Green
                }
            }
        }
        
        # Convert back to JSON and save
        $newContent = $json | ConvertTo-Json -Depth 100
        $newContent | Set-Content $filePath -Encoding UTF8
        Write-Host "`n>> Fixed and saved!" -ForegroundColor Green
    }
    else {
        Write-Host "No templates_types found in quests catalog" -ForegroundColor Red
    }
}
else {
    Write-Host "File not found: $filePath" -ForegroundColor Red
}

Write-Host "`nDone!" -ForegroundColor Green
