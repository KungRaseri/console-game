# Script to refactor NameListEditorView.xaml with component references

$filePath = "c:\code\console-game\Game.ContentBuilder\Views\NameListEditorView.xaml"
$content = Get-Content $filePath -Raw

Write-Host "Original file length: $($content.Length) characters"

# Create backup
$backupPath = "$filePath.bak"
$content | Out-File $backupPath -NoNewline
Write-Host "Backup created at: $backupPath"

# First, add the components namespace if not already present
if ($content -notmatch 'xmlns:components=') {
    $content = $content -replace '(xmlns:converters="clr-namespace:Game\.ContentBuilder\.Converters")', "`$1`r`n        xmlns:components=`"clr-namespace:Game.ContentBuilder.Views.Components`""
    Write-Host "Added components namespace"
}

# Add converter resources if not present
if ($content -notmatch 'RarityWeightToColorConverter') {
    $converterInsert = @"
                <converters:RarityWeightToColorConverter x:Key="RarityWeightToColorConverter"/>
                <converters:RarityWeightToNameConverter x:Key="RarityWeightToNameConverter"/>
                <converters:StringEqualityToVisibilityConverter x:Key="StringEqualityToVisibilityConverter"/>

"@
    $content = $content -replace '(<converters:TupleConverter x:Key="TupleConverter"/>)', "`$1`r`n$converterInsert"
    Write-Host "Added converter resources"
}

# Replace ComponentItemTemplate - find the exact pattern
$componentItemPattern = '(?s)<!-- Component Item Template -->\s*<DataTemplate x:Key="ComponentItemTemplate">.*?</DataTemplate>\s*(?=<!-- Component Group Template)'

$componentItemReplacement = @'
<!-- Component Item Template -->
                <DataTemplate x:Key="ComponentItemTemplate">
                        <components:ComponentItemControl 
                                Value="{Binding Value, Mode=TwoWay}"
                                RarityWeight="{Binding RarityWeight, Mode=TwoWay}"
                                Traits="{Binding Traits}"
                                DeleteCommand="{Binding DataContext.RemoveComponentCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                AddTraitCommand="{Binding DataContext.AddComponentTraitCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"/>
                </DataTemplate>

                
'@

if ($content -match $componentItemPattern) {
    $content = $content -replace $componentItemPattern, $componentItemReplacement
    Write-Host "Replaced ComponentItemTemplate"
} else {
    Write-Warning "Could not find ComponentItemTemplate pattern"
}

# Replace ComponentGroupTemplate
$componentGroupPattern = '(?s)<!-- Component Group Template for TreeView -->\s*<HierarchicalDataTemplate x:Key="ComponentGroupTemplate".*?</HierarchicalDataTemplate>\s*(?=<!-- Pattern Template)'

$componentGroupReplacement = @'
<!-- Component Group Template for TreeView -->
                <HierarchicalDataTemplate x:Key="ComponentGroupTemplate"
                                ItemsSource="{Binding Value}"
                                ItemTemplate="{StaticResource ComponentItemTemplate}">
                        <components:ComponentGroupHeaderControl 
                                GroupKey="{Binding Key}"
                                ItemCount="{Binding Value.Count}"
                                AddCommand="{Binding DataContext.AddComponentCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                DeleteCommand="{Binding DataContext.RemoveComponentGroupCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                IsBaseGroup="{Binding Key, Converter={StaticResource StringEqualityToVisibilityConverter}, ConverterParameter=base}"/>
                </HierarchicalDataTemplate>

                
'@

if ($content -match $componentGroupPattern) {
    $content = $content -replace $componentGroupPattern, $componentGroupReplacement
    Write-Host "Replaced ComponentGroupTemplate"
} else {
    Write-Warning "Could not find ComponentGroupTemplate pattern"
}

# Replace PatternItemTemplate
$patternItemPattern = '(?s)<!-- Pattern Template with Examples -->\s*<DataTemplate x:Key="PatternItemTemplate">.*?</DataTemplate>\s*(?=</UserControl.Resources>)'

$patternItemReplacement = @'
<!-- Pattern Template with Examples -->
                <DataTemplate x:Key="PatternItemTemplate">
                        <components:PatternItemControl 
                                Tokens="{Binding Tokens}"
                                Weight="{Binding Weight, Mode=TwoWay}"
                                WeightPercentage="{Binding WeightPercentage}"
                                Description="{Binding Description}"
                                GeneratedExamples="{Binding GeneratedExamples}"
                                IsReadOnly="{Binding IsReadOnly}"
                                ComponentNames="{Binding DataContext.ComponentNames, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                InsertComponentTokenCommand="{Binding DataContext.InsertComponentTokenCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                InsertReferenceTokenCommand="{Binding DataContext.InsertReferenceTokenCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                BrowseReferenceCommand="{Binding DataContext.BrowseReferenceCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                RegenerateExamplesCommand="{Binding DataContext.RegenerateExamplesCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                DuplicatePatternCommand="{Binding DataContext.DuplicatePatternCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                                DeleteTokenCommand="{Binding DataContext.RemovePatternTokenCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"/>
                </DataTemplate>
        
'@

if ($content -match $patternItemPattern) {
    $content = $content -replace $patternItemPattern, $patternItemReplacement
    Write-Host "Replaced PatternItemTemplate"
} else {
    Write-Warning "Could not find PatternItemTemplate pattern"
}

# Save the file
$content | Out-File $filePath -NoNewline

$newLines = ($content -split "`n").Count
Write-Host "`nFile updated! New line count: $newLines"
Write-Host "Original backup at: $backupPath"
