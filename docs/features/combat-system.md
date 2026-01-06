# Combat System

**Status**: See [IMPLEMENTATION_STATUS.md](../IMPLEMENTATION_STATUS.md)

## Overview

The Combat System provides turn-based tactical battles between players and enemies with multiple action options, damage calculations, and combat feedback.

## Core Components

### Turn-Based Combat
Players and enemies alternate actions each turn with tactical decision-making.

### Player Actions
- **Attack**: Strike with equipped weapons
- **Defend**: Defensive stance reducing damage and enabling blocks
- **Use Item**: Consume potions or scrolls
- **Flee**: Attempt escape based on speed

### Combat Mechanics
- **Damage Calculation**: Weapon power + attributes + modifiers - enemy defense
- **Dodge System**: Chance to avoid attacks based on DEX
- **Block System**: Chance to negate damage when defending
- **Critical Hits**: Increased damage on successful critical strikes
- **Combat Log**: Turn-by-turn colored feedback

### Enemy System
- **Procedural Generation**: Dynamic enemy creation with level scaling
- **Enemy Traits**: Behavioral (aggressive, defensive) and statistical traits
- **Boss Encounters**: Enhanced enemies with unique mechanics

## Key Features

- **Tactical Depth**: Multiple action options each turn
- **Attribute Scaling**: Combat effectiveness tied to character build
- **Visual Feedback**: Color-coded damage, crits, and status updates
- **Fair Scaling**: Enemies scale with player level for appropriate challenge

## Related Systems

- [Character System](character-system.md) - Attributes affect damage and defense
- [Inventory System](inventory-system.md) - Equipment and consumables used in combat
- [Progression System](progression-system.md) - Skills and abilities enhance combat
