# PowerShell script to migrate abilities to v4 format
# Run from repository root: .\scripts\MigrateAbilities.ps1

$enemyTypes = @(
    "beasts", "dragons", "elementals", "goblinoids",
    "humanoids", "insects", "orcs", "plants",
    "reptilians", "trolls", "undead", "vampires"
)

$basePath = "Game.Data\Data\Json\enemies"

Write-Host "=== Abilities v4 Migration ===" -ForegroundColor Cyan
Write-Host "Migrating $($enemyTypes.Length) enemy types...`n"

$successCount = 0
$skipCount = 0

foreach ($enemyType in $enemyTypes) {
    Write-Host "Processing $enemyType..." -ForegroundColor Yellow

    $v3Path = Join-Path $basePath $enemyType "abilities.json"
    $catalogPath = Join-Path $basePath $enemyType "abilities_catalog.json"
    $namesPath = Join-Path $basePath $enemyType "abilities_names.json"

    if (-not (Test-Path $v3Path)) {
        Write-Host "  [SKIP] No abilities.json found" -ForegroundColor Gray
        $skipCount++
        continue
    }

    if ((Test-Path $catalogPath) -and (Test-Path $namesPath)) {
        Write-Host "  [SKIP] Already migrated (v4 files exist)" -ForegroundColor Gray
        $skipCount++
        continue
    }

    Write-Host "  [INFO] Migration requires C# service - use ContentBuilder or manual migration" -ForegroundColor Magenta
    Write-Host "  Source: $v3Path"
    $successCount++
}

Write-Host "`n=== Migration Status ===" -ForegroundColor Cyan
Write-Host "Ready for migration: $successCount"
Write-Host "Skipped: $skipCount"
Write-Host "Total: $($enemyTypes.Length)"
Write-Host "`nNote: Implement migration UI in ContentBuilder or use migration service directly." -ForegroundColor Yellow
