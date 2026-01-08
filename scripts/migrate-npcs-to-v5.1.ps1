# Migrate NPC catalogs from v4.0 to v5.1
# Moves flat attributes into attributes object

$ErrorActionPreference = "Stop"

Write-Host "NPC v5.1 Migration Script" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

$npcCatalogs = Get-ChildItem -Path "RealmEngine.Data\Data\Json\npcs" -Recurse -Filter "catalog.json" | 
    Where-Object { $_.FullName -notlike "*bin*" -and $_.FullName -notlike "*obj*" }

Write-Host "Found $($npcCatalogs.Count) NPC catalogs to migrate" -ForegroundColor Yellow
Write-Host ""

$migrated = 0
$errors = 0

foreach ($file in $npcCatalogs) {
    $category = Split-Path $file.DirectoryName -Leaf
    Write-Host "Processing: npcs/$category/catalog.json" -ForegroundColor White
    
    try {
        # Read file
        $content = Get-Content $file.FullName -Raw
        
        # Update version
        $content = $content -replace '"version":\s*"4\.0"', '"version": "5.1"'
        
        # Move flat attributes to attributes object using regex
        # Match the pattern and capture attribute values
        $pattern = '("description":\s*"[^"]+",)\s*[\r\n]+\s*"strength":\s*(\d+),\s*[\r\n]+\s*"dexterity":\s*(\d+),\s*[\r\n]+\s*"constitution":\s*(\d+),\s*[\r\n]+\s*"intelligence":\s*(\d+),\s*[\r\n]+\s*"wisdom":\s*(\d+),\s*[\r\n]+\s*"charisma":\s*(\d+),'
        
        $replacement = '$1
          "attributes": {
            "strength": $2,
            "dexterity": $3,
            "constitution": $4,
            "intelligence": $5,
            "wisdom": $6,
            "charisma": $7
          },'
        
        $content = $content -replace $pattern, $replacement
        
        # Save
        Set-Content -Path $file.FullName -Value $content -NoNewline
        
        Write-Host "  Success: Migrated to v5.1" -ForegroundColor Green
        $migrated++
    }
    catch {
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        $errors++
    }
    
    Write-Host ""
}

Write-Host "===================" -ForegroundColor Cyan
Write-Host "Migration Complete" -ForegroundColor Cyan
Write-Host "  Migrated: $migrated catalogs" -ForegroundColor Green
Write-Host "  Errors: $errors catalogs" -ForegroundColor $(if ($errors -gt 0) { "Red" } else { "Green" })
