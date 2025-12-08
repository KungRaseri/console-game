# JSON Data System for Procedural Generation

This document demonstrates the new JSON-based data system for game content generation.

## 📁 File Structure

```
Game/Data/Json/
├── items/
│   ├── weapon_prefixes.json      # Quality prefixes (Rusty, Mythril, Dragon, etc.)
│   ├── weapon_names.json          # Weapon types (Swords, Axes, Bows, etc.)
│   ├── armor_materials.json       # Armor materials by rarity
│   └── enchantment_suffixes.json  # Magical suffixes (of Power, of Flames, etc.)
├── enemies/
│   ├── beast_names.json           # Beast prefixes, creatures, variants
│   ├── undead_names.json          # Undead types and variants
│   ├── demon_names.json           # Demon types and variants
│   ├── elemental_names.json       # Elemental types and variants
│   ├── dragon_names.json          # Dragon colors, types, variants
│   └── humanoid_names.json        # Humanoid enemy types
├── npcs/
│   ├── fantasy_names.json         # Fantasy first/last names
│   ├── occupations.json           # 10 categories of occupations
│   └── dialogue_templates.json    # Context-specific dialogue
└── general/
    ├── adjectives.json            # Descriptive adjectives
    └── materials.json             # Materials (metals, gems, natural)
```

## 🎮 Usage Examples

### Items

**Before (hardcoded):**
- "Iron Sword"
- "Steel Axe"
- "Mythril Bow"

**After (JSON-powered):**
- "Enchanted Katana"
- "Dragon Battleaxe"
- "Celestial Composite Bow"
- "Legendary Rapier"
- "Mythril Greatsword"
- "Ancient Claymore"

### Enemies

**Before:**
- "Dire Wolf"
- "Wild Bear"
- "Feral Spider"

**After:**
- "Alpha Moon Howler" (Beast variant)
- "Primal Cave Bear" (Beast variant)
- "Vicious Arachnid Horror" (Spider variant)
- "Ancient Frost Dragon"
- "Shadow Wraith"
- "Infernal Pit Fiend"
- "Chaos Demon Knight"

### NPCs

**Before (Bogus generic):**
- "John Smith, Accountant"
- "Jane Doe, Developer"

**After (Fantasy themed):**
- "Aldric Ironforge, Blacksmith"
- "Seraphina Stormwind, Enchanter"
- "Theron Ravencrest, Knight"
- "Lyra Moonbrook, Alchemist"

## 💡 Benefits

### 1. **Easy Content Expansion**
Add new items/enemies/names without touching code:
```json
// Just edit weapon_names.json
"swords": [
  "Longsword",
  "Katana",
  "Zweihander"  // ← Add new weapon
]
```

### 2. **Moddability**
Players/designers can customize content by editing JSON files.

### 3. **Better Variety**
- **Weapon Prefixes**: 38 total across 5 rarity tiers
- **Weapon Types**: 50+ weapon variations
- **Enemy Names**: 100+ combinations per enemy type
- **NPC Names**: 20 male + 20 female first names, 20 surnames = 800 combinations
- **Occupations**: 100+ unique jobs across 10 categories

### 4. **Localization Ready**
Translate JSON files to other languages without code changes.

### 5. **Hot Reload Support**
```csharp
// Reload data without restarting
GameDataService.Instance.Reload();
```

## 🛠️ Technical Implementation

### GameDataService
Singleton service that loads and caches all JSON data:
```csharp
var data = GameDataService.Instance;

// Access weapon data
var prefix = GameDataService.GetRandom(data.WeaponPrefixes.Legendary);
// "Godslayer"

// Access enemy data
var beastName = GameDataService.GetRandom(data.BeastNames.Creatures);
// "Wolverine"
```

### Generator Integration

**ItemGenerator** now uses:
- Weapon prefixes + weapon names
- Armor materials by rarity
- Enchantment suffixes for jewelry
- Material lists for shields

**EnemyGenerator** now uses:
- Type-specific name data (beast, undead, demon, etc.)
- Variant names for uniqueness
- Dragon-specific naming (color + type)
- Humanoid profession + role combinations

**NpcGenerator** now uses:
- Fantasy first/last names
- Categorized occupations (merchants, craftsmen, etc.)
- Context-aware dialogue templates

## 📊 Data Statistics

### Items
- **Weapon Prefixes**: 38 (8 common → 7 legendary)
- **Weapon Names**: 54 across 7 categories
- **Armor Materials**: 30 across 5 rarity tiers
- **Enchantment Suffixes**: 60 across 10 magic types
- **Total Weapon Combinations**: 38 × 54 = **2,052 unique weapons**

### Enemies
- **Beast Names**: 15 prefixes × 15 creatures + 24 variants = **249 combinations**
- **Undead Names**: 14 prefixes × 14 creatures + 16 variants = **212 combinations**
- **Demon Names**: 14 prefixes × 14 creatures + 16 variants = **212 combinations**
- **Dragon Names**: 10 prefixes × 13 colors × 6 types + 20 variants = **800 combinations**
- **Total Enemy Combinations**: **1,500+**

### NPCs
- **First Names**: 40 (20 male + 20 female)
- **Surnames**: 20
- **Name Combinations**: 40 × 20 = **800 unique names**
- **Occupations**: **100+** across 10 categories
- **Dialogue Lines**: **50+** context-specific templates

## 🎯 Future Enhancements

### Potential Additions
1. **quest_templates.json** - Quest name/description templates
2. **location_names.json** - Town, dungeon, region names
3. **lore_fragments.json** - Story snippets for flavor text
4. **skill_names.json** - Ability/spell naming
5. **item_descriptions.json** - Rich item lore
6. **weather_descriptions.json** - Atmospheric text

### Advanced Features
- **Weighted random selection** (rare items less frequent)
- **Conditional generation** (class-specific items)
- **Name generation rules** (elvish = vowel-heavy, etc.)
- **Procedural combination rules** (fire + sword = flaming sword)

## 🔧 Maintenance

### Adding New Weapons
1. Edit `weapon_names.json`
2. Add to appropriate category (swords, axes, etc.)
3. Rebuild project (JSON auto-copied to output)
4. Test with `ItemGenerator.GenerateByType(ItemType.Weapon, 10)`

### Adding New Enemy Types
1. Create new JSON file in `enemies/` folder
2. Add data model class to `GameDataModels.cs`
3. Update `GameDataService.cs` to load the file
4. Update `EnemyGenerator.cs` to use the data

### Testing Changes
```bash
# Build and run tests
dotnet build
dotnet test

# Run game to see new content
dotnet run --project Game
```

## ✅ Conclusion

The JSON data system provides:
- ✨ **2,000+ item variations** (vs ~20 before)
- 🐉 **1,500+ enemy variations** (vs ~30 before)  
- 👥 **80,000+ NPC combinations** (name × occupation)
- 🎨 **Easy content expansion** without code changes
- 🌍 **Localization support** for multiple languages
- 🔄 **Hot reload** for rapid iteration

This system transforms your console game from static to **truly procedural**, with virtually unlimited content variety!
