# Attribute Templates v1.0

**Version:** 1.0  
**Date:** December 29, 2025  
**Status:** ACTIVE  
**Purpose:** Standard attribute templates for enemies and NPCs with level-based scaling

---

## Overview

All entities (enemies and NPCs) use 6 core D&D-style attributes on a 1-20+ scale:
- **STR** (Strength) - Physical power, melee damage
- **DEX** (Dexterity) - Agility, ranged attacks, armor class
- **CON** (Constitution) - Endurance, hit points
- **INT** (Intelligence) - Reasoning, arcane magic
- **WIS** (Wisdom) - Perception, divine magic, insight
- **CHA** (Charisma) - Social skills, charisma-based magic, leadership

---

## Baseline: Average Humanoid

**10 across the board** - The "standard human" baseline

```json
{
  "traits": {
    "strength": 10,
    "dexterity": 10,
    "constitution": 10,
    "intelligence": 10,
    "wisdom": 10,
    "charisma": 10
  }
}
```

**Interpretation:** A level 1-2 average humanoid (farmer, commoner, basic bandit)

---

## Archetype Templates

### Template 1: Brute (Heavy Melee)
**Role:** Front-line melee combatant, high damage, low mobility

**Base Stats (Level 1-5):**
- STR: **16** (+6 from baseline)
- DEX: **8** (-2)
- CON: **16** (+6)
- INT: **6** (-4)
- WIS: **8** (-2)
- CHA: **7** (-3)

**Examples:** Trolls, Ogres, Orcs (common)

**Level Scaling:**
- +2 STR per 5 levels (caps at 22)
- +2 CON per 5 levels (caps at 22)
- INT/WIS/CHA stay low

**Level 10 Brute:**
```json
{
  "strength": 18,
  "dexterity": 8,
  "constitution": 18,
  "intelligence": 6,
  "wisdom": 8,
  "charisma": 7
}
```

**Level 15+ Brute (Boss/Elite):**
```json
{
  "strength": 20,
  "dexterity": 8,
  "constitution": 20,
  "intelligence": 6,
  "wisdom": 8,
  "charisma": 7
}
```

---

### Template 2: Agile (Light Melee/Ranged)
**Role:** Fast, evasive, ranged or finesse attacks

**Base Stats (Level 1-5):**
- STR: **10** (average)
- DEX: **16** (+6)
- CON: **12** (+2)
- INT: **10** (average)
- WIS: **14** (+4)
- CHA: **10** (average)

**Examples:** Wolves, Spiders (agile), Assassins, Archers

**Level Scaling:**
- +2 DEX per 5 levels (caps at 22)
- +1 WIS per 5 levels (caps at 18)

**Level 10 Agile:**
```json
{
  "strength": 10,
  "dexterity": 18,
  "constitution": 12,
  "intelligence": 10,
  "wisdom": 15,
  "charisma": 10
}
```

---

### Template 3: Tank (Defensive)
**Role:** High HP, absorbs damage, defensive stance

**Base Stats (Level 1-5):**
- STR: **14** (+4)
- DEX: **8** (-2)
- CON: **18** (+8)
- INT: **8** (-2)
- WIS: **12** (+2)
- CHA: **10** (average)

**Examples:** Armored enemies, Giants, Elite Guards

**Level Scaling:**
- +2 CON per 5 levels (caps at 24)
- +1 STR per 5 levels

**Level 10 Tank:**
```json
{
  "strength": 15,
  "dexterity": 8,
  "constitution": 20,
  "intelligence": 8,
  "wisdom": 12,
  "charisma": 10
}
```

---

### Template 4: Spellcaster (Arcane - INT)
**Role:** Magic damage, crowd control, low physical stats

**Base Stats (Level 1-5):**
- STR: **8** (-2)
- DEX: **12** (+2)
- CON: **10** (average)
- INT: **16** (+6)
- WIS: **12** (+2)
- CHA: **10** (average)

**Examples:** Mages, Wizards, Cultists (arcane), Liches

**Level Scaling:**
- +2 INT per 5 levels (caps at 22)
- +1 WIS per 5 levels

**Level 10 Spellcaster (Arcane):**
```json
{
  "strength": 8,
  "dexterity": 12,
  "constitution": 10,
  "intelligence": 18,
  "wisdom": 13,
  "charisma": 10
}
```

**Level 18+ Lich (Boss):**
```json
{
  "strength": 8,
  "dexterity": 12,
  "constitution": 10,
  "intelligence": 20,
  "wisdom": 16,
  "charisma": 14
}
```

---

### Template 5: Cleric/Shaman (Divine - WIS)
**Role:** Healing, support, divine magic

**Base Stats (Level 1-5):**
- STR: **12** (+2)
- DEX: **10** (average)
- CON: **14** (+4)
- INT: **12** (+2)
- WIS: **16** (+6)
- CHA: **12** (+2)

**Examples:** Priests, Shamans, Druids, Yuan-ti (ritual casters)

**Level Scaling:**
- +2 WIS per 5 levels (caps at 22)
- +1 CHA per 5 levels

**Level 10 Cleric:**
```json
{
  "strength": 12,
  "dexterity": 10,
  "constitution": 14,
  "intelligence": 12,
  "wisdom": 18,
  "charisma": 13
}
```

---

### Template 6: Leader/Commander (CHA)
**Role:** Leadership, buffs, charisma-based powers

**Base Stats (Level 1-5):**
- STR: **12** (+2)
- DEX: **12** (+2)
- CON: **14** (+4)
- INT: **14** (+4)
- WIS: **12** (+2)
- CHA: **16** (+6)

**Examples:** Vampire Lords, Goblin Kings, Captains, Noble NPCs

**Level Scaling:**
- +2 CHA per 5 levels (caps at 22)
- +1 INT per 5 levels

**Level 10 Leader:**
```json
{
  "strength": 12,
  "dexterity": 12,
  "constitution": 14,
  "intelligence": 15,
  "wisdom": 12,
  "charisma": 18
}
```

---

### Template 7: Balanced (All-Rounder)
**Role:** Versatile, no major weaknesses

**Base Stats (Level 1-5):**
- STR: **12** (+2)
- DEX: **12** (+2)
- CON: **12** (+2)
- INT: **10** (average)
- WIS: **12** (+2)
- CHA: **10** (average)

**Examples:** Experienced soldiers, hobgoblins, versatile humanoids

**Level Scaling:**
- +1 to all stats per 5 levels (distribute 3 points)

**Level 10 Balanced:**
```json
{
  "strength": 13,
  "dexterity": 13,
  "constitution": 13,
  "intelligence": 11,
  "wisdom": 13,
  "charisma": 10
}
```

---

### Template 8: Beast (Animal Intelligence)
**Role:** Natural predator, low intelligence

**Base Stats (Level 1-5):**
- STR: **14** (+4)
- DEX: **14** (+4)
- CON: **14** (+4)
- INT: **3-5** (animal)
- WIS: **12** (+2)
- CHA: **6** (-4)

**Examples:** Wolves, Bears, Big Cats, Monster Insects

**Level Scaling:**
- +1 STR, DEX, CON per 5 levels
- INT stays 3-5 (animal)

**Level 8 Beast:**
```json
{
  "strength": 15,
  "dexterity": 15,
  "constitution": 15,
  "intelligence": 4,
  "wisdom": 12,
  "charisma": 6
}
```

---

### Template 9: Mindless (Undead/Construct)
**Role:** No will, follows commands

**Base Stats (Level 1-5):**
- STR: **14** (+4)
- DEX: **8** (-2)
- CON: **14** (+4)
- INT: **1-2** (mindless)
- WIS: **6** (-4)
- CHA: **5** (-5)

**Examples:** Zombies, Skeletons (basic), Golems

**Level Scaling:**
- +2 STR per 5 levels
- +2 CON per 5 levels
- Mental stats stay low

**Level 8 Mindless:**
```json
{
  "strength": 16,
  "dexterity": 8,
  "constitution": 16,
  "intelligence": 1,
  "wisdom": 6,
  "charisma": 5
}
```

---

### Template 10: Dragon (Legendary)
**Role:** Apex predator, high all-around stats

**Base Stats (Level 8-12 Young):**
- STR: **20** (+10)
- DEX: **12** (+2)
- CON: **18** (+8)
- INT: **16** (+6)
- WIS: **14** (+4)
- CHA: **16** (+6)

**Examples:** Dragons (all types)

**Level Scaling:**
- +2 STR per 5 levels (caps at 26)
- +2 INT per 5 levels (caps at 22)
- +2 CHA per 5 levels (caps at 22)

**Level 18+ Ancient Dragon:**
```json
{
  "strength": 24,
  "dexterity": 12,
  "constitution": 22,
  "intelligence": 20,
  "wisdom": 16,
  "charisma": 20
}
```

---

## Level-Based Scaling Rules

### General Guidelines

**Level 1-5 (Low Tier):**
- Primary stat: 14-16
- Secondary stats: 10-12
- Dump stats: 6-8
- Total attribute points: ~60-70

**Level 6-10 (Mid Tier):**
- Primary stat: 16-18
- Secondary stats: 12-14
- Dump stats: 6-10
- Total attribute points: ~70-80

**Level 11-15 (High Tier):**
- Primary stat: 18-20
- Secondary stats: 14-16
- Dump stats: 8-12
- Total attribute points: ~80-95

**Level 16+ (Epic Tier):**
- Primary stat: 20-24
- Secondary stats: 16-18
- Dump stats: 10-14
- Total attribute points: 95-110

### Attribute Caps

- **Normal Maximum**: 20 (legendary mortal)
- **Epic Maximum**: 22 (exceptional beings)
- **Divine Maximum**: 24+ (gods, ancient dragons, progenitors)

---

## Species Modifiers

Apply **after** archetype, **before** level scaling:

### Humanoid Species

| Species | Modifiers |
|---------|-----------|
| Human | None (baseline) |
| Goblinoid | -2 STR, +2 DEX |
| Orc | +2 STR, +2 CON, -2 INT, -2 CHA |
| Elf (if added) | -2 CON, +2 DEX, +2 INT |
| Dwarf (if added) | +2 CON, -2 CHA |

### Creature Types

| Type | Modifiers |
|------|-----------|
| Undead | -4 CHA (usually), INT varies |
| Dragon | +2 to all stats |
| Elemental | STR/CON based on element, INT/CHA low |
| Beast | -6 INT (animal), -4 CHA |
| Plant | -4 INT, -6 CHA, CON +2 |
| Insect | -6 INT (hive exceptions), DEX +2 |

---

## Application Examples

### Example 1: Common Troll (Level 8)

**Base Template:** Brute  
**Species:** Troll (no modifier, already factored)  
**Level Scaling:** Level 8 (mid-tier)

```json
{
  "traits": {
    "category": "troll",
    "size": "large",
    "strength": 18,
    "dexterity": 8,
    "constitution": 18,
    "intelligence": 5,
    "wisdom": 8,
    "charisma": 6,
    "behavior": "aggressive",
    "regeneration": true,
    "vulnerability": "fire"
  }
}
```

**Reasoning:**
- High STR/CON for brute template + level 8
- Low INT (5) - stupid creature
- Low DEX (8) - slow, clumsy
- Low WIS/CHA - poor judgment, ugly

---

### Example 2: Hobgoblin Soldier (Level 4)

**Base Template:** Balanced  
**Species:** Goblinoid (-2 STR, +2 DEX)  
**Level Scaling:** Level 4 (low-tier)

```json
{
  "traits": {
    "strength": 10,
    "dexterity": 14,
    "constitution": 12,
    "intelligence": 12,
    "wisdom": 12,
    "charisma": 10
  }
}
```

**Reasoning:**
- Balanced base (12s) - versatile soldier
- Goblinoid modifier (-2 STR, +2 DEX)
- INT 12 - military training, tactics

---

### Example 3: Ancient Dragon (Level 18)

**Base Template:** Dragon  
**Level Scaling:** Epic tier

```json
{
  "traits": {
    "strength": 24,
    "dexterity": 12,
    "constitution": 22,
    "intelligence": 20,
    "wisdom": 16,
    "charisma": 20
  }
}
```

**Reasoning:**
- Legendary physical stats (STR 24, CON 22)
- Genius-level INT (20) - ancient knowledge
- High CHA (20) - commanding presence
- DEX stays moderate (12) - large creature

---

### Example 4: Goblin Scout (Level 2)

**Base Template:** Agile  
**Species:** Goblinoid (-2 STR, +2 DEX)  
**Level Scaling:** Level 2 (low-tier)

```json
{
  "traits": {
    "strength": 8,
    "dexterity": 18,
    "constitution": 12,
    "intelligence": 8,
    "wisdom": 14,
    "charisma": 10
  }
}
```

**Reasoning:**
- Agile base (DEX 16) + goblinoid (+2 DEX) = 18
- Low STR (10 - 2 = 8) - small, weak
- Good WIS (14) - perceptive scout
- Low INT (8) - not very smart

---

### Example 5: Vampire Lord (Level 15)

**Base Template:** Leader  
**Species:** Undead (no STR/DEX penalty, high INT)  
**Level Scaling:** High tier

```json
{
  "traits": {
    "strength": 16,
    "dexterity": 14,
    "constitution": 16,
    "intelligence": 18,
    "wisdom": 14,
    "charisma": 20
  }
}
```

**Reasoning:**
- High CHA (20) - commanding, seductive
- High INT (18) - centuries of knowledge
- Good physical stats (undead supernatural strength)
- Balanced WIS (14) - ancient wisdom

---

### Example 6: Zombie (Level 3)

**Base Template:** Mindless  
**Species:** Undead  
**Level Scaling:** Low tier

```json
{
  "traits": {
    "strength": 14,
    "dexterity": 8,
    "constitution": 14,
    "intelligence": 1,
    "wisdom": 6,
    "charisma": 5
  }
}
```

**Reasoning:**
- Mindless INT (1) - no thought
- Low DEX (8) - shambling, slow
- Moderate STR/CON (14) - zombie resilience
- Terrible WIS/CHA - no awareness, revolting

---

## NPC-Specific Considerations

### Profession-Based Stats

NPCs should have attributes that match their profession:

**Blacksmith NPC:**
```json
{
  "strength": 14,      // Swinging hammers
  "dexterity": 12,     // Precision work
  "constitution": 14,  // Heat tolerance
  "intelligence": 12,  // Crafting knowledge
  "wisdom": 10,
  "charisma": 10
}
```

**Merchant NPC:**
```json
{
  "strength": 10,
  "dexterity": 10,
  "constitution": 10,
  "intelligence": 14,  // Business sense
  "wisdom": 12,        // Read people
  "charisma": 14       // Persuasion
}
```

**Scholar NPC:**
```json
{
  "strength": 8,
  "dexterity": 10,
  "constitution": 10,
  "intelligence": 16,  // Primary stat
  "wisdom": 14,        // Insight
  "charisma": 10
}
```

---

## Implementation Checklist

### Phase 1: Templates ✅
- [✅] Define 10 archetype templates
- [✅] Define level scaling rules
- [✅] Define species modifiers
- [✅] Document 6 examples

### Phase 2: Apply to Enemies (Next)
- [ ] Apply templates to all 48 enemy types (13 catalogs)
- [ ] Validate builds pass
- [ ] Update TRAIT_STANDARDS.md

### Phase 3: Apply to NPCs (Future)
- [ ] Apply profession-based templates to NPCs
- [ ] Validate NPC interactions

---

## Related Standards

- **[TRAIT_STANDARDS.md](TRAIT_STANDARDS.md)** - Core trait taxonomy and formats
- **[CATALOG_JSON_STANDARD.md](CATALOG_JSON_STANDARD.md)** - Catalog structure

---

## Changelog

### v1.0 (December 29, 2025)
- ✅ Created 10 archetype templates
- ✅ Defined level-based scaling rules (1-5, 6-10, 11-15, 16+)
- ✅ Defined species modifiers
- ✅ Documented 6 application examples
- ✅ Ready for implementation
