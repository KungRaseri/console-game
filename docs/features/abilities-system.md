# Abilities System

**Status**: See [IMPLEMENTATION_STATUS.md](../IMPLEMENTATION_STATUS.md)

## Overview

The Abilities System provides class and species-specific active powers that characters can use in combat or exploration. Unlike passive skills, abilities require conscious activation and resource management (mana, cooldowns).

## Core Philosophy

**"Special Powers"**: Abilities are signature moves defining character identity. A Warrior's Charge, a Rogue's Backstab, a Mage's Fireball—these are memorable powers that feel impactful.

**Active Activation**: Abilities must be manually triggered during appropriate situations (combat turns, exploration moments).

**Class Identity**: Each class has a unique ability set reflecting their archetype. Warriors are melee-focused, Rogues are cunning, Mages are arcane.

**Distinct from Skills & Spells**: 
- **Skills** = How good you are at something (passive proficiency)
- **Abilities** = Class-granted special powers (active, limited use)
- **Spells** = Learnable magic anyone can acquire (universal, skill-dependent)

## Ability Categories

### Offensive Abilities
Direct damage powers for eliminating enemies:
- **Single-Target**: Focus damage on one enemy (Backstab, Execute, Smite)
- **Area-of-Effect**: Hit multiple enemies (Whirlwind, Cone of Fire, Chain Lightning)
- **Damage-Over-Time**: Apply lasting damage (Poison Strike, Hemorrhage, Curse)
- **Execute Abilities**: Bonus damage against low-health targets

### Defensive Abilities
Active defense maneuvers for survival:
- **Blocks**: Temporarily negate incoming damage (Shield Wall, Parry)
- **Dodges**: Avoid attacks with quick reflexes (Evasion, Roll)
- **Counter-Attacks**: Punish attackers (Riposte, Thorns Aura)
- **Damage Reduction**: Temporary armor buffs (Iron Skin, Stone Form)

### Support Abilities
Powers that assist the player or allies (future party system):
- **Healing**: Restore health to self or allies (Lay on Hands, Second Wind)
- **Buffs**: Temporary stat increases (Battle Cry, Blessing, Haste)
- **Debuffs**: Weaken enemies (Disarm, Slow, Weakness Curse)
- **Utility**: Non-combat benefits (Detect Traps, Night Vision, Leap)

### Passive Abilities
Always-active effects requiring no activation:
- **Auras**: Constant area effects (Leadership Aura, Regeneration Aura)
- **Triggered Effects**: Auto-activate on conditions (Critical Riposte on crit, Cleanse on healing)
- **Permanent Bonuses**: Class-specific stat boosts (Warrior +10% HP, Rogue +15% Crit)

### Ultimate Abilities
Powerful special moves with long cooldowns:
- **High Impact**: Significant damage or effect (Final Strike, Divine Intervention)
- **Long Cooldown**: 5-10 combat uses before recharge
- **Mana Intensive**: Costs substantial mana (30-50% of mana pool)
- **Game-Changing**: Can turn the tide of difficult battles

## Ability Mechanics

### Resource Management

**Mana Costs**:
- **Minor Abilities**: 10-20 mana (low-cost, frequent use)
- **Major Abilities**: 30-50 mana (moderate-cost, tactical use)
- **Ultimate Abilities**: 60-100 mana (high-cost, rare use)
- **Mana Scaling**: Costs may reduce with Intelligence or skill ranks

**Cooldown Timers**:
- **Short Cooldown**: 1-3 turns (spam protection)
- **Medium Cooldown**: 4-7 turns (tactical spacing)
- **Long Cooldown**: 8-15 turns (powerful abilities)
- **Ultimate Cooldown**: Once per combat or 20+ turns

### Requirements

**Equipment Requirements**:
- Some abilities require specific gear (staff for arcane abilities, shield for Shield Wall)
- Prevents ability abuse outside intended contexts
- Encourages equipment diversity

**Level Requirements**:
- Starting abilities: Available at level 1
- Unlocked abilities: Gate powerful abilities behind progression (levels 5, 10, 15, 20, etc.)
- Prevents low-level power spikes

**Skill Requirements** (future):
- High-level abilities may require skill thresholds (Two-Handed Rank 50 for Execute)
- Encourages specialization and mastery

### Ability Acquisition

**Starting Abilities**:
Each class begins with 2-3 signature abilities defining their playstyle:
- **Warrior**: Charge (gap closer), Shield Bash (stun)
- **Rogue**: Backstab (high damage), Evasion (dodge)
- **Mage**: Fireball (ranged damage), Mana Shield (defense)
- **Cleric**: Smite (holy damage), Heal (restoration)
- **Ranger**: Power Shot (ranged burst), Trap (utility)
- **Paladin**: Holy Strike (damage+heal), Protective Aura (party defense)

**Level-Up Unlocks**:
Gain new abilities at specific level milestones:
- **Level 5**: Second ability slot
- **Level 10**: Third ability slot (upgrade existing or new ability)
- **Level 15**: Fourth ability slot
- **Level 20**: Ultimate ability unlocked
- **Level 25+**: Advanced abilities and upgrades

**Ability Upgrades** (future):
Existing abilities improve with use or investment:
- Increased damage/effect
- Reduced cooldown
- Lower mana cost
- Additional effects (stun on Charge, heal on Smite)

## Class Ability Sets

### Warrior Abilities
Melee-focused powers for frontline combat:
- **Charge**: Rush at enemy, bonus damage, short stun
- **Shield Bash**: Interrupt attack, stun enemy
- **Whirlwind**: Damage all nearby enemies (AoE)
- **Execute**: Bonus damage against low-health targets
- **Battle Cry**: Temporary +damage/+defense buff
- **Last Stand** (Ultimate): Survive lethal damage, heal to 50%

### Rogue Abilities
Cunning powers for burst damage and evasion:
- **Backstab**: Massive damage from stealth/behind
- **Evasion**: Dodge next attack
- **Poison Strike**: Damage + DoT effect
- **Vanish**: Enter stealth, reset combat
- **Shadow Step**: Teleport behind enemy
- **Assassination** (Ultimate): Instant-kill low-health enemy

### Mage Abilities
Arcane powers for ranged magical damage:
- **Fireball**: High fire damage, small AoE
- **Mana Shield**: Absorb damage with mana
- **Frost Nova**: Freeze nearby enemies
- **Arcane Missiles**: Multiple weak hits
- **Spell Steal**: Copy enemy buff (future)
- **Meteor** (Ultimate): Massive fire AoE

### Cleric Abilities
Divine powers for healing and holy damage:
- **Smite**: Holy damage vs undead
- **Heal**: Restore health to self or ally
- **Divine Shield**: Temporary invulnerability
- **Cleanse**: Remove debuffs
- **Blessing**: Party-wide buff
- **Divine Intervention** (Ultimate): Full heal + resurrect (future)

### Ranger Abilities
Nature-focused powers for ranged combat and utility:
- **Power Shot**: High-damage bow attack
- **Trap**: Set trap at location
- **Hunter's Mark**: Increase damage vs target
- **Pet Summon**: Call animal companion (future)
- **Camouflage**: Stealth in wilderness
- **Arrow Storm** (Ultimate): Multiple arrow volley

### Paladin Abilities
Holy warrior powers balancing offense and defense:
- **Holy Strike**: Damage + self-heal
- **Protective Aura**: Reduce party damage
- **Divine Smite**: High holy damage
- **Lay on Hands**: Strong single-target heal
- **Consecration**: AoE holy damage zone
- **Judgment** (Ultimate): Execute + AoE holy explosion

## Species Abilities (Future)

Racial passive or active abilities based on character species:
- **Dwarf**: Stoneform (damage reduction), Magic Resistance
- **Elf**: Arcane Affinity (+magic damage), Keen Senses (+perception)
- **Orc**: Berserker Rage (+damage, -defense), Intimidation
- **Human**: Versatile (bonus skill XP), Diplomatic (+persuasion)

## Ability Integration

### Combat Integration
- **Turn-Based Usage**: Abilities replace Attack action during combat
- **Tactical Decisions**: Choose between normal attack, ability, item, or flee
- **Cooldown Tracking**: UI displays available vs on-cooldown abilities
- **Mana Management**: Balance resource spending across long fights

### Exploration Integration
- **Utility Abilities**: Some abilities usable outside combat (Detect Traps, Night Vision)
- **Stealth Abilities**: Rogues use abilities during exploration
- **Situational Powers**: Context-sensitive abilities (Leap across gap, Break door)

### Progression Integration
- **Level-Gated**: Abilities unlock as character levels
- **Skill-Synergy**: Skills improve ability effectiveness (high Destruction skill boosts Fireball)
- **Build Diversity**: Ability selection defines combat approach

## Ability UI

### Combat Interface
- **Ability Bar**: List available abilities with mana costs
- **Cooldown Display**: Show turns remaining until ready
- **Hotkeys**: Number keys (1-6) for quick activation
- **Tooltips**: Hover for ability details (damage, effect, cost)

### Ability Book/Page
- **All Abilities**: View learned and locked abilities
- **Descriptions**: Full ability details and requirements
- **Upgrade Paths**: See how abilities improve (future)

## Design Goals

### Impactful Power
- **Feel Powerful**: Abilities should feel significantly stronger than basic attacks
- **Distinct Effects**: Each ability has clear visual and mechanical identity
- **Memorable Moments**: Ultimate abilities create epic combat highlights

### Tactical Depth
- **Resource Management**: Balance mana and cooldowns
- **Situational Use**: Right ability at right time matters
- **Combo Potential**: Abilities synergize with skills and equipment

### Class Identity
- **Unique Kits**: Each class plays distinctly different
- **Archetype Clarity**: Warrior = melee, Rogue = burst, Mage = ranged magic
- **Build Variety**: Within class, different ability selections create sub-archetypes

### Simple Initially, Expandable Later
- **Core Set**: Start with 2-3 abilities per class
- **Gradual Unlocks**: Add abilities through leveling
- **Future Systems**: Support upgrades, combinations, customization

## Related Systems

- [Character System](character-system.md) - Class determines ability set
- [Skills System](skills-system.md) - Skills enhance ability effectiveness
- [Spells System](spells-system.md) - Distinct from learnable magic
- [Combat System](combat-system.md) - Abilities used in combat
- [Progression System](progression-system.md) - Leveling unlocks abilities
