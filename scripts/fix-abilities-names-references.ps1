# Fix abilities_names.json to reference abilities from abilities_catalog.json

$enemyFolders = @(
    "beasts", "dragons", "elementals", "goblinoids", "humanoids", "insects",
    "orcs", "plants", "reptilians", "trolls", "undead", "vampires"
)

$basePath = "C:\code\console-game\Game.Data\Data\Json\enemies"

foreach ($folder in $enemyFolders) {
    Write-Host "Processing $folder..." -ForegroundColor Cyan
    
    $catalogPath = Join-Path $basePath "$folder\abilities_catalog.json"
    $namesPath = Join-Path $basePath "$folder\abilities_names.json"
    
    if (-not (Test-Path $catalogPath) -or -not (Test-Path $namesPath)) {
        Write-Warning "Skipping $folder - files not found"
        continue
    }
    
    $catalog = Get-Content $catalogPath -Raw | ConvertFrom-Json
    $namesData = Get-Content $namesPath -Raw | ConvertFrom-Json
    
    $namesData.components.base = @()
    
    foreach ($abilityType in $catalog.ability_types.PSObject.Properties) {
        foreach ($item in $abilityType.Value.items) {
            $namesData.components.base += @{
                value = $item.name
                rarityWeight = $item.rarityWeight
            }
        }
    }
    
    $namesData | ConvertTo-Json -Depth 10 | Set-Content $namesPath -Encoding UTF8
    
    Write-Host "  Updated $($namesData.components.base.Count) base abilities" -ForegroundColor Green
}

Write-Host "Complete! All files updated." -ForegroundColor Green
