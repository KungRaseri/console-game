# Fix social/dialogue catalogs to have proper nested structure
# Handles two cases:
# 1. Direct arrays: { "*_types": [...] } -> { "*_types": { "category": { "items": [...] } } }
# 2. Direct objects: { "*_types": {...} } -> { "*_types": { "category": { "items": [{...}] } } }

$ErrorActionPreference = "Stop"

$filesToFix = @(
    "c:\code\console-game\Game.Data\Data\Json\social\dialogue\responses\catalog.json",
    "c:\code\console-game\Game.Data\Data\Json\social\dialogue\farewells\catalog.json"
)

# Note: greetings and styles have correct nested structure already

Write-Host "Processing $($filesToFix.Count) social/dialogue catalog files..." -ForegroundColor Cyan

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
            
            # Skip null values
            if ($null -eq $propValue) {
                continue
            }
            
            # Extract category name (e.g., affirmative_types -> affirmative)
            $category = $propName -replace '_types$', ''
            
            # Case 1: Property is a direct array
            if ($propValue -is [System.Array]) {
                Write-Host "  X Found direct array: $propName" -ForegroundColor Yellow
                Write-Host "  -> Wrapping in '$category' category with items array" -ForegroundColor Gray
                
                $newStructure = @{
                    $category = @{
                        items = $propValue
                    }
                }
                
                $json.$propName = $newStructure
                $fixed = $true
            }
            # Case 2: Property is a single object with 'name' field (should be in an array)
            elseif (($propValue -is [PSCustomObject]) -and ($propValue.PSObject.Properties['name'])) {
                Write-Host "  X Found single object: $propName" -ForegroundColor Yellow
                Write-Host "  -> Wrapping in '$category' category with items array (single item)" -ForegroundColor Gray
                
                $newStructure = @{
                    $category = @{
                        items = @($propValue)  # Wrap single object in array
                    }
                }
                
                $json.$propName = $newStructure
                $fixed = $true
            }
            else {
                Write-Host "  OK Property $propName has correct nested structure" -ForegroundColor Green
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

Write-Host "`nDone! All social/dialogue catalogs processed!" -ForegroundColor Green
