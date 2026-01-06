# Spells System

**Status**: See [IMPLEMENTATION_STATUS.md](../IMPLEMENTATION_STATUS.md)

## Overview

The Spells System provides learnable magic that any character can acquire (with sufficient skill) through spellbooks, scrolls, teachers, and quest rewards. Unlike class-specific abilities, spells are universal magical knowledge requiring magic skill proficiency.

## Core Philosophy

**"Learnable Magic"**: Spells are not class-restricted—anyone can learn magic if they invest in magic skills. A Warrior can learn Fireball if they develop Destruction skill, though they won't be as effective as a dedicated Mage.

**Skill-Dependent**: Spell effectiveness scales with magic skills. Low skill = weak spells, high failure chance. High skill = powerful spells, reliable casting.

**Knowledge Acquisition**: Spells must be found, purchased, or taught—they're not automatic. This creates exploration incentives and progression goals.

**Distinct from Abilities & Skills**:
- **Skills** = Proficiency at something (passive progression)
- **Abilities** = Class-granted powers (active, always available)
- **Spells** = Learnable magic (active, must be acquired, skill-dependent)

## Spell Schools (Domains)

### Destruction (Offensive Magic)
Elemental damage spells for combat:
- **Fire**: Burning damage, DoT effects (Fireball, Flame Wall, Incinerate)
- **Ice**: Freezing damage, slow effects (Ice Spike, Blizzard, Freeze)
- **Lightning**: Shock damage, chain effects (Lightning Bolt, Chain Lightning, Thunderstorm)
- **Arcane**: Pure magic damage, anti-magic (Arcane Missiles, Mana Burn, Dispel)

### Restoration (Healing & Curing)
Healing and recovery magic:
- **Healing**: Restore health (Heal, Greater Heal, Full Heal)
- **Regeneration**: Health-over-time (Regeneration, Fast Healing)
- **Curing**: Remove negative effects (Cure Poison, Cure Disease, Remove Curse)
- **Resurrection**: Revive fallen allies (Revive, Resurrection) *(future party system)*

### Alteration (Buffs & Utility)
Magic that alters reality or enhances abilities:
- **Shields**: Magical protection (Mana Shield, Stoneskin, Reflect Damage)
- **Buffs**: Enhance stats (Strength, Speed, Intelligence)
- **Transmutation**: Change properties (Feather Fall, Water Walking, Detect Magic)
- **Utility**: Practical magic (Light, Unlock, Telekinesis)

### Conjuration (Summoning & Binding)
Magic that summons creatures or objects:
- **Summon Creatures**: Call allies (Summon Wolf, Summon Elemental, Summon Demon)
- **Conjure Weapons**: Create magical weapons (Bound Sword, Spectral Bow)
- **Binding**: Control summoned entities (Command, Banish, Soul Trap)
- **Planar Magic**: Deal with other realms (Gate, Planar Ally) *(high level)*

### Illusion (Mind Magic)
Magic affecting perception and minds:
- **Charm**: Make enemies friendly (Charm Person, Mass Charm)
- **Fear**: Frighten enemies (Fear, Terror, Panic)
- **Invisibility**: Hide from sight (Invisibility, Greater Invisibility)
- **Mind Control**: Control actions (Dominate, Suggestion) *(high level)*

### Mysticism (Detection & Teleportation)
Magic for information and movement:
- **Detection**: Reveal hidden (Detect Life, Detect Magic, True Seeing)
- **Teleportation**: Instant movement (Recall, Teleport, Dimension Door)
- **Clairvoyance**: See distant places (Scrying, Far Sight)
- **Time Magic**: Manipulate time (Haste, Slow, Time Stop) *(high level)*

## Spell Mechanics

### Learning Spells

**Acquisition Methods**:
- **Spellbooks**: Find in loot, buy from merchants, quest rewards
  - Consumable item that teaches spell permanently
  - Rarity matches spell power (common to legendary)
- **Scrolls**: Single-use spell casting
  - Cast spell without knowing it
  - No skill check required (spell always succeeds)
  - Can learn spell by studying scroll (chance based on skill)
- **Trainers**: NPCs teach spells for gold
  - Must meet skill requirements
  - Training costs scale with spell level
- **Quest Rewards**: Special spells from story/side quests
  - Unique spells not available elsewhere
  - Often tied to spell school mastery quests

**Skill Requirements**:
Each spell has minimum skill rank in its school:
- **Novice Spells**: Rank 0-20 (beginner magic)
- **Apprentice Spells**: Rank 20-40 (journeyman magic)
- **Adept Spells**: Rank 40-60 (experienced magic)
- **Expert Spells**: Rank 60-80 (master magic)
- **Master Spells**: Rank 80-100 (legendary magic)

### Casting Mechanics

**Mana Costs**:
Spells consume mana based on power and school:
- **Novice**: 5-15 mana
- **Apprentice**: 15-30 mana
- **Adept**: 30-50 mana
- **Expert**: 50-80 mana
- **Master**: 80-150 mana

**Skill affects cost**: Higher skill = reduced mana cost (up to 50% reduction at max rank)

**Casting Time**:
- **Instant**: Most spells cast immediately (1 action)
- **Channeled**: Some spells require multiple turns (future)
- **Ritual**: Powerful spells require out-of-combat casting (future)

**Success/Failure**:
- **Success Rate**: Based on caster's skill rank vs spell difficulty
  - Meeting requirement = 90% base success
  - 20 ranks above = 99% success (near-guaranteed)
  - 10 ranks below = 60% success (risky but possible)
- **Fizzle**: Failed cast consumes half mana, no effect
- **Backfire**: Critical failure damages caster (rare, <5% chance)

**Skill Scaling**:
Spell effectiveness increases with skill:
- **Damage Spells**: +1% damage per skill rank above requirement
- **Healing Spells**: +1% healing per skill rank above requirement
- **Duration Spells**: +2% duration per skill rank above requirement
- **Efficiency**: -0.5% mana cost per skill rank above requirement (max 50% reduction)

### Spell Levels

**Novice (Rank 0-20)**:
- **Fireball**: 30 fire damage, 5m AoE (15 mana)
- **Healing**: Restore 40 HP (12 mana)
- **Mana Shield**: Absorb 50 damage (20 mana)
- **Summon Wolf**: Summon ally for 5 turns (25 mana)
- **Invisibility**: Hide for 30 seconds (18 mana)

**Apprentice (Rank 20-40)**:
- **Flame Wall**: 50 fire damage AoE + DoT (30 mana)
- **Greater Heal**: Restore 100 HP (28 mana)
- **Stoneskin**: +50% defense for 5 turns (35 mana)
- **Summon Bear**: Stronger summon for 5 turns (45 mana)
- **Charm Person**: Control one enemy for 3 turns (40 mana)

**Adept (Rank 40-60)**:
- **Blizzard**: 100 ice damage AoE + slow (55 mana)
- **Full Heal**: Restore to max HP (50 mana)
- **Strength of Steel**: +50% damage for 5 turns (45 mana)
- **Summon Elemental**: Powerful summon for 8 turns (70 mana)
- **Mass Invisibility**: Hide party for 45 seconds (65 mana)

**Expert (Rank 60-80)**:
- **Chain Lightning**: 150 shock damage, chains 3 enemies (75 mana)
- **Resurrection**: Revive dead ally (90 mana) *(future)*
- **Reflect Damage**: Return 50% damage to attackers (80 mana)
- **Gate**: Summon powerful demon for 10 turns (100 mana)
- **Dominate**: Control enemy for 5 turns (85 mana)

**Master (Rank 80-100)**:
- **Meteor**: 300 fire damage massive AoE (120 mana)
- **Divine Intervention**: Full heal + buff entire party (150 mana)
- **Time Stop**: Freeze enemies for 3 free turns (140 mana)
- **Summon Dragon**: Ultimate summon for 15 turns (180 mana)
- **Mass Charm**: Control all enemies for 3 turns (160 mana)

## Spell Integration

### Combat Integration
- **Turn Action**: Casting a spell uses combat turn
- **Targeting**: Select single enemy, area, or ally
- **Spell Selection**: UI shows learned spells with mana costs
- **Cooldowns**: Some powerful spells have cooldowns (separate from mana)

### Skill Integration
- **Skill Checks**: Success rate based on magic skill rank
- **Damage/Healing Scaling**: Spell power increases with skill
- **Mana Efficiency**: Higher skill reduces costs
- **Fizzle Reduction**: Higher skill = fewer failures

### Exploration Integration
- **Utility Spells**: Some spells usable outside combat
  - Light (illuminate dark areas)
  - Detect Magic (find hidden items)
  - Unlock (open locked doors/chests)
  - Teleport (fast travel) *(future)*
- **Scroll Usage**: Cast spells without learning them
- **Strategic Preparation**: Pre-cast buffs before combat

### Progression Integration
- **Spell Collection**: Finding spells is progression goal
- **Skill Requirements**: Encourages magic skill investment
- **Build Enabling**: Spells allow non-mages to use magic
- **Hybrid Builds**: Warriors casting buffs, Rogues using invisibility

## Spellbook UI

### Spell Selection (Combat)
- **Spell List**: Show learned spells grouped by school
- **Mana Display**: Show cost and remaining mana
- **Cooldown Status**: Highlight ready vs on-cooldown spells
- **Quick Cast**: Number hotkeys for fast casting

### Spellbook (Inventory)
- **All Learned Spells**: View entire spell collection
- **School Organization**: Filter by Destruction, Restoration, etc.
- **Spell Details**: Full descriptions, costs, requirements
- **Spell Progress**: Show how skill rank affects spell power

### Learning Interface
- **Spellbook Item**: Use to learn spell permanently
- **Skill Check**: Confirm meeting requirements
- **Success Message**: Spell added to spellbook
- **Failure Message**: Not enough skill (show requirement)

## Design Goals

### Universal Magic Access
- **Not Class-Locked**: Anyone can learn spells
- **Skill-Gated**: Proficiency required for effectiveness
- **Investment Rewarded**: Dedicated mages outperform dabblers

### Knowledge Collection
- **Exploration Incentive**: Spells found in world
- **Merchant Economy**: Buy spellbooks from shops
- **Quest Rewards**: Unique spells from story
- **Replayability**: Different spells each playthrough

### Tactical Depth
- **School Diversity**: Different schools solve different problems
- **Mana Management**: Limited resource forces decisions
- **Success Risk**: Low-skill casting is risky but possible
- **Build Variety**: Spell selection defines playstyle

### Distinct Identity
- **Different from Abilities**: Abilities = always available, Spells = must learn
- **Different from Skills**: Skills = passive bonuses, Spells = active magic
- **Complements Both**: Works with skills (scaling) and abilities (tactical options)

### Simple Core, Expandable
- **Start Small**: 3-5 spells per school initially
- **Expand Schools**: Add more spells over time
- **Advanced Mechanics**: Spell combinations, metamagic (future)
- **Spell Crafting**: Create custom spells (future)

## Related Systems

- [Skills System](skills-system.md) - Magic skills determine spell effectiveness
- [Abilities System](abilities-system.md) - Distinct from class-granted powers
- [Combat System](combat-system.md) - Spells used in combat
- [Progression System](progression-system.md) - Spell learning is progression goal
- [Inventory System](inventory-system.md) - Spellbooks and scrolls as items
