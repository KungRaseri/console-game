# Build and Package Script for Godot Integration
param(
    [string]$Configuration = "Release",
    [string]$OutputPath = "package"
)

$ErrorActionPreference = "Stop"

Write-Output "======================================"
Write-Output "  Game Package Build Script"
Write-Output "======================================"
Write-Output ""

# Get solution root
$SolutionRoot = Split-Path $PSScriptRoot -Parent
$PackageRoot = Join-Path $SolutionRoot $OutputPath

# Clean output directory
Write-Output "Cleaning output directory..."
if (Test-Path $PackageRoot) {
    Remove-Item $PackageRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $PackageRoot | Out-Null

# Create package structure
$Directories = @(
    "Libraries",
    "ContentBuilder",
    "Data\Json"
)

foreach ($dir in $Directories) {
    New-Item -ItemType Directory -Path (Join-Path $PackageRoot $dir) -Force | Out-Null
}

Write-Output "Package output: $PackageRoot"
Write-Output ""

# Build Game.Core
Write-Output "Building Game.Core..."
$CoreOutput = Join-Path $PackageRoot "Libraries\Game.Core"
dotnet publish (Join-Path $SolutionRoot "Game.Core\Game.Core.csproj") --configuration $Configuration --output $CoreOutput --no-self-contained --verbosity quiet
if ($LASTEXITCODE -ne 0) { Write-Error "Game.Core build failed!"; exit 1 }
Write-Output "[OK] Game.Core published"
Write-Output ""

# Build Game.Shared
Write-Output "Building Game.Shared..."
$SharedOutput = Join-Path $PackageRoot "Libraries\Game.Shared"
dotnet publish (Join-Path $SolutionRoot "Game.Shared\Game.Shared.csproj") --configuration $Configuration --output $SharedOutput --no-self-contained --verbosity quiet
if ($LASTEXITCODE -ne 0) { Write-Error "Game.Shared build failed!"; exit 1 }
Write-Output "[OK] Game.Shared published"
Write-Output ""

# Build Game.Data
Write-Output "Building Game.Data..."
$DataOutput = Join-Path $PackageRoot "Libraries\Game.Data"
dotnet publish (Join-Path $SolutionRoot "Game.Data\Game.Data.csproj") --configuration $Configuration --output $DataOutput --no-self-contained --verbosity quiet
if ($LASTEXITCODE -ne 0) { Write-Error "Game.Data build failed!"; exit 1 }
Write-Output "[OK] Game.Data published"
Write-Output ""

# Build ContentBuilder
Write-Output "Building ContentBuilder..."
$ContentBuilderOutput = Join-Path $PackageRoot "ContentBuilder"
dotnet publish (Join-Path $SolutionRoot "Game.ContentBuilder\Game.ContentBuilder.csproj") --configuration $Configuration --output $ContentBuilderOutput --no-self-contained --runtime win-x64 --verbosity quiet
if ($LASTEXITCODE -ne 0) { Write-Error "ContentBuilder build failed!"; exit 1 }

# Remove duplicate Data folder from ContentBuilder (it will reference package root Data)
$ContentBuilderDataPath = Join-Path $ContentBuilderOutput "Data"
if (Test-Path $ContentBuilderDataPath) {
    Remove-Item $ContentBuilderDataPath -Recurse -Force
    Write-Output "[CLEANUP] Removed duplicate Data folder from ContentBuilder"
}

# Create ContentBuilder config pointing to package Data location
$ContentBuilderConfig = @{
    DataPath = "..\Data\Json"
    Description = "ContentBuilder will use Data\Json at package root"
} | ConvertTo-Json
Set-Content -Path (Join-Path $ContentBuilderOutput "contentbuilder.config.json") -Value $ContentBuilderConfig

Write-Output "[OK] ContentBuilder published"
Write-Output ""

# Copy JSON Data Files
Write-Output "Copying JSON data files..."
$JsonSource = Join-Path $SolutionRoot "Game.Data\Data\Json"
$JsonDest = Join-Path $PackageRoot "Data\Json"
Copy-Item -Path "$JsonSource\*" -Destination $JsonDest -Recurse -Force
$JsonFileCount = (Get-ChildItem -Path $JsonDest -Recurse -File).Count
Write-Output "[OK] Copied $JsonFileCount JSON files"
Write-Output ""

# Generate Package Manifest
Write-Output "Generating package manifest..."
$Manifest = @{
    PackageDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Configuration = $Configuration
    Components = @{
        GameCore = @{
            Path = "Libraries\Game.Core"
            Assembly = "Game.Core.dll"
        }
        GameShared = @{
            Path = "Libraries\Game.Shared"
            Assembly = "Game.Shared.dll"
        }
        GameData = @{
            Path = "Libraries\Game.Data"
            Assembly = "Game.Data.dll"
        }
        ContentBuilder = @{
            Path = "ContentBuilder"
            Executable = "Game.ContentBuilder.exe"
        }
        JsonData = @{
            Path = "Data\Json"
            FileCount = $JsonFileCount
        }
    }
}

$ManifestJson = $Manifest | ConvertTo-Json -Depth 10
$ManifestPath = Join-Path $PackageRoot "package-manifest.json"
Set-Content -Path $ManifestPath -Value $ManifestJson
Write-Output "[OK] Package manifest generated"
Write-Output ""

# Summary
Write-Output "======================================"
Write-Output "  Build Complete!"
Write-Output "======================================"
Write-Output ""
Write-Output "Package Location: $PackageRoot"
Write-Output ""
Write-Output "Package Contents:"
Write-Output "  - Libraries\Game.Core\      (Core game mechanics)"
Write-Output "  - Libraries\Game.Shared\    (Shared models/services)"
Write-Output "  - Libraries\Game.Data\      (Data loading)"
Write-Output "  - ContentBuilder\           (JSON editor application)"
Write-Output "  - Data\Json\                (Game data: $JsonFileCount files)"
Write-Output ""
Write-Output "Next: Run deploy-to-godot.ps1 to copy to Godot project"
Write-Output ""
