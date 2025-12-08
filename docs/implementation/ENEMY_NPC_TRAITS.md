# Enhanced JSON Data - Enemies & NPCs

## Overview

Extended the trait system to **enemies and NPCs**, making all procedurally generated entities have meaningful gameplay-impacting properties.

## Changes Made

### 1. Model Updates

#### Enemy Model (`Enemy.cs`)
```csharp
public class Enemy : ITraitable
{
    // ...existing properties...
    
    // NEW: Trait system support
    public Dictionary<string, TraitValue> Traits { get; } = new();
}
```

#### NPC Model (`NPC.cs`)
```csharp
public class NPC : ITraitable  
{
    // ...existing properties...
    
    // NEW: Trait system support
    public Dictionary<string, TraitValue> Traits { get; } = new();
}
```

### 2. Data Models Created

**`EnemyNpcTraitDataModels.cs`** - New file containing:
- `EnemyPrefixTraitData` - Enemy prefix with traits (Dire, Ancient, etc.)
- `OccupationTraitData` - NPC occupation with traits (Blacksmith, Wizard, etc.)
- `EnemyPrefixDataEnhanced` - Structured enemy prefix data (Common → Boss)
- `OccupationDataEnhanced` - Structured occupation data (by category)

### 3. Enhanced JSON Files Created

#### `beast_prefixes_enhanced.json`

**15 enemy prefixes across 5 tiers** with combat-meaningful traits:

| Tier | Prefixes | Key Traits |
|------|----------|------------|
| **Common** | Wild, Feral, Rabid | healthMultiplier (0.9-1.1), damageMultiplier (1.0-1.2), basic effects |
| **Uncommon** | Dire, Giant, Savage | healthMultiplier (1.3-2.0), damageMultiplier (1.3-1.5), knockback/bleed |
| **Rare** | Primal, Alpha, Vicious | healthMultiplier (1.5-2.0), damageMultiplier (1.6-2.0), regeneration, pack tactics |
| **Elite** | Ancient, Elder, Enraged | healthMultiplier (2.0-3.0), damageMultiplier (1.8-2.5), resistances, summons |
| **Boss** | Monstrous, Frenzied, Bloodthirsty | healthMultiplier (3.0-4.0), damageMultiplier (2.5-3.0), AoE, life steal, unstoppable |

**Trait Examples:**
- **Wild Wolf**: 1.0x health, 1.0x damage, +5 attack speed
- **Dire Wolf**: 1.5x health, 1.3x damage, +5 armor
- **Alpha Wolf**: 2.0x health, 1.7x damage, 20% crit, summons 2 allies
- **Ancient Wolf**: 2.5x health, 1.8x damage, 25% all resists, 10 HP regen
- **Monstrous Wolf**: 4.0x health, 2.5x damage, 30 armor, 40% all resists, multi-attack, AoE

#### `occupations_enhanced.json`

**50+ NPC occupations across 10 categories** with service/ability traits:

| Category | Occupations | Key Traits |
|----------|-------------|------------|
| **Merchants** | Merchant, Apothecary, Weaponsmith, Armorer, Jeweler | shopDiscount (5-20%), sellPriceBonus, specialization, crafting abilities |
| **Craftsmen** | Blacksmith, Fletcher, Leatherworker, Artificer | repairCost (-30%), craftingSpeed, quality bonuses, enchanting |
| **Professionals** | Healer, Herbalist, Scholar, Sage, Cartographer | healing power, XP bonus, knowledge, map reveal |
| **Service** | Innkeeper, Cook, Stable Master | cost reduction, effectiveness bonuses, buffs |
| **Nobility** | Knight, Noble, Lord | quest rewards (+50-200 gold), influence, training, titles |
| **Religious** | Priest, Paladin, Oracle | healing, blessings, holy damage, prophecy |
| **Adventurers** | Mercenary, Bounty Hunter, Ranger | combat training, tracking, hireable, bonuses |
| **Magical** | Wizard, Enchanter, Necromancer, Alchemist | spell power, enchanting, summoning, transmutation |
| **Criminal** | Thief, Assassin, Smuggler | stealth, lockpicking, black market, critical damage |
| **Common** | Farmer, Laborer, Beggar | basic bonuses, rumors, quest help |

**Trait Examples:**
- **Blacksmith**: -30% repair cost, 100% repair quality, can craft weapons/armor
- **Wizard**: +40 spell power, +50 mana, -25% spell cost, teaches spells, +4 INT
- **Apothecary**: +10% shop discount, +25% potion effectiveness, +20% healing
- **Assassin**: +50 stealth, 30% crit chance, +75% crit damage, poison damage
- **Noble**: +100 gold reward, +40% influence, grants quests, +3 CHA

## Trait Categories

### Enemy Traits

**Combat Modifiers:**
- `healthMultiplier`, `damageMultiplier` - Scale base stats
- `attackSpeed`, `movementSpeed` - Speed modifications
- `armorBonus`, `resistPhysical`, `resistMagic`, `resistAll` - Defenses

**Special Abilities:**
- `criticalChance`, `criticalDamage` - Critical hits
- `lifeSteal`, `healthRegeneration` - Sustain
- `bleedChance`, `poisonDamage` - Status effects
- `summonAllies`, `multiAttack`, `aoeAttack` - Special attacks
- `packLeader`, `berserk`, `unstoppable`, `bloodRage` - Unique mechanics

### NPC Traits

**Economic:**
- `shopDiscount`, `sellPriceBonus` - Trading bonuses
- `repairCost`, `hireableCost` - Service pricing
- `goldReward`, `questReward` - Quest rewards

**Services:**
- `healingPower`, `healingCost` - Healing services
- `potionEffectiveness`, `canCraftPotions` - Potion crafting
- `canEnchant`, `enchantmentPower` - Enchanting
- `canCraftWeapons`, `canCraftArmor` - Crafting abilities

**Information:**
- `rumorChance`, `secretInformation` - Rumors and secrets
- `questInformation`, `questAccess` - Quest data
- `mapReveal`, `prophecy`, `futureKnowledge` - Exploration

**Character Stats:**
- `strengthBonus`, `dexterityBonus`, `intelligenceBonus` - Stat modifiers
- `wisdomBonus`, `charismaBonus`, `constitutionBonus` - More stats
- `experienceBonus` - Learning speed

**Special:**
- `specialization` - Type (weapons, potions, etc.)
- `teachingAbility`, `combatTraining` - Training
- `blackMarketAccess`, `contraband` - Underground economy

## Usage Example

### Enemy with Traits

```csharp
// Generate "Ancient Dire Wolf" (Elite tier beast)
var enemy = new Enemy
{
    Name = "Ancient Dire Wolf",
    Level = 15
};

// Load and apply "Ancient" prefix traits
var prefixData = /* load from JSON */;
TraitApplicator.ApplyTraits(enemy, prefixData.Traits);

// Enemy now has:
enemy.Traits["healthMultiplier"]   // 2.5 (150% more HP)
enemy.Traits["damageMultiplier"]   // 1.8 (80% more damage)
enemy.Traits["resistPhysical"]     // 25 (25% physical resist)
enemy.Traits["resistMagic"]        // 25 (25% magic resist)
enemy.Traits["healthRegeneration"] // 10 (10 HP per turn)
enemy.Traits["wisdom"]             // true (smart AI)

// Calculate actual stats
int actualHealth = baseHealth * TraitApplicator.GetTrait<double>(enemy, "healthMultiplier", 1.0);
int actualDamage = baseDamage * TraitApplicator.GetTrait<double>(enemy, "damageMultiplier", 1.0);
```

### NPC with Traits

```csharp
// Generate "Master Blacksmith" NPC
var npc = new NPC
{
    Name = "Grond Ironforge",
    Occupation = "Blacksmith"
};

// Load and apply "Blacksmith" occupation traits
var occupationData = /* load from JSON */;
TraitApplicator.ApplyTraits(npc, occupationData.Traits);

// NPC now has:
npc.Traits["repairCost"]        // -30 (-30% repair cost)
npc.Traits["repairQuality"]     // 100 (perfect repairs)
npc.Traits["canCraftWeapons"]   // true
npc.Traits["canCraftArmor"]     // true
npc.Traits["craftingSpeed"]     // 25 (+25% faster crafting)

// Use traits in gameplay
bool canRepair = TraitApplicator.HasTrait(npc, "repairQuality");
int repairDiscount = TraitApplicator.GetTrait<int>(npc, "repairCost", 0);
int finalCost = baseRepairCost + (baseRepairCost * repairDiscount / 100);
```

## Next Steps

### Immediate

1. **Update GameDataService** - Add loading for enhanced enemy/NPC files
2. **Update EnemyGenerator** - Apply traits when generating enemies
3. **Update NpcGenerator** - Apply traits when generating NPCs
4. **Create helper methods** - `GetActualHealth()`, `GetActualDamage()`, etc.

### Future Enhancements

5. **Enemy AI** - Use traits to determine behavior (pack tactics, berserk, etc.)
6. **NPC Services** - Implement shop discounts, crafting, enchanting
7. **Quest System** - Use NPC traits for quest requirements/rewards
8. **Combat System** - Apply enemy trait effects (AoE, life steal, summons)
9. **More Enemy Types** - Dragon prefixes, undead prefixes, demon prefixes
10. **Unique NPCs** - Named NPCs with custom trait combinations

## Benefits

### ✅ Meaningful Variety
- Same creature name with different prefix = completely different fight
- "Wild Wolf" vs "Monstrous Wolf" = 4x health, 2.5x damage, AoE attacks

### ✅ Emergent Gameplay
- NPC traits enable complex interactions (discounts, crafting, teaching)
- Enemy traits create tactical decisions (focus healer, kite slow boss)

### ✅ Data-Driven Balance
- Designers can adjust traits in JSON without code changes
- Easy to add new prefixes/occupations with unique combinations

### ✅ Reusable System
- Same trait infrastructure works for items, enemies, NPCs
- Any entity can have traits with minimal code

### ✅ Extensible
- Add new trait types without changing architecture
- Combine traits for synergies (pack leader + summon allies)

---

**Status**: ✅ Enemy and NPC models updated, enhanced JSON files created  
**Next**: Update generators to load and apply enhanced data  
**Author**: GitHub Copilot  
**Date**: 2024-12-07
