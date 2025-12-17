# v4.0 Migration Activation Checklist

**Migration Status:** ✅ All categories converted to v4.0  
**Current State:** v4 files created, legacy files still active  
**Next Action:** Activate v4.0 and deprecate legacy files

---

## Option 1: Quick Activation (Recommended)

**Replace legacy files with v4 in one step**

### Commands to Run:

```powershell
# Navigate to workspace
cd C:\code\console-game\Game.Shared\Data\Json\items

# WEAPONS - Replace with v4
Rename-Item "weapons\names.json" "weapons\names_v3_backup.json"
Rename-Item "weapons\prefixes.json" "weapons\prefixes_v3_backup.json"
Rename-Item "weapons\suffixes.json" "weapons\suffixes_v3_backup.json"
Rename-Item "weapons\names_v4.json" "weapons\names.json"

# ARMOR - Replace with v4
Rename-Item "armor\names.json" "armor\names_v3_backup.json"
Rename-Item "armor\prefixes.json" "armor\prefixes_v3_backup.json"
Rename-Item "armor\suffixes.json" "armor\suffixes_v3_backup.json"
Rename-Item "armor\names_v4.json" "armor\names.json"

# ENCHANTMENTS - Replace with v4
Rename-Item "enchantments\prefixes.json" "enchantments\prefixes_v3_backup.json"
Rename-Item "enchantments\suffixes.json" "enchantments\suffixes_v3_backup.json"
Rename-Item "enchantments\names_v4.json" "enchantments\names.json"

# Optional: Create backup folder
New-Item -ItemType Directory -Path "_v3_legacy_backups" -ErrorAction SilentlyContinue
Move-Item "*\*_v3_backup.json" "_v3_legacy_backups\"
```

---

## Option 2: Gradual Testing (Safer)

**Test each category individually before full activation**

### Step 1: Test Weapons First
```powershell
# Temporarily rename to test
Rename-Item "weapons\names.json" "weapons\names_v3_temp.json"
Rename-Item "weapons\names_v4.json" "weapons\names.json"

# Test in ContentBuilder/Game
# ... verify it works ...

# If successful, backup v3
Rename-Item "weapons\names_v3_temp.json" "weapons\names_v3_backup.json"
# Keep prefixes/suffixes as backup for now
```

### Step 2: Test Armor Next
```powershell
# Same process...
Rename-Item "armor\names.json" "armor\names_v3_temp.json"
Rename-Item "armor\names_v4.json" "armor\names.json"
```

### Step 3: Test Enchantments Last
```powershell
Rename-Item "enchantments\names_v4.json" "enchantments\names.json"
```

---

## Option 3: Git Workflow (Best Practice)

**Use version control to safely migrate**

```powershell
# Create migration branch
git checkout -b naming-system-v4-migration

# Stage v4 files
git add Game.Shared/Data/Json/items/*/names_v4.json

# Commit v4 addition
git commit -m "Add v4.0 unified naming files for weapons, armor, enchantments"

# Replace files (same commands as Option 1)
# ... rename v4 → names.json, backup old files ...

# Stage changes
git add Game.Shared/Data/Json/items/

# Commit migration
git commit -m "Activate v4.0 naming system, backup legacy files"

# If everything works, merge to main
git checkout main
git merge naming-system-v4-migration

# If problems occur, revert easily
git reset --hard HEAD~1
```

---

## What Happens After Activation

### ✅ Expected Changes

1. **ContentBuilder Will:**
   - Load unified `names.json` files (v4.0)
   - Show Components tab for all pattern_generation files
   - Display traits in component lists (if UI supports it)
   - Continue working normally for all other features

2. **Game.Core Generators Will:**
   - Read components from new structure
   - Apply traits from components when generating items
   - Calculate emergent rarity from component weights

3. **File Structure Will Be:**
   ```
   items/
   ├── weapons/
   │   ├── names.json          ← v4.0 ACTIVE
   │   ├── types.json          ← unchanged
   │   └── (prefixes/suffixes backed up)
   ├── armor/
   │   ├── names.json          ← v4.0 ACTIVE
   │   ├── types.json          ← unchanged
   │   └── (prefixes/suffixes backed up)
   └── enchantments/
       ├── names.json          ← v4.0 ACTIVE (NEW FILE!)
       └── (prefixes/suffixes backed up)
   ```

### ⚠️ Potential Issues

1. **Code References to Prefixes/Suffixes:**
   - If any C# code directly loads `prefixes.json` or `suffixes.json`, it will break
   - Search codebase for references: `prefixes.json`, `suffixes.json`
   - Update to use unified `names.json` structure

2. **ContentBuilder Trait UI:**
   - Current UI may not support editing traits yet (enhancement pending)
   - Components will load, but trait editing requires new UI features

3. **Generator Code:**
   - May need updates to read traits from new structure
   - Test item generation after migration

---

## Testing Commands

After activation, run these to verify:

```powershell
# Build solution
dotnet build Game.sln

# Run ContentBuilder
dotnet run --project Game.ContentBuilder

# Run tests
dotnet test Game.Tests
dotnet test Game.ContentBuilder.Tests

# Run console game (test item generation)
dotnet run --project Game.Console
```

---

## Rollback Plan

If anything breaks:

```powershell
# Quick rollback (if backups in same folder)
Rename-Item "weapons\names.json" "weapons\names_v4_failed.json"
Rename-Item "weapons\names_v3_backup.json" "weapons\names.json"
# Repeat for armor, enchantments...

# Git rollback (if using Option 3)
git reset --hard HEAD~1
# Or specific commit
git reset --hard <commit-hash-before-migration>
```

---

## Next Steps After Activation

### 1. ContentBuilder Trait Editor (User Requested)

**Features to Add:**
- [ ] Trait list view in component editor
- [ ] Add new trait button
- [ ] Trait type selector (number/string/boolean)
- [ ] Trait value editor with validation
- [ ] Delete trait button
- [ ] Trait preview panel

### 2. Enhanced Pattern Editor
- [ ] Visual pattern builder (drag-drop tokens)
- [ ] Live item preview with traits calculated
- [ ] Emergent rarity calculator

### 3. Migration Tools (Optional)
- [ ] Import v3 legacy files
- [ ] Auto-convert to v4
- [ ] Trait inference from descriptions

---

## User Confirmation Needed

**Before proceeding, please confirm:**

1. ✅ Do you want to activate v4.0 now?
2. 🔍 Which activation method? (Quick / Gradual / Git Workflow)
3. 🧪 Do you want to test in ContentBuilder first?
4. 📋 Should we search codebase for `prefixes.json` / `suffixes.json` references?

---

## Summary

**Migration Complete:** ✅ 3 categories, 242 components, 750+ traits  
**Files Created:** 3 v4.0 JSON files (3,650 lines total)  
**Legacy Files:** 8 files ready to backup  
**Status:** Ready to activate when you approve! 🚀
