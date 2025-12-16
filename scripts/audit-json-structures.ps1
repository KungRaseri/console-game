# JSON Structure Audit Script
# This script analyzes all JSON files to determine their structure type

$jsonPath = "Game.Shared\Data\Json"
$results = @()

Get-ChildItem -Path $jsonPath -Filter "*.json" -Recurse | ForEach-Object {
    $relativePath = $_.FullName -replace '.*\\Game.Shared\\Data\\Json\\', ''
    $content = Get-Content $_.FullName -Raw | ConvertFrom-Json
    
    $structure = "Unknown"
    $hasRarity = $false
    $hasItems = $false
    $hasComponents = $false
    $hasPatterns = $false
    $hasTraits = $false
    $hasNames = $false
    
    # Check for different structure patterns
    $properties = $content | Get-Member -MemberType NoteProperty | Select-Object -ExpandProperty Name
    
    if ($properties -contains "items") { $hasItems = $true }
    if ($properties -contains "components") { $hasComponents = $true }
    if ($properties -contains "patterns") { $hasPatterns = $true }
    if ($properties -contains "names") { $hasNames = $true }
    
    # Check for rarity levels in first-level properties
    $rarityLevels = @("common", "uncommon", "rare", "epic", "legendary", "power", "speed", "defense", "utility")
    foreach ($prop in $properties) {
        if ($rarityLevels -contains $prop.ToLower()) {
            $hasRarity = $true
            break
        }
    }
    
    # Determine structure type
    if ($hasNames -and $properties.Count -eq 1) {
        $structure = "NameList"
    }
    elseif ($hasRarity) {
        if ($relativePath -like "*prefixes.json") {
            $structure = "ItemPrefix"
        }
        elseif ($relativePath -like "*suffixes.json") {
            $structure = "ItemSuffix"
        }
        else {
            $structure = "RarityBased"
        }
    }
    elseif ($hasItems -and ($hasComponents -or $hasPatterns)) {
        $structure = "HybridArray"
    }
    elseif ($hasItems) {
        $structure = "ItemArray"
    }
    else {
        # Check if it's a flat item structure
        $firstProp = $properties | Select-Object -First 1
        if ($firstProp -and $content.$firstProp.displayName) {
            $structure = "FlatItem"
        }
    }
    
    $results += [PSCustomObject]@{
        File = $relativePath
        Structure = $structure
        HasRarity = $hasRarity
        HasItems = $hasItems
        HasComponents = $hasComponents
        HasPatterns = $hasPatterns
        TopLevelProps = ($properties -join ", ")
    }
}

# Output results grouped by category
$results | Format-Table -AutoSize -Wrap | Out-String -Width 200
