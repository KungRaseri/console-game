# Migrate abilities from enemies/*/abilities_*.json to abilities/* domain structure
# This consolidates enemy-specific abilities into shared, categorized ability files

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Abilities Domain Migration" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$basePath = "C:\code\console-game\Game.Data\Data\Json"
$abilitiesPath = Join-Path $basePath "abilities"

# Category mapping: old category → new path
$categoryMap = @{
    'offensive' = 'active/offensive'
    'offensive_traits' = 'passive/offensive'
    'basic_combat' = 'active/offensive'
    'combat_styles' = 'active/offensive'
    'combat_traits' = 'passive/offensive'
    'aggression' = 'passive/offensive'
    'predatory_traits' = 'passive/offensive'
    'power_traits' = 'passive/offensive'
    'magical_traits' = 'passive/offensive'
    
    'defensive' = 'active/defensive'
    'defensive_traits' = 'passive/defensive'
    'basic_traits' = 'passive/defensive'
    'immunities' = 'passive/defensive'
    'vulnerabilities' = 'passive/defensive'
    'core_traits' = 'passive/defensive'
    'special_traits' = 'passive/defensive'
    
    'control' = 'active/control'
    'mental' = 'active/control'
    
    'utility' = 'active/utility'
    'specialized' = 'active/utility'
    'summoning' = 'active/utility'
    'tactical' = 'active/utility'
    'magical' = 'active/utility'
    
    'leadership' = 'passive/leadership'
    'leadership_traits' = 'passive/leadership'
    'command_traits' = 'passive/leadership'
    'auras' = 'passive/leadership'
    'tactical_traits' = 'passive/leadership'
    
    'mobility' = 'passive/mobility'
    'mobility_traits' = 'passive/mobility'
    
    'sensory' = 'passive/sensory'
    
    'environmental' = 'passive/environmental'
    'environmental_traits' = 'passive/environmental'
    'colossal_traits' = 'passive/environmental'
    'spawning_traits' = 'passive/environmental'
    'evolution_traits' = 'passive/environmental'
    'mystical_traits' = 'passive/environmental'
    
    'legendary' = 'ultimate'
    'legendary_traits' = 'ultimate'
    'elite' = 'ultimate'
}

# Step 1: Collect all abilities
Write-Host "Step 1: Analyzing abilities across 13 enemy types..." -ForegroundColor Yellow
$allAbilities = @{}
$allComponents = @{}

Get-ChildItem "$basePath\enemies\*\abilities_catalog.json" | ForEach-Object {
    $enemyType = $_.Directory.Name
    $catalogContent = Get-Content $_.FullName -Raw | ConvertFrom-Json
    
    foreach ($categoryProp in $catalogContent.ability_types.PSObject.Properties) {
        $oldCategory = $categoryProp.Name
        $newPath = $categoryMap[$oldCategory]
        
        if (-not $newPath) {
            Write-Host "  ⚠ Unknown category: $oldCategory (in $enemyType)" -ForegroundColor Yellow
            $newPath = 'active/utility'  # Default fallback
        }
        
        if (-not $allAbilities.ContainsKey($newPath)) {
            $allAbilities[$newPath] = @()
        }
        
        foreach ($item in $categoryProp.Value.items) {
            # Check for duplicates by name
            $existing = $allAbilities[$newPath] | Where-Object { $_.name -eq $item.name }
            if (-not $existing) {
                $allAbilities[$newPath] += $item
            }
        }
    }
}

# Step 2: Collect pattern components (prefix/suffix)
Write-Host "Step 2: Collecting pattern components..." -ForegroundColor Yellow
Get-ChildItem "$basePath\enemies\*\abilities_names.json" | ForEach-Object {
    $enemyType = $_.Directory.Name
    $namesContent = Get-Content $_.FullName -Raw | ConvertFrom-Json
    
    if ($namesContent.components.prefix) {
        if (-not $allComponents.ContainsKey('prefix')) {
            $allComponents['prefix'] = @()
        }
        foreach ($comp in $namesContent.components.prefix) {
            $existing = $allComponents['prefix'] | Where-Object { $_.value -eq $comp.value }
            if (-not $existing) {
                $allComponents['prefix'] += $comp
            }
        }
    }
    
    if ($namesContent.components.suffix) {
        if (-not $allComponents.ContainsKey('suffix')) {
            $allComponents['suffix'] = @()
        }
        foreach ($comp in $namesContent.components.suffix) {
            $existing = $allComponents['suffix'] | Where-Object { $_.value -eq $comp.value }
            if (-not $existing) {
                $allComponents['suffix'] += $comp
            }
        }
    }
}

Write-Host "  Found $($allAbilities.Keys.Count) categories" -ForegroundColor Green
foreach ($path in $allAbilities.Keys | Sort-Object) {
    Write-Host "    $path - $($allAbilities[$path].Count) abilities" -ForegroundColor Gray
}
Write-Host "  Found $($allComponents.prefix.Count) unique prefixes, $($allComponents.suffix.Count) unique suffixes" -ForegroundColor Green

# Step 3: Create catalog.json files
Write-Host "`nStep 3: Creating catalog.json files..." -ForegroundColor Yellow
$createdCatalogs = 0

foreach ($path in $allAbilities.Keys) {
    $fullPath = Join-Path $abilitiesPath $path
    $catalogPath = Join-Path $fullPath "catalog.json"
    
    $catalog = [PSCustomObject]@{
        metadata = [PSCustomObject]@{
            description = "Ability catalog for $path"
            version = "4.0"
            lastUpdated = (Get-Date -Format "yyyy-MM-dd")
            type = "ability_catalog"
            supportsTraits = $true
            totalAbilities = $allAbilities[$path].Count
            notes = @(
                "Consolidated from enemy-specific abilities",
                "Abilities are shared across enemies, players, and NPCs",
                "rarityWeight determines selection probability"
            )
        }
        items = $allAbilities[$path]
    }
    
    $json = $catalog | ConvertTo-Json -Depth 10
    Set-Content -Path $catalogPath -Value $json -NoNewline
    $createdCatalogs++
    $count = $allAbilities[$path].Count
    Write-Host "  [OK] $path/catalog.json - $count abilities" -ForegroundColor Green
}

# Step 4: Create names.json files
Write-Host "`nStep 4: Creating names.json files..." -ForegroundColor Yellow
$createdNames = 0

foreach ($path in $allAbilities.Keys) {
    $fullPath = Join-Path $abilitiesPath $path
    $namesPath = Join-Path $fullPath "names.json"
    
    $names = [PSCustomObject]@{
        metadata = [PSCustomObject]@{
            description = "Pattern-based ability name generation for $path"
            version = "4.0"
            lastUpdated = (Get-Date -Format "yyyy-MM-dd")
            type = "pattern_generation"
            supportsTraits = $true
            componentKeys = @("prefix", "suffix")
            patternTokens = @("base", "prefix", "suffix")
            totalPatterns = 4
            raritySystem = "weight-based"
            notes = @(
                "Base token resolves from catalog.json",
                "Components have rarityWeight for emergent rarity calculation",
                "Traits are applied when components are selected in patterns"
            )
        }
        components = [PSCustomObject]@{
            prefix = $allComponents.prefix
            suffix = $allComponents.suffix
        }
        patterns = @(
            [PSCustomObject]@{ rarityWeight = 40; pattern = "{base}" }
            [PSCustomObject]@{ rarityWeight = 35; pattern = "{prefix} {base}" }
            [PSCustomObject]@{ rarityWeight = 20; pattern = "{base} {suffix}" }
            [PSCustomObject]@{ rarityWeight = 5; pattern = "{prefix} {base} {suffix}" }
        )
    }
    
    $json = $names | ConvertTo-Json -Depth 10
    Set-Content -Path $namesPath -Value $json -NoNewline
    $createdNames++
    Write-Host "  [OK] $path/names.json" -ForegroundColor Green
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Migration Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Created $createdCatalogs catalog files" -ForegroundColor White
Write-Host "Created $createdNames names files" -ForegroundColor White
Write-Host "Total abilities migrated: $(($allAbilities.Values | ForEach-Object { $_.Count } | Measure-Object -Sum).Sum)" -ForegroundColor White
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "  1. Update FileTypeDetector.cs to route abilities/* files" -ForegroundColor White
Write-Host "  2. Create .cbconfig.json for abilities folder" -ForegroundColor White
Write-Host "  3. Test in ContentBuilder" -ForegroundColor White
Write-Host "  4. Backup and remove old enemy abilities files" -ForegroundColor White
