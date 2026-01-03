# Fix Invalid JSON References
$ErrorActionPreference = "Stop"
$dataPath = Join-Path $PSScriptRoot ".." "RealmEngine.Data" "Data" "Json"
$dataPath = Resolve-Path $dataPath
Write-Host "`nJSON Reference Fix Script" -ForegroundColor Cyan
Write-Host "Data Path: $dataPath`n"

$fixes = @(
    @{Pattern = '@abilities/passive/passive:'; Replacement = '@abilities/passive:'},
    @{Pattern = '@abilities/ultimate/ultimate:'; Replacement = '@abilities/ultimate:'},
    @{Pattern = '@abilities/reactive/reactive:'; Replacement = '@abilities/reactive:'},
    @{Pattern = '@world/locations/towns/towns:'; Replacement = '@world/locations/towns:'}
)

$totalFixed = 0
foreach ($fix in $fixes) {
    Write-Host "Searching for: $($fix.Pattern)" -ForegroundColor Yellow
    $jsonFiles = Get-ChildItem -Path $dataPath -Filter "*.json" -Recurse | Where-Object { $_.Name -notmatch '\.cbconfig\.json$' }
    $fixCount = 0
    foreach ($file in $jsonFiles) {
        $content = Get-Content $file.FullName -Raw -Encoding UTF8
        $pattern = [regex]::Escape($fix.Pattern)
        if ($content -match $pattern) {
            $matchCount = ([regex]::Matches($content, $pattern)).Count
            Write-Host "  Fix $($file.Name): $matchCount occurrence(s)" -ForegroundColor Green
            $newContent = $content -replace $pattern, $fix.Replacement
            Set-Content -Path $file.FullName -Value $newContent -Encoding UTF8 -NoNewline
            $fixCount += $matchCount
        }
    }
    Write-Host "  Total fixed: $fixCount`n" -ForegroundColor Green
    $totalFixed += $fixCount
}
Write-Host "`nGrand Total: $totalFixed references fixed`n" -ForegroundColor Cyan
