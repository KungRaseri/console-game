# Script to batch update remaining UI test classes to use UITestBase
# This script performs find-and-replace operations on multiple test files

$testFiles = @(
    "HybridArrayEditorUITests.cs",
    "NameListEditorUITests.cs",
    "FlatItemEditorUITests.cs",
    "GenericCatalogEditorUITests.cs",
    "NameCatalogEditorUITests.cs",
    "AbilitiesEditorUITests.cs",
    "DiagnosticUITests.cs"
)

$basePath = "C:\code\console-game\Game.ContentBuilder.Tests\UI"

foreach ($file in $testFiles) {
    $filePath = Join-Path $basePath $file
    
    if (Test-Path $filePath) {
        Write-Host "Processing $file..." -ForegroundColor Cyan
        
        # Read content
        $content = Get-Content $filePath -Raw
        
        # Remove unnecessary using statements
        $content = $content -replace 'using System\.IO;[\r\n]+', ''
        $content = $content -replace 'using FlaUI\.Core;[\r\n]+', ''
        $content = $content -replace 'using FlaUI\.UIA3;[\r\n]+', ''
        
        # Change base class
        $content = $content -replace 'public class (\w+) : IDisposable', 'public class $1 : UITestBase'
        
        # Remove field declarations
        $content = $content -replace '[\r\n]+    private readonly Application _app;', ''
        $content = $content -replace '[\r\n]+    private readonly UIA3Automation _automation;', ''
        $content = $content -replace '[\r\n]+    private readonly Window _mainWindow;', ''
        
        # Replace constructor pattern (complex regex)
        $pattern = @'
    public (\w+)\(\)
    \{
        var testAssemblyPath = AppDomain\.CurrentDomain\.BaseDirectory;
        var exePath = Path\.Combine\(
            testAssemblyPath,
            "\.\.", "\.\.", "\.\.", "\.\.",
            "Game\.ContentBuilder", "bin", "Debug", "net9\.0-windows",
            "Game\.ContentBuilder\.exe"
        \);

        var fullExePath = Path\.GetFullPath\(exePath\);

        if \(!File\.Exists\(fullExePath\)\)
        \{
            throw new FileNotFoundException\(
                \$"ContentBuilder executable not found at: \{fullExePath\}[^"]*"\);
        \}

        _automation = new UIA3Automation\(\);
        _app = Application\.Launch\(fullExePath\);
        _mainWindow = _app\.GetMainWindow\(_automation, TimeSpan\.FromSeconds\(\d+\)\);

        if \(_mainWindow == null\)
        \{
            throw new InvalidOperationException\("Main window failed to load[^"]*"\);
        \}

        Thread\.Sleep\(\d+\);
'@
        
        $replacement = @'
    public $1() : base()
    {
        LaunchApplication();
        Thread.Sleep(1000);
'@
        
        $content = $content -replace $pattern, $replacement
        
        # Remove Dispose method
        $disposePattern = @'

    public void Dispose\(\)
    \{
        try
        \{
            // Try graceful shutdown first
            _app\?\.Close\(\);
        \}
        catch
        \{
            // If graceful shutdown fails, force kill
            _app\?\.Kill\(\);
        \}
        finally
        \{
            _automation\?\.Dispose\(\);[^\}]*
        \}
    \}
'@
        
        $content = $content -replace $disposePattern, ''
        
        # Write back
        Set-Content $filePath $content -NoNewline
        
        Write-Host "  ✓ Updated $file" -ForegroundColor Green
    }
    else {
        Write-Host "  ✗ File not found: $file" -ForegroundColor Red
    }
}

Write-Host "`nDone! Updated $($testFiles.Count) files." -ForegroundColor Green
Write-Host "Run 'dotnet build Game.ContentBuilder.Tests' to verify." -ForegroundColor Yellow
