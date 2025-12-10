# Documentation Index

Welcome to the Console RPG documentation! This guide will help you navigate all available documentation.

---

## 📖 Main Documentation

### 🎮 [Game Design Document (GDD-Main.md)](./GDD-Main.md)
**The primary documentation for the entire project.** Contains:
- Complete game overview and feature list
- All game systems (combat, inventory, quests, etc.)
- Technical architecture and design patterns
- Progression tables and content details
- Future roadmap and planned features

**Start here for a complete understanding of the game!**

---

## 📚 Quick Reference Guides

### For Developers

- **[Vertical Slice Quick Reference](./VERTICAL_SLICE_QUICK_REFERENCE.md)** - How to add new features using CQRS pattern
- **[Architecture Decisions](./ARCHITECTURE_DECISIONS.md)** - Key architectural choices and rationale
- **[Organization & Layers Guide](./ORGANIZATION_AND_LAYERS_GUIDE.md)** - Project structure and layering

### For Users/Players

- **[Game Loop Guide](./guides/GAME_LOOP_GUIDE.md)** - Understanding the GameEngine and game flow
- **[Inventory Guide](./guides/INVENTORY_GUIDE.md)** - Complete item management system
- **[Save/Load Guide](./guides/SAVE_LOAD_GUIDE.md)** - Game persistence and save files
- **[Settings Guide](./guides/SETTINGS_GUIDE.md)** - Configuration and options

---

## 🔧 Implementation Guides

Located in [`implementation/`](./implementation/):

### Core Systems
- **[Combat Log Implementation](./implementation/COMBAT_LOG_IMPLEMENTATION.md)** - Combat logging system
- **[Inventory Implementation](./implementation/INVENTORY_IMPLEMENTATION.md)** - Item management details
- **[Save/Load Implementation](./implementation/SAVE_LOAD_IMPLEMENTATION.md)** - Persistence system
- **[Settings Implementation](./implementation/SETTINGS_IMPLEMENTATION.md)** - Configuration system
- **[GameEngine Modularization](./implementation/GAMEENGINE_MODULARIZATION.md)** - Game loop architecture

### Content Systems
- **[Trait System Implementation](./implementation/TRAIT_SYSTEM_IMPLEMENTATION.md)** - Generic trait system for items/enemies
- **[Enemy NPC Traits](./implementation/ENEMY_NPC_TRAITS.md)** - Enemy trait details
- **[Equipment Expansion](./implementation/EQUIPMENT_EXPANSION.md)** - Equipment slot system
- **[JSON Data System](./implementation/JSON_DATA_SYSTEM.md)** - Data-driven content

### Game Phases (Difficulty, Death, Endgame)
- **[Phase 1: Difficulty Foundation](./implementation/PHASE_1_DIFFICULTY_FOUNDATION.md)** - 5 difficulty modes
- **[Phase 2: Death System](./implementation/PHASE_2_DEATH_SYSTEM.md)** - Permadeath and Hall of Fame
- **[Phase 3: Apocalypse Mode](./implementation/PHASE_3_APOCALYPSE_MODE.md)** - Timer-based hardcore mode
- **[Phase 4: Endgame](./implementation/PHASE_4_ENDGAME.md)** - Quests, achievements, victory, New Game+

### Technical
- **[Test Infrastructure Setup](./implementation/TEST_INFRASTRUCTURE_SETUP.md)** - Testing framework
- **[Finalized](./implementation/FINALIZED.md)** - Initial project setup completion

---

## 🧪 Testing Documentation

Located in [`testing/`](./testing/):

- **[Test Coverage Report](./testing/TEST_COVERAGE_REPORT.md)** - Comprehensive test statistics (375 tests)

---

## 🛠️ Library Guides

Located in [`guides/`](./guides/):

### UI & Console
- **[ConsoleUI Guide](./guides/CONSOLEUI_GUIDE.md)** - Using Spectre.Console UI wrapper
- **[Quick Start Loops](./guides/QUICK_START_LOOPS.md)** - Simple game loop examples

### Configuration
- **[Settings Quick Start](./guides/SETTINGS_QUICK_START.cs)** - Code examples for settings
- **[Environment File Guide](./guides/ENV_FILE_GUIDE.md)** - .env configuration (if used)

---

## 🚀 Getting Started

### New to the Project?

1. **Start with the [Game Design Document (GDD-Main.md)](./GDD-Main.md)** - Complete overview
2. Review [Vertical Slice Quick Reference](./VERTICAL_SLICE_QUICK_REFERENCE.md) - Learn the architecture
3. Check [Game Loop Guide](./guides/GAME_LOOP_GUIDE.md) - Understand game flow
4. Read [ConsoleUI Guide](./guides/CONSOLEUI_GUIDE.md) - Build UI components


### Adding Features?

- **[Vertical Slice Quick Reference](./VERTICAL_SLICE_QUICK_REFERENCE.md)** - Complete guide to adding features
- **UI Changes**: [ConsoleUI Guide](./guides/CONSOLEUI_GUIDE.md)
- **Configuration**: [Settings Guide](./guides/SETTINGS_GUIDE.md)
- **Game Logic**: [Game Loop Guide](./guides/GAME_LOOP_GUIDE.md)

### Writing Tests?

- **[Test Coverage Report](./testing/TEST_COVERAGE_REPORT.md)** - Examples and current coverage (375 tests)

---

## � Documentation Structure

```
docs/
├── README.md                          ← This file
├── GDD-Main.md                        ← 🎮 PRIMARY DOCUMENTATION
├── VERTICAL_SLICE_QUICK_REFERENCE.md  ← Dev quick reference
├── ARCHITECTURE_DECISIONS.md          ← Architecture choices
├── ORGANIZATION_AND_LAYERS_GUIDE.md   ← Project structure
│
├── guides/                            ← User/developer guides
│   ├── CONSOLEUI_GUIDE.md
│   ├── GAME_LOOP_GUIDE.md
│   ├── INVENTORY_GUIDE.md
│   ├── SAVE_LOAD_GUIDE.md
│   ├── SETTINGS_GUIDE.md
│   └── ...
│
├── implementation/                    ← Implementation details
│   ├── COMBAT_LOG_IMPLEMENTATION.md
│   ├── INVENTORY_IMPLEMENTATION.md
│   ├── TRAIT_SYSTEM_IMPLEMENTATION.md
│   ├── PHASE_1_DIFFICULTY_FOUNDATION.md
│   ├── PHASE_2_DEATH_SYSTEM.md
│   ├── PHASE_3_APOCALYPSE_MODE.md
│   ├── PHASE_4_ENDGAME.md
│   └── ...
│
└── testing/                           ← Test documentation
    └── TEST_COVERAGE_REPORT.md
```

---

## � Contributing to Documentation

When adding new features:

1. Update **GDD-Main.md** with feature overview
2. Create implementation guide in `implementation/` if complex
3. Add user guide in `guides/` if user-facing
4. Update test coverage in `testing/TEST_COVERAGE_REPORT.md`
5. Update this index if adding new documents

---

## 🔗 External Links

- **Spectre.Console Docs**: https://spectreconsole.net/
- **MediatR GitHub**: https://github.com/jbogard/MediatR
- **LiteDB Docs**: https://www.litedb.org/
- **FluentValidation Docs**: https://docs.fluentvalidation.net/
- **Microsoft Configuration**: https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration

---

**Last Updated**: December 9, 2025  
**Documentation Version**: 2.0 (Post-GDD cleanup)  
**Test Coverage**: 375 tests (98.9% pass rate)

