using System.IO;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Game.Shared.Data.Models;

namespace Game.ContentBuilder.Services;

/// <summary>
/// Service for migrating v3 abilities.json files to v4 format (abilities_catalog.json + abilities_names.json)
/// </summary>
public class AbilityMigrationService
{
    // Category mapping table for each enemy type
    private static readonly Dictionary<string, Dictionary<string, string>> CategoryMappings = new()
    {
        ["demons"] = new()
        {
            ["offensive"] = "offensive",
            ["defensive"] = "defensive",
            ["control"] = "control",
            ["summoning"] = "utility",
            ["utility"] = "utility",
            ["legendary"] = "legendary"
        },
        ["beasts"] = new()
        {
            ["combat_traits"] = "offensive",
            ["defensive_traits"] = "defensive",
            ["offensive_traits"] = "offensive",
            ["environmental"] = "utility",
            ["sensory"] = "utility",
            ["leadership"] = "legendary"
        },
        ["dragons"] = new()
        {
            ["offensive"] = "offensive",
            ["defensive"] = "defensive",
            ["mobility"] = "utility",
            ["mental"] = "control",
            ["magical"] = "utility",
            ["environmental"] = "utility",
            ["legendary"] = "legendary"
        },
        ["elementals"] = new()
        {
            ["core_traits"] = "defensive",
            ["offensive"] = "offensive",
            ["defensive"] = "defensive",
            ["control"] = "control",
            ["utility"] = "utility",
            ["legendary"] = "legendary"
        },
        ["undead"] = new()
        {
            ["immunities"] = "defensive",
            ["vulnerabilities"] = "defensive",
            ["offensive"] = "offensive",
            ["defensive"] = "defensive",
            ["auras"] = "control",
            ["summoning"] = "utility",
            ["mobility"] = "utility",
            ["legendary"] = "legendary"
        },
        ["orcs"] = new()
        {
            ["basic_combat"] = "offensive",
            ["defensive_traits"] = "defensive",
            ["offensive_traits"] = "offensive",
            ["leadership_traits"] = "control",
            ["supportsTraits"] = "utility",
            ["legendary_traits"] = "legendary"
        }
    };

    /// <summary>
    /// Migrate a v3 abilities.json file to v4 format
    /// </summary>
    public (AbilityCatalogData catalog, AbilityNamesData names) MigrateToV4(string enemyType, string v3FilePath)
    {
        Log.Information("Starting migration for {EnemyType}", enemyType);

        var v3Json = File.ReadAllText(v3FilePath);
        var v3Data = JObject.Parse(v3Json);

        // Create catalog
        var catalog = CreateCatalog(enemyType, v3Data);

        // Create names/patterns
        var names = CreateNames(enemyType, v3Data, catalog);

        Log.Information("Migration complete for {EnemyType}: {AbilityCount} abilities, {CategoryCount} categories",
            enemyType, catalog.Metadata.TotalAbilities, catalog.Metadata.TotalAbilityTypes);

        return (catalog, names);
    }

    private AbilityCatalogData CreateCatalog(string enemyType, JObject v3Data)
    {
        var catalog = new AbilityCatalogData
        {
            Metadata = new AbilityCatalogMetadata
            {
                Description = $"{char.ToUpper(enemyType[0]) + enemyType.Substring(1)} ability catalog with base definitions (v4.0)",
                Version = "4.0",
                LastUpdated = DateTime.Now.ToString("yyyy-MM-dd"),
                Type = "ability_catalog",
                SupportsTraits = true,
                Usage = "Provides base ability definitions for pattern generation with abilities_names.json"
            }
        };

        // Initialize standard categories with type-level traits
        catalog.AbilityTypes["offensive"] = new AbilityType
        {
            TypeJsonTraits = new Dictionary<string, JsonTraitValue>
            {
                ["category"] = new JsonTraitValue { Value = "offensive", Type = "string" },
                ["damageType"] = new JsonTraitValue { Value = "physical", Type = "string" },
                ["targetType"] = new JsonTraitValue { Value = "enemy", Type = "string" },
                ["abilityClass"] = new JsonTraitValue { Value = "active", Type = "string" }
            },
            Items = new List<AbilityItem>()
        };

        catalog.AbilityTypes["defensive"] = new AbilityType
        {
            TypeJsonTraits = new Dictionary<string, JsonTraitValue>
            {
                ["category"] = new JsonTraitValue { Value = "defensive", Type = "string" },
                ["targetType"] = new JsonTraitValue { Value = "self", Type = "string" },
                ["abilityClass"] = new JsonTraitValue { Value = "passive", Type = "string" }
            },
            Items = new List<AbilityItem>()
        };

        catalog.AbilityTypes["control"] = new AbilityType
        {
            TypeJsonTraits = new Dictionary<string, JsonTraitValue>
            {
                ["category"] = new JsonTraitValue { Value = "control", Type = "string" },
                ["targetType"] = new JsonTraitValue { Value = "enemy", Type = "string" },
                ["abilityClass"] = new JsonTraitValue { Value = "active", Type = "string" }
            },
            Items = new List<AbilityItem>()
        };

        catalog.AbilityTypes["utility"] = new AbilityType
        {
            TypeJsonTraits = new Dictionary<string, JsonTraitValue>
            {
                ["category"] = new JsonTraitValue { Value = "utility", Type = "string" },
                ["abilityClass"] = new JsonTraitValue { Value = "active", Type = "string" }
            },
            Items = new List<AbilityItem>()
        };

        catalog.AbilityTypes["legendary"] = new AbilityType
        {
            TypeJsonTraits = new Dictionary<string, JsonTraitValue>
            {
                ["category"] = new JsonTraitValue { Value = "legendary", Type = "string" },
                ["abilityClass"] = new JsonTraitValue { Value = "ultimate", Type = "string" }
            },
            Items = new List<AbilityItem>()
        };

        // Load abilities from v3 and categorize them
        var items = v3Data["items"] as JArray;
        var components = v3Data["components"] as JObject;

        if (items == null)
        {
            Log.Warning("No items found in v3 data for {EnemyType}", enemyType);
            return catalog;
        }

        // Map abilities to categories
        foreach (var item in items)
        {
            var name = item["name"]?.ToString() ?? "";
            var displayName = item["displayName"]?.ToString() ?? name;
            var description = item["description"]?.ToString() ?? "";
            var rarityWeight = item["rarityWeight"]?.ToObject<int>() ?? 10;

            // Determine category from v3 components
            var category = DetermineCategory(name, components, enemyType);

            var ability = new AbilityItem
            {
                Name = name,
                DisplayName = displayName,
                Description = description,
                RarityWeight = rarityWeight,
                JsonTraits = GenerateTraits(name, description, category, rarityWeight)
            };

            // Add base damage and cooldown for offensive/control abilities
            if (category == "offensive" || category == "control")
            {
                ability.BaseDamage = DetermineBaseDamage(rarityWeight);
                ability.Cooldown = DetermineCooldown(rarityWeight);
                ability.Range = DetermineRange(category);
            }

            catalog.AbilityTypes[category].Items.Add(ability);
        }

        // Update metadata counts
        catalog.Metadata.TotalAbilityTypes = catalog.AbilityTypes.Count;
        catalog.Metadata.TotalAbilities = catalog.AbilityTypes.Values.Sum(t => t.Items.Count);

        return catalog;
    }

    private AbilityNamesData CreateNames(string enemyType, JObject v3Data, AbilityCatalogData catalog)
    {
        var names = new AbilityNamesData
        {
            Metadata = new AbilityNamesMetadata
            {
                Description = $"Pattern-based ability name generation for {enemyType}",
                Version = "4.0",
                LastUpdated = DateTime.Now.ToString("yyyy-MM-dd"),
                Type = "pattern_generation",
                SupportsTraits = true,
                ComponentKeys = new List<string> { "prefix", "base", "suffix" },
                PatternTokens = new List<string> { "base", "prefix", "suffix" },
                TotalPatterns = 4,
                RaritySystem = "weight-based",
                Notes = new List<string>
                {
                    "Base token resolves from abilities_catalog.json",
                    "Components have rarityWeight for emergent rarity calculation",
                    "Traits are applied when components are selected in patterns",
                    "Component weights multiply for final rarity"
                }
            }
        };

        // Extract components from ability names
        var (prefixes, bases, suffixes) = ExtractComponents(catalog);

        names.Components["prefix"] = prefixes;
        names.Components["base"] = bases;
        names.Components["suffix"] = suffixes;

        // Define standard patterns
        names.Patterns = new List<AbilityPattern>
        {
            new() { Pattern = "{base}", Weight = 100, Example = bases.FirstOrDefault()?.Value ?? "Strike" },
            new() { Pattern = "{prefix} {base}", Weight = 40, Example = $"{prefixes.FirstOrDefault()?.Value ?? "Greater"} {bases.FirstOrDefault()?.Value ?? "Strike"}" },
            new() { Pattern = "{base} {suffix}", Weight = 35, Example = $"{bases.FirstOrDefault()?.Value ?? "Strike"} {suffixes.FirstOrDefault()?.Value ?? "of Power"}" },
            new() { Pattern = "{prefix} {base} {suffix}", Weight = 15, Example = $"{prefixes.FirstOrDefault()?.Value ?? "Ancient"} {bases.FirstOrDefault()?.Value ?? "Strike"} {suffixes.FirstOrDefault()?.Value ?? "of the Ancients"}" }
        };

        return names;
    }

    private string DetermineCategory(string abilityName, JObject? components, string enemyType)
    {
        if (components == null) return "utility";

        // Get category mapping for this enemy type
        var mappings = CategoryMappings.GetValueOrDefault(enemyType) ??
                      new Dictionary<string, string>(); // Fallback for unmapped types

        // Search through components to find which category contains this ability
        foreach (var component in components)
        {
            var categoryName = component.Key;
            var abilities = component.Value as JArray;

            if (abilities == null) continue;

            foreach (var ability in abilities)
            {
                if (ability.ToString() == abilityName)
                {
                    // Map to standardized category
                    if (mappings.TryGetValue(categoryName, out var standardCategory))
                    {
                        return standardCategory;
                    }

                    // Fallback mapping based on keywords
                    return categoryName.ToLowerInvariant() switch
                    {
                        var c when c.Contains("offensive") || c.Contains("attack") || c.Contains("combat") => "offensive",
                        var c when c.Contains("defensive") || c.Contains("armor") || c.Contains("resistance") => "defensive",
                        var c when c.Contains("control") || c.Contains("fear") || c.Contains("charm") => "control",
                        var c when c.Contains("summon") || c.Contains("utility") || c.Contains("mobility") => "utility",
                        var c when c.Contains("legendary") || c.Contains("lord") || c.Contains("ancient") => "legendary",
                        _ => "utility"
                    };
                }
            }
        }

        // Fallback based on rarity weight
        return "utility";
    }

    private Dictionary<string, JsonTraitValue> GenerateTraits(string name, string description, string category, int rarityWeight)
    {
        var traits = new Dictionary<string, JsonTraitValue>();

        var lowerName = name.ToLowerInvariant();
        var lowerDesc = description.ToLowerInvariant();

        // Damage-related traits
        if (category == "offensive" || category == "control")
        {
            // Determine damage type from keywords
            if (lowerName.Contains("fire") || lowerName.Contains("flame") || lowerName.Contains("hellfire") || lowerName.Contains("infernal"))
            {
                traits["damageType"] = new JsonTraitValue { Value = "fire", Type = "string" };
                traits["statusEffect"] = new JsonTraitValue { Value = "burning", Type = "string" };
                traits["statusChance"] = new JsonTraitValue { Value = 50, Type = "number" };
            }
            else if (lowerName.Contains("ice") || lowerName.Contains("frost") || lowerName.Contains("frozen"))
            {
                traits["damageType"] = new JsonTraitValue { Value = "ice", Type = "string" };
                traits["statusEffect"] = new JsonTraitValue { Value = "frozen", Type = "string" };
                traits["statusChance"] = new JsonTraitValue { Value = 40, Type = "number" };
            }
            else if (lowerName.Contains("poison") || lowerName.Contains("toxic") || lowerName.Contains("venom"))
            {
                traits["damageType"] = new JsonTraitValue { Value = "poison", Type = "string" };
                traits["statusEffect"] = new JsonTraitValue { Value = "poisoned", Type = "string" };
                traits["statusChance"] = new JsonTraitValue { Value = 100, Type = "number" };
                traits["duration"] = new JsonTraitValue { Value = 10, Type = "number" };
            }
            else if (lowerName.Contains("dark") || lowerName.Contains("shadow") || lowerName.Contains("soul") || lowerName.Contains("curse"))
            {
                traits["damageType"] = new JsonTraitValue { Value = "dark", Type = "string" };
            }
            else if (lowerName.Contains("lightning") || lowerName.Contains("thunder") || lowerName.Contains("storm"))
            {
                traits["damageType"] = new JsonTraitValue { Value = "lightning", Type = "string" };
                traits["statusEffect"] = new JsonTraitValue { Value = "stunned", Type = "string" };
            }

            // Damage bonus based on rarity
            var damageBonus = Math.Max(2, rarityWeight / 15);
            traits["damageBonus"] = new JsonTraitValue { Value = damageBonus, Type = "number" };
        }

        // Control effects
        if (lowerName.Contains("fear") || lowerDesc.Contains("terrif"))
        {
            traits["statusEffect"] = new JsonTraitValue { Value = "feared", Type = "string" };
            traits["areaOfEffect"] = new JsonTraitValue { Value = 10, Type = "number" };
            traits["duration"] = new JsonTraitValue { Value = 5, Type = "number" };
        }
        else if (lowerName.Contains("charm") || lowerDesc.Contains("mind-control"))
        {
            traits["statusEffect"] = new JsonTraitValue { Value = "charmed", Type = "string" };
            traits["duration"] = new JsonTraitValue { Value = 10, Type = "number" };
        }
        else if (lowerName.Contains("stun") || lowerName.Contains("paralyze"))
        {
            traits["statusEffect"] = new JsonTraitValue { Value = "stunned", Type = "string" };
            traits["duration"] = new JsonTraitValue { Value = 3, Type = "number" };
        }

        // Defensive traits
        if (category == "defensive")
        {
            if (lowerName.Contains("regen") || lowerDesc.Contains("heal"))
            {
                traits["healthRegen"] = new JsonTraitValue { Value = 5, Type = "number" };
                traits["regenInterval"] = new JsonTraitValue { Value = 5, Type = "number" };
            }
            else if (lowerName.Contains("armor") || lowerName.Contains("shield"))
            {
                traits["armorBonus"] = new JsonTraitValue { Value = 5, Type = "number" };
                traits["resistPhysical"] = new JsonTraitValue { Value = 25, Type = "number" };
            }
            else if (lowerName.Contains("immun") || lowerDesc.Contains("cannot be harmed"))
            {
                traits["resistMagic"] = new JsonTraitValue { Value = 100, Type = "number" };
            }
        }

        // Utility traits
        if (category == "utility")
        {
            if (lowerName.Contains("summon") || lowerDesc.Contains("calls forth"))
            {
                traits["summonType"] = new JsonTraitValue { Value = "minion", Type = "string" };
                traits["summonCount"] = new JsonTraitValue { Value = 3, Type = "number" };
                traits["duration"] = new JsonTraitValue { Value = 60, Type = "number" };
            }
            else if (lowerName.Contains("teleport") || lowerDesc.Contains("teleport"))
            {
                traits["rangeBonus"] = new JsonTraitValue { Value = 5, Type = "number" };
                traits["castTime"] = new JsonTraitValue { Value = 1, Type = "number" };
            }
        }

        // Legendary multipliers
        if (category == "legendary" || rarityWeight >= 250)
        {
            traits["damageMultiplier"] = new JsonTraitValue { Value = 2.0, Type = "number" };
            traits["healthMultiplier"] = new JsonTraitValue { Value = 2.0, Type = "number" };
        }

        return traits;
    }

    private (List<AbilityComponent> prefixes, List<AbilityComponent> bases, List<AbilityComponent> suffixes)
        ExtractComponents(AbilityCatalogData catalog)
    {
        var prefixes = new List<AbilityComponent>();
        var bases = new List<AbilityComponent>();
        var suffixes = new List<AbilityComponent>();

        // Common prefixes
        var commonPrefixes = new[] { "Greater", "Lesser", "Ancient", "Prime", "Dark", "Cursed", "Holy", "Unholy", "Infernal", "Divine" };
        foreach (var prefix in commonPrefixes)
        {
            prefixes.Add(new AbilityComponent
            {
                Value = prefix,
                RarityWeight = prefix switch
                {
                    "Greater" => 50,
                    "Ancient" => 100,
                    "Prime" => 150,
                    _ => 35
                },
                JsonTraits = new Dictionary<string, JsonTraitValue>
                {
                    ["damageMultiplier"] = new JsonTraitValue { Value = prefix switch { "Ancient" => 1.5, "Prime" => 2.0, _ => 1.2 }, Type = "number" }
                }
            });
        }

        // Extract bases from ability names
        foreach (var abilityType in catalog.AbilityTypes.Values)
        {
            foreach (var ability in abilityType.Items)
            {
                var words = ability.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var baseWord = words.Length > 1 ? words[^1] : ability.Name;

                if (!bases.Any(b => b.Value == baseWord))
                {
                    bases.Add(new AbilityComponent
                    {
                        Value = baseWord,
                        RarityWeight = Math.Max(10, ability.RarityWeight / 5)
                    });
                }
            }
        }

        // Common suffixes
        var commonSuffixes = new[] { "of Power", "of the Ancients", "of Destruction", "of Doom", "of the Damned" };
        foreach (var suffix in commonSuffixes)
        {
            suffixes.Add(new AbilityComponent
            {
                Value = suffix,
                RarityWeight = suffix switch
                {
                    "of the Ancients" => 150,
                    "of the Damned" => 100,
                    _ => 50
                },
                JsonTraits = new Dictionary<string, JsonTraitValue>
                {
                    ["damageBonus"] = new JsonTraitValue { Value = suffix.Contains("Ancients") ? 20 : 10, Type = "number" }
                }
            });
        }

        return (prefixes, bases, suffixes);
    }

    private string? DetermineBaseDamage(int rarityWeight)
    {
        return rarityWeight switch
        {
            < 35 => "1d6",
            < 75 => "2d6",
            < 150 => "3d6",
            < 250 => "4d8",
            _ => "6d10"
        };
    }

    private int DetermineCooldown(int rarityWeight)
    {
        return rarityWeight switch
        {
            < 35 => 5,
            < 75 => 10,
            < 150 => 15,
            < 250 => 30,
            _ => 60
        };
    }

    private int DetermineRange(string category)
    {
        return category switch
        {
            "control" => 10,
            "offensive" => 5,
            _ => 0
        };
    }
}
