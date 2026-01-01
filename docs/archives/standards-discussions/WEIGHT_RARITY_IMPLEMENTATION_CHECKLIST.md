# Weight-Based Rarity System - Implementation Checklist

**Date:** December 16, 2025  
**Status:** 📋 Ready to Execute

## Quick Reference

**What Changed:**
- ✅ Items and components now have `weight` properties
- ✅ Prefixes/suffixes flattened (no more rarity tiers)
- ✅ Rarity calculated from total weight, not predetermined
- ✅ New `rarity_config.json` for thresholds and multipliers

**Key Files:**
- 📄 `docs/standards/PATTERN_COMPONENT_STANDARDS.md` - Updated with weight system
- 📄 `docs/standards/WEIGHT_BASED_raritySystem.md` - Complete implementation guide
- 📄 `docs/standards/DRAFT_FINALIZATION_ANALYSIS.md` - Decision rationale

---

## Implementation Checklist

### ✅ Phase 1: Documentation (COMPLETE)

- [x] Update PATTERN_COMPONENT_STANDARDS.md with weight system
- [x] Create WEIGHT_BASED_raritySystem.md comprehensive guide
- [x] Document rarity thresholds and multipliers
- [x] Define weight ranges by component type
- [x] Create example calculations

### 📋 Phase 2: Create Configuration File

- [ ] Create `RealmEngine.Shared/Data/Json/general/rarity_config.json`
- [ ] Add rarity thresholds (common 0-20, uncommon 21-50, etc.)
- [ ] Add weight multipliers (material 1.0, quality 1.2, etc.)
- [ ] Add display properties (colors, glow effects, drop rates)
- [ ] Add metadata section

**File to Create:**
```
RealmEngine.Shared/Data/Json/general/rarity_config.json
```

**Template:** See WEIGHT_BASED_raritySystem.md section "New Configuration File"

---

### 📋 Phase 3: Update Sample Files (Proof of Concept)

#### 3.1: Update items/weapons/types.json

- [ ] Add `rarityWeight` field to each weapon item
  - Example: `{ "name": "Longsword", "damage": "1d8", "rarityWeight": 10 }`
- [ ] Assign appropriate weights based on weapon tier
- [ ] Keep existing stats intact (damage, physical weight, value, etc.)
- [ ] Test `base` token resolution

#### 3.2: Update items/weapons/names.json

- [ ] Convert component arrays to objects with weights
  - `material`, `quality`, `enchantment`, etc.
  - Example: `["Iron"]` → `[{ "name": "Iron", "weight": 5 }]`
- [ ] **REMOVE items array** (items belong in types.json)
- [ ] Assign appropriate weights based on guidelines
- [ ] Test pattern execution still works

#### 3.3: Update items/weapons/prefixes.json

- [ ] Remove rarity tier organization (common/rare/epic sections)
- [ ] Flatten to single `"prefixes"` object
- [ ] Add `weight` field to each prefix
- [ ] Verify trait structures intact
- [ ] Test trait application still works

#### 3.3: Update items/weapons/suffixes.json

- [ ] Same process as prefixes.json
- [ ] Flatten to single `"suffixes"` object
- [ ] Add weights to each suffix
- [ ] Test suffix application

---

### 📋 Phase 4: Update Code (Runtime Support)

#### 4.1: RealmEngine.Shared - Data Models

**Create new classes:**

```csharp
// RealmEngine.Shared/Models/WeightedComponent.cs
public class WeightedComponent
{
    public string Name { get; set; }
    public int Weight { get; set; }
}

// RealmEngine.Shared/Models/WeightedItem.cs
public class WeightedItem
{
    public string Name { get; set; }
    public int Weight { get; set; }
    // Additional properties from types.json if needed
}

// RealmEngine.Shared/Models/RarityConfig.cs
public class RarityConfig
{
    public Dictionary<string, RarityTier> Thresholds { get; set; }
    public Dictionary<string, double> WeightMultipliers { get; set; }
}

public class RarityTier
{
    public int Min { get; set; }
    public int Max { get; set; }
    public string Color { get; set; }
    public string DisplayName { get; set; }
    public double DropRate { get; set; }
    public bool GlowEffect { get; set; }
}
```

**Tasks:**
- [ ] Create WeightedComponent class
- [ ] Create WeightedItem class
- [ ] Create RarityConfig classes
- [ ] Add JSON deserialization support

#### 4.2: RealmEngine.Shared - Rarity Calculator

**Create new service:**

```csharp
// RealmEngine.Shared/Services/RarityCalculator.cs
public class RarityCalculator
{
    private RarityConfig _config;
    
    public RarityCalculator(RarityConfig config)
    {
        _config = config;
    }
    
    public (string rarity, int totalWeight) CalculateRarity(
        Dictionary<string, WeightedComponent> components)
    {
        // Calculate total weight with multipliers
        // Map to rarity tier
        // Return both rarity name and total weight
    }
    
    public string GetRarityFromWeight(int weight)
    {
        // Map weight to rarity tier
    }
}
```

**Tasks:**
- [ ] Create RarityCalculator service
- [ ] Implement weight calculation with multipliers
- [ ] Implement rarity tier mapping
- [ ] Add unit tests for calculation logic

#### 4.3: RealmEngine.Shared - Pattern Executor Update

**Update existing PatternExecutor:**

```csharp
// Update executePattern() method to:
// 1. Track total weight during execution
// 2. Apply weight multipliers per component type
// 3. Calculate final rarity
// 4. Return rarity + weight with generated name
```

**Tasks:**
- [ ] Update PatternExecutor.ExecutePattern() method
- [ ] Add weight tracking during pattern execution
- [ ] Apply multipliers from config
- [ ] Return rarity information with result
- [ ] Update unit tests

#### 4.4: RealmEngine.Shared - Load Configuration

**Update data loading:**

```csharp
// Load rarity_config.json at startup
// Make available to all services
// Add to dependency injection if used
```

**Tasks:**
- [ ] Add RarityConfig loader
- [ ] Register in DI container (if applicable)
- [ ] Load at application startup

---

### 📋 Phase 5: Update ContentBuilder UI

#### 5.1: Add Weight Input Fields

**Update component editors:**
- [ ] Add "Weight" numeric input to component editors
- [ ] Add weight input to item editors
- [ ] Add validation (warn if weight outside expected range)
- [ ] Add tooltips explaining weight ranges

#### 5.2: Live Rarity Preview

**Add preview panel:**
- [ ] Show calculated total weight in real-time
- [ ] Display resulting rarity tier with color
- [ ] Update as user changes components
- [ ] Show weight breakdown by component type

Example UI:
```
┌─ Rarity Preview ─────────────────────┐
│ Total Weight: 135                    │
│ Rarity: Epic (Purple)                │
│                                      │
│ Breakdown:                           │
│   Quality:  40 × 1.2 = 48           │
│   Material: 50 × 1.0 = 50           │
│   Base:     10 × 0.5 = 5            │
│   Suffix:   40 × 0.8 = 32           │
└──────────────────────────────────────┘
```

**Tasks:**
- [ ] Create RarityPreviewControl
- [ ] Wire up to component selection
- [ ] Add real-time calculation
- [ ] Style with rarity colors

#### 5.3: Rarity Simulator Tool

**Create new tool window:**
- [ ] Allow users to test component combinations
- [ ] Show resulting weight and rarity
- [ ] Generate sample items at target weight
- [ ] Export test results

---

### 📋 Phase 6: Migrate All Files

**Files to Update: ~95 total**

#### General Files (9 files)
- [ ] Review if weights needed (component libraries)
- [ ] May not need weights if used for reference only

#### Items Category (~14 files)
- [ ] items/weapons/names.json
- [ ] items/weapons/prefixes.json
- [ ] items/weapons/suffixes.json
- [ ] items/armor/names.json
- [ ] items/armor/prefixes.json
- [ ] items/armor/suffixes.json
- [ ] items/armor/materials.json
- [ ] items/consumables/names.json
- [ ] items/consumables/prefixes.json
- [ ] items/consumables/suffixes.json
- [ ] items/enchantments/*.json files

#### Enemies Category (~52 files)
- [ ] enemies/beasts/*.json (4 files)
- [ ] enemies/demons/*.json (4 files)
- [ ] enemies/dragons/*.json (4 files)
- [ ] enemies/elementals/*.json (4 files)
- [ ] enemies/goblinoids/*.json (4 files)
- [ ] enemies/humanoids/*.json (4 files)
- [ ] enemies/insects/*.json (4 files)
- [ ] enemies/orcs/*.json (4 files)
- [ ] enemies/plants/*.json (4 files)
- [ ] enemies/reptilians/*.json (4 files)
- [ ] enemies/trolls/*.json (4 files)
- [ ] enemies/undead/*.json (4 files)
- [ ] enemies/vampires/*.json (4 files)

#### NPCs Category (~20 files estimated)
- [ ] npcs/*/*.json files

**Migration Strategy:**
1. Start with weapons (proof of concept)
2. Migrate armor (verify consistency)
3. Batch migrate enemies (similar structure)
4. Migrate NPCs
5. Final review and testing

---

### 📋 Phase 7: Update Game.Console (Loot System)

#### 7.1: Loot Generation

**Create/Update loot generator:**

```csharp
public class LootGenerator
{
    public Item GenerateLoot(string lootTableName)
    {
        // Load loot table config
        // Get target weight range
        // Build item to match target weight
        // Return generated item with rarity
    }
}
```

**Tasks:**
- [ ] Create LootGenerator service
- [ ] Implement weight-targeted item building
- [ ] Create loot table configurations
- [ ] Test with various encounter types

#### 7.2: Item Display

**Update item tooltips/display:**
- [ ] Show rarity with color
- [ ] Add glow effect for rare+ items
- [ ] Display weight in tooltip (for debugging)
- [ ] Show rarity tier name

---

### 📋 Phase 8: Testing & Validation

#### 8.1: Unit Tests

- [ ] Test weight calculation algorithm
- [ ] Test rarity tier mapping
- [ ] Test component combination edge cases
- [ ] Test multiplier application
- [ ] Test threshold boundary conditions

#### 8.2: Integration Tests

- [ ] Test pattern execution with weights
- [ ] Test loot generation at various weight targets
- [ ] Test ContentBuilder weight input/preview
- [ ] Test full item generation pipeline

#### 8.3: Balance Testing

- [ ] Generate 1000 random items
- [ ] Verify rarity distribution matches drop rates
- [ ] Test loot tables for each encounter type
- [ ] Adjust weights if balance feels off
- [ ] Player testing for progression feel

---

### 📋 Phase 9: Documentation Updates

- [ ] Update GDD with weight-based rarity system
- [ ] Update ContentBuilder user guide
- [ ] Add weight assignment guidelines for content creators
- [ ] Create troubleshooting guide for balance issues
- [ ] Document testing results and final weight ranges

---

## Priority Order

### High Priority (Do First)
1. ✅ Documentation updates (DONE)
2. 📋 Create rarity_config.json
3. 📋 Update weapons/names.json (proof of concept)
4. 📋 Update weapons/prefixes.json
5. 📋 Create RarityCalculator service
6. 📋 Update PatternExecutor

### Medium Priority (Do Next)
7. 📋 ContentBuilder UI updates
8. 📋 Migrate remaining item files
9. 📋 Update loot generation
10. 📋 Testing and validation

### Lower Priority (Polish)
11. 📋 Migrate enemy files
12. 📋 Migrate NPC files
13. 📋 Advanced ContentBuilder tools
14. 📋 Balance tuning

---

## Quick Start Guide

**To begin implementation:**

1. Create `RealmEngine.Shared/Data/Json/general/rarity_config.json`
2. Update `items/weapons/names.json` with weights
3. Update `items/weapons/prefixes.json` (flatten + add weights)
4. Test pattern execution manually
5. Create RarityCalculator service
6. Add unit tests
7. Continue with remaining files

**Estimated Time:**
- Phase 2-3: 2-3 hours (config + sample files)
- Phase 4: 4-6 hours (code updates)
- Phase 5: 3-4 hours (ContentBuilder UI)
- Phase 6: 8-12 hours (file migration)
- Phase 7-9: 4-6 hours (loot system + testing)

**Total: ~25-35 hours of work**

---

## Success Criteria

✅ **System is successful when:**
- All JSON files use weight-based structure
- Pattern execution calculates rarity correctly
- ContentBuilder shows live rarity preview
- Loot generation targets weight ranges accurately
- Item rarity distribution matches expected drop rates
- Balance feels good in playtesting

---

## Support Resources

- **Full System Guide:** `docs/standards/WEIGHT_BASED_raritySystem.md`
- **Pattern Standards:** `docs/standards/PATTERN_COMPONENT_STANDARDS.md`
- **Decision Rationale:** `docs/standards/DRAFT_FINALIZATION_ANALYSIS.md`

---

**Ready to begin? Start with Phase 2: Create rarity_config.json**
