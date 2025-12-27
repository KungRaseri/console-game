# Fix UI issues in ContentBuilder XAML files

$xamlFile = "Game.ContentBuilder\Views\NameListEditorView.xaml"
$content = Get-Content $xamlFile -Raw

Write-Host "Fixing NameListEditorView.xaml..." -ForegroundColor Cyan

# Fix 1: Replace metadata field labels with floating hints
$metadataFields = @(
    @{Old = '<TextBlock Text="Description" Margin="0,8,0,4" FontWeight="SemiBold"/>\s*<TextBox materialDesign:HintAssist.Hint=""[^>]*Text="{Binding Metadata.Description[^}]*}"/>'; 
      New = '<TextBox Margin="0,8,0,0" materialDesign:HintAssist.Hint="Description" materialDesign:HintAssist.IsFloating="True" Text="{Binding Metadata.Description, UpdateSourceTrigger=PropertyChanged}"/>'},
    @{Old = '<TextBlock Text="Type" Margin="0,8,0,4" FontWeight="SemiBold"/>\s*<TextBox materialDesign:HintAssist.Hint=""[^>]*Text="{Binding Metadata.Type[^}]*}"/>'; 
      New = '<TextBox Margin="0,8,0,0" materialDesign:HintAssist.Hint="Type" materialDesign:HintAssist.IsFloating="True" Text="{Binding Metadata.Type, UpdateSourceTrigger=PropertyChanged}"/>'},
    @{Old = '<TextBlock Text="Version" Margin="0,8,0,4" FontWeight="SemiBold"/>\s*<TextBox materialDesign:HintAssist.Hint=""[^>]*Text="{Binding Metadata.Version[^}]*}"/>'; 
      New = '<TextBox Margin="0,8,0,0" materialDesign:HintAssist.Hint="Version" materialDesign:HintAssist.IsFloating="True" Text="{Binding Metadata.Version, UpdateSourceTrigger=PropertyChanged}"/>'},
    @{Old = '<TextBlock Text="Last Modified" Margin="0,8,0,4" FontWeight="SemiBold"/>\s*<TextBox materialDesign:HintAssist.Hint=""[^>]*Text="{Binding Metadata.LastModified[^}]*}"/>'; 
      New = '<TextBox Margin="0,8,0,0" materialDesign:HintAssist.Hint="Last Modified" materialDesign:HintAssist.IsFloating="True" Text="{Binding Metadata.LastModified, UpdateSourceTrigger=PropertyChanged}"/>'},
    @{Old = '<TextBlock Text="Rarity System" Margin="0,8,0,4" FontWeight="SemiBold"/>\s*<TextBox materialDesign:HintAssist.Hint=""[^>]*Text="{Binding Metadata.RaritySystem[^}]*}"/>'; 
      New = '<TextBox Margin="0,8,0,0" materialDesign:HintAssist.Hint="Rarity System" materialDesign:HintAssist.IsFloating="True" Text="{Binding Metadata.RaritySystem, UpdateSourceTrigger=PropertyChanged}"/>'},
    @{Old = '<TextBlock Text="Usage" Margin="0,8,0,4" FontWeight="SemiBold"/>\s*<TextBox materialDesign:HintAssist.Hint=""[^>]*Text="{Binding Metadata.Usage[^}]*}"/>'; 
      New = '<TextBox Margin="0,8,0,0" materialDesign:HintAssist.Hint="Usage" materialDesign:HintAssist.IsFloating="True" Text="{Binding Metadata.Usage, UpdateSourceTrigger=PropertyChanged}"/>'}
)

$modified = $false
foreach ($field in $metadataFields) {
    if ($content -match $field.Old) {
        $content = $content -replace $field.Old, $field.New
        $modified = $true
        Write-Host "  Fixed metadata field" -ForegroundColor Green
    }
}

# Fix 2: Fix pattern search text box visibility (remove padding that shrinks it)
if ($content -match 'PatternSearchText[^>]*Padding="[^"]*"') {
    $content = $content -replace '(<TextBox[^>]*PatternSearchText[^>]*)(Padding="[^"]*")', '$1Padding="8,4"'
    $modified = $true
    Write-Host "  Fixed pattern search padding" -ForegroundColor Green
}

if ($modified) {
    Set-Content -Path $xamlFile -Value $content -NoNewline
    Write-Host "`nNameListEditorView.xaml updated successfully!" -ForegroundColor Green
} else {
    Write-Host "`nNo changes needed" -ForegroundColor Yellow
}

# Fix materials names.json - remove invalid tokens
Write-Host "`nFixing materials names.json..." -ForegroundColor Cyan

$materialsNames = "Game.Data\Data\Json\items\materials\names.json"
$matContent = Get-Content $materialsNames -Raw | ConvertFrom-Json

# Remove invalid tokens
if ($matContent.metadata.patternTokens -contains "material") {
    $matContent.metadata.patternTokens = @($matContent.metadata.patternTokens | Where-Object { $_ -notin @("material", "quality") })
    $matContent.metadata.patternTokens += "base"
    $matContent.metadata.patternTokens = $matContent.metadata.patternTokens | Select-Object -Unique
    
    $matContent | ConvertTo-Json -Depth 100 | Set-Content $materialsNames -Encoding UTF8
    Write-Host "  Removed invalid tokens (material, quality) from materials names.json" -ForegroundColor Green
}

Write-Host "`nAll fixes applied!" -ForegroundColor Cyan
