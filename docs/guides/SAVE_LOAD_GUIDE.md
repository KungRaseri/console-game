# Save/Load System Guide

## Overview

The save/load system uses **LiteDB** (a lightweight NoSQL database) to persist game state, including your character, inventory, learned skills, equipped items, and all progress.

## Features

### ✅ What Gets Saved
- **Character Data**
  - Name, class, level, experience
  - Health, mana, gold
  - All D20 attributes (STR, DEX, CON, INT, WIS, CHA)
  - Unspent attribute and skill points
  - Pending level-ups
  
- **Skills**
  - All learned skills with current ranks
  - Skill progression tracked between sessions
  
- **Equipment**
  - All 13 equipment slots (weapons, armor, jewelry)
  - Item stats, bonuses, and enchantments preserved
  
- **Inventory**
  - All items in your inventory
  - Item properties, upgrades, and rarities
  
- **Metadata**
  - Save date/time
  - Total playtime
  - Unique save ID

### 🎮 Save Methods

#### 1. **Manual Save**
- Select "Save Game" from the main in-game menu
- Saves to your active save slot
- Shows confirmation message

#### 2. **Auto-Save** (Recommended)
- Automatically saves after every combat victory
- Transparent - shows "[grey]Game auto-saved[/]" message
- Never lose progress from unexpected exits
- Uses the same save slot as your loaded game

#### 3. **Quick Save** (New Game)
- When starting a new game, your progress is auto-saved after first combat
- Creates a new save slot for the new character

### 📂 Load Game

1. Select **"Load Game"** from the main menu
2. View all available saves in a table:
   - Player name and class
   - Character level
   - Time since last save
   - Total playtime

3. Choose a save to load or:
   - **Delete a Save**: Remove unwanted save files
   - **Back to Menu**: Return without loading

### 🗑️ Delete Saves

1. From the Load Game menu, select **"Delete a Save"**
2. Choose which save to delete
3. Confirm deletion (⚠️ cannot be undone!)
4. Returns to load menu

### 💾 Save File Location

- **Database File**: `savegames.db` in the game root directory
- **Format**: LiteDB NoSQL database
- **Persistence**: Saves persist between game sessions
- **Portability**: Copy `savegames.db` to backup or transfer saves

### 🔄 Save Slots

- **Multiple Characters**: Each character gets their own save
- **Auto-Update**: Auto-save overwrites the existing save for that character
- **No Limit**: Create as many characters as you want

### 🧪 Technical Details

#### SaveGame Model
```csharp
public class SaveGame
{
    public string Id { get; set; }
    public string PlayerName { get; set; }
    public DateTime SaveDate { get; set; }
    public Character Character { get; set; }
    public List<Item> Inventory { get; set; }
    public int PlayTimeMinutes { get; set; }
}
```

#### SaveGameService Methods
- `SaveGame(character, inventory, saveId?)` - Save or update game
- `LoadGame(saveId)` - Load game by ID
- `AutoSave(character, inventory)` - Auto-save (overwrites existing)
- `DeleteSave(saveId)` - Delete a save
- `GetAllSaves()` - Get list of all saves
- `GetMostRecentSave()` - Get latest save
- `HasAnySaves()` - Check if saves exist

#### Storage
- Uses **LiteDB** for efficient NoSQL storage
- Automatic indexing on `PlayerName` and `SaveDate`
- Resilient error handling with Polly retry logic
- Structured logging with Serilog

### 🎯 Best Practices

1. **Trust Auto-Save**: The game saves after every combat - you rarely need manual saves
2. **Multiple Playthroughs**: Create different characters to try different classes
3. **Backup Saves**: Copy `savegames.db` before major updates
4. **Clean Up**: Delete old/unused saves to keep the list manageable

### 🐛 Troubleshooting

**Problem**: "Failed to save game"
- **Solution**: Check disk space and file permissions

**Problem**: "No saved games found"
- **Solution**: Complete at least one combat to create a save

**Problem**: Loaded character has wrong stats
- **Solution**: Ensure you loaded the correct save (check name/level)

**Problem**: Save file corrupted
- **Solution**: Restore from backup or start new game

### 📊 Tested Features

All save/load functionality is covered by **13 comprehensive tests**:
- ✅ Save game creates new save
- ✅ Load game retrieves saved data
- ✅ Update existing save (same ID)
- ✅ Delete save removes from database
- ✅ Get most recent save
- ✅ Check for existing saves
- ✅ Auto-save overwrites character save
- ✅ Learned skills persist
- ✅ Equipped items persist
- ✅ D20 attributes persist
- ✅ Inventory items persist
- ✅ Character metadata saved
- ✅ Multiple save management

### 🔮 Future Enhancements

Potential additions for the save system:
- 📸 Save screenshots/thumbnails
- 🏆 Achievement tracking across saves
- 📊 Statistics dashboard (kills, deaths, gold earned)
- ☁️ Cloud save support
- 🔄 Import/export saves as JSON
- 🗂️ Save profiles/categories
- ⏱️ Autosave intervals (every X minutes)
- 💾 Quick save slots (F5/F9 style)

## Example Usage

```csharp
// Manual save from GameEngine
await SaveGameAsync();

// Auto-save after combat
_saveGameService.AutoSave(_player, _inventory);

// Load game
await LoadGameAsync();

// Delete save
await DeleteSaveAsync(saves);
```

## Summary

The save/load system provides robust, automatic game state persistence with minimal player intervention. Auto-save ensures you never lose progress, while manual save and load options give you full control over your game sessions.

**Total Tests**: 286 (13 new save/load tests)
**Status**: ✅ All tests passing
**Database**: LiteDB (savegames.db)
**Auto-Save**: After every combat victory
