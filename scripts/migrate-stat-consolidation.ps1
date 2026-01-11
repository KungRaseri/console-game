# Migration script to replace legacy BonusX properties with Traits dictionary in test files
# Part of stat consolidation refactoring

param(
    [string]$RootPath = "c:\code\console-game"
)

$testFiles = Get-ChildItem -Path $RootPath -Include "*.cs" -Recurse | Where-Object {
    $_.FullName -like "*Tests*" -and 
    $_.FullName -notlike "*\obj\*" -and 
    $_.FullName -notlike "*\bin\*"
}

$replacements = @(
    # Simple property assignments
    @{ Old = 'BonusStrength = '; New = 'Traits = new Dictionary<string, TraitValue> { { "Strength", new TraitValue('; Suffix = ', TraitType.Number) } }' }
    @{ Old = 'BonusDexterity = '; New = 'Traits = new Dictionary<string, TraitValue> { { "Dexterity", new TraitValue('; Suffix = ', TraitType.Number) } }' }
    @{ Old = 'BonusConstitution = '; New = 'Traits = new Dictionary<string, TraitValue> { { "Constitution", new TraitValue('; Suffix = ', TraitType.Number) } }' }
    @{ Old = 'BonusIntelligence = '; New = 'Traits = new Dictionary<string, TraitValue> { { "Intelligence", new TraitValue('; Suffix = ', TraitType.Number) } }' }
    @{ Old = 'BonusWisdom = '; New = 'Traits = new Dictionary<string, TraitValue> { { "Wisdom", new TraitValue('; Suffix = ', TraitType.Number) } }' }
    @{ Old = 'BonusCharisma = '; New = 'Traits = new Dictionary<string, TraitValue> { { "Charisma", new TraitValue('; Suffix = ', TraitType.Number) } }' }
)

$totalFiles = 0
$totalReplacements = 0

foreach ($file in $testFiles) {
    $content = Get-Content $file.FullName -Raw
    $original = $content
    $fileChanged = $false
    
    # Replace simple assignments like: BonusStrength = 5
    $content = $content -replace 'BonusStrength\s*=\s*(\d+)', 'Traits = new Dictionary<string, TraitValue> { { "Strength", new TraitValue($1, TraitType.Number) } }'
    $content = $content -replace 'BonusDexterity\s*=\s*(\d+)', 'Traits = new Dictionary<string, TraitValue> { { "Dexterity", new TraitValue($1, TraitType.Number) } }'
    $content = $content -replace 'BonusConstitution\s*=\s*(\d+)', 'Traits = new Dictionary<string, TraitValue> { { "Constitution", new TraitValue($1, TraitType.Number) } }'
    $content = $content -replace 'BonusIntelligence\s*=\s*(\d+)', 'Traits = new Dictionary<string, TraitValue> { { "Intelligence", new TraitValue($1, TraitType.Number) } }'
    $content = $content -replace 'BonusWisdom\s*=\s*(\d+)', 'Traits = new Dictionary<string, TraitValue> { { "Wisdom", new TraitValue($1, TraitType.Number) } }'
    $content = $content -replace 'BonusCharisma\s*=\s*(\d+)', 'Traits = new Dictionary<string, TraitValue> { { "Charisma", new TraitValue($1, TraitType.Number) } }'
    
    if ($content -ne $original) {
        Set-Content $file.FullName -Value $content -NoNewline
        $totalFiles++
        Write-Host "Updated: $($file.FullName)" -ForegroundColor Green
    }
}

Write-Host "`nMigration Complete!" -ForegroundColor Cyan
Write-Host "Files Updated: $totalFiles" -ForegroundColor Yellow
Write-Host "`nNote: This handles simple cases. Complex test methods may need manual fixes." -ForegroundColor Yellow
