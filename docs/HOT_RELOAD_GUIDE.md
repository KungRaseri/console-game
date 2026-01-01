# Hot Reload & Watch Mode Guide

## Quick Start

Run ContentBuilder with hot reload enabled:
```powershell
# Option 1: Use VS Code task (recommended)
Ctrl+Shift+P → "Tasks: Run Task" → "watch-contentbuilder"

# Option 2: Command line
dotnet watch run --project RealmForge/RealmForge.csproj --non-interactive
```

## What Gets Hot Reloaded?

### ✅ Supported Changes (No Restart)
- **C# method bodies** - Logic changes, calculations
- **Property values** - Initial values, defaults
- **XAML styling** - Colors, fonts, margins, padding (with XAML Hot Reload)
- **String literals** - Text, messages, labels
- **Lambda expressions** - LINQ, event handlers

### ⚠️ Requires Restart (Rude Edits)
- **New types** - Adding classes, interfaces, enums
- **Type signatures** - Method parameters, return types
- **Property types** - Changing property types
- **Constructor changes** - Adding/removing constructors
- **Namespace changes** - Renaming namespaces
- **Assembly references** - Adding NuGet packages

When a restart is required, the watch task will automatically restart the app (enabled via `DOTNET_WATCH_RESTART_ON_RUDE_EDIT`).

## Features

### ContentBuilder Watch Task
```json
{
  "label": "watch-contentbuilder",
  "command": "dotnet watch run",
  "args": [
    "--project", "RealmForge/RealmForge.csproj",
    "--non-interactive"  // Prevents prompts, auto-restarts
  ],
  "isBackground": true,  // Keeps running in background
  "env": {
    "DOTNET_WATCH_RESTART_ON_RUDE_EDIT": "true"  // Auto-restart on breaking changes
  }
}
```

### Project Configuration
```xml
<PropertyGroup>
  <EnableHotReload>true</EnableHotReload>
</PropertyGroup>
```

## How It Works

1. **File Watcher** - Monitors .cs, .xaml, .json files for changes
2. **Incremental Compilation** - Only recompiles changed files
3. **Hot Reload** - Patches running process with new IL code
4. **XAML Hot Reload** - Updates UI without restart (VS Code C# Dev Kit)
5. **Auto-Restart** - Falls back to restart if hot reload not possible

## Typical Workflow

### Making Code Changes
1. Start watch task: `Ctrl+Shift+P` → "watch-contentbuilder"
2. Edit ViewModel logic (e.g., LoadData method)
3. Save file (`Ctrl+S`)
4. Hot reload applies change instantly
5. Test in running ContentBuilder

### Making XAML Changes
1. Edit MainWindow.xaml or views
2. Save file
3. XAML Hot Reload updates UI (if supported)
4. Otherwise, app restarts automatically

### Adding New Features
1. Add new method/property → Hot reload ✅
2. Change method signature → Auto-restart ⚠️
3. Add new class → Auto-restart ⚠️
4. Add NuGet package → Manual restart + rebuild required ❌

## Performance Tips

### Faster Rebuilds
- Keep solution structure shallow
- Split large files into smaller components
- Use partial classes for generated code

### Reduce Restart Frequency
- Plan API changes (method signatures) in batches
- Avoid frequent type signature changes
- Test logic changes before structural changes

## Troubleshooting

### Hot Reload Not Working
```powershell
# Check if hot reload is enabled
dotnet watch run --project RealmForge/RealmForge.csproj --verbose

# Look for: "Hot reload capabilities: Baseline, AddMethodToExistingType, ..."
```

### XAML Not Hot Reloading
- Ensure C# Dev Kit extension is installed in VS Code
- Check Output → "XAML Hot Reload" for errors
- Some XAML changes require restart (e.g., data templates)

### Watch Not Detecting Changes
```powershell
# Clear watch cache
dotnet watch run --project RealmForge/RealmForge.csproj --no-hot-reload

# Force full rebuild
dotnet clean && dotnet watch run --project RealmForge/RealmForge.csproj
```

### File Locked Errors
- Close ContentBuilder if running manually
- Kill any stuck processes: `Get-Process RealmForge | Stop-Process`
- Restart watch task

## Commands Reference

### Start Watch Mode
```powershell
# With hot reload (default)
dotnet watch --project RealmForge/RealmForge.csproj

# Without hot reload (always restart)
dotnet watch --project RealmForge/RealmForge.csproj --no-hot-reload

# Verbose output (debugging)
dotnet watch --project RealmForge/RealmForge.csproj --verbose
```

### Interactive Commands (while watch is running)
- `Ctrl+R` - Restart app manually
- `Ctrl+C` - Stop watch mode
- `Ctrl+Q` - Quit watch mode

### Environment Variables
```powershell
# Enable auto-restart on rude edits
$env:DOTNET_WATCH_RESTART_ON_RUDE_EDIT = "true"

# Suppress hot reload (testing)
$env:DOTNET_WATCH_SUPPRESS_LAUNCH_BROWSER = "true"

# Change polling interval (default: 1 second)
$env:DOTNET_WATCH_POLLING_INTERVAL = "500"
```

## Best Practices

### Development Workflow
1. **Start with watch mode** - Begin each session with watch task running
2. **Test incrementally** - Make small changes, test, repeat
3. **Plan major refactors** - Batch breaking changes to minimize restarts
4. **Use logging** - Add Serilog statements to debug without restarts

### Performance
- **Avoid unnecessary builds** - Don't save files repeatedly while typing
- **Exclude large files** - Use .gitignore patterns to skip watched files
- **Close unused projects** - Focus watch on active project only

### Team Collaboration
- **Document watch limitations** - Note which changes require restart
- **Share launch configs** - Commit .vscode/tasks.json
- **Standardize environment** - Ensure all devs use same .NET SDK version

## Additional Resources

- [.NET Hot Reload Documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-watch)
- [WPF Hot Reload Limitations](https://learn.microsoft.com/en-us/visualstudio/debugger/hot-reload)
- [XAML Hot Reload Guide](https://learn.microsoft.com/en-us/xamarin/xaml-hot-reload/)

## Comparison with Other Tools

| Feature | dotnet watch | Visual Studio Live | Rider Hot Reload |
|---------|--------------|-------------------|------------------|
| C# Hot Reload | ✅ | ✅ | ✅ |
| XAML Hot Reload | ⚠️ Limited | ✅ Full | ✅ Full |
| Auto-Restart | ✅ | ❌ | ✅ |
| CLI Support | ✅ | ❌ | ⚠️ Limited |
| Free | ✅ | ❌ (VS Pro+) | ❌ (Paid) |

For VS Code, `dotnet watch` with C# Dev Kit provides the best experience.
