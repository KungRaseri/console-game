# Fix Invalid JSON References
# Automatically fixes all duplicate path segment references found by validation tests

$ErrorActionPreference = "Stop"

$dataPath = Join-Path $PSScriptRoot ".." "RealmEngine.Data" "Data" "Json"
$dataPath = Resolve-Path $dataPath

Write-Host "`n🔧 JSON Reference Fix Script" -ForegroundColor Cyan
Write-Host "=" * 60
Write-Host "Data Path: $dataPath`n" -ForegroundColor Gray

# Define all the patterns to fix
$fixes = @(
    # Passive abilities - Remove duplicate "passive"
    @{Pattern = '@abilities/passive/passive:'; Replacement = '@abilities/passive:'; Description = 'Passive abilities'},
    
    # Ultimate abilities - Remove duplicate "ultimate"
    @{Pattern = '@abilities/ultimate/ultimate:'; Replacement = '@abilities/ultimate:'; Description = 'Ultimate abilities'},
    
    # Reactive abilities - Remove duplicate "reactive"
    @{Pattern = '@abilities/reactive/reactive:'; Replacement = '@abilities/reactive:'; Description = 'Reactive abilities'},
    
    # Towns locations - Remove duplicate "towns"
    @{Pattern = '@world/locations/towns/towns:'; Replacement = '@world/locations/towns:'; Description = 'Town locations'}
)

$totalFixed = 0
$filesModified = @()

foreach ($fix in $fixes) {
    Write-Host "🔍 Searching for: $($fix.Pattern)" -ForegroundColor Yellow
    
    $jsonFiles = Get-ChildItem -Path $dataPath -Filter "*.json" -Recurse | 
        Where-Object { $_.Name -notmatch '\.cbconfig\.json$' }
    
    $fixCount = 0
    
    foreach ($file in $jsonFiles) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        $pattern = [regex]::Escape($fix.Pattern)
        
        if ($content -match $pattern) {
            $matchCount = ([regex]::Matches($content, $pattern)).Count
            
            Write-Host "  📝 $($file.FullName.Replace($dataPath, '').TrimStart('\'))" -ForegroundColor Green
            Write-Host "     Found $matchCount occurrence(s)" -ForegroundColor Gray
            
            $newContent = $content -replace $pattern, $fix.Replacement
            Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
            
            $fixCount += $matchCount
            
            if ($filesModified -notcontains $file.FullName) {
                $filesModified += $file.FullName
            }
        }
    }
    
    if ($fixCount -gt 0) {
        Write-Host "  ✅ Fixed $fixCount reference(s) in $($fix.Description)" -ForegroundColor Green
        $totalFixed += $fixCount
    } else {
        Write-Host "  ⏭️  No matches found" -ForegroundColor Gray
    }
    Write-Host ""
}

Write-Host "`n" + ("=" * 60)
Write-Host "📊 Summary" -ForegroundColor Cyan
Write-Host ("=" * 60)
Write-Host "Total references fixed: $totalFixed" -ForegroundColor Green
Write-Host "Files modified: $($filesModified.Count)" -ForegroundColor Green

if ($filesModified.Count -gt 0) {
    Write-Host "`nModified files:" -ForegroundColor Gray
    foreach ($file in $filesModified | Sort-Object) {
        $relativePath = $file.Replace($dataPath, '').TrimStart('\')
        Write-Host "  • $relativePath" -ForegroundColor DarkGray
    }
}

Write-Host "`n✅ Reference fix complete!" -ForegroundColor Green
Write-Host "Run validation tests to verify: dotnet test --filter ReferenceValidationTests`n" -ForegroundColor Yellow
