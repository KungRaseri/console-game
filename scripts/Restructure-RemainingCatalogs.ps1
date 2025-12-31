# Restructure quests, styles, and greetings catalogs to match standard pattern
# Pattern: *_types -> category -> items

$ErrorActionPreference = "Stop"

Write-Host "Restructuring catalogs to standard pattern..." -ForegroundColor Cyan

# ===== QUESTS CATALOG =====
Write-Host "`n1. Processing quests catalog..." -ForegroundColor Yellow
$questsPath = "c:\code\console-game\Game.Data\Data\Json\quests\catalog.json"
$quests = Get-Content $questsPath -Raw | ConvertFrom-Json

# Flatten the 4-level structure to 3-level
# Old: templates_types -> fetch -> easy_fetch -> items
# New: quest_types -> fetch -> items (combining all difficulties)
$questTypes = @{}

foreach ($questTypeProp in $quests.templates_types.PSObject.Properties) {
    $questTypeName = $questTypeProp.Name
    $questTypeValue = $questTypeProp.Value
    
    $allItems = @()
    foreach ($difficultyProp in $questTypeValue.PSObject.Properties) {
        if ($difficultyProp.Value.items) {
            $allItems += $difficultyProp.Value.items
        }
    }
    
    $questTypes[$questTypeName] = @{
        items = $allItems
    }
}

# Remove old property and add new
$quests.PSObject.Properties.Remove('templates_types')
$quests | Add-Member -MemberType NoteProperty -Name 'quest_types' -Value $questTypes

# Clean up metadata
$quests.metadata.PSObject.Properties.Remove('locations_migrated_to')
$quests.metadata.PSObject.Properties.Remove('quest_types')
$quests.metadata.PSObject.Properties.Remove('difficulty_levels')
if ($quests.metadata.PSObject.Properties['componentKeys']) {
    $quests.metadata.componentKeys = @("quest_types")
} else {
    $quests.metadata | Add-Member -MemberType NoteProperty -Name 'componentKeys' -Value @("quest_types")
}

Write-Host "  Restructured: templates_types -> quest_types with flattened categories" -ForegroundColor Green

# Save
$quests | ConvertTo-Json -Depth 100 | Set-Content $questsPath -Encoding UTF8

# ===== STYLES CATALOG =====
Write-Host "`n2. Processing social/dialogue/styles catalog..." -ForegroundColor Yellow
$stylesPath = "c:\code\console-game\Game.Data\Data\Json\social\dialogue\styles\catalog.json"
$styles = Get-Content $stylesPath -Raw | ConvertFrom-Json

# Convert flat structure to items arrays
# Old: formal_types -> scholarly (object)
# New: formal_types -> formal -> items (array)
$newStyleTypes = @{
    formal = @{ items = @() }
    casual = @{ items = @() }
    mystical = @{ items = @() }
    gruff = @{ items = @() }
}

foreach ($prop in $styles.PSObject.Properties) {
    if ($prop.Name -eq "metadata") { continue }
    
    $typeName = $prop.Name -replace '_types$', ''
    $items = @()
    
    foreach ($styleProp in $prop.Value.PSObject.Properties) {
        $items += $styleProp.Value
    }
    
    # Group into categories
    if ($typeName -eq "formal") {
        $newStyleTypes.formal.items = $items
    } elseif ($typeName -in @("casual", "friendly")) {
        $newStyleTypes.casual.items += $items
    } elseif ($typeName -in @("mystical", "spiritual")) {
        $newStyleTypes.mystical.items += $items
    } else {
        $newStyleTypes.gruff.items += $items
    }
}

# Remove old properties
$propsToRemove = @($styles.PSObject.Properties | Where-Object { $_.Name -ne "metadata" } | ForEach-Object { $_.Name })
foreach ($prop in $propsToRemove) {
    $styles.PSObject.Properties.Remove($prop)
}

# Add new structure
$styles | Add-Member -MemberType NoteProperty -Name 'style_types' -Value $newStyleTypes
if ($styles.metadata.PSObject.Properties['componentKeys']) {
    $styles.metadata.componentKeys = @("style_types")
} else {
    $styles.metadata | Add-Member -MemberType NoteProperty -Name 'componentKeys' -Value @("style_types")
}

Write-Host "  Restructured: multiple *_types -> style_types with categorized items" -ForegroundColor Green

# Save
$styles | ConvertTo-Json -Depth 100 | Set-Content $stylesPath -Encoding UTF8

# ===== GREETINGS CATALOG =====
Write-Host "`n3. Processing social/dialogue/greetings catalog..." -ForegroundColor Yellow
$greetingsPath = "c:\code\console-game\Game.Data\Data\Json\social\dialogue\greetings\catalog.json"
$greetings = Get-Content $greetingsPath -Raw | ConvertFrom-Json

# Convert flat structure to items arrays
# Old: noble_types -> noble-formal (object)
# New: greeting_types -> noble -> items (array)
$newGreetingTypes = @{}

foreach ($prop in $greetings.PSObject.Properties) {
    if ($prop.Name -eq "metadata") { continue }
    
    $categoryName = $prop.Name -replace '_types$', ''
    $items = @()
    
    foreach ($greetingProp in $prop.Value.PSObject.Properties) {
        $items += $greetingProp.Value
    }
    
    $newGreetingTypes[$categoryName] = @{
        items = $items
    }
}

# Remove old properties
$propsToRemove = @($greetings.PSObject.Properties | Where-Object { $_.Name -ne "metadata" } | ForEach-Object { $_.Name })
foreach ($prop in $propsToRemove) {
    $greetings.PSObject.Properties.Remove($prop)
}

# Add new structure
$greetings | Add-Member -MemberType NoteProperty -Name 'greeting_types' -Value $newGreetingTypes
if ($greetings.metadata.PSObject.Properties['componentKeys']) {
    $greetings.metadata.componentKeys = @("greeting_types")
} else {
    $greetings.metadata | Add-Member -MemberType NoteProperty -Name 'componentKeys' -Value @("greeting_types")
}

Write-Host "  Restructured: multiple *_types -> greeting_types with categorized items" -ForegroundColor Green

# Save
$greetings | ConvertTo-Json -Depth 100 | Set-Content $greetingsPath -Encoding UTF8

Write-Host "`nDone! All catalogs restructured to standard pattern!" -ForegroundColor Green
Write-Host "Pattern: *_types -> category -> items" -ForegroundColor Cyan
