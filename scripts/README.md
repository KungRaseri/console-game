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
- `Libraries/` - Game.Core, Game.Shared, Game.Data DLLs
- `ContentBuilder/` - WPF JSON editor application
- `Data/Json/` - 186 game data files
- `package-manifest.json` - Build metadata

**Parameters**:
- `Configuration` - Build configuration (default: Release)

---

### `deploy-to-godot.ps1`

**Purpose**: Deploys packaged game files to a Godot project directory.

**Usage**:
```powershell
.\deploy-to-godot.ps1 -GodotProjectPath "C:\path\to\godot-project"
```

**What it does**:
1. Validates package exists
2. Validates Godot project (checks for project.godot)
3. Copies Libraries/ to Godot project
4. Copies Data/Json/ to Godot project
5. Optionally copies ContentBuilder
6. Creates deployment info file

**Parameters**:
- `GodotProjectPath` (required) - Path to Godot project root
- `PackagePath` (optional) - Path to package folder (default: ..\package)

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
│   ├── Game.Core/      [74 DLLs]
│   ├── Game.Shared/    [7 DLLs]
│   └── Game.Data/      [15 DLLs]
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
