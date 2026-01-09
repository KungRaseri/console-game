using RealmEngine.Shared.Models;
using RealmEngine.Core.Features.SaveLoad;
using RealmEngine.Shared.Utilities;
using Serilog;
using MediatR;
using RealmEngine.Core.Features.Progression.Commands;
using RealmEngine.Core.Services;
using RealmEngine.Core.Features.Progression.Services;
using RealmEngine.Core.Features.Combat.Services;

namespace RealmEngine.Core.Features.Combat;

/// <summary>
/// Service for handling combat mechanics and calculations.
/// </summary>
public class CombatService
{
    private readonly Random _random = new();
    private readonly SaveGameService _saveGameService;
    private readonly IMediator _mediator;
    private readonly ReactiveAbilityService _reactiveAbilityService;
    private readonly EnemyAbilityAIService _enemyAbilityAI;
    private readonly EnemySpellCastingService _enemySpellCastingAI;

    /// <summary>
    /// Initialize the combat service with required dependencies.
    /// </summary>
    public CombatService(
        SaveGameService saveGameService,
        IMediator mediator,
        AbilityCatalogService abilityCatalogService,
        SpellCatalogService spellCatalogService)
    {
        _saveGameService = saveGameService;
        _mediator = mediator;
        _reactiveAbilityService = new ReactiveAbilityService(abilityCatalogService);
        _enemyAbilityAI = new EnemyAbilityAIService(abilityCatalogService);
        _enemySpellCastingAI = new EnemySpellCastingService(spellCatalogService);
    }

    /// <summary>
    /// Parameterless constructor for testing/mocking purposes.
    /// </summary>
    protected CombatService()
    {
        _saveGameService = null!;
        _mediator = null!;
        _reactiveAbilityService = new ReactiveAbilityService();
        _enemyAbilityAI = new EnemyAbilityAIService();
        _enemySpellCastingAI = new EnemySpellCastingService();
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
    public virtual async Task<CombatResult> ExecutePlayerAttack(Character player, Enemy enemy, bool isDefending = false)
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

        // Trigger reactive abilities on critical hit
        if (isCritical)
        {
            _reactiveAbilityService.CheckAndTriggerReactiveAbilities(player, "onCrit");
        }

        // Determine weapon skill for XP awards (from item's skillReference trait)
        string? weaponSkillSlug = player.GetEquippedWeaponSkillSlug();

        // Calculate base damage (weapon + attribute bonuses)
        int baseDamage = CalculatePlayerDamage(player);

        // Apply skill damage multiplier (pass weapon skill slug for bonus)
        double skillMultiplier = SkillEffectCalculator.GetPhysicalDamageMultiplier(player, weaponSkillSlug);
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

        // Award skill XP for successful hit
        if (!string.IsNullOrEmpty(weaponSkillSlug))
        {
            int xpAmount = isCritical ? 15 : 5; // More XP for critical hits
            await _mediator.Send(new AwardSkillXPCommand
            {
                Character = player,
                SkillId = weaponSkillSlug,
                XPAmount = xpAmount,
                ActionSource = isCritical ? "critical_hit" : $"{weaponSkillSlug.Replace("-", "_")}_hit"
            });
        }

        // Award precision skill XP for critical hits
        if (isCritical)
        {
            await _mediator.Send(new AwardSkillXPCommand
            {
                Character = player,
                SkillId = "precision",
                XPAmount = 10,
                ActionSource = "critical_hit"
            });
        }

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
    public async Task<CombatResult> ExecuteEnemyAttack(Enemy enemy, Character player, bool isDefending = false)
    {
        var result = new CombatResult { Success = true };

        // Check if player dodges (base + skill bonus)
        double dodgeChance = player.GetDodgeChance() + SkillEffectCalculator.GetDodgeChanceBonus(player);
        if (RollDodge(dodgeChance))
        {
            result.IsDodged = true;
            result.Message = $"You dodged {enemy.Name}'s attack!";
            
            // Trigger reactive abilities on dodge
            _reactiveAbilityService.CheckAndTriggerReactiveAbilities(player, "onDodge");
            
            // Award acrobatics XP for successful dodge
            await _mediator.Send(new AwardSkillXPCommand
            {
                Character = player,
                SkillId = "acrobatics",
                XPAmount = 8,
                ActionSource = "successful_dodge"
            });
            
            return result;
        }

        // If defending, chance to block
        if (isDefending && RollBlock(50.0)) // 50% block chance when defending
        {
            result.IsBlocked = true;
            result.Message = $"You blocked {enemy.Name}'s attack!";
            
            // Trigger reactive abilities on block
            _reactiveAbilityService.CheckAndTriggerReactiveAbilities(player, "onBlock");
            
            // Award block XP for successful block
            await _mediator.Send(new AwardSkillXPCommand
            {
                Character = player,
                SkillId = "block",
                XPAmount = 8,
                ActionSource = "successful_block"
            });
            
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
            
            // Award armor skill XP for defending (taking reduced damage)
            await _mediator.Send(new AwardSkillXPCommand
            {
                Character = player,
                SkillId = "armor",
                XPAmount = 5,
                ActionSource = "damage_reduction"
            });
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

        // Trigger reactive abilities on damage taken
        _reactiveAbilityService.CheckAndTriggerReactiveAbilities(player, "onDamageTaken");

        // Award armor skill XP when taking damage (not defending)
        if (!isDefending && finalDamage > 0)
        {
            await AwardArmorSkillXP(player, finalDamage);
        }

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
    /// Execute an enemy ability on the player (decided by AI).
    /// </summary>
    /// <param name="enemy">The enemy using the ability</param>
    /// <param name="player">The target player</param>
    /// <param name="abilityStates">Dictionary tracking ability cooldowns (abilityId -> turns remaining)</param>
    /// <returns>Result of the ability usage, or null if no ability was used</returns>
    public UseAbilityResult? ExecuteEnemyAbility(Enemy enemy, Character player, Dictionary<string, int> abilityStates)
    {
        // AI decides whether to use an ability
        var chosenAbilityId = _enemyAbilityAI.DecideAbilityUsage(enemy, player, abilityStates);
        
        if (chosenAbilityId == null)
        {
            return null; // AI chose to use basic attack instead
        }

        // Find the chosen ability
        var ability = enemy.Abilities.FirstOrDefault(a => a.Id == chosenAbilityId);
        if (ability == null)
        {
            Log.Warning($"Enemy {enemy.Name} tried to use ability {chosenAbilityId} but it was not found");
            return null;
        }

        // Check if ability requires mana (future enhancement - enemies don't have mana yet)
        // For now, assume all enemy abilities are usable
        
        // Apply ability cooldown
        abilityStates[chosenAbilityId] = ability.Cooldown;

        // Execute ability effect
        var result = new UseAbilityResult
        {
            Success = true,
            Message = $"{enemy.Name} used {ability.Name}!",
            AbilityUsed = ability,
            ManaCost = 0, // Enemies don't use mana for now
            DamageDealt = 0,
            HealingDone = 0
        };

        // Apply ability effects based on type
        switch (ability.Type)
        {
            case AbilityTypeEnum.Offensive:
                // Calculate ability damage
                int abilityDamage = CalculateAbilityDamage(enemy, ability);
                player.Health = Math.Max(0, player.Health - abilityDamage);
                result = result with 
                { 
                    DamageDealt = abilityDamage,
                    Message = $"{enemy.Name} used {ability.Name}! You took {abilityDamage} damage!"
                };
                
                // Trigger reactive abilities on damage taken
                _reactiveAbilityService.CheckAndTriggerReactiveAbilities(player, "onDamageTaken");
                break;

            case AbilityTypeEnum.Defensive:
            case AbilityTypeEnum.Healing:
                // Healing ability
                int healAmount = CalculateAbilityHealing(enemy, ability);
                enemy.Health = Math.Min(enemy.MaxHealth, enemy.Health + healAmount);
                result = result with 
                { 
                    HealingDone = healAmount,
                    Message = $"{enemy.Name} used {ability.Name} and restored {healAmount} health!"
                };
                break;

            case AbilityTypeEnum.Buff:
                // Buff abilities (future enhancement - apply buffs to enemy)
                result = result with { Message = $"{enemy.Name} used {ability.Name}!" };
                break;

            case AbilityTypeEnum.Debuff:
                // Debuff abilities (future enhancement - apply debuffs to player)
                result = result with { Message = $"{enemy.Name} used {ability.Name} on you!" };
                break;
        }

        Log.Information($"Enemy {enemy.Name} used ability {ability.Name} (Type: {ability.Type})");
        return result;
    }

    /// <summary>
    /// Execute spell casting by enemy using AI to decide which spell to use.
    /// Returns null if no spell was cast.
    /// </summary>
    public CastSpellResult? ExecuteEnemySpell(Enemy enemy, Character player, SpellCatalogService spellCatalog)
    {
        // AI decides which spell to cast (if any)
        string? chosenSpellId = _enemySpellCastingAI.DecideSpellCasting(enemy, player);
        
        if (chosenSpellId == null)
        {
            return null; // AI chose not to cast spell
        }

        // Get spell from catalog
        var spell = spellCatalog.GetSpell(chosenSpellId);
            
        if (spell == null)
        {
            Log.Warning($"Enemy {enemy.Name} tried to cast unknown spell {chosenSpellId}");
            return null;
        }

        // Calculate actual mana cost (Intelligence reduces cost)
        int manaCost = _enemySpellCastingAI.CalculateManaCost(spell, enemy);
        
        // Deduct mana
        enemy.Mana -= manaCost;

        // Initialize result
        var result = new CastSpellResult
        {
            Success = true,
            Message = string.Empty, // Will be set below based on effect
            ManaCostPaid = manaCost,
            SpellCast = spell
        };

        // Apply spell effects based on type
        switch (spell.EffectType)
        {
            case SpellEffectType.Damage:
                // Damage spell - apply damage to player
                int spellDamage = CalculateSpellDamage(enemy, spell);
                player.Health = Math.Max(0, player.Health - spellDamage);
                result = result with 
                { 
                    EffectValue = spellDamage.ToString(),
                    Message = $"{enemy.Name} cast {spell.DisplayName} and dealt {spellDamage} damage!"
                };

                // Trigger reactive abilities (player takes damage)
                _reactiveAbilityService.CheckAndTriggerReactiveAbilities(player, "onDamageTaken");
                break;

            case SpellEffectType.Heal:
                // Healing spell
                int healAmount = CalculateSpellHealing(enemy, spell);
                enemy.Health = Math.Min(enemy.MaxHealth, enemy.Health + healAmount);
                result = result with 
                { 
                    EffectValue = healAmount.ToString(),
                    Message = $"{enemy.Name} cast {spell.DisplayName} and restored {healAmount} health!"
                };
                break;

            case SpellEffectType.Buff:
            case SpellEffectType.Protection:
                // Defensive buff (future enhancement - apply buffs to enemy)
                result = result with { Message = $"{enemy.Name} cast {spell.DisplayName}!" };
                break;

            case SpellEffectType.Debuff:
            case SpellEffectType.Control:
                // Debuff spell (future enhancement - apply debuffs to player)
                result = result with { Message = $"{enemy.Name} cast {spell.DisplayName} on you!" };
                break;

            case SpellEffectType.Utility:
            case SpellEffectType.Summon:
                // Utility spell (future enhancement)
                result = result with { Message = $"{enemy.Name} cast {spell.DisplayName}!" };
                break;
        }

        // Set cooldown
        if (!enemy.SpellCooldowns.ContainsKey(spell.SpellId))
        {
            enemy.SpellCooldowns[spell.SpellId] = spell.Cooldown;
        }
        else
        {
            enemy.SpellCooldowns[spell.SpellId] = spell.Cooldown;
        }

        Log.Information($"Enemy {enemy.Name} cast spell {spell.DisplayName} (Type: {spell.EffectType}, Mana: {manaCost})");
        return result;
    }

    /// <summary>
    /// Calculate damage for an enemy ability.
    /// </summary>
    private int CalculateAbilityDamage(Enemy enemy, Ability ability)
    {
        // Base damage from ability (if no BaseDamage string, use flat 10)
        int baseDamage = 10;
        
        if (!string.IsNullOrEmpty(ability.BaseDamage))
        {
            // TODO: Implement dice rolling - for now, use simple average
            // e.g., "2d6" = 2 * 3.5 = 7
            baseDamage = EstimateDiceDamage(ability.BaseDamage);
        }

        // Add enemy's intelligence bonus (for magic attacks)
        baseDamage += enemy.GetMagicDamageBonus();

        // Apply variance (±20%)
        baseDamage = (int)(baseDamage * _random.Next(80, 121) / 100.0);

        return Math.Max(1, baseDamage);
    }

    /// <summary>
    /// Calculate damage for an enemy spell.
    /// </summary>
    private int CalculateSpellDamage(Enemy enemy, Spell spell)
    {
        // Base damage from spell
        int baseDamage = 15; // Spells generally stronger than abilities
        
        if (!string.IsNullOrEmpty(spell.BaseEffectValue))
        {
            baseDamage = EstimateDiceDamage(spell.BaseEffectValue);
        }

        // Add enemy's intelligence bonus (spells scale with intelligence)
        baseDamage += enemy.GetMagicDamageBonus();

        // Apply variance (±15% for spells, more reliable than physical attacks)
        baseDamage = (int)(baseDamage * _random.Next(85, 116) / 100.0);

        return Math.Max(1, baseDamage);
    }

    /// <summary>
    /// Calculate healing for an enemy ability.
    /// </summary>
    private int CalculateAbilityHealing(Enemy enemy, Ability ability)
    {
        // Base healing from ability (if no BaseDamage string, use flat 10)
        int baseHealing = 10;
        
        if (!string.IsNullOrEmpty(ability.BaseDamage))
        {
            // Use BaseDamage for healing amount
            baseHealing = EstimateDiceDamage(ability.BaseDamage);
        }

        // Add intelligence bonus
        baseHealing += enemy.Intelligence / 2;

        // Apply variance (±20%)
        baseHealing = (int)(baseHealing * _random.Next(80, 121) / 100.0);

        return Math.Max(1, baseHealing);
    }

    /// <summary>
    /// Calculate healing for an enemy spell.
    /// </summary>
    private int CalculateSpellHealing(Enemy enemy, Spell spell)
    {
        // Base healing from spell
        int baseHealing = 20; // Healing spells generally stronger
        
        if (!string.IsNullOrEmpty(spell.BaseEffectValue))
        {
            baseHealing = EstimateDiceDamage(spell.BaseEffectValue);
        }

        // Add intelligence bonus (spells scale with intelligence)
        baseHealing += enemy.Intelligence / 2;

        // Apply variance (±15% for spells)
        baseHealing = (int)(baseHealing * _random.Next(85, 116) / 100.0);

        return Math.Max(1, baseHealing);
    }

    /// <summary>
    /// Estimate average damage from dice notation (e.g., "2d6" returns 7).
    /// TODO: Replace with proper dice roller service.
    /// </summary>
    private int EstimateDiceDamage(string diceNotation)
    {
        try
        {
            // Simple parser for "XdY" format
            var parts = diceNotation.Split('d');
            if (parts.Length == 2)
            {
                int count = int.Parse(parts[0]);
                int sides = int.Parse(parts[1].Split('+')[0]); // Handle "2d6+3"
                int average = count * (sides + 1) / 2;
                
                // Handle bonus (e.g., "2d6+3")
                if (parts[1].Contains('+'))
                {
                    int bonus = int.Parse(parts[1].Split('+')[1]);
                    average += bonus;
                }
                
                return average;
            }
        }
        catch
        {
            // Fallback to default if parsing fails
        }
        
        return 10; // Default fallback
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
    public async Task<CombatOutcome> GenerateVictoryOutcome(Character player, Enemy enemy)
    {
        var outcome = new CombatOutcome
        {
            PlayerVictory = true,
            XPGained = enemy.XPReward,
            GoldGained = enemy.GoldReward
        };

        // Award weapon skill XP for kill (from item's skillReference trait)
        string? weaponSkillSlug = player.GetEquippedWeaponSkillSlug();
        if (!string.IsNullOrEmpty(weaponSkillSlug))
        {
            await _mediator.Send(new AwardSkillXPCommand
            {
                Character = player,
                SkillId = weaponSkillSlug,
                XPAmount = 20,
                ActionSource = $"{weaponSkillSlug.Replace("-", "_")}_kill"
            });
        }

        // Try to generate loot
        // TODO: Modernize - var loot = GenerateLoot(player, enemy);
        // TODO: Modernize - if (loot != null)
        {
        // TODO: Modernize - outcome.LootDropped.Add(loot);
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
        // TODO: Modernize - // TODO: Modernize - var lootItems = Generators.ItemGenerator.GenerateByType(lootType, 1);

        // TODO: Modernize - if (lootItems.Count == 0)
        {
            return null;
        }

        // TODO: Modernize - var loot = lootItems[0];
        // TODO: Modernize - loot.Rarity = lootRarity;
        // TODO: Modernize - return loot;
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
        // TODO: Modernize - summary += $"\n[cyan]Loot:[/]\n";
            foreach (var item in outcome.LootDropped)
            {
                summary += $"  • {item.Name} ({item.Rarity})\n";
            }
        }

        return summary;
    }

    /// <summary>
    /// Award armor skill XP based on equipped armor pieces and damage taken.
    /// </summary>
    /// <param name="player">The character taking damage.</param>
    /// <param name="damageTaken">Amount of damage taken.</param>
    private async Task AwardArmorSkillXP(Character player, int damageTaken)
    {
        // Calculate base XP from damage taken (minimum 2 XP, scale with damage)
        int baseXP = Math.Max(2, damageTaken / 10);

        // Get all equipped armor pieces
        var armorPieces = new[]
        {
            player.EquippedHelmet,
            player.EquippedShoulders,
            player.EquippedChest,
            player.EquippedBracers,
            player.EquippedGloves,
            player.EquippedBelt,
            player.EquippedLegs,
            player.EquippedBoots
        }.Where(item => item != null).ToList();

        // Award XP to each armor piece's associated skill
        foreach (var armor in armorPieces)
        {
            // Get the skillReference trait from the armor
            var totalTraits = armor!.GetTotalTraits();
            
            if (totalTraits.TryGetValue("skillReference", out var skillRefTrait) &&
                skillRefTrait.Type == TraitType.String &&
                !string.IsNullOrWhiteSpace(skillRefTrait.AsString()))
            {
                // Extract skill ID from reference (e.g., "@skills/armor:plate-armor" -> "plate-armor")
                string skillReference = skillRefTrait.AsString();
                string skillId = ExtractSkillIdFromReference(skillReference);

                if (!string.IsNullOrWhiteSpace(skillId))
                {
                    await _mediator.Send(new AwardSkillXPCommand
                    {
                        Character = player,
                        SkillId = skillId,
                        XPAmount = baseXP,
                        ActionSource = "armor_damage_taken"
                    });
                }
            }
        }
    }

    /// <summary>
    /// Extract the skill ID from a reference string (e.g., "@skills/armor:plate-armor" -> "plate-armor").
    /// </summary>
    private string ExtractSkillIdFromReference(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
            return string.Empty;

        // Format: @domain/path:item-name
        int colonIndex = reference.IndexOf(':');
        if (colonIndex > 0 && colonIndex < reference.Length - 1)
        {
            return reference.Substring(colonIndex + 1);
        }

        return string.Empty;
    }
}