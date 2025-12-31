# Reorganize enchantments from 14 components to 5 semantic components
# Date: 2025-12-30

$filePath = "Game.Data\Data\Json\items\enchantments\names.json"

Write-Host "Reading enchantments file..." -ForegroundColor Cyan
$json = Get-Content -Path $filePath -Raw | ConvertFrom-Json

Write-Host "Reorganizing components..." -ForegroundColor Yellow

# Create new component structure
$newComponents = [ordered]@{}

# 1. Quality - keep as-is (8)
$newComponents["quality"] = $json.components.quality

# 2. Element - merge elemental + alignment + special + add new (21)
$elementArray = @()
$elementArray += $json.components.elemental  # Flaming, Freezing, Shocking (3)
$elementArray += $json.components.alignment  # Radiant, Shadow, Divine, Demonic (4)
$elementArray += $json.components.special    # Arcane, Vampiric, Ethereal, Prismatic, Void (5)

# Add new earth elements
$elementArray += @(
    [PSCustomObject]@{
        value = "Earthen"
        rarityWeight = 20
        traits = @{
            earthDamage = @{ value = 10; type = "number" }
            resistEarth = @{ value = 10; type = "number" }
            crushingDamage = @{ value = 8; type = "number" }
            armorBonus = @{ value = 5; type = "number" }
            visualColor = @{ value = "stone_brown"; type = "string" }
        }
    },
    [PSCustomObject]@{
        value = "Verdant"
        rarityWeight = 30
        traits = @{
            natureDamage = @{ value = 12; type = "number" }
            resistNature = @{ value = 15; type = "number" }
            poisonDamage = @{ value = 5; type = "number" }
            entangleChance = @{ value = 15; type = "number" }
            healthRegen = @{ value = 2; type = "number" }
            visualColor = @{ value = "forest_green"; type = "string" }
        }
    }
)

# Add new air elements
$elementArray += @(
    [PSCustomObject]@{
        value = "Gale"
        rarityWeight = 20
        traits = @{
            windDamage = @{ value = 8; type = "number" }
            resistWind = @{ value = 10; type = "number" }
            knockbackChance = @{ value = 20; type = "number" }
            movementSpeed = @{ value = 15; type = "number" }
            evasion = @{ value = 10; type = "number" }
            visualColor = @{ value = "sky_blue"; type = "string" }
        }
    }
)

# Add new water element
$elementArray += @(
    [PSCustomObject]@{
        value = "Tidal"
        rarityWeight = 25
        traits = @{
            waterDamage = @{ value = 10; type = "number" }
            resistWater = @{ value = 12; type = "number" }
            knockbackChance = @{ value = 15; type = "number" }
            healingBonus = @{ value = 8; type = "number" }
            fluidMovement = @{ value = true; type = "boolean" }
            visualColor = @{ value = "ocean_blue"; type = "string" }
        }
    }
)

$newComponents["element"] = $elementArray

# 3. Combat - merge power + protection (12)
$combatArray = @()
$combatArray += $json.components.power       # 6
$combatArray += $json.components.protection  # 6
$newComponents["combat"] = $combatArray

# 4. Magic - merge wisdom + magic (12)
$magicArray = @()
$magicArray += $json.components.wisdom  # 6
$magicArray += $json.components.magic   # 6
$newComponents["magic"] = $magicArray

# 5. Special - merge agility + element suffixes (13)
$specialArray = @()
$specialArray += $json.components.agility  # 6

# Add element suffixes (7) - keep one from each old group
# of Flames (from fire group)
$specialArray += $json.components.fire | Select-Object -First 1

# of Light (renamed from of Life)
$ofLight = $json.components.life | Select-Object -First 1
$ofLight.value = "of Light"
$ofLight.traits.holyDamage = @{ value = 8; type = "number" }
$ofLight.traits.healingPower = $ofLight.traits.healthRegen
$ofLight.traits.PSObject.Properties.Remove('healthRegen')
$specialArray += $ofLight

# of Dark (renamed from of Death)
$ofDark = $json.components.death | Select-Object -First 1
$ofDark.value = "of Dark"
$specialArray += $ofDark

# Add new element suffixes
$specialArray += @(
    [PSCustomObject]@{
        value = "of Earth"
        rarityWeight = 15
        traits = @{
            earthDamage = @{ value = 8; type = "number" }
            armorBonus = @{ value = 5; type = "number" }
            resistEarth = @{ value = 10; type = "number" }
            crushingPower = @{ value = 5; type = "number" }
        }
    },
    [PSCustomObject]@{
        value = "of Wind"
        rarityWeight = 15
        traits = @{
            windDamage = @{ value = 8; type = "number" }
            lightningDamage = @{ value = 8; type = "number" }
            movementSpeed = @{ value = 15; type = "number" }
            evasion = @{ value = 10; type = "number" }
            resistWind = @{ value = 10; type = "number" }
        }
    },
    [PSCustomObject]@{
        value = "of Water"
        rarityWeight = 15
        traits = @{
            waterDamage = @{ value = 8; type = "number" }
            iceDamage = @{ value = 8; type = "number" }
            healingPower = @{ value = 10; type = "number" }
            resistWater = @{ value = 10; type = "number" }
            resistIce = @{ value = 10; type = "number" }
        }
    }
)

$newComponents["special"] = $specialArray

# Update metadata
$json.metadata.componentKeys = @("quality", "element", "combat", "magic", "special")
$json.metadata.patternTokens = @("base", "quality", "element", "combat", "magic", "special")
$json.metadata.lastUpdated = "2025-12-30"
$json.metadata.description = "Unified enchantment naming system with 5-component structure (v4.0)"
$json.metadata.notes = @(
    "Base token refers to enchantment types from catalog.json",
    "quality: power level multipliers (Minor to Transcendent)",
    "element: 7 primary elements with sub-types - damage type prefixes",
    "combat: physical stats (strength, defense) - suffixes",
    "magic: spell stats (intelligence, spell power) - suffixes", 
    "special: agility + element specialization suffixes",
    "Traits are multiplied by quality level where applicable"
)

# Update patterns to use new component names
$json.patterns = @(
    @{ pattern = "{base}"; rarityWeight = 100 },
    @{ pattern = "{quality} {base}"; rarityWeight = 80 },
    @{ pattern = "{element} {base}"; rarityWeight = 60 },
    @{ pattern = "{base} {combat}"; rarityWeight = 50 },
    @{ pattern = "{base} {magic}"; rarityWeight = 50 },
    @{ pattern = "{base} {special}"; rarityWeight = 40 },
    @{ pattern = "{quality} {element} {base}"; rarityWeight = 30 },
    @{ pattern = "{quality} {base} {combat}"; rarityWeight = 25 },
    @{ pattern = "{element} {base} {magic}"; rarityWeight = 20 },
    @{ pattern = "{quality} {element} {base} {special}"; rarityWeight = 10 }
)

# Assign new components
$json.components = $newComponents

Write-Host "Writing reorganized file..." -ForegroundColor Green

# Convert to JSON with proper formatting
$jsonOutput = $json | ConvertTo-Json -Depth 10

# Fix PowerShell's array formatting issues
$jsonOutput = $jsonOutput -replace '(?m)^\s+"(\w+)":\s+\[', '    "$1": ['
$jsonOutput = $jsonOutput -replace '(?m)^\s+\{', '      {'

# Write to file
$jsonOutput | Set-Content -Path $filePath -Encoding UTF8

Write-Host "`nReorganization complete!" -ForegroundColor Green
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  - quality: 8 enchantments" -ForegroundColor White
Write-Host "  - element: 21 enchantments (16 existing + 5 new)" -ForegroundColor White
Write-Host "  - combat: 12 enchantments" -ForegroundColor White
Write-Host "  - magic: 12 enchantments" -ForegroundColor White
Write-Host "  - special: 13 enchantments (6 agility + 7 element suffixes)" -ForegroundColor White
Write-Host "  Total: 66 enchantments" -ForegroundColor Yellow
