# Deploy to Godot Project Script
# Copies packaged game files to Godot project directory

param(
    [Parameter(Mandatory=$true)]
    [string]$GodotProjectPath,
    
    [string]$PackagePath = "package"
)

$ErrorActionPreference = "Stop"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Deploy to Godot Project" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Validate paths
$SolutionRoot = Split-Path $PSScriptRoot -Parent
$PackageRoot = Join-Path $SolutionRoot $PackagePath

if (-not (Test-Path $PackageRoot)) {
    Write-Error "Package not found at: $PackageRoot"
    Write-Host "Run build-game-package.ps1 first!" -ForegroundColor Yellow
    exit 1
}

if (-not (Test-Path $GodotProjectPath)) {
    Write-Error "Godot project not found at: $GodotProjectPath"
    exit 1
}

# Check for project.godot
$GodotProjectFile = Join-Path $GodotProjectPath "project.godot"
if (-not (Test-Path $GodotProjectFile)) {
    Write-Error "Not a valid Godot project (project.godot not found)"
    exit 1
}

Write-Host "Source Package: $PackageRoot" -ForegroundColor Gray
Write-Host "Target Godot:   $GodotProjectPath" -ForegroundColor Gray
Write-Host ""

# Read package manifest
$ManifestPath = Join-Path $PackageRoot "package-manifest.json"
if (Test-Path $ManifestPath) {
    $Manifest = Get-Content $ManifestPath | ConvertFrom-Json
    Write-Host "Package Date: $($Manifest.PackageDate)" -ForegroundColor Gray
    Write-Host "Configuration: $($Manifest.Configuration)" -ForegroundColor Gray
    Write-Host ""
}

# ============================================
# 1. Deploy Game Libraries
# ============================================
Write-Host "Deploying game libraries..." -ForegroundColor Yellow

$LibrariesSource = Join-Path $PackageRoot "Libraries"
$LibrariesDest = Join-Path $GodotProjectPath "Libraries"

if (Test-Path $LibrariesDest) {
    Write-Host "Removing existing Libraries folder..." -ForegroundColor Gray
    $retryCount = 0
    $maxRetries = 3
    while ($retryCount -lt $maxRetries) {
        try {
            Remove-Item $LibrariesDest -Recurse -Force -ErrorAction Stop
            break
        }
        catch {
            $retryCount++
            if ($retryCount -ge $maxRetries) {
                Write-Host ""
                Write-Host "ERROR: Cannot remove Libraries folder - files are in use!" -ForegroundColor Red
                Write-Host ""
                Write-Host "Please close the following applications and try again:" -ForegroundColor Yellow
                Write-Host "  • Godot Editor" -ForegroundColor Yellow
                Write-Host "  • Any RealmForge instances" -ForegroundColor Yellow
                Write-Host "  • Any applications referencing the game DLLs" -ForegroundColor Yellow
                Write-Host ""
                
                # Try to identify locked processes
                $godotProcesses = Get-Process | Where-Object { $_.ProcessName -match 'Godot' }
                if ($godotProcesses) {
                    Write-Host "Detected Godot processes:" -ForegroundColor Cyan
                    $godotProcesses | ForEach-Object { Write-Host "  • PID $($_.Id): $($_.ProcessName)" -ForegroundColor Gray }
                    Write-Host ""
                }
                
                exit 1
            }
            Write-Host "Retrying removal... ($retryCount/$maxRetries)" -ForegroundColor Yellow
            Start-Sleep -Seconds 1
        }
    }
}

Copy-Item -Path $LibrariesSource -Destination $LibrariesDest -Recurse -Force

# Copy XML documentation files to Libraries root for IntelliSense (need explicit copy after folder creation)
Write-Host "Copying XML documentation files..." -ForegroundColor Gray
$XmlFiles = Get-ChildItem -Path $LibrariesSource -Filter "*.xml"
if ($XmlFiles.Count -eq 0) {
    Write-Host "  ⚠ No XML files found in package!" -ForegroundColor Yellow
} else {
    foreach ($xmlFile in $XmlFiles) {
        try {
            Copy-Item -Path $xmlFile.FullName -Destination $LibrariesDest -Force -ErrorAction Stop
            Write-Host "  → Copied $($xmlFile.Name)" -ForegroundColor Gray
        }
        catch {
            Write-Host "  ✗ Failed to copy $($xmlFile.Name): $_" -ForegroundColor Red
        }
    }
}

$DllCount = (Get-ChildItem -Path $LibrariesDest -Recurse -Filter "*.dll").Count
$XmlCount = (Get-ChildItem -Path $LibrariesDest -Filter "*.xml").Count
$PdbCount = (Get-ChildItem -Path $LibrariesDest -Recurse -Filter "*.pdb").Count

Write-Host "✓ Deployed $DllCount DLL files" -ForegroundColor Green
if ($XmlCount -gt 0) {
    Write-Host "✓ Deployed $XmlCount XML documentation files (IntelliSense support)" -ForegroundColor Green
} else {
    Write-Host "⚠ No XML documentation files deployed" -ForegroundColor Yellow
}
if ($PdbCount -gt 0) {
    Write-Host "✓ Deployed $PdbCount PDB symbol files (debugging support)" -ForegroundColor Green
}
Write-Host ""
Write-Host ""

# ============================================
# 2. Deploy JSON Data
# ============================================
Write-Host "Deploying JSON data..." -ForegroundColor Yellow

$JsonSource = Join-Path $PackageRoot "Data\Json"
$JsonDest = Join-Path $GodotProjectPath "Data\Json"

if (Test-Path $JsonDest) {
    Remove-Item $JsonDest -Recurse -Force
}

Copy-Item -Path $JsonSource -Destination $JsonDest -Recurse -Force

$JsonFileCount = (Get-ChildItem -Path $JsonDest -Recurse -Filter "*.json").Count
$CbconfigCount = (Get-ChildItem -Path $JsonDest -Recurse -Filter "*.cbconfig.json").Count

Write-Host "✓ Deployed $JsonFileCount JSON data files" -ForegroundColor Green
if ($CbconfigCount -gt 0) {
    Write-Host "  → Including $CbconfigCount RealmForge config files" -ForegroundColor Gray
}
Write-Host ""

# ============================================
# 3. Deploy RealmForge (Optional)
# ============================================
$DeployRealmForge = Read-Host "Deploy RealmForge to Godot project? (y/N)"

if ($DeployRealmForge -eq "y" -or $DeployRealmForge -eq "Y") {
    Write-Host "Deploying RealmForge..." -ForegroundColor Yellow
    
    $RealmForgeSource = Join-Path $PackageRoot "RealmForge"
    $RealmForgeDest = Join-Path $GodotProjectPath "Tools\RealmForge"
    
    if (Test-Path $ContentBuilderDest) {
        Remove-Item $ContentBuilderDest -Recurse -Force
    }
    
    New-Item -ItemType Directory -Path $ContentBuilderDest -Force | Out-Null
    Copy-Item -Path "$ContentBuilderSource\*" -Destination $ContentBuilderDest -Recurse -Force

    $ExeCount = (Get-ChildItem -Path $ContentBuilderDest -Filter "*.exe" -Recurse).Count
    $DllCount = (Get-ChildItem -Path $ContentBuilderDest -Filter "*.dll" -Recurse).Count
    
    Write-Host "✓ RealmForge deployed to Tools/RealmForge" -ForegroundColor Green
    Write-Host "  → $ExeCount executable, $DllCount DLLs" -ForegroundColor Gray
    Write-Host ""
}

# ============================================
# 4. Generate Deployment Info
# ============================================
Write-Host "Generating deployment info..." -ForegroundColor Yellow

$DeploymentInfo = @{
    DeploymentDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    SourcePackage = $PackageRoot
    GodotProject = $GodotProjectPath
    LibrariesDeployed = $DllCount
    JsonFilesDeployed = $JsonFileCount
}

$DeploymentJson = $DeploymentInfo | ConvertTo-Json -Depth 10
$DeploymentPath = Join-Path $GodotProjectPath ".deployment-info.json"
Set-Content -Path $DeploymentPath -Value $DeploymentJson

Write-Host "Deployment info saved" -ForegroundColor Green
Write-Host ""

# ============================================
# 5. Summary
# ============================================
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Deployment Complete!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Deployed to: " -NoNewline -ForegroundColor Yellow
Write-Host $GodotProjectPath -ForegroundColor White
Write-Host ""
Write-Host "Components:" -ForegroundColor Yellow
Write-Host "  - $DllCount DLL files in Libraries" -ForegroundColor Green
Write-Host "  - $JsonFileCount JSON files in Data/Json" -ForegroundColor Green
if ($DeployContentBuilder -eq "y" -or $DeployContentBuilder -eq "Y") {
    Write-Host "  - ContentBuilder in Tools/ContentBuilder" -ForegroundColor Green
}
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Cyan
Write-Host "  1. Open Godot project" -ForegroundColor Gray
Write-Host "  2. Build CSharp solution in Godot (Ctrl+B)" -ForegroundColor Gray
Write-Host "  3. Reference game DLLs in your GDScript or CSharp code" -ForegroundColor Gray
Write-Host ""
