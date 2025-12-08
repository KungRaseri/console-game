# Trait System Implementation

## Overview

The trait system provides a **flexible, generic, and reusable** way to add gameplay-meaningful properties to entities (items, enemies, NPCs) without hardcoding properties for each type. This replaces the purely cosmetic JSON data with data that impacts gameplay mechanics.

## Architecture

### Core Components

1. **`TraitValue`** - Type-safe wrapper for trait values
   - Supports: `int`, `double`, `bool`, `List<string>`
   - Auto-conversion methods: `AsInt()`, `AsDouble()`, `AsBool()`, `AsStringList()`

2. **`ITraitable`** - Interface for any entity that can have traits
   - Single property: `Dictionary<string, TraitValue> Traits { get; }`

3. **`StandardTraits`** - Constants for 80+ predefined trait names
   - Ensures consistency across all entities
   - Categories: Stats, Combat, Defense, Elemental, Utility

4. **`TraitApplicator`** - Static utility class for trait operations
   - Methods: `GetTrait<T>()`, `HasTrait()`, `ApplyTraits()`, `GetResistance()`, etc.

### File Structure

```
Game/
├── Models/
│   ├── TraitSystem.cs              # Core trait classes
│   └── Item.cs                     # Now implements ITraitable
├── Services/
│   └── TraitApplicator.cs          # Trait utility methods
└── Data/
    ├── Models/
    │   └── ItemTraitDataModels.cs  # Enhanced JSON models
    └── Json/
        └── items/
            ├── weapon_prefixes_enhanced.json
            ├── armor_materials_enhanced.json
            └── enchantment_suffixes_enhanced.json
```

## JSON Data Format

### Structure

Each enhanced JSON file follows this pattern:

```json
{
  "category": {
    "Name": {
      "displayName": "Display Name",
      "traits": {
        "traitName": { "value": 10, "type": "number" },
        "anotherTrait": { "value": true, "type": "boolean" }
      }
    }
  }
}
```

### Example: Weapon Prefix

```json
{
  "legendary": {
    "Godslayer": {
      "displayName": "Godslayer",
      "traits": {
        "damageBonus": { "value": 20, "type": "number" },
        "criticalChance": { "value": 15, "type": "number" },
        "resistFire": { "value": 50, "type": "number" },
        "resistIce": { "value": 50, "type": "number" },
        "resistLightning": { "value": 50, "type": "number" },
        "glowEffect": { "value": "divine", "type": "string" }
      }
    }
  }
}
```

## Enhanced JSON Files

### 1. weapon_prefixes_enhanced.json

**15 prefixes across 5 rarities** with traits for weapon modifications:

| Rarity | Prefixes | Key Traits |
|--------|----------|------------|
| Common | Rusty, Worn, Chipped | damageMultiplier (0.8-0.9), durabilityMax (50-70) |
| Uncommon | Sharp, Balanced, Reinforced | damageBonus (2-5), criticalChance (3-5), durabilityMax (120-150) |
| Rare | Masterwork, Enchanted, Blessed | damageBonus (8-10), criticalChance (8-10), resistances (15-25), glowEffect |
| Epic | Legendary, Ancient, Runic | damageBonus (12-15), criticalChance (12-15), resistances (30-40), onHitEffects |
| Legendary | Godslayer, Worldbreaker, Starforged | damageBonus (15-20), criticalChance (15), resistances (50), divine/cosmic effects |

**Trait Types Used:**
- `damageBonus`, `damageMultiplier` - Direct damage modifiers
- `criticalChance`, `criticalDamage` - Critical hit mechanics
- `durabilityMax` - Item durability
- `resistFire/Ice/Lightning` - Elemental resistances
- `glowEffect`, `onHitEffect` - Visual/special effects
- `lifeDrain`, `manaDrain` - Life/mana steal

### 2. armor_materials_enhanced.json

**15 materials across 5 rarities** with traits for armor pieces:

| Rarity | Materials | Key Traits |
|--------|-----------|------------|
| Common | Cloth, Leather, Hide | armorRating (1-3), weight (1-5), movementSpeed (-5 to -10) |
| Uncommon | Chainmail, Bronze, Iron | armorRating (5-10), weight (10-20), resistances (5-10) |
| Rare | Steel, Mithril, Dragonscale | armorRating (12-20), resistances (15-25), stat bonuses (+3 to +5) |
| Epic | Adamantine, Celestial, Demonic | armorRating (22-25), resistances (30-40), major stat bonuses (+8 to +10) |
| Legendary | Starforged, Divine, Voidsteel | armorRating (28-30), resistances (50), legendary bonuses (healthBonus: 100, dodge: 15) |

**Trait Types Used:**
- `armorRating` - Base armor value
- `weight` - Affects movement/encumbrance
- `movementSpeed` - Movement modifier
- `resistFire/Ice/Lightning/Poison/Magic` - Damage resistances
- `strengthBonus`, `dexterityBonus`, `intelligenceBonus` - Stat modifiers
- `healthBonus`, `manaBonus` - Resource bonuses
- `dodgeChance` - Evasion mechanics

### 3. enchantment_suffixes_enhanced.json

**10 categories with 51 total suffixes** for magical enhancements:

| Category | Suffixes | Key Traits |
|----------|----------|------------|
| **power** | of Power, of Might, of Strength, of the Titan, of the Bear, of the Warrior | strengthBonus (3-10), damageBonus (2-8), healthBonus (20-30), knockbackChance |
| **protection** | of Protection, of Warding, of Defense, of the Guardian, of the Shield, of Fortitude | armorBonus (3-10), healthBonus (10-50), blockChance (10-20), damageReflection |
| **wisdom** | of Wisdom, of Insight, of Knowledge, of the Sage, of the Oracle, of Enlightenment | intelligenceBonus (3-12), manaBonus (15-60), spellPower (5-10), manaRegeneration |
| **agility** | of Agility, of Speed, of Swiftness, of the Cat, of the Wind, of the Falcon | dexterityBonus (3-10), dodgeChance (5-20), movementSpeed (10-25), criticalChance |
| **magic** | of Magic, of Sorcery, of the Arcane, of the Magi, of Spellcraft, of the Enchanter | spellPower (5-15), manaBonus (20-60), spellCriticalChance, cooldownReduction |
| **fire** | of Flames, of the Inferno, of Burning, of the Phoenix, of Dragonfire | fireDamage (5-20), resistFire (10-40), burnChance (20-30), areaDamage |
| **ice** | of Frost, of Winter, of the Frozen, of Glaciers, of Eternal Ice | iceDamage (5-18), resistIce (10-40), freezeChance (15-25), slowChance |
| **lightning** | of Lightning, of Thunder, of the Storm, of Storms, of the Tempest | lightningDamage (6-18), resistLightning (10-35), stunChance (15-25), chainLightning |
| **life** | of Life, of Vitality, of Regeneration, of the Phoenix, of Renewal | healthBonus (20-50), healthRegeneration (2-10), resurrectChance |
| **death** | of Death, of Shadows, of the Void, of Souls, of the Reaper | necroticDamage (8-20), lifeSteal (5-15), executeChance, soulTrap |

**Trait Types Used:**
- **Stat Bonuses**: strength/dexterity/intelligence/constitutionBonus
- **Resource Bonuses**: health/manaBonus, health/manaRegeneration
- **Elemental Damage**: fire/ice/lightning/necroticDamage
- **Elemental Resistance**: resistFire/Ice/Lightning/Magic/Poison
- **Combat Effects**: criticalChance/Damage, dodgeChance, blockChance, attackSpeed
- **Magic Effects**: spellPower, spellCriticalChance, cooldownReduction
- **Special Effects**: lifeSteal, resurrectChance, stealthBonus, visionRange

## StandardTraits Constants

The `StandardTraits` class defines 80+ trait name constants organized by category:

### Stats & Resources
```csharp
public const string StrengthBonus = "strengthBonus";
public const string DexterityBonus = "dexterityBonus";
public const string IntelligenceBonus = "intelligenceBonus";
public const string HealthBonus = "healthBonus";
public const string ManaBonus = "manaBonus";
```

### Combat
```csharp
public const string DamageBonus = "damageBonus";
public const string CriticalChance = "criticalChance";
public const string AttackSpeed = "attackSpeed";
public const string LifeSteal = "lifeSteal";
```

### Defense
```csharp
public const string ArmorBonus = "armorBonus";
public const string DodgeChance = "dodgeChance";
public const string BlockChance = "blockChance";
public const string DamageReflection = "damageReflection";
```

### Elemental
```csharp
public const string FireDamage = "fireDamage";
public const string ResistFire = "resistFire";
public const string BurnChance = "burnChance";
// ... ice, lightning, necrotic
```

### Utility
```csharp
public const string MovementSpeed = "movementSpeed";
public const string ExperienceBonus = "experienceBonus";
public const string GoldFind = "goldFind";
public const string VisionRange = "visionRange";
```

## Usage Examples

### 1. Creating Trait-Enabled Items

```csharp
var item = new Item
{
    Name = "Rusty Iron Sword",
    Type = ItemType.Weapon,
    Rarity = ItemRarity.Common
};

// Apply traits from JSON data
item.Traits[StandardTraits.DamageBonus] = new TraitValue(5);
item.Traits[StandardTraits.DurabilityMax] = new TraitValue(50);
item.Traits[StandardTraits.DamageMultiplier] = new TraitValue(0.8);
```

### 2. Querying Traits

```csharp
// Using TraitApplicator
int damageBonus = TraitApplicator.GetTrait<int>(item, StandardTraits.DamageBonus, 0);
double damageMultiplier = TraitApplicator.GetTrait<double>(item, StandardTraits.DamageMultiplier, 1.0);

// Check if trait exists
if (TraitApplicator.HasTrait(item, StandardTraits.CriticalChance))
{
    int critChance = TraitApplicator.GetTrait<int>(item, StandardTraits.CriticalChance);
    // Apply critical chance logic
}
```

### 3. Combining Traits from Multiple Sources

```csharp
// Weapon: "Sharp Steel Sword of Flames"
// Prefix: Sharp (+5 damage, +5 crit chance)
// Material: Steel (+armorRating for guard)
// Enchantment: of Flames (+8 fire damage, +10 fire resist)

var totalFireDamage = TraitApplicator.GetTrait<int>(sword, StandardTraits.FireDamage, 0); // 8
var totalCritChance = TraitApplicator.GetTrait<int>(sword, StandardTraits.CriticalChance, 0); // 5
var totalDamage = TraitApplicator.GetTrait<int>(sword, StandardTraits.DamageBonus, 0); // 5
```

### 4. Resistance Calculation

```csharp
// Calculate total elemental resistance
double fireResist = TraitApplicator.GetResistance(armor, StandardTraits.ResistFire);
double iceResist = TraitApplicator.GetResistance(armor, StandardTraits.ResistIce);

// Apply resistance to damage
int damageAfterResist = (int)(fireDamage * (1.0 - fireResist / 100.0));
```

### 5. Merging Traits

```csharp
// Combine traits from multiple items (e.g., full equipment set)
var combinedTraits = new Dictionary<string, TraitValue>();

foreach (var equippedItem in equipment)
{
    TraitApplicator.MergeTraits(combinedTraits, equippedItem.Traits);
}

// Now combinedTraits contains all bonuses from all equipment
int totalStrength = TraitApplicator.GetTrait<int>(
    new { Traits = combinedTraits }, 
    StandardTraits.StrengthBonus, 
    0
);
```

## Next Steps

### Immediate (Pending Implementation)

1. **Update GameDataService**
   - Add loading methods for enhanced JSON files
   - Add properties: `WeaponPrefixDataEnhanced`, `ArmorMaterialDataEnhanced`, `EnchantmentSuffixDataEnhanced`
   - Load enhanced files in constructor

2. **Update ItemGenerator**
   - Look up prefix/material/enchantment from enhanced data
   - Apply traits from JSON to generated items
   - Use `TraitApplicator.ApplyTraits()` for merging

3. **Create Item Trait Helpers**
   - Extension methods for common calculations
   - `GetTotalDamage()`, `GetEffectiveArmor()`, `GetTotalResistance()`
   - Display helpers for showing trait bonuses in UI

4. **Test Item Generation**
   - Generate items with traits
   - Verify traits are applied correctly
   - Test trait queries work as expected
   - Create demo showing trait system in action

### Future Enhancements

5. **Extend to Enemies**
   - Create enhanced enemy JSON with traits
   - Update Enemy model to implement ITraitable
   - Enemy traits: healthBonus, damageBonus, resistances, abilities

6. **Extend to NPCs**
   - Create enhanced NPC JSON with traits
   - Update NPC model to implement ITraitable
   - NPC traits: personality, skills, shop bonuses, quest rewards

7. **Equipment Sets**
   - Set bonuses when wearing matching items
   - Trait synergies (e.g., fire damage + fire resist = bonus)

8. **Trait Modifications**
   - Temporary buffs/debuffs using traits
   - Skill effects that modify traits
   - Enchanting system to add/modify traits

## Benefits

### ✅ Generic & Reusable
- Same trait system works for items, enemies, NPCs
- No hardcoded properties per entity type
- Easy to add new traits without code changes

### ✅ Data-Driven
- All trait values in JSON files
- Designers can balance without code changes
- Easy to create new content variations

### ✅ Gameplay-Meaningful
- Traits impact combat, defense, stats
- Items have real differences beyond cosmetic names
- Player choices matter based on trait combinations

### ✅ Extensible
- Add new traits by adding constants to StandardTraits
- Add new categories without changing architecture
- Support for complex traits (lists, conditional effects)

### ✅ Type-Safe
- TraitValue enforces type safety
- Compile-time constants prevent typos
- Auto-conversion methods with sensible defaults

## Trait Naming Conventions

- **Bonuses**: Use `*Bonus` suffix (e.g., `damageBonus`, `armorBonus`)
- **Multipliers**: Use `*Multiplier` suffix (e.g., `damageMultiplier`)
- **Resistances**: Use `resist*` prefix (e.g., `resistFire`, `resistMagic`)
- **Chances**: Use `*Chance` suffix (e.g., `criticalChance`, `dodgeChance`)
- **Effects**: Use descriptive names (e.g., `glowEffect`, `onHitEffect`)
- **Damage Types**: Use `*Damage` suffix (e.g., `fireDamage`, `necroticDamage`)
- **Regeneration**: Use `*Regeneration` suffix (e.g., `healthRegeneration`)

## Performance Considerations

- Traits are stored in `Dictionary<string, TraitValue>` - O(1) lookup
- Use `StandardTraits` constants to avoid string allocations
- TraitValue is a struct - minimal memory overhead
- TraitApplicator methods are static - no instantiation overhead

## Testing Strategy

1. **Unit Tests for TraitValue**
   - Test type conversions (AsInt, AsDouble, etc.)
   - Test default values
   - Test type mismatches

2. **Unit Tests for TraitApplicator**
   - Test GetTrait with different types
   - Test MergeTraits with overlapping traits
   - Test resistance calculations

3. **Integration Tests for Items**
   - Generate items with traits from JSON
   - Verify trait application from multiple sources
   - Test trait queries match expected values

4. **End-to-End Tests**
   - Full item generation pipeline
   - Combat with trait-enabled items
   - Equipment set bonuses

---

**Status**: ✅ Core trait system implemented, enhanced JSON files created  
**Next**: Update GameDataService and ItemGenerator to use enhanced data  
**Author**: GitHub Copilot  
**Date**: 2024-12-07
