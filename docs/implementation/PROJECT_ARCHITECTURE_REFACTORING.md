# 🏗️ Project Architecture Refactoring Plan

**Date**: December 14, 2025  
**Status**: IN PROGRESS

---

## 🎯 Objective

Restructure the solution to support multiple UI frontends (Console, WPF ContentBuilder, future web/mobile) by separating:
- **Business Logic** (RealmEngine.Core)
- **Data Access** (RealmEngine.Data)
- **Shared Resources** (RealmEngine.Shared)
- **Console UI** (Game.Console - renamed from Game)
- **WPF Editor** (RealmForge - existing)

---

## 📊 New Solution Structure

```
console-game/
├── RealmEngine.Core/              ✨ NEW: Core game logic (UI-agnostic)
│   ├── Features/           ← CQRS handlers, commands, queries
│   ├── Models/             ← Domain models (Character, Item, etc.)
│   ├── Services/           ← Business services (LevelUpService, etc.)
│   ├── Generators/         ← Content generators (Bogus)
│   ├── Validators/         ← FluentValidation validators
│   ├── Events/             ← Domain events
│   └── Settings/           ← Configuration classes
│
├── RealmEngine.Data/              ✨ NEW: Data access layer
│   ├── Repositories/       ← LiteDB repositories
│   ├── Services/           ← Data services (SaveGameService, etc.)
│   └── Models/             ← Database-specific models (if needed)
│
├── RealmEngine.Shared/            ✅ EXISTING: Shared utilities & data
│   ├── Data/Json/          ← JSON game content files
│   ├── Models/             ← Shared DTOs
│   └── Services/           ← JsonDataService
│
├── Game.Console/           🔄 RENAMED from "Game": Console UI only
│   ├── UI/                 ← ConsoleUI, MenuService (Spectre.Console)
│   ├── Orchestrators/      ← Console-specific workflow orchestrators
│   ├── Program.cs          ← Console entry point
│   ├── GameEngine.cs       ← Console game loop
│   └── Audio/              ← NAudio (console-specific)
│
├── RealmForge/    ✅ EXISTING: WPF editor
│   └── (WPF application)
│
└── Game.Tests/             ✅ EXISTING: All tests
    └── (test project)
```

---

## 📦 Project Dependencies

```
Game.Console  ─┬─→  RealmEngine.Core  ─┬─→  RealmEngine.Data  ──→  RealmEngine.Shared
               │                 │
               └─→  RealmEngine.Data ───┘
               
RealmForge  ──→  RealmEngine.Data  ──→  RealmEngine.Shared

Game.Tests  ──→  RealmEngine.Core
            ──→  RealmEngine.Data
            ──→  Game.Console
```

---

## 📋 Migration Checklist

### Phase 1: Create New Projects ✅
- [x] Create RealmEngine.Core class library
- [x] Create RealmEngine.Data class library
- [x] Add to solution
- [x] Configure project files with packages

### Phase 2: Move Code to RealmEngine.Core
**From Game/ → RealmEngine.Core/**

- [ ] Features/ (all CQRS code)
  - [ ] Achievement/
  - [ ] CharacterCreation/
  - [ ] Combat/
  - [ ] Death/
  - [ ] Exploration/
  - [ ] HallOfFame/
  - [ ] Inventory/
  - [ ] Quest/
  - [ ] SaveLoad/
  - [ ] Victory/

- [ ] Models/ (domain models)
  - [ ] Character.cs
  - [ ] Item.cs
  - [ ] Enemy.cs
  - [ ] Quest.cs
  - [ ] Achievement.cs
  - [ ] SaveGame.cs
  - [ ] (all other models)

- [ ] Services/ (business services)
  - [ ] LevelUpService.cs
  - [ ] XpCalculator.cs
  - [ ] (non-UI services)

- [ ] Generators/ (content generators)
  - [ ] CharacterGenerator.cs
  - [ ] EnemyGenerator.cs
  - [ ] ItemGenerator.cs
  - [ ] NpcGenerator.cs
  - [ ] QuestGenerator.cs

- [ ] Validators/ (FluentValidation)
  - [ ] CharacterValidator.cs
  - [ ] (all validators)

- [ ] Settings/ (configuration)
  - [ ] GameSettings.cs
  - [ ] GameplaySettings.cs
  - [ ] (all settings classes)

- [ ] Utilities/ (non-UI utilities)
  - [ ] ColorMap.cs
  - [ ] DiceRoller.cs
  - [ ] (shared utilities)

### Phase 3: Move Code to RealmEngine.Data
**From Game/Shared/Data/ → RealmEngine.Data/**

- [ ] Repositories/
  - [ ] SaveGameRepository.cs
  - [ ] HallOfFameRepository.cs
  - [ ] (LiteDB repositories)

- [ ] Services/
  - [ ] JsonDataService.cs (if it loads JSON)
  - [ ] SaveGameService.cs (if exists)

### Phase 4: Keep in RealmEngine.Shared
**No changes needed**

- [x] Data/Json/ (all JSON files)
- [x] Models/ (simple DTOs)
- [x] Services/JsonEditorService.cs (used by ContentBuilder)

### Phase 5: Rename Game → Game.Console
**Keep Console-Specific Code**

- [ ] Rename project folder
- [ ] Update .csproj file
- [ ] Update namespaces
- [ ] Keep:
  - [ ] UI/ (ConsoleUI, IConsoleUI, MenuService)
  - [ ] Audio/ (NAudio - console-specific)
  - [ ] Program.cs (console entry point)
  - [ ] GameEngine.cs (console game loop)
  - [ ] appsettings.json
  - [ ] .env

### Phase 6: Update References

- [ ] Game.Console.csproj
  - [ ] Add reference to RealmEngine.Core
  - [ ] Add reference to RealmEngine.Data
  - [ ] Keep Spectre.Console, NAudio

- [ ] RealmForge.csproj
  - [ ] Already references RealmEngine.Shared ✅
  - [ ] Add reference to RealmEngine.Data (if needed)

- [ ] Game.Tests.csproj
  - [ ] Add reference to RealmEngine.Core
  - [ ] Add reference to RealmEngine.Data
  - [ ] Update reference from Game → Game.Console

### Phase 7: Update Namespaces

- [ ] Change `namespace Game;` → `namespace RealmEngine.Core;` (in Core files)
- [ ] Change `namespace Game;` → `namespace RealmEngine.Data;` (in Data files)
- [ ] Change `namespace Game;` → `namespace Game.Console;` (in Console files)
- [ ] Update all `using Game;` statements

### Phase 8: Fix Compilation & Test

- [ ] Build RealmEngine.Shared
- [ ] Build RealmEngine.Data
- [ ] Build RealmEngine.Core
- [ ] Build Game.Console
- [ ] Build RealmForge
- [ ] Run all tests (379 tests should still pass)

---

## 🎯 Expected Benefits

1. **UI Agnostic**: RealmEngine.Core has zero UI dependencies
2. **Reusability**: Multiple UIs can share same business logic
3. **Testability**: Test core logic without UI
4. **Maintainability**: Clear separation of concerns
5. **Scalability**: Easy to add new UI frontends (web, mobile, etc.)

---

## 🚧 Potential Issues & Solutions

### Issue 1: Circular Dependencies
**Problem**: RealmEngine.Core needs IConsoleUI for events, but IConsoleUI is in Game.Console

**Solution**: 
- Create `RealmEngine.Core/Abstractions/IGameUI.cs` (generic interface)
- Move IConsoleUI → Game.Console
- RealmEngine.Core uses IGameUI, Game.Console implements it

### Issue 2: Shared Event Handlers
**Problem**: Some event handlers use ConsoleUI

**Solution**:
- Keep event handler base in RealmEngine.Core
- Console-specific handlers in Game.Console
- Use dependency injection for UI

### Issue 3: Orchestrators
**Problem**: Orchestrators use ConsoleUI but also business logic

**Solution**:
- Move to Game.Console (they're UI-specific)
- Core handlers can be called from any UI

### Issue 4: Test References
**Problem**: Tests reference old Game project

**Solution**:
- Update test project references
- May need to split tests: Core tests vs Console tests

---

## 📝 Next Steps

1. ✅ Create new projects (DONE)
2. 🔄 Move Features/ to RealmEngine.Core (IN PROGRESS)
3. Move Models/ to RealmEngine.Core
4. Move Services/ to RealmEngine.Core/Data based on responsibility
5. Rename Game → Game.Console
6. Update all references and namespaces
7. Build and test

---

## 🎉 Success Criteria

- [ ] All 5 projects build successfully
- [ ] All 379 tests pass
- [ ] Game.Console runs without errors
- [ ] RealmForge runs without errors
- [ ] No circular dependencies
- [ ] Clean project references (Data → Shared, Core → Data, Console → Core)

---

**Status**: Phase 1 complete, starting Phase 2...
