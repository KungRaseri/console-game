# Skills System

**Status**: See [IMPLEMENTATION_STATUS.md](../IMPLEMENTATION_STATUS.md)

## Overview

The Skills System provides practice-based progression where using skills improves proficiency over time. Skills are passive bonuses that rank up through repeated use, creating organic character development distinct from level-based progression.

## Core Philosophy

**"Learning by Doing"**: Skills improve naturally through gameplay actions. Swing swords to improve One-Handed skill, cast spells to improve Destruction magic, sneak to improve Stealth.

**Passive Bonuses**: Skills provide always-active benefits requiring no manual activation. Effects automatically apply during relevant actions (increased damage, better success rates, reduced costs).

**Distinct from Abilities**: Skills are proficiencies (how good you are), abilities are powers (special moves you can do). A Warrior's high One-Handed skill makes normal attacks stronger, while their Charge ability is a special attack requiring activation.

## Skill Categories

### Attribute Skills
Improve core attributes through physical or mental practice:
- **Strength**: Heavy lifting, melee combat with heavy weapons
- **Dexterity**: Archery, dodging, quick movements
- **Constitution**: Endurance running, resisting damage, survival
- **Intelligence**: Spellcasting, puzzle solving, magical study
- **Wisdom**: Perception, meditation, spiritual practice
- **Charisma**: NPC interactions, leadership, persuasion

### Combat Skills
Enhance physical combat effectiveness:
- **One-Handed**: Swords, axes, maces, daggers (single hand)
- **Two-Handed**: Greatswords, battleaxes, warhammers (both hands)
- **Archery**: Bows, crossbows, thrown weapons
- **Block**: Shields, parrying, damage mitigation
- **Heavy Armor**: Plate armor, damage reduction, defense
- **Light Armor**: Leather/cloth, evasion, mobility

### Magic Skills
Boost spellcasting power and efficiency:
- **Destruction**: Offensive magic (fire, ice, lightning)
- **Restoration**: Healing, curing, regeneration
- **Alteration**: Buffs, shields, utility magic
- **Conjuration**: Summoning, binding, conjured weapons
- **Illusion**: Charm, fear, invisibility
- **Mysticism**: Detection, teleportation, magical utility

### Profession Skills
Improve crafting and resource gathering:
- **Blacksmithing**: Forging weapons and armor
- **Alchemy**: Potion and elixir creation
- **Enchanting**: Magical item enhancement

### Survival Skills
Enhance exploration and interaction:
- **Lockpicking**: Opening locked chests and doors
- **Sneaking**: Stealth movement, detection avoidance
- **Pickpocketing**: Stealing from NPCs (future)
- **Speech**: Persuasion, intimidation, better prices

## Skill Mechanics

### Ranking System
- **Initial Rank**: Skills start at Rank 0 (untrained)
- **Max Rank**: Cap at Rank 100 (master level)
- **Rank-Up Cost**: XP required increases each rank (simple scaling initially)
  - Early ranks: Fast progression (1-20 ranks quickly)
  - Mid ranks: Moderate progression (20-60 steady growth)
  - High ranks: Slow progression (60-100 significant investment)

### Experience Gain
- **Use-Based**: Skills gain XP from relevant actions
  - Combat skills: Successful attacks, blocks, kills
  - Magic skills: Spell casting, successful effects
  - Profession skills: Crafting items, gathering materials
  - Survival skills: Successful checks, discoveries
- **Scaling**: Higher-level challenges grant more skill XP
- **Natural Progression**: No grinding required, skills improve through normal gameplay

### Rank Effects
- **Per-Rank Modifiers**: Each skill has specific bonuses per rank
  - **One-Handed Rank 1-100**: +0.5% damage per rank (50% at max)
  - **Destruction Rank 1-100**: +0.4% spell damage per rank (40% at max)
  - **Block Rank 1-100**: +0.3% block chance per rank (30% at max)
  - **Alchemy Rank 1-100**: +1% potion effectiveness per rank (100% at max)
- **Cumulative Bonuses**: Effects stack with attributes and equipment
- **Milestone Unlocks**: Special perks at ranks 25, 50, 75, 100 (future expansion)

### Cost Scaling
- **Simple Formula** (initial implementation): `cost = base_cost * (1 + rank * cost_multiplier)`
  - Example: Rank 1→2 costs 100 XP, Rank 50→51 costs 600 XP, Rank 99→100 costs 1,000 XP
- **Prevents Instant Mastery**: High ranks require significant time investment
- **Rewards Specialization**: Easier to master a few skills than max everything

## Skill Integration

### Combat Integration
- **Damage Calculation**: Weapon skills multiply base damage
- **Success Rates**: Armor skills improve defense, dodge, block
- **Efficiency**: Skills reduce stamina/mana costs (future)

### Magic Integration
- **Spell Power**: Magic skills increase spell damage/healing
- **Mana Efficiency**: Higher ranks reduce mana costs
- **Spell Success**: Low skill = chance to fizzle/fail

### Crafting Integration
- **Quality**: Crafting skills improve item stats
- **Success Rates**: Higher rank = less chance of failure
- **Unlock Recipes**: Advanced crafting requires skill thresholds

### Exploration Integration
- **Lockpicking**: Higher rank = better chance on difficult locks
- **Sneaking**: Higher rank = harder to detect
- **Speech**: Better prices, more dialogue options

## Skill Display

### Character Sheet
- **Skill List**: All skills with current rank (0-100)
- **Progress Bars**: Visual XP progress to next rank
- **Bonuses Shown**: Current effects (+25% damage, +15% block, etc.)
- **Sorting Options**: By category, by rank, by progress

### Rank-Up Notifications
- **In-Game Feedback**: Alert when skill ranks up
- **Bonus Display**: Show new rank and improved bonuses
- **Milestone Celebrations**: Special notifications at major ranks (25, 50, 75, 100)

## Design Goals

### Simplicity First
- **Straightforward Progression**: Use skill → gain XP → rank up → get stronger
- **No Complex Trees**: Avoid skill tree complexity initially
- **Clear Benefits**: Obvious what each rank does

### Future Expansion
System designed to support later additions:
- **Perk Trees**: Unlock special abilities at milestones
- **Skill Synergies**: Bonuses when multiple skills reach thresholds
- **Training**: Pay trainers to boost skill XP gain
- **Skill Books**: Find/buy books granting instant skill XP
- **Specialization Bonuses**: Extra benefits for mastering skill categories

## Related Systems

- [Character System](character-system.md) - Attributes complement skills
- [Progression System](progression-system.md) - Leveling and XP sources
- [Abilities System](abilities-system.md) - Active powers unlocked by skills
- [Combat System](combat-system.md) - Skills affect combat calculations
- [Crafting System](crafting-system.md) - Crafting skills enable item creation
