using System;
using System.Linq;
using System.Threading;
using FlaUI.Core.AutomationElements;
using Xunit;
using Xunit.Abstractions;

namespace Game.ContentBuilder.Tests.UI;

/// <summary>
/// Diagnostic tests to explore the UI structure and help debug automation issues
/// </summary>
public class DiagnosticUITests : UITestBase
{
    private readonly ITestOutputHelper _output;

    public DiagnosticUITests(ITestOutputHelper output) : base()
    {
        _output = output;
        LaunchApplication();
    }

    [Fact]
    [Trait("Category", "Diagnostic")]
    public void Print_All_UI_Elements()
    {
        _output.WriteLine("=== MAIN WINDOW STRUCTURE ===");
        _output.WriteLine($"Title: {_mainWindow.Title}");
        _output.WriteLine($"ClassName: {SafeGetProperty(() => _mainWindow.ClassName)}");
        _output.WriteLine($"AutomationId: {SafeGetProperty(() => _mainWindow.AutomationId)}");
        _output.WriteLine("");

        // Print all immediate children
        _output.WriteLine("=== TOP LEVEL CHILDREN ===");
        var children = _mainWindow.FindAllChildren();
        for (int i = 0; i < children.Length; i++)
        {
            var child = children[i];
            _output.WriteLine($"[{i}] {child.ControlType} - Name: '{SafeGetProperty(() => child.Name)}' - AutomationId: '{SafeGetProperty(() => child.AutomationId)}' - ClassName: '{SafeGetProperty(() => child.ClassName)}'");
        }

        // Find and explore tree view
        _output.WriteLine("");
        _output.WriteLine("=== TREE VIEW SEARCH ===");
        var treeControls = _mainWindow.FindAllDescendants(cf => 
            cf.ByControlType(FlaUI.Core.Definitions.ControlType.Tree));
        
        _output.WriteLine($"Found {treeControls.Length} tree control(s)");
        
        if (treeControls.Length > 0)
        {
            var tree = treeControls[0].AsTree();
            _output.WriteLine($"Tree AutomationId: '{SafeGetProperty(() => tree.AutomationId)}'");
            _output.WriteLine($"Tree Name: '{SafeGetProperty(() => tree.Name)}'");
            _output.WriteLine($"Tree ClassName: '{SafeGetProperty(() => tree.ClassName)}'");
            _output.WriteLine("");
            
            _output.WriteLine("=== TREE ITEMS ===");
            var items = tree.Items;
            _output.WriteLine($"Found {items.Length} top-level tree item(s)");
            
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                _output.WriteLine($"[{i}] Name: '{SafeGetProperty(() => item.Name)}' - AutomationId: '{SafeGetProperty(() => item.AutomationId)}' - Text: '{SafeGetProperty(() => item.Text)}'");
                
                // Try to get the header text using different methods
                try
                {
                    var properties = item.Properties;
                    _output.WriteLine($"    Properties.Name: '{properties.Name.ValueOrDefault}'");
                }
                catch (Exception ex)
                {
                    _output.WriteLine($"    Error getting properties: {ex.Message}");
                }
            }
        }
    }

    private string SafeGetProperty(Func<string> getter)
    {
        try
        {
            return getter() ?? "(null)";
        }
        catch (Exception ex)
        {
            return $"(error: {ex.Message})";
        }
    }
}

