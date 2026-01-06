# Progression System

**Status**: See [IMPLEMENTATION_STATUS.md](../IMPLEMENTATION_STATUS.md)

## Overview

The Progression System handles character advancement through experience, leveling, attribute allocation, skills, abilities, and spells—providing multiple paths for character development.

## Core Components

### Experience & Leveling
- **XP Sources**: Combat, quests, exploration, achievements
- **Leveling Curve**: Exponential scaling to level cap (50)
- **Level Rewards**: Stat increases, attribute points, ability unlocks

### Attribute Allocation
Flexible point distribution each level for specialization, hybrid builds, or balanced approaches.

### Skills System (Practice-Based Progression)
Learned proficiencies that improve through use—"learning by doing":
- **Skill Categories**: Attribute, Combat, Magic, Profession, Survival
- **Ranking System**: Rank 0 (untrained) to 100 (master)
- **Use-Based XP**: Gain skill XP by using the skill
- **Scaling Costs**: Each rank-up costs more XP
- **Per-Rank Bonuses**: Passive bonuses that automatically apply
- **Build Differentiation**: Organic specialization based on playstyle

See [Skills System](skills-system.md) for full details.

### Abilities System (Class-Specific Powers)
Class-granted active powers requiring conscious activation:
- **Ability Categories**: Offensive, Defensive, Support, Passive, Ultimate
- **Resource Management**: Mana costs and cooldown timers
- **Class Identity**: Each class has unique ability set
- **Level Unlocks**: Gain new abilities at milestone levels (5, 10, 15, 20)
- **Tactical Depth**: Growing toolkit of combat options

See [Abilities System](abilities-system.md) for full details.

### Magic & Spell System (Learnable Magic)
Universal magical knowledge accessible to all with sufficient skill:
- **Spell Schools**: Destruction, Restoration, Alteration, Conjuration, Illusion, Mysticism
- **Acquisition**: Find spellbooks, buy from merchants, learn from trainers
- **Skill Requirements**: Minimum magic skill rank per spell level
- **Spell Levels**: Novice, Apprentice, Adept, Expert, Master
- **Skill Scaling**: Higher skill = stronger spells, lower costs, better success rate
- **Build Enabling**: Allows hybrid builds (warrior-mage, rogue-mage, etc.)

See [Spells System](spells-system.md) for full details.

## Key Features

- **Triple Progression**: Levels (core stats), Skills (practice-based), Abilities/Spells (active powers)
- **Flexible Growth**: Multiple valid build paths per class
- **Organic Development**: Skills improve naturally through gameplay
- **Knowledge Collection**: Finding spells creates exploration goals
- **Strategic Investment**: Balance between specialization and versatility
- **Incremental Power**: Steady growth throughout level range

## System Distinctions

**Skills vs Abilities vs Spells:**
- **Skills** = Passive proficiency that improves with use (how good you are at something)
- **Abilities** = Class-granted active powers (special moves always available)
- **Spells** = Learnable magic requiring acquisition (universal access, skill-dependent)

All three systems work together:
- Skills enhance ability/spell effectiveness
- Abilities provide immediate tactical options
- Spells offer build variety and collection goals

## Related Systems

- [Character System](character-system.md) - Base attributes and classes
- [Skills System](skills-system.md) - Practice-based progression details
- [Abilities System](abilities-system.md) - Class power details
- [Spells System](spells-system.md) - Magic system details
- [Combat System](combat-system.md) - Skills/abilities/spells affect combat
- [Quest System](quest-system.md) - XP rewards from quests

