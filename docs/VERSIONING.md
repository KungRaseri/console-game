# Automatic Versioning System

**Status**: ✅ Implemented (January 1, 2026)

## Overview

The build system automatically generates semantic versions for all libraries and applications using git commit history.

**Version Format**: `[Major].[Minor].[Patch]-[git hash]`

Example: `0.1.323-9318b2b`

## Version Components

- **Major.Minor**: Set manually in `Directory.Build.props`
- **Patch**: Auto-calculated from git commit count
- **Git Hash**: 7-character short SHA from current HEAD

## Configuration

### Setting Major.Minor

Edit [`Directory.Build.props`](../Directory.Build.props) in solution root:

```xml
<PropertyGroup>
  <VersionMajor>0</VersionMajor>
  <VersionMinor>1</VersionMinor>
</PropertyGroup>
```

### Version Generation Script

[`scripts/generate-version.ps1`](../scripts/generate-version.ps1) generates versions with 3 output formats:

**String Format** (default):
```powershell
.\scripts\generate-version.ps1
# Output: 0.1.323-9318b2b
```

**Environment Variables Format**:
```powershell
.\scripts\generate-version.ps1 -OutputFormat env
# Output:
# VERSION=0.1.323-9318b2b
# VERSION_NO_HASH=0.1.323
# FILE_VERSION=0.1.323.0
# VERSION_MAJOR=0
# VERSION_MINOR=1
# VERSION_PATCH=323
# GIT_HASH=9318b2b
```

**MSBuild Properties Format**:
```powershell
.\scripts\generate-version.ps1 -OutputFormat msbuild
# Output:
# /p:Version=0.1.323-9318b2b
# /p:AssemblyVersion=0.1.323
# /p:FileVersion=0.1.323.0
# /p:InformationalVersion=0.1.323-9318b2b
```

## Build Integration

### Local Builds

[`scripts/build-game-package.ps1`](../scripts/build-game-package.ps1) automatically:

1. Generates version from git history
2. Applies to all `dotnet publish` commands
3. Includes version in `package-manifest.json`

**Usage**:
```powershell
.\scripts\build-game-package.ps1
```

Output shows version:
```
Building version: 0.1.323-9318b2b
```

### CI/CD Pipeline

[`.github/workflows/build-and-release.yml`](../.github/workflows/build-and-release.yml) uses versioning in:

#### Build Package Job
```yaml
- name: Build and package with script
  run: |
    .\build-game-package.ps1 -Configuration Release
    $manifest = Get-Content "package/package-manifest.json" | ConvertFrom-Json
    $version = $manifest.Version
    Write-Output "version=$version" >> $env:GITHUB_OUTPUT
```

#### Publish NuGet Job
```yaml
- name: Generate version
  run: |
    $version = .\scripts\generate-version.ps1 -OutputFormat "string"
    Write-Output "VERSION=$version" >> $env:GITHUB_ENV
    
- name: Pack RealmEngine.Core
  run: dotnet pack RealmEngine.Core/RealmEngine.Core.csproj -p:PackageVersion=${{ env.VERSION }}
```

## Version Metadata

Versions are embedded in multiple locations:

### 1. DLL Assembly Metadata

```powershell
(Get-Item .\package\Libraries\RealmEngine.Core\RealmEngine.Core.dll).VersionInfo

FileVersion      : 0.1.323.0
ProductVersion   : 0.1.323-9318b2b+[full-git-hash]
```

Properties set:
- **Version**: Full semver with hash (`0.1.323-9318b2b`)
- **AssemblyVersion**: Numeric only (`0.1.323`)
- **FileVersion**: 4-part version (`0.1.323.0`)
- **InformationalVersion**: Full semver with hash

### 2. Package Manifest

`package/package-manifest.json`:
```json
{
  "Version": "0.1.323-9318b2b",
  "PackageDate": "2026-01-01 11:06:07",
  "Configuration": "Release",
  "Components": { ... }
}
```

### 3. NuGet Package

NuGet packages include version in:
- Package filename: `RealmEngine.Core.0.1.323-9318b2b.nupkg`
- Package metadata: `<version>0.1.323-9318b2b</version>`

## Version Bumping Strategy

### Patch Version (Automatic)
**Increments on every commit** via git commit count.

No action required.

### Minor Version (Manual)
**Increment for new features**, non-breaking changes:

1. Edit `Directory.Build.props`:
   ```xml
   <VersionMinor>2</VersionMinor>
   ```
2. Commit change
3. Build automatically uses new version

### Major Version (Manual)
**Increment for breaking changes**:

1. Edit `Directory.Build.props`:
   ```xml
   <VersionMajor>1</VersionMajor>
   <VersionMinor>0</VersionMinor>
   ```
2. Commit change
3. Build automatically uses new version

## Advantages

✅ **Unique versions**: Every commit gets unique version  
✅ **Traceable**: Git hash links version to exact commit  
✅ **Automatic**: No manual version updates needed  
✅ **Consistent**: Same version across all projects in solution  
✅ **CI/CD ready**: Works in GitHub Actions without tags  
✅ **NuGet compatible**: Follows semver standards  

## Non-Git Environments

If git is unavailable (e.g., extracted source):
- Patch version defaults to `0`
- Git hash defaults to `"dev"`
- Result: `0.1.0-dev`

## Querying Versions

### From Package
```powershell
$manifest = Get-Content package/package-manifest.json | ConvertFrom-Json
$manifest.Version
```

### From DLL
```powershell
(Get-Item package/Libraries/RealmEngine.Core/RealmEngine.Core.dll).VersionInfo.ProductVersion
```

### From Build Script
```powershell
.\scripts\generate-version.ps1
```

## Related Files

- [`Directory.Build.props`](../Directory.Build.props) - Version configuration
- [`scripts/generate-version.ps1`](../scripts/generate-version.ps1) - Version generator
- [`scripts/build-game-package.ps1`](../scripts/build-game-package.ps1) - Build integration
- [`.github/workflows/build-and-release.yml`](../.github/workflows/build-and-release.yml) - CI/CD integration

## Future Enhancements

Consider:
- [ ] Version validation in CI (ensure Major.Minor set correctly)
- [ ] Changelog generation from commit messages
- [ ] Pre-release suffixes for feature branches (`0.1.323-feature-xyz.9318b2b`)
- [ ] Version display in ContentBuilder About dialog
