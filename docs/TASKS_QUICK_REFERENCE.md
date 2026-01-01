# VS Code Tasks & Launch Quick Reference

## 🎮 Running the Applications

### Console Game (Text-Based RPG)
**Method 1: Using Tasks (Ctrl+Shift+P → Tasks: Run Task)**
- **Task**: `run-console`
- **What it does**: Builds and runs the console game in the integrated terminal
- **Best for**: Quick testing and gameplay

**Method 2: Using Debugger (F5)**
- **Launch Config**: "Console Game (Debug)"
- **What it does**: Builds, launches with debugger attached
- **Best for**: Debugging game logic, stepping through code

**Method 3: Watch Mode (Auto-reload)**
- **Task**: `watch-console`
- **What it does**: Auto-rebuilds and restarts when code changes
- **Best for**: Active development

**Method 4: Command Line**
```powershell
dotnet run --project Game.Console/Game.Console.csproj
```

### Content Builder (WPF Tool)
**Method 1: Using Tasks (Ctrl+Shift+P → Tasks: Run Task)**
- **Task**: `run-contentbuilder`
- **What it does**: Builds and launches the WPF content editor
- **Best for**: Creating/editing game content (items, classes, enemies)

**Method 2: Using Debugger (F5)**
- **Launch Config**: "Content Builder (Debug)"
- **What it does**: Builds, launches with debugger attached
- **Best for**: Debugging the content builder UI

**Method 3: Command Line**
```powershell
dotnet run --project RealmForge/RealmForge.csproj
```

---

## 🔨 Build Tasks

### Standard Build Tasks
| Task | Shortcut | Description |
|------|----------|-------------|
| `build` | **Ctrl+Shift+B** | Build entire solution (default) |
| `build-console` | - | Build only the console game |
| `build-contentbuilder` | - | Build only the content builder |
| `clean` | - | Clean all build outputs |
| `rebuild` | - | Clean then build everything |

---

## 🧪 Testing Tasks

| Task | Shortcut | Description |
|------|----------|-------------|
| `test` | **Ctrl+Shift+T** | Run all tests (default test task) |
| `test-with-coverage` | - | Run tests with code coverage |
| `generate-coverage-report` | - | Generate HTML coverage report |
| `test-coverage` | - | Run tests + generate coverage report (combined) |

**View Coverage Report**:
After running `test-coverage`, open `TestResults/coverage-report/index.html` in a browser.

---

## 📁 Project Structure

```
console-game/
├── Game.Console/          # Main console game application
├── RealmForge/   # WPF tool for creating game content
├── RealmEngine.Core/             # Core game logic (shared)
├── RealmEngine.Data/             # Data access (repositories)
├── RealmEngine.Shared/           # Shared models and services
├── Game.Tests/            # Unit and integration tests
└── .vscode/
    ├── tasks.json         # Task definitions
    └── launch.json        # Debugger configurations
```

---

## 🚀 Quick Start Workflows

### Play the Game
1. Press **F5** or select "Console Game (Debug)" from debug dropdown
2. OR: **Ctrl+Shift+P** → Tasks: Run Task → `run-console`

### Edit Game Content
1. **Ctrl+Shift+P** → Tasks: Run Task → `run-contentbuilder`
2. Edit items, character classes, enemies, etc.
3. Save changes (content is stored in `RealmEngine.Shared/Data/`)

### Development with Auto-Reload
1. **Ctrl+Shift+P** → Tasks: Run Task → `watch-console`
2. Make code changes
3. Game automatically rebuilds and restarts

### Run Tests Before Committing
1. Press **Ctrl+Shift+B** to build
2. Press **Ctrl+Shift+T** to run tests (or run `test` task)
3. Check for 0 failures

### Generate Coverage Report
1. **Ctrl+Shift+P** → Tasks: Run Task → `test-coverage`
2. Open `TestResults/coverage-report/index.html`

---

## 💡 Tips

### Accessing Tasks
- **Quick**: Press `Ctrl+Shift+P`, type "task", select "Tasks: Run Task"
- **Terminal Menu**: Terminal → Run Task...
- **Command Palette**: Search for specific task name

### Debugging
- **Set Breakpoints**: Click left margin in code editor
- **F5**: Start debugging (uses selected launch config)
- **F10**: Step over
- **F11**: Step into
- **Shift+F11**: Step out
- **F9**: Toggle breakpoint

### Terminal Tips
- Tasks run in **dedicated panels** (won't interfere with each other)
- `run-console` gets its own terminal with focus
- `run-contentbuilder` launches as standalone window

### Common Issues
1. **"Program has exited" error**: Make sure you ran the correct build task first
2. **Port already in use**: Close previous instance of the application
3. **Build errors**: Run `clean` task, then `rebuild`

---

## 📝 Task Reference

### All Available Tasks

```
Build Tasks:
  build                    - Build entire solution (default)
  build-console           - Build console game only
  build-contentbuilder    - Build content builder only
  clean                   - Clean all outputs
  rebuild                 - Clean + build

Run Tasks:
  run-console             - Run the console game
  run-contentbuilder      - Run the content builder tool
  watch-console           - Auto-reload console game on changes

Test Tasks:
  test                    - Run all tests (default)
  test-with-coverage      - Run tests with coverage collection
  generate-coverage-report - Generate HTML coverage report
  test-coverage           - Run tests + generate report
```

---

## 🎯 Launch Configurations

When you press **F5**, you can choose from:

1. **Console Game (Debug)** - Debug the text-based RPG
2. **Content Builder (Debug)** - Debug the WPF content editor
3. **.NET Core Attach** - Attach debugger to running process

Switch configurations using the dropdown in the Debug panel or Run & Debug sidebar.

---

**Last Updated**: December 14, 2025  
**Solution Status**: ✅ All 6 projects building successfully
