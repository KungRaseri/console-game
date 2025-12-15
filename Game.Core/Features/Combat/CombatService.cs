using Game.Core.Models;
using Game.Core.Features.SaveLoad;
using Game.Core.Utilities;
using Serilog;

namespace Game.Core.Features.Combat;

/// <summary>
/// Service for handling combat mechanics and calculations.
/// </summary>
public class CombatService
{
    private readonly Random _random = new();
    private readonly SaveGameService _saveGameService;
    
    /// <summary>
    /// Initialize the combat service with required dependencies.
    /// </summary>
    public CombatService(SaveGameService saveGameService)
    {
        _saveGameService = saveGameService;
    }
    
    /// <summary>
    /// Initialize combat by applying difficulty multipliers to enemy stats.
    /// This should be called before combat begins.
    /// </summary>
    public virtual void InitializeCombat(Enemy enemy)
    {
        var difficulty = _saveGameService.GetDifficultySettings();
        
        // Scale enemy health based on difficulty
        enemy.MaxHealth = (int)(enemy.MaxHealth * difficulty.EnemyHealthMultiplier);
        enemy.Health = enemy.MaxHealth;
        
        Log.Information("Enemy {Name} initialized with {Health} HP (difficulty: {Difficulty}, multiplier: {Multiplier})",
            enemy.Name, enemy.Health, difficulty.Name, difficulty.EnemyHealthMultiplier);
    }
    
    /// <summary>
    /// Execute a player attack on an enemy.
    /// </summary>
    public virtual CombatResult ExecutePlayerAttack(Character player, Enemy enemy, bool isDefending = false)
    {
        var result = new CombatResult { Success = true };
        
        // Check if enemy dodges
        if (RollDodge(enemy.GetDodgeChance()))
        {
            result.IsDodged = true;
            result.Message = $"{enemy.Name} dodged your attack!";
            return result;
        }
        
        // Check for critical hit (base + skill bonus)
        double critChance = player.GetCriticalChance() + SkillEffectCalculator.GetCriticalChanceBonus(player);
        bool isCritical = RollCritical(critChance);
        result.IsCritical = isCritical;
        
        // Calculate base damage (weapon + attribute bonuses)
        int baseDamage = CalculatePlayerDamage(player);
        
        // Apply skill damage multiplier
        double skillMultiplier = SkillEffectCalculator.GetPhysicalDamageMultiplier(player);
        baseDamage = (int)(baseDamage * skillMultiplier);
        
        // Apply critical multiplier
        if (isCritical)
        {
            baseDamage = (int)(baseDamage * 2.0);
        }
        
        // Apply enemy defense
        int finalDamage = Math.Max(1, baseDamage - enemy.GetPhysicalDefense());
        
        // Apply difficulty multiplier to player damage
        var difficulty = _saveGameService.GetDifficultySettings();
        finalDamage = (int)(finalDamage * difficulty.PlayerDamageMultiplier);
        finalDamage = Math.Max(1, finalDamage); // Ensure at least 1 damage
        
        // Apply damage to enemy
        enemy.Health = Math.Max(0, enemy.Health - finalDamage);
        result.Damage = finalDamage;
        
        // Generate message
        if (isCritical)
        {
            result.Message = $"CRITICAL HIT! You dealt {finalDamage} damage to {enemy.Name}!";
        }
        else
        {
            result.Message = $"You dealt {finalDamage} damage to {enemy.Name}.";
        }
        
        return result;
    }
    
    /// <summary>
    /// Execute an enemy attack on the player.
    /// </summary>
    public CombatResult ExecuteEnemyAttack(Enemy enemy, Character player, bool isDefending = false)
    {
        var result = new CombatResult { Success = true };
        
        // Check if player dodges (base + skill bonus)
        double dodgeChance = player.GetDodgeChance() + SkillEffectCalculator.GetDodgeChanceBonus(player);
        if (RollDodge(dodgeChance))
        {
            result.IsDodged = true;
            result.Message = $"You dodged {enemy.Name}'s attack!";
            return result;
        }
        
        // If defending, chance to block
        if (isDefending && RollBlock(50.0)) // 50% block chance when defending
        {
            result.IsBlocked = true;
            result.Message = $"You blocked {enemy.Name}'s attack!";
            return result;
        }
        
        // Check for critical hit
        bool isCritical = RollCritical(enemy.GetCriticalChance());
        result.IsCritical = isCritical;
        
        // Calculate base damage
        int baseDamage = enemy.BasePhysicalDamage + enemy.GetPhysicalDamageBonus();
        
        // Roll variance (±20%)
        baseDamage = (int)(baseDamage * _random.Next(80, 121) / 100.0);
        
        // Apply critical multiplier
        if (isCritical)
        {
            baseDamage = (int)(baseDamage * 2.0);
        }
        
        // If defending, reduce damage by 50%
        if (isDefending)
        {
            baseDamage = baseDamage / 2;
        }
        
        // Apply player defense (base + skill multiplier)
        int playerDefense = player.GetPhysicalDefense();
        double defenseMultiplier = SkillEffectCalculator.GetPhysicalDefenseMultiplier(player);
        playerDefense = (int)(playerDefense * defenseMultiplier);
        
        int finalDamage = Math.Max(1, baseDamage - playerDefense);
        
        // Apply difficulty multiplier to enemy damage
        var difficulty = _saveGameService.GetDifficultySettings();
        finalDamage = (int)(finalDamage * difficulty.EnemyDamageMultiplier);
        finalDamage = Math.Max(1, finalDamage); // Ensure at least 1 damage
        
        // Apply damage to player
        player.Health = Math.Max(0, player.Health - finalDamage);
        result.Damage = finalDamage;
        
        // Generate message
        if (isCritical)
        {
            result.Message = $"{enemy.Name} scored a CRITICAL HIT! You took {finalDamage} damage!";
        }
        else if (isDefending)
        {
            result.Message = $"{enemy.Name} attacked! You defended and took {finalDamage} damage.";
        }
        else
        {
            result.Message = $"{enemy.Name} attacked! You took {finalDamage} damage.";
        }
        
        return result;
    }
    
    /// <summary>
    /// Calculate player's attack damage based on equipped weapon and attributes.
    /// </summary>
    private int CalculatePlayerDamage(Character player)
    {
        int weaponDamage = 5; // Base unarmed damage
        
        // Check if player has a weapon equipped
        if (player.EquippedMainHand != null && player.EquippedMainHand.Type == ItemType.Weapon)
        {
            weaponDamage = 8 + (player.EquippedMainHand.Rarity switch
            {
                ItemRarity.Common => 2,
                ItemRarity.Uncommon => 5,
                ItemRarity.Rare => 10,
                ItemRarity.Epic => 18,
                ItemRarity.Legendary => 30,
                _ => 2
            });
            
            // Add weapon's bonus strength
            weaponDamage += player.EquippedMainHand.GetTotalBonusStrength();
        }
        
        // Add STR bonus
        int totalDamage = weaponDamage + player.GetPhysicalDamageBonus();
        
        // Add variance (±15%)
        totalDamage = (int)(totalDamage * _random.Next(85, 116) / 100.0);
        
        return Math.Max(1, totalDamage);
    }
    
    /// <summary>
    /// Attempt to flee from combat.
    /// </summary>
    public CombatResult AttemptFlee(Character player, Enemy enemy)
    {
        // Success chance based on player DEX vs enemy DEX
        double baseChance = 50.0;
        double dexDifference = (player.Dexterity - enemy.Dexterity) * 5.0;
        double fleeChance = Math.Clamp(baseChance + dexDifference, 10.0, 90.0);
        
        bool success = _random.Next(0, 100) < fleeChance;
        
        return new CombatResult
        {
            Success = success,
            Message = success 
                ? "You successfully fled from combat!" 
                : "Failed to escape! The enemy blocks your path!"
        };
    }
    
    /// <summary>
    /// Use a consumable item in combat.
    /// </summary>
    public CombatResult UseItemInCombat(Character player, Item item)
    {
        var result = new CombatResult { Success = true };
        
        if (item.Type != ItemType.Consumable)
        {
            result.Success = false;
            result.Message = $"{item.Name} cannot be used in combat.";
            return result;
        }
        
        // Apply consumable effects (simplified - expand later)
        if (item.Name.Contains("Health", StringComparison.OrdinalIgnoreCase) || 
            item.Name.Contains("Potion", StringComparison.OrdinalIgnoreCase))
        {
            int healing = 30; // Base healing
            player.Health = Math.Min(player.MaxHealth, player.Health + healing);
            result.Healing = healing;
            result.Message = $"You used {item.Name} and restored {healing} health!";
        }
        else if (item.Name.Contains("Mana", StringComparison.OrdinalIgnoreCase))
        {
            int manaRestore = 20;
            player.Mana = Math.Min(player.MaxMana, player.Mana + manaRestore);
            result.Message = $"You used {item.Name} and restored {manaRestore} mana!";
        }
        else
        {
            result.Message = $"You used {item.Name}.";
        }
        
        // Remove item from inventory
        player.Inventory.Remove(item);
        
        return result;
    }
    
    /// <summary>
    /// Roll for dodge based on chance percentage.
    /// </summary>
    private bool RollDodge(double dodgeChance)
    {
        return _random.Next(0, 10000) < (dodgeChance * 100);
    }
    
    /// <summary>
    /// Roll for critical hit based on chance percentage.
    /// </summary>
    private bool RollCritical(double critChance)
    {
        return _random.Next(0, 10000) < (critChance * 100);
    }
    
    /// <summary>
    /// Roll for block when defending.
    /// </summary>
    private bool RollBlock(double blockChance)
    {
        return _random.Next(0, 100) < blockChance;
    }
    
    /// <summary>
    /// Generate combat outcome after victory.
    /// </summary>
    public CombatOutcome GenerateVictoryOutcome(Character player, Enemy enemy)
    {
        var outcome = new CombatOutcome
        {
            PlayerVictory = true,
            XPGained = enemy.XPReward,
            GoldGained = enemy.GoldReward
        };
        
        // Try to generate loot
        var loot = GenerateLoot(player, enemy);
        if (loot != null)
        {
            outcome.LootDropped.Add(loot);
        }
        
        // Generate summary
        outcome.Summary = GenerateVictorySummary(enemy, outcome);
        
        return outcome;
    }
    
    /// <summary>
    /// Generate loot item based on enemy difficulty and player stats.
    /// </summary>
    private Item? GenerateLoot(Character player, Enemy enemy)
    {
        double lootChance = GetLootChance(enemy.Difficulty);
        lootChance += player.GetRareItemChance();
        
        if (_random.Next(0, 100) >= lootChance)
        {
            return null;
        }
        
        var lootRarity = DetermineLootRarity(enemy.Difficulty);
        var lootType = (ItemType)_random.Next(1, 14); // Skip Consumable at 0
        var lootItems = Generators.ItemGenerator.GenerateByType(lootType, 1);
        
        if (lootItems.Count == 0)
        {
            return null;
        }
        
        var loot = lootItems[0];
        loot.Rarity = lootRarity;
        return loot;
    }
    
    /// <summary>
    /// Get base loot drop chance for enemy difficulty.
    /// </summary>
    private double GetLootChance(EnemyDifficulty difficulty)
    {
        return difficulty switch
        {
            EnemyDifficulty.Easy => 20.0,
            EnemyDifficulty.Normal => 40.0,
            EnemyDifficulty.Hard => 60.0,
            EnemyDifficulty.Elite => 80.0,
            EnemyDifficulty.Boss => 100.0,
            _ => 30.0
        };
    }
    
    /// <summary>
    /// Determine loot rarity based on enemy difficulty.
    /// </summary>
    private ItemRarity DetermineLootRarity(EnemyDifficulty difficulty)
    {
        return difficulty switch
        {
            EnemyDifficulty.Easy => ItemRarity.Common,
            EnemyDifficulty.Normal => _random.Next(0, 100) < 70 ? ItemRarity.Common : ItemRarity.Uncommon,
            EnemyDifficulty.Hard => _random.Next(0, 100) < 50 ? ItemRarity.Uncommon : ItemRarity.Rare,
            EnemyDifficulty.Elite => _random.Next(0, 100) < 60 ? ItemRarity.Rare : ItemRarity.Epic,
            EnemyDifficulty.Boss => _random.Next(0, 100) < 30 ? ItemRarity.Epic : ItemRarity.Legendary,
            _ => ItemRarity.Common
        };
    }
    
    /// <summary>
    /// Generate victory summary message.
    /// </summary>
    private string GenerateVictorySummary(Enemy enemy, CombatOutcome outcome)
    {
        var summary = $"Victory! Defeated {enemy.Name}!\n\n";
        summary += $"[green]+{outcome.XPGained} XP[/]\n";
        summary += $"[yellow]+{outcome.GoldGained} Gold[/]\n";
        
        if (outcome.LootDropped.Any())
        {
            summary += $"\n[cyan]Loot:[/]\n";
            foreach (var item in outcome.LootDropped)
            {
                summary += $"  • {item.Name} ({item.Rarity})\n";
            }
        }
        
        return summary;
    }
}
