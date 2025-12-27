# Split abilities.json files into abilities_names.json and abilities_catalog.json
# Following the pattern established in the demons folder

$enemyFolders = @(
    "beasts",
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

foreach ($folder in $enemyFolders) {
    $abilitiesPath = Join-Path $basePath "$folder\abilities.json"
    
    if (-not (Test-Path $abilitiesPath)) {
        Write-Warning "Skipping $folder - abilities.json not found"
        continue
    }
    
    Write-Host "Processing $folder..." -ForegroundColor Cyan
    
    # Read the original file
    $content = Get-Content $abilitiesPath -Raw | ConvertFrom-Json
    
    # Create abilities_names.json structure (pattern-based generation)
    $namesJson = @{
        metadata = @{
            description = "Pattern-based ability name generation for $folder"
            version = "4.0"
            lastUpdated = (Get-Date -Format "yyyy-MM-dd")
            type = "pattern_generation"
            supportsTraits = $true
            componentKeys = @("prefix", "base", "suffix")
            patternTokens = @("base", "prefix", "suffix")
            totalPatterns = 4
            raritySystem = "weight-based"
            notes = @(
                "Base token resolves from abilities_catalog.json",
                "Components have rarityWeight for emergent rarity calculation",
                "Traits are applied when components are selected in patterns",
                "Component weights multiply for final rarity: prefix(35) * base(10) = uncommon"
            )
        }
        components = @{
            prefix = @(
                @{
                    value = "Enhanced"
                    rarityWeight = 10
                    traits = @{
                        damageBonus = @{
                            value = 2
                            type = "number"
                        }
                    }
                },
                @{
                    value = "Superior"
                    rarityWeight = 35
                    traits = @{
                        damageBonus = @{
                            value = 3
                            type = "number"
                        }
                    }
                },
                @{
                    value = "Legendary"
                    rarityWeight = 75
                    traits = @{
                        damageBonus = @{
                            value = 5
                            type = "number"
                        }
                    }
                }
            )
            base = @()  # Will be populated from abilities
            suffix = @(
                @{
                    value = "of Power"
                    rarityWeight = 10
                    traits = @{
                        damageBonus = @{
                            value = 1
                            type = "number"
                        }
                    }
                },
                @{
                    value = "of Mastery"
                    rarityWeight = 35
                    traits = @{
                        cooldownReduction = @{
                            value = 2
                            type = "number"
                        }
                    }
                },
                @{
                    value = "of the Gods"
                    rarityWeight = 75
                    traits = @{
                        damageBonus = @{
                            value = 3
                            type = "number"
                        }
                        cooldownReduction = @{
                            value = 5
                            type = "number"
                        }
                    }
                }
            )
        }
        patterns = @(
            @{
                pattern = "[base]"
                weight = 40
                example = "Ability Name"
            },
            @{
                pattern = "[prefix] [base]"
                weight = 35
                example = "Enhanced Ability Name"
            },
            @{
                pattern = "[base] [suffix]"
                weight = 20
                example = "Ability Name of Power"
            },
            @{
                pattern = "[prefix] [base] [suffix]"
                weight = 5
                example = "Enhanced Ability Name of Power"
            }
        )
    }
    
    # Populate base components from items
    foreach ($item in $content.items) {
        $namesJson.components.base += @{
            value = $item.name
            rarityWeight = $item.rarityWeight
            traits = @{
                abilityName = @{
                    value = $item.name
                    type = "string"
                }
            }
        }
    }
    
    # Create abilities_catalog.json structure
    $catalogJson = @{
        metadata = @{
            description = "$folder ability catalog with base definitions (v4.0)"
            version = "4.0"
            lastUpdated = (Get-Date -Format "yyyy-MM-dd")
            type = "ability_catalog"
            supportsTraits = $true
            totalAbilityTypes = $content.components.PSObject.Properties.Name.Count
            totalAbilities = $content.items.Count
            usage = "Provides base ability definitions for pattern generation with abilities_names.json"
        }
        ability_types = @{}
    }
    
    # Organize abilities by component type
    foreach ($componentType in $content.components.PSObject.Properties) {
        $typeName = $componentType.Name
        $abilityNames = $componentType.Value
        
        $catalogJson.ability_types[$typeName] = @{
            traits = @{
                category = @{
                    value = $typeName
                    type = "string"
                }
                abilityClass = @{
                    value = "active"
                    type = "string"
                }
            }
            items = @()
        }
        
        # Add abilities that belong to this type
        foreach ($abilityName in $abilityNames) {
            $ability = $content.items | Where-Object { $_.name -eq $abilityName } | Select-Object -First 1
            if ($ability) {
                $catalogJson.ability_types[$typeName].items += @{
                    name = $ability.name
                    displayName = $ability.displayName
                    description = $ability.description
                    rarityWeight = $ability.rarityWeight
                    traits = @{
                        category = @{
                            value = $typeName
                            type = "string"
                        }
                    }
                }
            }
        }
    }
    
    # Write the split files
    $namesPath = Join-Path $basePath "$folder\abilities_names.json"
    $catalogPath = Join-Path $basePath "$folder\abilities_catalog.json"
    
    $namesJson | ConvertTo-Json -Depth 10 | Set-Content $namesPath -Encoding UTF8
    $catalogJson | ConvertTo-Json -Depth 10 | Set-Content $catalogPath -Encoding UTF8
    
    Write-Host "  ✓ Created abilities_names.json" -ForegroundColor Green
    Write-Host "  ✓ Created abilities_catalog.json" -ForegroundColor Green
}

Write-Host "`nSplit complete! Created files for $($enemyFolders.Count) enemy types." -ForegroundColor Green
Write-Host "Original abilities.json files are preserved. Review the split files before deleting originals." -ForegroundColor Yellow
