using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using Xunit;
using Xunit.Abstractions;

namespace Game.ContentBuilder.Tests.UI;

[Trait("Category", "UI")]
public class TreeStructureDebugTests : UITestBase
{
    private readonly ITestOutputHelper _output;

    public TreeStructureDebugTests(ITestOutputHelper output) : base()
    {
        _output = output;
        LaunchApplication();
        Thread.Sleep(2000);
    }

    [Fact]
    public void Debug_Print_Full_Tree_Structure()
    {
        // Find tree view
        var treeView = _mainWindow!.FindFirstDescendant(cf => cf.ByAutomationId("CategoryTreeView"))?.AsTree();
        _output.WriteLine($"TreeView found: {treeView != null}");

        if (treeView != null)
        {
            _output.WriteLine($"\n=== TOP LEVEL NODES ===");
            foreach (var topNode in treeView.Items)
            {
                _output.WriteLine($"Top: {topNode.Name}");
                PrintNodeChildren(topNode, 1);
            }
        }
    }

    private void PrintNodeChildren(TreeItem node, int level)
    {
        try
        {
            node.Expand();
            Thread.Sleep(300);

            var indent = new string(' ', level * 2);
            foreach (var child in node.Items)
            {
                _output.WriteLine($"{indent}{child.Name}");
                
                if (level < 3) // Limit depth to prevent too much output
                {
                    PrintNodeChildren(child, level + 1);
                }
            }
        }
        catch
        {
            // Some nodes might not be expandable
        }
    }
}
