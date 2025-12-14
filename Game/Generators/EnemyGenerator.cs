using Bogus;
using Game.Shared.Data.Models;
using Game.Models;
using Game.Shared.Models;
using Game.Shared.Services;
using Game.Utilities;
using Serilog;

namespace Game.Generators;

/// <summary>
/// Generates random enemies using Bogus and JSON data, scaled to player level.
/// </summary>
public static class EnemyGenerator
{
    private static readonly Random _random = new();
    
    /// <summary>
    /// Generate a random enemy scaled to player level.
    /// </summary>
    public static Enemy Generate(int playerLevel, EnemyDifficulty difficulty = EnemyDifficulty.Normal)
    {
        var type = (EnemyType)_random.Next(0, 7); // Random type except Boss
        return GenerateByType(type, playerLevel, difficulty);
    }
    
    /// <summary>
    /// Generate an enemy of a specific type.
    /// </summary>
    public static Enemy GenerateByType(EnemyType type, int playerLevel, EnemyDifficulty difficulty = EnemyDifficulty.Normal)
    {
        var faker = new Faker();
        
        // Calculate enemy level based on difficulty
        var enemyLevel = CalculateEnemyLevel(playerLevel, difficulty);
        
        // Generate base enemy
        var enemy = new Enemy
        {
            Name = GenerateEnemyName(faker, type),
            Description = GenerateEnemyDescription(type),
            Level = enemyLevel,
            Type = type,
            Difficulty = difficulty
        };
        
        // Set attributes based on level and type
        SetEnemyAttributes(enemy, type);
        
        // Calculate health based on CON and level
        enemy.MaxHealth = (enemy.Constitution * 8) + (enemy.Level * 5);
        enemy.Health = enemy.MaxHealth;
        
        // Set damage based on type
        SetEnemyDamage(enemy, type);
        
        // Apply prefix traits based on difficulty and type
        ApplyPrefixTraits(enemy, type, difficulty);
        
        // Apply dragon color traits if this is a dragon
        if (type == EnemyType.Dragon)
        {
            ApplyDragonColorTraits(enemy);
        }
        
        // Calculate rewards
        enemy.XPReward = CalculateXPReward(enemyLevel, difficulty);
        enemy.GoldReward = CalculateGoldReward(enemyLevel, difficulty);
        
        return enemy;
    }
    
    /// <summary>
    /// Generate a boss enemy for the player's level.
    /// </summary>
    public static Enemy GenerateBoss(int playerLevel)
    {
        var faker = new Faker();
        var bossLevel = (int)(playerLevel * 1.5) + 2;
        
        var enemy = new Enemy
        {
            Name = $"{faker.PickRandom("Lord", "King", "Queen", "Ancient", "Elder", "Dark")} {faker.Name.LastName()}",
            Description = $"A fearsome boss-level {faker.PickRandom("dragon", "demon", "lich", "warlord", "beast")}",
            Level = bossLevel,
            Type = EnemyType.Boss,
            Difficulty = EnemyDifficulty.Boss,
            
            // Boss stats are higher
            Strength = 12 + bossLevel,
            Dexterity = 10 + bossLevel,
            Constitution = 14 + bossLevel,
            Intelligence = 10 + bossLevel,
            Wisdom = 10 + bossLevel,
            Charisma = 12 + bossLevel
        };
        
        enemy.MaxHealth = (enemy.Constitution * 12) + (enemy.Level * 8);
        enemy.Health = enemy.MaxHealth;
        
        enemy.BasePhysicalDamage = 8 + (bossLevel * 2);
        enemy.BaseMagicDamage = 5 + bossLevel;
        
        enemy.XPReward = bossLevel * 100;
        enemy.GoldReward = bossLevel * 50;
        
        return enemy;
    }
    
    /// <summary>
    /// Calculate enemy level based on player level and difficulty.
    /// </summary>
    private static int CalculateEnemyLevel(int playerLevel, EnemyDifficulty difficulty)
    {
        var level = difficulty switch
        {
            EnemyDifficulty.Easy => Math.Max(1, (int)(playerLevel * 0.5)),
            EnemyDifficulty.Normal => Math.Max(1, (int)(playerLevel * 0.8)),
            EnemyDifficulty.Hard => playerLevel,
            EnemyDifficulty.Elite => playerLevel + 2,
            EnemyDifficulty.Boss => (int)(playerLevel * 1.5) + 2,
            _ => playerLevel
        };
        
        return Math.Max(1, level);
    }
    
    /// <summary>
    /// Generate enemy name based on type using JSON data.
    /// </summary>
    private static string GenerateEnemyName(Faker faker, EnemyType type)
    {
        var data = GameDataService.Instance;
        
        return type switch
        {
            EnemyType.Beast => GenerateNameFromData(faker, data.BeastNames),
            EnemyType.Undead => GenerateNameFromData(faker, data.UndeadNames),
            EnemyType.Demon => GenerateNameFromData(faker, data.DemonNames),
            EnemyType.Elemental => GenerateNameFromData(faker, data.ElementalNames),
            EnemyType.Dragon => GenerateDragonName(faker),
            EnemyType.Humanoid => GenerateHumanoidName(faker),
            _ => $"{faker.PickRandom("Common", "Wild", "Rogue")} {faker.PickRandom("Creature", "Monster", "Beast")}"
        };
    }
    
    /// <summary>
    /// Generate a name from enemy name data (Beast, Undead, Demon, Elemental).
    /// </summary>
    private static string GenerateNameFromData(Faker faker, EnemyNameData data)
    {
        // 70% chance for prefix + creature, 30% chance for variant
        if (faker.Random.Bool(0.7f) && data.Prefixes.Count > 0 && data.Creatures.Count > 0)
        {
            var prefix = GameDataService.GetRandom(data.Prefixes);
            var creature = GameDataService.GetRandom(data.Creatures);
            return $"{prefix} {creature}";
        }
        
        // Try to get a variant
        if (data.Variants.Count > 0)
        {
            var variantKey = GameDataService.GetRandom(data.Variants.Keys.ToList());
            var variantList = data.Variants[variantKey];
            if (variantList.Count > 0)
            {
                return GameDataService.GetRandom(variantList);
            }
        }
        
        // Fallback to prefix + creature
        if (data.Creatures.Count > 0)
        {
            var prefix = data.Prefixes.Count > 0 ? GameDataService.GetRandom(data.Prefixes) : "Wild";
            var creature = GameDataService.GetRandom(data.Creatures);
            return $"{prefix} {creature}";
        }
        
        return "Unknown Creature";
    }
    
    /// <summary>
    /// Generate a dragon name using dragon-specific data.
    /// </summary>
    private static string GenerateDragonName(Faker faker)
    {
        var data = GameDataService.Instance.DragonNames;
        
        // 60% chance for prefix + color + type, 40% for variant
        if (faker.Random.Bool(0.6f) && data.Prefixes.Count > 0 && data.Colors.Count > 0 && data.Types.Count > 0)
        {
            var prefix = GameDataService.GetRandom(data.Prefixes);
            var color = GameDataService.GetRandom(data.Colors);
            var dragonType = GameDataService.GetRandom(data.Types);
            return $"{prefix} {color} {dragonType}";
        }
        
        // Try variant
        if (data.Variants.Count > 0)
        {
            var variantKey = GameDataService.GetRandom(data.Variants.Keys.ToList());
            var variantList = data.Variants[variantKey];
            if (variantList.Count > 0)
            {
                return GameDataService.GetRandom(variantList);
            }
        }
        
        // Fallback
        var fallbackColor = data.Colors.Count > 0 ? GameDataService.GetRandom(data.Colors) : "Red";
        return $"{fallbackColor} Dragon";
    }
    
    /// <summary>
    /// Generate a humanoid enemy name using humanoid-specific data.
    /// </summary>
    private static string GenerateHumanoidName(Faker faker)
    {
        var data = GameDataService.Instance.HumanoidNames;
        
        // 60% chance for profession + role, 40% for variant or faction-based name
        if (faker.Random.Bool(0.6f) && data.Professions.Count > 0 && data.Roles.Count > 0)
        {
            var profession = GameDataService.GetRandom(data.Professions);
            var role = GameDataService.GetRandom(data.Roles);
            return $"{profession} {role}";
        }
        
        // Try variant
        if (data.Variants.Count > 0)
        {
            var variantKey = GameDataService.GetRandom(data.Variants.Keys.ToList());
            var variantList = data.Variants[variantKey];
            if (variantList.Count > 0)
            {
                return GameDataService.GetRandom(variantList);
            }
        }
        
        // Fallback
        if (data.Professions.Count > 0)
        {
            return GameDataService.GetRandom(data.Professions);
        }
        
        return "Hostile Warrior";
    }
    
    /// <summary>
    /// Generate enemy description based on type.
    /// </summary>
    private static string GenerateEnemyDescription(EnemyType type)
    {
        return type switch
        {
            EnemyType.Beast => "A dangerous creature driven by primal hunger.",
            EnemyType.Undead => "A shambling horror that defies death itself.",
            EnemyType.Demon => "An evil entity from the dark realms below.",
            EnemyType.Elemental => "A being of pure elemental energy and fury.",
            EnemyType.Humanoid => "A dangerous adversary with ill intentions.",
            EnemyType.Dragon => "A draconic creature of terrible power.",
            _ => "A hostile enemy blocking your path."
        };
    }
    
    /// <summary>
    /// Set enemy attributes based on type and level.
    /// </summary>
    private static void SetEnemyAttributes(Enemy enemy, EnemyType type)
    {
        var baseAttr = 8 + enemy.Level;
        
        switch (type)
        {
            case EnemyType.Beast:
                enemy.Strength = baseAttr + 2;
                enemy.Dexterity = baseAttr + 1;
                enemy.Constitution = baseAttr + 1;
                enemy.Intelligence = baseAttr - 3;
                enemy.Wisdom = baseAttr - 1;
                enemy.Charisma = baseAttr - 2;
                break;
                
            case EnemyType.Undead:
                enemy.Strength = baseAttr + 1;
                enemy.Dexterity = baseAttr - 1;
                enemy.Constitution = baseAttr + 3;
                enemy.Intelligence = baseAttr - 2;
                enemy.Wisdom = baseAttr - 1;
                enemy.Charisma = baseAttr - 3;
                break;
                
            case EnemyType.Demon:
                enemy.Strength = baseAttr + 2;
                enemy.Dexterity = baseAttr + 1;
                enemy.Constitution = baseAttr + 1;
                enemy.Intelligence = baseAttr + 2;
                enemy.Wisdom = baseAttr;
                enemy.Charisma = baseAttr + 1;
                break;
                
            case EnemyType.Elemental:
                enemy.Strength = baseAttr;
                enemy.Dexterity = baseAttr + 2;
                enemy.Constitution = baseAttr;
                enemy.Intelligence = baseAttr + 3;
                enemy.Wisdom = baseAttr + 2;
                enemy.Charisma = baseAttr - 2;
                break;
                
            case EnemyType.Humanoid:
                enemy.Strength = baseAttr + 1;
                enemy.Dexterity = baseAttr + 1;
                enemy.Constitution = baseAttr;
                enemy.Intelligence = baseAttr;
                enemy.Wisdom = baseAttr;
                enemy.Charisma = baseAttr;
                break;
                
            case EnemyType.Dragon:
                enemy.Strength = baseAttr + 3;
                enemy.Dexterity = baseAttr + 1;
                enemy.Constitution = baseAttr + 2;
                enemy.Intelligence = baseAttr + 2;
                enemy.Wisdom = baseAttr + 1;
                enemy.Charisma = baseAttr + 2;
                break;
                
            default:
                enemy.Strength = baseAttr;
                enemy.Dexterity = baseAttr;
                enemy.Constitution = baseAttr;
                enemy.Intelligence = baseAttr;
                enemy.Wisdom = baseAttr;
                enemy.Charisma = baseAttr;
                break;
        }
    }
    
    /// <summary>
    /// Set enemy damage values based on type.
    /// </summary>
    private static void SetEnemyDamage(Enemy enemy, EnemyType type)
    {
        var baseDamage = 3 + enemy.Level;
        
        switch (type)
        {
            case EnemyType.Beast:
                enemy.BasePhysicalDamage = baseDamage + 2;
                enemy.BaseMagicDamage = 0;
                break;
                
            case EnemyType.Undead:
                enemy.BasePhysicalDamage = baseDamage;
                enemy.BaseMagicDamage = baseDamage / 2;
                break;
                
            case EnemyType.Demon:
                enemy.BasePhysicalDamage = baseDamage + 1;
                enemy.BaseMagicDamage = baseDamage + 1;
                break;
                
            case EnemyType.Elemental:
                enemy.BasePhysicalDamage = baseDamage - 1;
                enemy.BaseMagicDamage = baseDamage + 3;
                break;
                
            case EnemyType.Humanoid:
                enemy.BasePhysicalDamage = baseDamage;
                enemy.BaseMagicDamage = baseDamage / 3;
                break;
                
            case EnemyType.Dragon:
                enemy.BasePhysicalDamage = baseDamage + 3;
                enemy.BaseMagicDamage = baseDamage + 2;
                break;
                
            default:
                enemy.BasePhysicalDamage = baseDamage;
                enemy.BaseMagicDamage = 0;
                break;
        }
    }
    
    /// <summary>
    /// Calculate XP reward based on enemy level and difficulty.
    /// </summary>
    private static int CalculateXPReward(int enemyLevel, EnemyDifficulty difficulty)
    {
        var baseXP = enemyLevel * 20;
        
        var multiplier = difficulty switch
        {
            EnemyDifficulty.Easy => 0.5,
            EnemyDifficulty.Normal => 1.0,
            EnemyDifficulty.Hard => 1.5,
            EnemyDifficulty.Elite => 2.0,
            EnemyDifficulty.Boss => 3.0,
            _ => 1.0
        };
        
        return (int)(baseXP * multiplier);
    }
    
    /// <summary>
    /// Calculate gold reward based on enemy level and difficulty.
    /// </summary>
    private static int CalculateGoldReward(int enemyLevel, EnemyDifficulty difficulty)
    {
        var baseGold = enemyLevel * 10;
        
        var multiplier = difficulty switch
        {
            EnemyDifficulty.Easy => 0.5,
            EnemyDifficulty.Normal => 1.0,
            EnemyDifficulty.Hard => 1.5,
            EnemyDifficulty.Elite => 2.0,
            EnemyDifficulty.Boss => 3.0,
            _ => 1.0
        };
        
        return (int)(baseGold * multiplier);
    }
    
    /// <summary>
    /// Apply prefix traits to an enemy based on type and difficulty.
    /// This applies trait multipliers and special abilities from JSON data.
    /// </summary>
    private static void ApplyPrefixTraits(Enemy enemy, EnemyType type, EnemyDifficulty difficulty)
    {
        try
        {
            // Get the appropriate prefix data for the enemy type
            var prefixData = type switch
            {
                EnemyType.Beast => GameDataService.Instance.BeastPrefixes,
                EnemyType.Undead => GameDataService.Instance.UndeadPrefixes,
                EnemyType.Demon => GameDataService.Instance.DemonPrefixes,
                EnemyType.Elemental => GameDataService.Instance.ElementalPrefixes,
                EnemyType.Dragon => GameDataService.Instance.DragonPrefixes,
                EnemyType.Humanoid => GameDataService.Instance.HumanoidPrefixes,
                _ => null
            };
            
            if (prefixData == null)
                return;
            
            // Select prefix tier based on difficulty
            Dictionary<string, EnemyPrefixTraitData>? tierData = difficulty switch
            {
                EnemyDifficulty.Easy => prefixData.Common,
                EnemyDifficulty.Normal => prefixData.Uncommon,
                EnemyDifficulty.Hard => prefixData.Rare,
                EnemyDifficulty.Elite => prefixData.Elite,
                EnemyDifficulty.Boss => prefixData.Boss,
                _ => prefixData.Common
            };
            
            if (tierData == null || tierData.Count == 0)
                return;
            
            // Pick a random prefix from the tier
            var prefixNames = tierData.Keys.ToList();
            var prefixName = prefixNames[_random.Next(prefixNames.Count)];
            var prefix = tierData[prefixName];
            
            // Update enemy name to include prefix
            if (!enemy.Name.Contains(prefix.DisplayName))
            {
                enemy.Name = $"{prefix.DisplayName} {enemy.Name}";
            }
            
            // Apply all traits from the prefix
            foreach (var trait in prefix.Traits)
            {
                enemy.Traits[trait.Key] = trait.Value;
            }
            
            // Apply health and damage multipliers to base stats
            var healthMultiplier = TraitApplicator.GetTrait<double>(enemy, StandardTraits.HealthMultiplier, 1.0);
            var damageMultiplier = TraitApplicator.GetTrait<double>(enemy, StandardTraits.DamageMultiplier, 1.0);
            
            enemy.MaxHealth = (int)(enemy.MaxHealth * healthMultiplier);
            enemy.Health = enemy.MaxHealth;
            enemy.BasePhysicalDamage = (int)(enemy.BasePhysicalDamage * damageMultiplier);
            enemy.BaseMagicDamage = (int)(enemy.BaseMagicDamage * damageMultiplier);
            
            Log.Debug("Applied {PrefixName} traits to {EnemyName}: Health={Health}, PhysDmg={PhysDmg}, Traits={TraitCount}",
                prefix.DisplayName, enemy.Name, enemy.MaxHealth, enemy.BasePhysicalDamage, enemy.Traits.Count);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to apply prefix traits to enemy {EnemyName}", enemy.Name);
        }
    }
    
    /// <summary>
    /// Apply dragon color traits to a dragon enemy.
    /// </summary>
    private static void ApplyDragonColorTraits(Enemy enemy)
    {
        try
        {
            var colorData = GameDataService.Instance.DragonColors;
            
            if (colorData == null || colorData.Count == 0)
            {
                Log.Warning("No dragon color data available");
                return;
            }
            
            // Pick a random dragon color
            var colors = colorData.Values.ToList();
            var colorIndex = _random.Next(colors.Count);
            var color = colors[colorIndex];
            
            // Update enemy name to include color (e.g., "Ancient Dragon" -> "Ancient Red Dragon")
            enemy.Name = $"{enemy.Name.Replace("Dragon", color.DisplayName + " Dragon")}";
            
            // Apply all color traits
            foreach (var trait in color.Traits)
            {
                // Don't override healthMultiplier/damageMultiplier from prefix, but combine them
                if (trait.Key == StandardTraits.HealthMultiplier || trait.Key == StandardTraits.DamageMultiplier)
                {
                    var existingValue = TraitApplicator.GetTrait<double>(enemy, trait.Key, 1.0);
                    var colorValue = trait.Value.AsDouble();
                    enemy.Traits[trait.Key] = new TraitValue(existingValue * colorValue, TraitType.Number);
                }
                else
                {
                    enemy.Traits[trait.Key] = trait.Value;
                }
            }
            
            // Apply any additional multipliers from color
            var healthMultiplier = TraitApplicator.GetTrait<double>(enemy, StandardTraits.HealthMultiplier, 1.0);
            var damageMultiplier = TraitApplicator.GetTrait<double>(enemy, StandardTraits.DamageMultiplier, 1.0);
            
            enemy.MaxHealth = (int)(enemy.MaxHealth * healthMultiplier);
            enemy.Health = enemy.MaxHealth;
            enemy.BasePhysicalDamage = (int)(enemy.BasePhysicalDamage * damageMultiplier);
            enemy.BaseMagicDamage = (int)(enemy.BaseMagicDamage * damageMultiplier);
            
            Log.Debug("Applied {ColorName} dragon color traits to {EnemyName}: Traits={TraitCount}",
                color.DisplayName, enemy.Name, enemy.Traits.Count);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to apply dragon color traits to enemy {EnemyName}", enemy.Name);
        }
    }
}
