# Generate Version Script
# Generates semantic version: [Major].[Minor].[Patch]-[git hash]
# Where Patch = commit count on current branch

param(
    [string]$OutputFormat = "string"  # "string", "env", or "msbuild"
)

$ErrorActionPreference = "Stop"

# Read Major.Minor from Directory.Build.props
$PropsFile = Join-Path $PSScriptRoot "..\Directory.Build.props"
if (-not (Test-Path $PropsFile)) {
    Write-Error "Directory.Build.props not found at: $PropsFile"
    exit 1
}

[xml]$Props = Get-Content $PropsFile
$VersionMajor = $Props.Project.PropertyGroup.VersionMajor
$VersionMinor = $Props.Project.PropertyGroup.VersionMinor

if (-not $VersionMajor -or -not $VersionMinor) {
    Write-Error "VersionMajor and VersionMinor must be set in Directory.Build.props"
    exit 1
}

# Get git commit count as patch version
try {
    $CommitCount = git rev-list --count HEAD 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Not a git repository or git not available, using patch version 0"
        $CommitCount = 0
    }
}
catch {
    Write-Warning "Failed to get git commit count, using patch version 0"
    $CommitCount = 0
}

# Get short git hash
try {
    $GitHash = git rev-parse --short=7 HEAD 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Failed to get git hash, using 'dev'"
        $GitHash = "dev"
    }
}
catch {
    Write-Warning "Failed to get git hash, using 'dev'"
    $GitHash = "dev"
}

# Build version string
$Version = "$VersionMajor.$VersionMinor.$CommitCount-$GitHash"
$VersionWithoutHash = "$VersionMajor.$VersionMinor.$CommitCount"
$FileVersion = "$VersionMajor.$VersionMinor.$CommitCount.0"

# Output based on format
switch ($OutputFormat) {
    "string" {
        Write-Output $Version
    }
    "env" {
        # For GitHub Actions environment variables
        Write-Output "VERSION=$Version"
        Write-Output "VERSION_NO_HASH=$VersionWithoutHash"
        Write-Output "FILE_VERSION=$FileVersion"
        Write-Output "VERSION_MAJOR=$VersionMajor"
        Write-Output "VERSION_MINOR=$VersionMinor"
        Write-Output "VERSION_PATCH=$CommitCount"
        Write-Output "GIT_HASH=$GitHash"
    }
    "msbuild" {
        # For MSBuild properties
        Write-Output "/p:Version=$Version"
        Write-Output "/p:AssemblyVersion=$VersionWithoutHash"
        Write-Output "/p:FileVersion=$FileVersion"
        Write-Output "/p:InformationalVersion=$Version"
    }
    default {
        Write-Error "Unknown output format: $OutputFormat. Use 'string', 'env', or 'msbuild'"
        exit 1
    }
}
