# NPC Reorganization Script
# Creates all remaining catalog.json files for the NPC social class split
# Run this from Game.Data\Data\Json\npcs directory

$ErrorActionPreference = "Stop"

Write-Host "NPC Reorganization - Catalog File Generator" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Read the original catalog
$originalCatalogPath = "catalog.json"
if (!(Test-Path $originalCatalogPath)) {
    Write-Error "Original catalog.json not found!"
    exit 1
}

Write-Host "Reading original catalog.json..." -ForegroundColor Yellow
$originalCatalog = Get-Content $originalCatalogPath -Raw | ConvertFrom-Json

Write-Host "Original catalog loaded: $($originalCatalog.metadata.componentKeys -join ', ')" -ForegroundColor Green
Write-Host ""

# Define the structure for each social class
$socialClasses = @{
    "merchants" = @{
        "backgrounds" = @()
        "occupations" = @("GeneralMerchant")
        "description" = "Merchant NPCs - traders and sellers specializing in commerce"
        "socialClass" = "merchant"
    }
    "professionals" = @{
        "backgrounds" = @("ScholarlyScribe", "HealerApprentice", "Alchemist", "LoreKeeper")
        "occupations" = @("Apothecary", "Healer", "Herbalist", "Scholar", "Sage", "Cartographer")
        "description" = "Professional NPCs - educated specialists providing services"
        "socialClass" = "professional"
    }
    "military" = @{
        "backgrounds" = @("Soldier", "FormerMercenary", "BountyHunterRetired")
        "occupations" = @("Guard")
        "description" = "Military NPCs - soldiers, guards, and combat specialists"
        "socialClass" = "military"
    }
    "criminal" = @{
        "backgrounds" = @("FormerCriminal", "FormerSpy", "FormerSmuggler", "ReformedThief", "FormerPirate")
        "occupations" = @()
        "description" = "Criminal NPCs - reformed outlaws and underworld figures"
        "socialClass" = "criminal"
    }
    "noble" = @{
        "backgrounds" = @("NobleBorn", "KnightErrant", "CourtAttendant")
        "occupations" = @("Noble")
        "description" = "Noble NPCs - aristocracy and high-ranking individuals"
        "socialClass" = "noble"
    }
    "magical" = @{
        "backgrounds" = @("ApprenticeMage", "HedgeWizard")
        "occupations" = @("Artificer")
        "description" = "Magical NPCs - arcane practitioners and enchanters"
        "socialClass" = "magical"
    }
    "religious" = @{
        "backgrounds" = @("Acolyte")
        "occupations" = @("Priest")
        "description" = "Religious NPCs - clergy and divine practitioners"
        "socialClass" = "religious"
    }
    "service" = @{
        "backgrounds" = @()
        "occupations" = @("Innkeeper", "TavernKeeper", "Cook", "StableMaster")
        "description" = "Service NPCs - hospitality workers and service providers"
        "socialClass" = "service"
    }
}

Write-Host "Generating catalog files for $($socialClasses.Count) social classes..." -ForegroundColor Yellow
Write-Host ""

foreach ($className in $socialClasses.Keys) {
    $classInfo = $socialClasses[$className]
    $targetDir = $className
    
    Write-Host "Processing: $className" -ForegroundColor Cyan
    Write-Host "  Backgrounds: $($classInfo.backgrounds.Count)" -ForegroundColor Gray
    Write-Host "  Occupations: $($classInfo.occupations.Count)" -ForegroundColor Gray
    
    # Create directory if it doesn't exist
    if (!(Test-Path $targetDir)) {
        New-Item -ItemType Directory -Path $targetDir | Out-Null
        Write-Host "  Created directory: $targetDir" -ForegroundColor Green
    }
    
    # Create basic catalog structure
    $catalog = @{
        metadata = @{
            type = "hierarchical_catalog"
            version = "4.0"
            description = $classInfo.description
            lastUpdated = (Get-Date -Format "yyyy-MM-dd")
            socialClass = $classInfo.socialClass
            componentKeys = @("backgrounds", "occupations")
            notes = @(
                "Split from original npcs/catalog.json on 2025-12-29"
                "Part of hierarchical reorganization matching enemies pattern"
                "Backgrounds represent past/history, occupations represent current job"
            )
        }
        backgrounds = @{}
        occupations = @{}
    }
    
    $catalogPath = Join-Path $targetDir "catalog.json"
    $catalog | ConvertTo-Json -Depth 50 | Set-Content $catalogPath -Encoding UTF8
    
    Write-Host "  Created: $catalogPath" -ForegroundColor Green
}

Write-Host ""
Write-Host "Catalog generation complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Manually populate catalog files with NPC data from original catalog.json" -ForegroundColor Gray
Write-Host "2. Add dialogue references to all NPCs" -ForegroundColor Gray
Write-Host "3. Create names.json files for each category" -ForegroundColor Gray
Write-Host "4. Create .cbconfig.json files" -ForegroundColor Gray
Write-Host "5. Run dotnet build to validate" -ForegroundColor Gray
