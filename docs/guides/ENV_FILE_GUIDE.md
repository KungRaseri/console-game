# .env File Setup (Just Like TypeScript!)

## What is it?

The `.env` file works exactly like it does in TypeScript/Node.js projects. It stores environment variables that are automatically loaded when the app starts.

## How to Use

### 1. Edit the `.env` file

Open `.env` in the project root and add your MediatR license key:

```env
MEDIATR_LICENSE_KEY=your_actual_license_key_here
```

### 2. Run the game

```powershell
dotnet run --project Game
```

**That's it!** No scripts, no environment setup, no VS Code restart needed.

## How It Works

```csharp
// In Program.cs
using DotNetEnv;

// Loads .env file automatically
Env.Load();

// Now all variables from .env are available
var licenseKey = Environment.GetEnvironmentVariable("MEDIATR_LICENSE_KEY");
```

## Security

✅ `.env` is in `.gitignore` - won't be committed to source control
✅ Safe to store sensitive keys
✅ Different `.env` files for dev/prod/test environments

## Benefits vs Other Methods

| Method | Restart Required? | Easy to Update? | Like TypeScript? |
|--------|------------------|-----------------|------------------|
| **.env file** | ❌ No | ✅ Yes | ✅ Yes |
| PowerShell script | ❌ No | ⚠️ Medium | ❌ No |
| Environment variables | ✅ Yes | ❌ No | ❌ No |
| Hardcode in Program.cs | ❌ No | ❌ No | ❌ No |

## Multiple Environments

You can create different `.env` files:

```
.env.development
.env.production
.env.test
```

Then load the appropriate one:

```csharp
var envFile = args.Length > 0 ? $".env.{args[0]}" : ".env";
Env.Load(envFile);
```

Run with:
```powershell
dotnet run --project Game production
```

## Adding More Variables

Just add them to `.env`:

```env
# MediatR License
MEDIATR_LICENSE_KEY=your_key_here

# Optional: Custom settings
GAME_LOG_LEVEL=Debug
GAME_SAVE_PATH=./my-saves
GAME_MUSIC_VOLUME=0.7
```

Access in code:

```csharp
var logLevel = Environment.GetEnvironmentVariable("GAME_LOG_LEVEL") ?? "Information";
var savePath = Environment.GetEnvironmentVariable("GAME_SAVE_PATH") ?? "./saves";
var volume = float.Parse(Environment.GetEnvironmentVariable("GAME_MUSIC_VOLUME") ?? "1.0");
```

## Library Used

**DotNetEnv** (v3.1.1) - The .NET equivalent of `dotenv` in Node.js

- GitHub: https://github.com/tonerdo/dotnet-env
- NuGet: https://www.nuget.org/packages/DotNetEnv/

## Verification

Check if your `.env` file is loaded:

```powershell
# Run the game - it will show a warning if the key is missing
dotnet run --project Game

# Check logs (logs/game-*.txt) for:
[INFO] MediatR license key loaded from environment
```

## Troubleshooting

### `.env` file not found

Make sure `.env` is in the project root (same level as `Game/` folder):

```
console-game/
├── .env          ← Here!
├── Game/
│   └── Program.cs
└── README.md
```

### Variables not loading

1. Check file name is exactly `.env` (no `.txt` extension)
2. Check file format:
   ```env
   KEY=value
   ANOTHER_KEY=another_value
   ```
3. No spaces around `=`
4. No quotes needed (unless value has spaces)

### Want to use a different file name?

```csharp
Env.Load(".env.custom");
```

---

**Now you can manage environment variables just like in TypeScript!** 🎉
