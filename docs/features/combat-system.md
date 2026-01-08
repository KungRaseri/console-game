# Combat System

**Status**: ✅ 100% Complete - Abilities & Spells Integrated (See [IMPLEMENTATION_STATUS.md](../IMPLEMENTATION_STATUS.md))

## Overview

The Combat System provides turn-based tactical battles between players and enemies with multiple action options, damage calculations, and combat feedback. **Full integration with abilities, spells, and reactive systems complete as of January 7, 2026.**

## Core Components

### Turn-Based Combat
Players and enemies alternate actions each turn with tactical decision-making.

### Player Actions
- **Attack**: Strike with equipped weapons
- **Defend**: Defensive stance reducing damage and enabling blocks
- **Use Ability**: Execute learned class abilities (mana cost, cooldown)
- **Cast Spell**: Cast learned spells (mana cost, cooldown, success check)
- **Use Item**: Consume potions or scrolls
- **Flee**: Attempt escape based on speed

### Combat Mechanics
- **Damage Calculation**: Weapon power + attributes + modifiers - enemy defense
- **Dodge System**: Chance to avoid attacks based on DEX (triggers reactive abilities)
- **Block System**: Chance to negate damage when defending (triggers reactive abilities)
- **Critical Hits**: Increased damage on successful critical strikes (triggers reactive abilities)
- **Reactive Abilities**: Auto-triggered abilities on combat events (4 trigger types)
- **Passive Bonuses**: Always-active ability bonuses (+5 damage, +2% crit, etc.)
- **Combat Log**: Turn-by-turn colored feedback

### Enemy System
- **Procedural Generation**: Dynamic enemy creation with level scaling
- **Enemy Traits**: Behavioral (aggressive, defensive) and statistical traits
- **Enemy Abilities**: Intelligent AI chooses abilities based on combat situation
- **Enemy Ability AI**: Considers health %, player status, ability types
- **Boss Encounters**: Enhanced enemies with unique mechanics

## Recent Integration (January 7, 2026)

### Abilities & Spells in Combat ✅
- **Combat State Query**: Returns available abilities/spells filtered by cooldowns
- **Action Types**: Extended with `UseAbility` and `CastSpell`
- **Resource Management**: Mana costs and cooldowns tracked per character
- **Execution**: Full damage/healing calculation with dice notation

### Reactive Ability System ✅
- **4 Trigger Events**: onCrit, onDodge, onBlock, onDamageTaken
- **Auto-Execution**: Reactive abilities trigger automatically based on combat events
- **Cooldown Tracking**: Prevents reactive ability spam

### Enemy AI ✅
- **Intelligent Decisions**: Enemies choose abilities based on:
  - Health threshold (defensive when low, offensive when high)
  - Combat phase (buffs at start, debuffs when player strong)
  - Ability cooldowns (uses available abilities strategically)
- **Dice Damage**: Enemy abilities use dice notation for damage calculation

### Passive Bonuses ✅
- **Physical Damage**: +5 per combat/offensive passive ability
- **Magic Damage**: +5 per magical/elemental/divine passive ability
- **Critical Chance**: +2% per offensive/combat passive ability
- **Dodge Chance**: +3% per defensive/stealth passive ability
- **Defense**: +5 per defensive/combat passive ability

## Key Features

- **Tactical Depth**: Multiple action options each turn
- **Attribute Scaling**: Combat effectiveness tied to character build
- **Visual Feedback**: Color-coded damage, crits, and status updates
- **Fair Scaling**: Enemies scale with player level for appropriate challenge

## Related Systems

- [Character System](character-system.md) - Attributes affect damage and defense
- [Skills System](skills-system.md) - Combat skills enhance damage, defense, accuracy
- [Abilities System](abilities-system.md) - Class powers provide tactical options
- [Spells System](spells-system.md) - Learnable magic for magical combat
- [Inventory System](inventory-system.md) - Equipment and consumables used in combat
- [Progression System](progression-system.md) - Leveling unlocks abilities and spell access
- [Status Effects System](status-effects-system.md) - Buffs, debuffs, and conditions in combat
