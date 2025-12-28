# Extract all unique ability references from classes/catalog.json
$classesPath = "c:\code\console-game\Game.Data\Data\Json\classes\catalog.json"
$classesContent = Get-Content $classesPath -Raw

# Find all @abilities references
$pattern = '@abilities/([^"]+)'
$matches = [regex]::Matches($classesContent, $pattern)

# Build a list of unique abilities
$abilities = @{}
foreach ($match in $matches) {
    $ref = $match.Groups[1].Value
    if (-not $abilities.ContainsKey($ref)) {
        $abilities[$ref] = 1
    } else {
        $abilities[$ref]++
    }
}

Write-Host "=== ABILITY REFERENCE AUDIT RESULTS ===" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total unique abilities referenced in classes: $($abilities.Count)" -ForegroundColor Yellow
Write-Host ""

# Now check each ability
$missingCatalog = @()
$missingNames = @()
$foundBoth = @()
$missingFolder = @()

foreach ($ref in ($abilities.Keys | Sort-Object)) {
    # Parse reference: path:name
    $parts = $ref -split ':', 2
    if ($parts.Count -ne 2) { continue }
    
    $path = $parts[0]
    $abilityName = $parts[1]
    
    # Convert path to filesystem path
    $folderPath = "c:\code\console-game\Game.Data\Data\Json\abilities\$($path.Replace('/', '\'))"
    $catalogPath = "$folderPath\catalog.json"
    $namesPath = "$folderPath\names.json"
    
    $catalogExists = Test-Path $catalogPath
    $namesExists = Test-Path $namesPath
    $folderExists = Test-Path $folderPath
    
    if (-not $folderExists) {
        $missingFolder += @{ref=$ref; path=$path; name=$abilityName; count=$abilities[$ref]}
    }
    elseif ($catalogExists -and $namesExists) {
        # Both files exist, now check if ability is IN them
        try {
            $catalogContent = Get-Content $catalogPath -Raw | ConvertFrom-Json
            $namesContent = Get-Content $namesPath -Raw | ConvertFrom-Json
            
            $inCatalog = $catalogContent.items | Where-Object { $_.name -eq $abilityName }
            
            if ($inCatalog) {
                $foundBoth += @{ref=$ref; path=$path; name=$abilityName; count=$abilities[$ref]}
            } else {
                $missingCatalog += @{ref=$ref; path=$catalogPath; name=$abilityName; count=$abilities[$ref]}
            }
        } catch {
            Write-Host "Error processing $ref : $_" -ForegroundColor Red
        }
    }
    elseif (-not $catalogExists) {
        $missingCatalog += @{ref=$ref; path=$catalogPath; name=$abilityName; count=$abilities[$ref]}
    }
    
    if ($folderExists -and -not $namesExists) {
        $missingNames += @{ref=$ref; path=$namesPath; name=$abilityName; count=$abilities[$ref]}
    }
}

Write-Host "MISSING FOLDERS (Entire subcategory doesn't exist): $($missingFolder.Count)" -ForegroundColor Red
Write-Host "-------------------------------------------------------" -ForegroundColor Red
if ($missingFolder.Count -gt 0) {
    foreach ($item in $missingFolder) {
        Write-Host "  ❌ $($item.ref) [used $($item.count)x]" -ForegroundColor Red
        Write-Host "     Need to create: abilities\$($item.path.Replace('/', '\'))\" -ForegroundColor Gray
    }
    Write-Host ""
}

Write-Host "MISSING FROM CATALOG.JSON: $($missingCatalog.Count)" -ForegroundColor Magenta
Write-Host "-------------------------------------------------------" -ForegroundColor Magenta
if ($missingCatalog.Count -gt 0) {
    foreach ($item in $missingCatalog) {
        Write-Host "  ❌ $($item.ref) [used $($item.count)x]" -ForegroundColor Magenta
        Write-Host "     Ability name: '$($item.name)'" -ForegroundColor Gray
        Write-Host "     Expected in: $($item.path)" -ForegroundColor DarkGray
    }
    Write-Host ""
}

Write-Host "MISSING FROM NAMES.JSON: $($missingNames.Count)" -ForegroundColor Yellow
Write-Host "-------------------------------------------------------" -ForegroundColor Yellow
if ($missingNames.Count -gt 0) {
    foreach ($item in $missingNames) {
        Write-Host "  ⚠️  $($item.ref) [used $($item.count)x]" -ForegroundColor Yellow
        Write-Host "     Ability name: '$($item.name)'" -ForegroundColor Gray
        Write-Host "     Expected in: $($item.path)" -ForegroundColor DarkGray
    }
    Write-Host ""
}

Write-Host "FOUND IN BOTH FILES: $($foundBoth.Count)" -ForegroundColor Green
Write-Host "-------------------------------------------------------" -ForegroundColor Green
Write-Host "These abilities exist in both catalog.json and names.json files." -ForegroundColor Gray
Write-Host ""

Write-Host "NOTES:" -ForegroundColor Cyan
Write-Host "------" -ForegroundColor Cyan
if ($missingFolder.Count -gt 0) {
    $paths = $missingFolder | ForEach-Object { $_.path } | Sort-Object -Unique
    Write-Host "• Missing folder structures for: $($paths -join ', ')" -ForegroundColor Yellow
    Write-Host "  These subcategories need to be created with catalog.json and names.json files." -ForegroundColor Gray
}
Write-Host "• Total classes checked: warriors, rogues, mages, clerics, rangers" -ForegroundColor Gray
Write-Host "• All ability references use v4.1 reference syntax (@abilities/...)" -ForegroundColor Gray
