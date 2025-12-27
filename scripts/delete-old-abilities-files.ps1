# Delete original abilities.json files after verifying split files work

$enemyFolders = @(
    "beasts",
    "demons",
    "dragons",
    "elementals",
    "goblinoids",
    "humanoids",
    "insects",
    "orcs",
    "plants",
    "reptilians",
    "trolls",
    "undead",
    "vampires"
)

$basePath = "C:\code\console-game\Game.Data\Data\Json\enemies"

Write-Host "Deleting original abilities.json files..." -ForegroundColor Yellow

foreach ($folder in $enemyFolders) {
    $abilitiesPath = Join-Path $basePath "$folder\abilities.json"
    
    if (Test-Path $abilitiesPath) {
        Remove-Item $abilitiesPath -Force
        Write-Host "  ✓ Deleted $folder\abilities.json" -ForegroundColor Green
    } else {
        Write-Host "  ⚠ $folder\abilities.json not found (already deleted?)" -ForegroundColor DarkGray
    }
}

Write-Host "`nCleanup complete! All original abilities.json files removed." -ForegroundColor Green
Write-Host "Split files (abilities_names.json + abilities_catalog.json) are now active." -ForegroundColor Cyan
