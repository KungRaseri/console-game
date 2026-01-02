# Fix Social Domain References
# Automatically maps dialogue/greeting/farewell/style references to correct subcategories

$dataRoot = "c:\code\console-game\RealmEngine.Data\Data\Json"

# Build mapping from catalogs
$mappings = @{
    greetings = @{}
    farewells = @{}
    styles = @{}
}

Write-Host "Building mappings from catalogs..." -ForegroundColor Cyan

# Parse greetings catalog
$greetingsPath = "$dataRoot\social\dialogue\greetings\catalog.json"
$greetingsJson = Get-Content $greetingsPath -Raw | ConvertFrom-Json
foreach ($type in $greetingsJson.greeting_types.PSObject.Properties) {
    $subcategory = $type.Name
    foreach ($item in $type.Value.items) {
        $mappings.greetings[$item.name] = $subcategory
        Write-Host "  greetings: $($item.name) -> $subcategory" -ForegroundColor Gray
    }
}

# Parse farewells catalog
$farewellsPath = "$dataRoot\social\dialogue\farewells\catalog.json"
$farewellsJson = Get-Content $farewellsPath -Raw | ConvertFrom-Json
# Farewells uses different structure: noble_types.noble.items, scholarly_types.scholarly.items, etc.
foreach ($type in $farewellsJson.PSObject.Properties) {
    if ($type.Name -match '_types$') {
        foreach ($subtype in $type.Value.PSObject.Properties) {
            $subcategory = $subtype.Name
            if ($subtype.Value.items) {
                foreach ($item in $subtype.Value.items) {
                    $mappings.farewells[$item.name] = $subcategory
                    Write-Host "  farewells: $($item.name) -> $subcategory" -ForegroundColor Gray
                }
            }
        }
    }
}

# Parse styles catalog
$stylesPath = "$dataRoot\social\dialogue\styles\catalog.json"
$stylesJson = Get-Content $stylesPath -Raw | ConvertFrom-Json
foreach ($type in $stylesJson.style_types.PSObject.Properties) {
    $subcategory = $type.Name
    foreach ($item in $type.Value.items) {
        $mappings.styles[$item.name] = $subcategory
        Write-Host "  styles: $($item.name) -> $subcategory" -ForegroundColor Gray
    }
}

Write-Host "`nMappings built. Fixing references..." -ForegroundColor Cyan
Write-Host "  Greetings: $($mappings.greetings.Count) items"
Write-Host "  Farewells: $($mappings.farewells.Count) items"
Write-Host "  Styles: $($mappings.styles.Count) items"

# Find all JSON files
$jsonFiles = Get-ChildItem "$dataRoot" -Recurse -Filter "*.json"
$totalFixed = 0

foreach ($file in $jsonFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    $fileFixed = 0
    
    # Fix greetings references
    foreach ($itemName in $mappings.greetings.Keys) {
        $subcategory = $mappings.greetings[$itemName]
        $oldPattern = "@social/dialogue/greetings:" + $itemName
        $newPattern = "@social/dialogue/greetings/$subcategory" + ":" + $itemName
        if ($content -match [regex]::Escape($oldPattern)) {
            $content = $content -replace [regex]::Escape($oldPattern), $newPattern
            $fileFixed++
        }
    }
    
    # Fix farewells references
    foreach ($itemName in $mappings.farewells.Keys) {
        $subcategory = $mappings.farewells[$itemName]
        $oldPattern = "@social/dialogue/farewells:" + $itemName
        $newPattern = "@social/dialogue/farewells/$subcategory" + ":" + $itemName
        if ($content -match [regex]::Escape($oldPattern)) {
            $content = $content -replace [regex]::Escape($oldPattern), $newPattern
            $fileFixed++
        }
    }
    
    # Fix styles references
    foreach ($itemName in $mappings.styles.Keys) {
        $subcategory = $mappings.styles[$itemName]
        $oldPattern = "@social/dialogue/styles:" + $itemName
        $newPattern = "@social/dialogue/styles/$subcategory" + ":" + $itemName
        if ($content -match [regex]::Escape($oldPattern)) {
            $content = $content -replace [regex]::Escape($oldPattern), $newPattern
            $fileFixed++
        }
    }
    
    # Fix schedules references (simpler - just add wildcard)
    $content = $content -replace '@social/schedules:(\w+)','@social/schedules/*:$1'
    
    # Save if changed
    if ($content -ne $originalContent) {
        Set-Content $file.FullName $content -NoNewline
        Write-Host "  Fixed $fileFixed references in: $($file.Name)" -ForegroundColor Green
        $totalFixed += $fileFixed
    }
}

Write-Host "`nComplete! Fixed $totalFixed total references." -ForegroundColor Green
