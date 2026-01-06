# Inventory System

**Status**: See [IMPLEMENTATION_STATUS.md](../IMPLEMENTATION_STATUS.md)

## Overview

The Inventory System manages item storage, equipment, consumables, and procedural item generation with rarity tiers and trait systems.

## Core Components

### Item Management
- **Storage**: Limited inventory capacity (20 slots)
- **Equipment Slots**: Weapon, armor, shield, accessory
- **Sorting**: Organize by name, type, or rarity

### Item Categories
- **Weapons**: Melee and ranged with varying damage types
- **Armor**: Light to heavy protective gear
- **Shields**: Defensive equipment increasing block chance
- **Consumables**: Potions and scrolls for healing/buffs
- **Accessories**: Rings, amulets providing passive bonuses

### Rarity System
Five tiers: Common, Uncommon, Rare, Epic, Legendary affecting stats and drop rates.

### Trait System
Items possess traits providing offensive, defensive, or utility bonuses with procedural trait assignment.

### Procedural Generation
- **Name Generation**: Composable patterns for unlimited variety
- **Stat Generation**: Scales with rarity and level
- **Trait Assignment**: Based on item type and rarity pools

## Key Features

- **Capacity Management**: Strategic choices about what to carry
- **Equipment Optimization**: Multiple slots for build customization
- **Procedural Variety**: Virtually unlimited item combinations
- **Data-Driven**: JSON v4.0 specification for content expansion

## Related Systems

- [Combat System](combat-system.md) - Equipment affects combat effectiveness
- [Crafting System](crafting-system.md) - Materials and crafted items
- [Exploration System](exploration-system.md) - Loot distribution by location
