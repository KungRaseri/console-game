# RealmForge - Game Data Editor

**Version**: 2.0  
**Last Updated**: January 5, 2026  
**Technology**: WPF (.NET 9.0)  
**Purpose**: Visual editing of RealmEngine game data files

---

## Overview

**RealmForge** (formerly ContentBuilder) is a WPF desktop application that provides visual editing of JSON game data files. It eliminates manual JSON editing and prevents syntax errors through UI-driven data composition.

### Why RealmForge?

- **Error Prevention**: Token buttons prevent manual typing errors (`{base}` vs `{bse}`)
- **Visual Workflow**: See patterns, components, and generated examples in real-time
- **No JSON Knowledge Required**: Non-technical users can edit game data
- **Data Integrity**: Validation ensures all references exist and syntax is correct
- **Productivity**: Faster than manual JSON editing with Find/Replace

---

## Features

### 1. Pattern Editor (Name Lists)

Edit pattern-based name generation files (`names.json`) with visual tools.

**Dynamic Component Buttons:**
- Auto-generates buttons for each component in the loaded file
- **Weapons**: `{base}`, `{prefix}`, `{suffix}`, `{quality}`, `{descriptive}`
- **NPCs**: `{firstName}`, `{lastName}`, `{title}`, `{nickname}`
- **Quests**: `{action}`, `{target}`, `{location}`, `{reward}`
- Adapts to ANY JSON structure - buttons reflect actual components

**Reference Token Buttons:**
- `@materialRef/weapon` - Insert weapon-compatible material reference
- `@materialRef/armor` - Insert armor-compatible material reference
- `@itemRef` - Reference another item
- `@enemyRef` - Reference enemy type
- **Browse Dialog**: Visual catalog browser for selecting specific references (e.g., Iron, Steel, Mithril)

**Default Patterns:**
- Every file gets a readonly `{base}` pattern automatically
- Cannot be edited or deleted (data integrity protection)
- Visual indicator: Gray background and text
- Delete button disabled for readonly patterns
- Not saved to JSON (regenerated on load)

**Real-time Preview:**
- Generate example names as you edit patterns
- See how components and references combine
- Test pattern weights and rarity distribution
- Validate syntax before saving

**Weight-based Selection:**
- Configure rarity/frequency of each component/pattern
- Higher weights = more common in generated names
- Used by Bogus library for procedural generation in RealmEngine

### 2. Component Editor

Manage component groups (prefix, suffix, quality, base, etc.):
- Add/remove component groups
- Edit component values with weights
- Trait assignment to components (planned)
- Visual organization by category
- Validation: Ensures no duplicate keys

### 3. Quest Catalog Editor

Design quests with full objective and reward configuration:

**Quest Metadata:**
- Quest ID, Name, Description
- Level requirement
- Quest giver (NPC name)
- Social class requirements (Noble, Merchant, Peasant, etc.)

**Prerequisite Management:**
- Select prerequisite quests from dropdown
- Chain quests together (Quest B requires Quest A)
- Visual dependency tree (planned)

**Objective Definition:**
- Objective type (Kill, Collect, Explore, Interact)
- Target (enemy type, item, location, NPC)
- Required count (defeat 5 enemies, collect 3 items)
- Progress tracking configuration

**Reward Configuration:**
- XP reward
- Gold reward
- Item rewards (select from catalog)
- Apocalypse time bonuses
- Skill unlocks (planned)

### 4. Material Catalog Editor

Manage materials used in item generation:

**Material Properties:**
- Name, Description
- Rarity (Common to Legendary)
- Base Value (gold cost)
- Hardness (affects durability)
- Weight (affects item stats)
- Elemental affinity (Fire, Ice, Lightning, etc.)

**Context-Specific Traits:**
- **Weapon Materials**: Damage bonus, critical chance, attack speed
- **Armor Materials**: Defense bonus, resistances, weight reduction
- Same material can have different traits for weapons vs armor

**Rarity Weights:**
- Configure how often materials appear in generation
- Iron: Common (high weight)
- Mithril: Rare (medium weight)
- Adamantine: Legendary (low weight)

**Visual Trait Editor:**
- Add/remove traits with key-value pairs
- Trait templates (damage bonuses, resistances, stat boosts)
- Validation: Numeric values, valid trait keys

---

## Technical Architecture

### Technology Stack

- **WPF** (.NET 9.0) - Desktop UI framework
- **MaterialDesignThemes** 5.1.0 - Material Design UI components
- **CommunityToolkit.Mvvm** 8.4.0 - MVVM framework (RelayCommand, ObservableProperty)
- **Newtonsoft.Json** 13.0.4 - JSON parsing/serialization
- **FluentValidation** 12.1.1 - Input validation
- **Serilog** 4.3.0 - Structured logging

### MVVM Pattern

**ViewModels:**
- Observable properties for data binding
- RelayCommands for user actions
- Business logic and state management

**Models:**
- Data structures (NameListCategory, ComponentBase, PatternBase)
- Domain entities (QuestTemplate, MaterialDefinition)

**Services:**
- `JsonEditorService` - File I/O operations
- `ValidationService` - Data validation
- `PatternExecutor` - Pattern parsing and execution (in RealmEngine.Shared)
- `DataReferenceResolver` - Cross-file reference resolution (in RealmEngine.Shared)

**Converters:**
- `InverseBooleanConverter` - UI data transformation
- Custom converters for complex bindings

**Views:**
- XAML with Material Design styling
- Responsive layouts
- Accessibility support

### Key Classes

**ViewModels:**
- `NameListEditorViewModel` - Pattern/component editing logic
- `ReferenceSelectorViewModel` - Catalog browser logic
- `QuestCatalogEditorViewModel` - Quest editing logic
- `MaterialCatalogEditorViewModel` - Material editing logic

**Models:**
- `NamePatternBase` - Abstract pattern model (Item/NPC specializations)
- `NameComponentBase` - Abstract component model (Item/NPC specializations)
- `QuestTemplate` - Quest data model
- `MaterialDefinition` - Material data model

---

## Usage Workflow

### Typical Editing Session

1. **Launch RealmForge**
   - Open the WPF application
   - Application loads recent file history

2. **Open File**
   - Browse to JSON data file (e.g., `weapons/names.json`)
   - File loads with validation
   - Component buttons auto-generate

3. **Edit Components**
   - Add new `{quality}` component with values
   - Values: Masterwork (weight 10), Fine (weight 30), Standard (weight 60)
   - Assign traits (optional)

4. **Create Pattern**
   - Click token buttons to compose pattern
   - Click `{quality}` → `{base}` → `{suffix}`
   - Result: `{quality} {base} {suffix}` (no manual typing!)
   - Assign pattern weight

5. **Preview**
   - Generate example names
   - Examples: "Masterwork Iron Longsword of Flame"
   - Test weight distribution

6. **Save**
   - Validation runs automatically
   - Backup created before overwrite
   - Write back to JSON
   - Success notification

7. **Test In-Game**
   - Launch RealmEngine
   - Regenerate content with new patterns
   - Verify results match preview

---

## Data Validation

### Real-time Validation

- **Pattern syntax**: Validates token format (`{component}`, `@reference/context`)
- **Component keys**: Ensures referenced components exist in file
- **Reference paths**: Validates cross-file reference format
- **Rarity weights**: Enforces positive integers (no negative or zero)
- **Required fields**: Name, weight, etc. must be present
- **Duplicate detection**: Warns on duplicate component names

### Pre-Save Validation

- JSON structure validation
- FluentValidation rules for all models
- File write permissions check
- Backup creation before overwrite
- Reference existence verification

---

## Error Prevention

### UI-Driven Safety

- **No Manual Typing**: Token buttons eliminate syntax errors
- **Readonly Defaults**: `{base}` pattern can't be deleted (data integrity)
- **Visual Feedback**: Gray background for readonly fields, tooltips
- **Disabled Actions**: Delete button disabled for protected patterns
- **Auto-spacing**: Proper spacing when appending tokens
- **Undo Support**: Ctrl+Z for recent changes (planned)

### Data Integrity

- Atomic saves (all-or-nothing file writes)
- Backup before overwrite
- JSON schema validation
- Reference existence verification
- File locking during writes

---

## Testing

### Unit Tests

**RealmForge.Tests** (73 tests):
- ViewModel behavior (commands, properties, validation)
- Pattern composition logic
- Reference resolution
- File I/O operations
- Error handling
- Data model validation

### Integration Tests

**Integration** (35 tests):
- Cross-file reference resolution
- Pattern execution with references
- Data loading and saving
- Validation pipeline
- End-to-end workflows

---

## Known Limitations

1. **No Undo/Redo** - Currently no action history (planned for v2.1)
2. **Single File Edit** - Can't edit multiple files simultaneously
3. **No Merge Conflicts** - Manual resolution required for concurrent edits
4. **Limited Quest Support** - v4.2 standard partially implemented
5. **No Trait Editor UI** - Some traits must be edited in JSON manually

---

## Future Enhancements

### Version 2.1 (Planned - Q1 2026)

- Multi-file editing with tabbed interface
- Full undo/redo with action history
- Dark mode theme
- Enhanced trait editor UI

### Version 2.2 (Planned - Q2 2026)

- Visual dependency graph for references
- Drag-and-drop pattern composition
- Auto-complete for references
- Bulk operations (edit multiple files)

### Version 3.0 (Future)

- Real-time collaboration (multi-user editing)
- Version control integration
- Export to game-ready binary formats
- Plugin system for custom editors

---

## Installation & Setup

### Requirements

- Windows 10/11 (64-bit)
- .NET 9.0 Runtime
- 100 MB disk space
- 2 GB RAM minimum

### Installation

1. Download RealmForge from releases
2. Extract to desired location
3. Run `RealmForge.exe`
4. Configure data folder path in settings

### Configuration

**On First Launch:**
- Set RealmEngine data folder path
- Configure editor preferences
- Set backup location
- Choose theme (light/dark when available)

---

## Support

For issues, feature requests, or questions:
- GitHub Issues: [Repository Link]
- Documentation: `docs/REALMFORGE.md` (this file)
- Standards: `docs/standards/README.md`

---

**Version History:**
- **v2.0** (January 2026): Renamed from ContentBuilder, JSON v4.1 support
- **v1.0** (December 2025): Initial release with pattern editor
