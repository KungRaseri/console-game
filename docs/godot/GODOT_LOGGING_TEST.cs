using RealmEngine.Shared.Services;
using Godot;

/// <summary>
/// Simple test script to verify RealmEngine logging integration with Godot.
/// Add this script to a Node in your Godot scene to test the logging system.
/// 
/// Usage:
/// 1. Copy this script to your Godot project
/// 2. Attach it to a Node in your scene
/// 3. Run the scene and check the Godot console for RealmEngine logs
/// </summary>
public partial class GodotLoggingTest : Node
{
    public override void _Ready()
    {
        // Initialize the Godot logging integration
        GodotLogger.Initialize();

        // Subscribe to receive logs in Godot console
        GodotLogger.Subscribe((level, message) => 
        {
            // Forward RealmEngine logs to Godot console with custom prefix
            GD.Print($"[RealmEngine:{level}] {message}");
        });

        // Test the logging integration
        TestRealmEngineLogging();
        
        // Clean up when done
        GodotLogger.Shutdown();
        
        GD.Print("[GodotLoggingTest] RealmEngine logging integration test completed!");
    }

    private void TestRealmEngineLogging()
    {
        // This would simulate using RealmEngine generators
        // which now produce structured logs that should appear in Godot console
        
        GD.Print("[GodotLoggingTest] Starting RealmEngine logging test...");
        
        // In a real scenario, you would:
        // 1. Create a RealmEngine generator service
        // 2. Call generator methods (like GenerateCharacter, GenerateItem, etc.)
        // 3. The generators will produce structured logs via ILogger
        // 4. Those logs will be captured by GodotSink and forwarded to this subscription
        // 5. You'll see the logs appear in Godot console with contextual information
        
        GD.Print("[GodotLoggingTest] In a real scenario, RealmEngine generator logs would appear here");
        GD.Print("[GodotLoggingTest] Look for logs like: '[RealmEngine:Warning] Catalog not found for reference: @abilities/active/offensive:bite'");
    }
}