# Build & Deployment Scripts

This directory contains essential scripts for building and deploying the game.

## Available Scripts

### `build-game-package.ps1`

**Purpose**: Builds and packages all game components for distribution or Godot integration.

**Usage**:
```powershell
.\build-game-package.ps1 [-Configuration Release|Debug]
```

**Output**: Creates `package/` folder in repo root with:
- `Libraries/` - RealmEngine.Core, RealmEngine.Shared, RealmEngine.Data DLLs
- `ContentBuilder/` - WPF JSON editor application
- `Data/Json/` - 186 game data files
- `package-manifest.json` - Build metadata

**Parameters**:
- `Configuration` - Build configuration (default: Release)

---

### `deploy-to-godot.ps1`

**Purpose**: Deploys packaged game files to a Godot project directory with automatic API changelog generation.

**Usage**:
```powershell
.\deploy-to-godot.ps1 -GodotProjectPath "C:\path\to\godot-project"
```

**What it does**:
1. **Analyzes XML documentation changes** (compares old vs new)
2. **Generates API changelog** (CHANGELOG_API.md) with:
   - Added methods/properties/types
   - Removed API members
   - Modified documentation
3. Validates package exists
4. Validates Godot project (checks for project.godot)
5. Copies Libraries/ to Godot project
6. Copies Data/Json/ to Godot project
7. Optionally copies RealmForge
8. Creates deployment info file

**Automatic Changelog Features**:
- Detects new, removed, and modified XML documentation
- Parses API members (Types, Methods, Properties, Fields, Events)
- Prepends changes to existing CHANGELOG_API.md
- No manual tracking required!

**Parameters**:
- `GodotProjectPath` (required) - Path to Godot project root
- `PackagePath` (optional) - Path to package folder (default: ..\package)

**Example Output**:
```
Analyzing XML documentation changes...
  • Changes detected in RealmEngine.Core.xml
    → Added: 5 | Removed: 0 | Modified: 3
  • New file: RealmEngine.Shared.xml
✓ Changelog generated: CHANGELOG_API.md
  → 2 file(s) with changes documented
```

---

### `view-api-changes.ps1`

**Purpose**: View recent API changes from the auto-generated changelog.

**Usage**:
```powershell
.\view-api-changes.ps1 -GodotProjectPath "C:\path\to\godot-project" [-Entries 3]
```

**What it does**:
- Reads CHANGELOG_API.md from Godot project
- Shows most recent N deployment changes
- Color-coded output (Added=Green, Removed=Red, Modified=Cyan)
- Shows API member details (Methods, Properties, Types, etc.)

**Parameters**:
- `GodotProjectPath` (required) - Path to Godot project root
- `Entries` (optional) - Number of recent deployments to show (default: 3)

**Example Output**:
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Deployment #1 - 2026-01-03 17:30:00
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

  📄 RealmEngine.Core.xml
    ➕ Added (5):
      [Method] GemGenerator.Generate
      [Method] EssenceGenerator.Generate
      [Property] SocketSlot.SocketType
    📝 Modified (3):
      [Method] ItemGenerator.Generate (documentation updated)
```

---

### `generate-coverage-report.ps1`

**Purpose**: Generates code coverage reports for test projects.

**Usage**:
```powershell
.\generate-coverage-report.ps1
```

**Requirements**:
- ReportGenerator tool (`dotnet tool install -g dotnet-reportgenerator-globaltool`)

**Output**: Creates `coverage-report/` with HTML coverage reports.

---

## Workflow

### Standard Development Build
```powershell
# 1. Build the package
.\build-game-package.ps1

# 2. Deploy to Godot project
.\deploy-to-godot.ps1 -GodotProjectPath "C:\path\to\godot-project"
```

### CI/CD Build

The GitHub Actions workflow (`.github/workflows/build-and-release.yml`) automatically:
1. Runs all tests
2. Builds package
3. Creates release artifacts on tagged commits

---

## Package Structure

After running `build-game-package.ps1`, the package structure is:

```
package/
├── Libraries/
│   ├── RealmEngine.Core/      [74 DLLs]
│   ├── RealmEngine.Shared/    [7 DLLs]
│   └── RealmEngine.Data/      [15 DLLs]
├── ContentBuilder/     [232 files - WPF app]
├── Data/
│   └── Json/           [186 JSON files]
└── package-manifest.json
```

---

## Notes

- Package output is gitignored (configured in `.gitignore`)
- ContentBuilder automatically uses package Data/ folder when deployed
- JSON data is deduplicated (not copied to ContentBuilder folder)
- All scripts require PowerShell 7+
