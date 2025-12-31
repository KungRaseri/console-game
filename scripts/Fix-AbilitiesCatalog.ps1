# Fix abilities catalogs to have proper nested structure
# Old: { "ability_types": [...] }
# New: { "ability_types": { "category": { "items": [...] } } }

$ErrorActionPreference = "Stop"

$rootPath = "c:\code\console-game\Game.Data\Data\Json\abilities"

# Find all catalog.json files in abilities domain
$catalogFiles = Get-ChildItem -Path $rootPath -Filter "catalog.json" -Recurse

Write-Host "Found $($catalogFiles.Count) abilities catalog files"

foreach ($file in $catalogFiles) {
    Write-Host "`nProcessing: $($file.FullName)" -ForegroundColor Cyan
    
    # Read file
    $content = Get-Content $file.FullName -Raw
    $json = $content | ConvertFrom-Json
    
    # Check if ability_types is an array (old structure)
    if ($json.ability_types -is [System.Array]) {
        Write-Host "  X Found old structure (ability_types is array)" -ForegroundColor Yellow
        
        # Determine category name from path
        $pathParts = $file.DirectoryName.Replace($rootPath, "").TrimStart('\').Split('\')
        
        # Build category name from path parts (e.g., active/offensive → "offensive")
        $category = $pathParts[-1] # Last part of path
        
        Write-Host "  -> Category: $category" -ForegroundColor Gray
        
        # Create new structure with category wrapper
        $newAbilityTypes = @{
            $category = @{
                items = $json.ability_types
            }
        }
        
        # Replace ability_types with new structure
        $json.ability_types = $newAbilityTypes
        
        # Convert back to JSON and save
        $newContent = $json | ConvertTo-Json -Depth 100
        $newContent | Set-Content $file.FullName -Encoding UTF8
        
        Write-Host "  >> Fixed: wrapped in '$category' category with items array" -ForegroundColor Green
    }
    else {
        Write-Host "  OK Already has correct structure (ability_types is object)" -ForegroundColor Green
    }
}

Write-Host "`nDone! All abilities catalogs processed!" -ForegroundColor Green
