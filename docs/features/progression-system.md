# Progression System

**Status**: See [IMPLEMENTATION_STATUS.md](../IMPLEMENTATION_STATUS.md)

## Overview

The Progression System handles character advancement through experience, leveling, attribute allocation, skills, and abilities.

## Core Components

### Experience & Leveling
- **XP Sources**: Combat, quests, exploration, achievements
- **Leveling Curve**: Exponential scaling to level cap (50)
- **Level Rewards**: Stat increases, attribute points, skill/ability unlocks

### Attribute Allocation
Flexible point distribution each level for specialization, hybrid builds, or balanced approaches.

### Skills System (Passive Progression)
Learned proficiencies providing permanent passive bonuses:
- **Skill Categories**: Combat, Defensive, Magic, Utility
- **Multiple Ranks**: Progressive investment in preferred skills
- **Always Active**: Bonuses require no activation
- **Build Differentiation**: Same class plays differently with different skills

### Abilities System (Active Powers)
Active powers requiring conscious activation:
- **Ability Categories**: Offensive, Defensive, Support, Passive, Ultimate
- **Resource Management**: Mana costs and cooldown timers
- **Equipment Requirements**: Some abilities require specific gear
- **Tactical Depth**: Growing toolkit of combat options

## Key Features

- **Flexible Growth**: Multiple valid build paths per class
- **Dual Progression**: Passive skills + active abilities
- **Strategic Investment**: Choose between specialization or versatility
- **Incremental Power**: Steady growth throughout level range

## Related Systems

- [Character System](character-system.md) - Base attributes and classes
- [Combat System](combat-system.md) - Skills/abilities affect combat
- [Quest System](quest-system.md) - XP rewards from quests
