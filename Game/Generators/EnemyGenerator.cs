using Bogus;
using Game.Models;

namespace Game.Generators;

/// <summary>
/// Generates random enemies using Bogus, scaled to player level.
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
            Description = GenerateEnemyDescription(faker, type),
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
    /// Generate enemy name based on type.
    /// </summary>
    private static string GenerateEnemyName(Faker faker, EnemyType type)
    {
        return type switch
        {
            EnemyType.Beast => $"{faker.PickRandom("Dire", "Wild", "Rabid", "Giant", "Feral")} {faker.PickRandom("Wolf", "Bear", "Boar", "Spider", "Rat")}",
            EnemyType.Undead => $"{faker.PickRandom("Rotting", "Ancient", "Cursed", "Skeletal")} {faker.PickRandom("Zombie", "Skeleton", "Ghoul", "Wraith")}",
            EnemyType.Demon => $"{faker.PickRandom("Lesser", "Greater", "Infernal", "Abyssal")} {faker.PickRandom("Demon", "Devil", "Fiend", "Hellhound")}",
            EnemyType.Elemental => $"{faker.PickRandom("Fire", "Ice", "Earth", "Air", "Lightning")} {faker.PickRandom("Elemental", "Spirit", "Wisp", "Golem")}",
            EnemyType.Humanoid => $"{faker.PickRandom("Bandit", "Thug", "Mercenary", "Cultist", "Raider")} {faker.PickRandom("Scout", "Warrior", "Archer", "Mage")}",
            EnemyType.Dragon => $"{faker.PickRandom("Young", "Wyrm", "Drake", "Dragonling")} {faker.PickRandom("Red", "Blue", "Green", "Black", "White")}",
            _ => $"{faker.PickRandom("Common", "Wild", "Rogue")} {faker.PickRandom("Creature", "Monster", "Beast")}"
        };
    }
    
    /// <summary>
    /// Generate enemy description based on type.
    /// </summary>
    private static string GenerateEnemyDescription(Faker faker, EnemyType type)
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
}
