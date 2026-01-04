# View API Changes Script
# Shows recent API changes from the auto-generated changelog

param(
    [string]$GodotProjectPath,
    [int]$Entries = 3
)

$ErrorActionPreference = "Stop"

if (-not $GodotProjectPath) {
    Write-Host "Usage: .\view-api-changes.ps1 -GodotProjectPath <path> [-Entries <count>]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Example:" -ForegroundColor Cyan
    Write-Host "  .\view-api-changes.ps1 -GodotProjectPath 'C:\Projects\MyGodotGame' -Entries 5" -ForegroundColor Gray
    exit 1
}

$ChangelogPath = Join-Path $GodotProjectPath "CHANGELOG_API.md"

if (-not (Test-Path $ChangelogPath)) {
    Write-Host "No API changelog found at:" -ForegroundColor Yellow
    Write-Host $ChangelogPath -ForegroundColor Gray
    Write-Host ""
    Write-Host "Run deploy-to-godot.ps1 first to generate the changelog." -ForegroundColor Cyan
    exit 1
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  API Changes Viewer" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

$ChangelogContent = Get-Content $ChangelogPath -Raw

# Extract recent deployment entries
$DeploymentPattern = "## Deployment - (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})"
$Matches = [regex]::Matches($ChangelogContent, $DeploymentPattern)

if ($Matches.Count -eq 0) {
    Write-Host "No deployment entries found in changelog." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($Matches.Count) deployment(s) in changelog" -ForegroundColor Green
Write-Host "Showing most recent $Entries:" -ForegroundColor Cyan
Write-Host ""

# Split content by deployment headers
$Sections = $ChangelogContent -split "## Deployment - "
$DeploymentSections = $Sections | Select-Object -Skip 1 | Select-Object -First $Entries

$DeploymentNumber = 1
foreach ($section in $DeploymentSections) {
    # Extract date/time
    if ($section -match "^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})") {
        $DeploymentDate = $Matches[1]
        
        Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
        Write-Host "Deployment #$DeploymentNumber - $DeploymentDate" -ForegroundColor Cyan
        Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
        Write-Host ""
        
        # Extract content until next separator
        $Content = ($section -split "---")[0]
        
        # Parse and colorize output
        $Lines = $Content -split "`n"
        foreach ($line in $Lines) {
            if ($line -match "^### (.+)$") {
                # File header
                Write-Host "  📄 $($Matches[1])" -ForegroundColor Yellow
            }
            elseif ($line -match "^\*\*Added \((\d+)\):\*\*") {
                Write-Host "    ➕ Added ($($Matches[1])):" -ForegroundColor Green
            }
            elseif ($line -match "^\*\*Removed \((\d+)\):\*\*") {
                Write-Host "    ➖ Removed ($($Matches[1])):" -ForegroundColor Red
            }
            elseif ($line -match "^\*\*Modified \((\d+)\):\*\*") {
                Write-Host "    📝 Modified ($($Matches[1])):" -ForegroundColor Cyan
            }
            elseif ($line -match "^- \[(.+?)\] ``(.+?)``") {
                # API member entry
                $MemberType = $Matches[1]
                $MemberName = $Matches[2]
                $Prefix = switch ($MemberType) {
                    "Type" { "  " }
                    "Method" { "  " }
                    "Property" { "  " }
                    "Field" { "  " }
                    "Event" { "  " }
                    default { "  " }
                }
                Write-Host "      $Prefix[$MemberType] $MemberName" -ForegroundColor Gray
            }
            elseif ($line -match "^- ~~\[(.+?)\] ``(.+?)``~~") {
                # Removed member (strikethrough)
                $MemberType = $Matches[1]
                $MemberName = $Matches[2]
                Write-Host "      [$MemberType] $MemberName" -ForegroundColor DarkGray
            }
            elseif ($line.Trim() -ne "" -and $line -notmatch "^$DeploymentDate") {
                # Other content
                Write-Host "    $line" -ForegroundColor Gray
            }
        }
        
        Write-Host ""
        $DeploymentNumber++
    }
}

Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor DarkGray
Write-Host ""
Write-Host "Full changelog: " -NoNewline
Write-Host $ChangelogPath -ForegroundColor Cyan
Write-Host ""
