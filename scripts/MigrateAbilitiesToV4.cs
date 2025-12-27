/*
 * Abilities v4 Migration Script
 *
 * Usage: Run with dotnet-script or copy into a console app
 * This script migrates all enemy type abilities from v3 to v4 format
 */

using System;
using System.IO;
using System.Text.Json;
using Game.ContentBuilder.Services;
using Serilog;
using Serilog.Core;

// Configure logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("migration-log.txt")
    .CreateLogger();

var enemyTypes = new[]
{
    "beasts", "dragons", "elementals", "goblinoids",
    "humanoids", "insects", "orcs", "plants",
    "reptilians", "trolls", "undead", "vampires"
};

var basePath = @"C:\code\console-game\Game.Data\Data\Json\enemies";
var migrationService = new AbilityMigrationService();

Console.WriteLine("=== Abilities v4 Migration ===");
Console.WriteLine($"Migrating {enemyTypes.Length} enemy types...\n");

int successCount = 0;
int failCount = 0;

foreach (var enemyType in enemyTypes)
{
    try
    {
        Console.WriteLine($"Processing {enemyType}...");

        var v3Path = Path.Combine(basePath, enemyType, "abilities.json");
        var catalogPath = Path.Combine(basePath, enemyType, "abilities_catalog.json");
        var namesPath = Path.Combine(basePath, enemyType, "abilities_names.json");

        if (!File.Exists(v3Path))
        {
            Console.WriteLine($"  [SKIP] No abilities.json found for {enemyType}");
            continue;
        }

        // Check if already migrated
        if (File.Exists(catalogPath) && File.Exists(namesPath))
        {
            Console.WriteLine($"  [SKIP] Already migrated (v4 files exist)");
            continue;
        }

        // Perform migration
        var (catalog, names) = migrationService.MigrateToV4(enemyType, v3Path);

        // Serialize with indentation
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var catalogJson = JsonSerializer.Serialize(catalog, options);
        var namesJson = JsonSerializer.Serialize(names, options);

        // Write files
        File.WriteAllText(catalogPath, catalogJson);
        File.WriteAllText(namesPath, namesJson);

        Console.WriteLine($"  [SUCCESS] Created abilities_catalog.json and abilities_names.json");
        Console.WriteLine($"  - {catalog.Metadata.TotalAbilities} abilities across {catalog.Metadata.TotalAbilityTypes} categories");
        Console.WriteLine($"  - {names.Components["base"].Count} base components extracted");

        successCount++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [ERROR] {ex.Message}");
        Log.Error(ex, "Failed to migrate {EnemyType}", enemyType);
        failCount++;
    }

    Console.WriteLine();
}

Console.WriteLine("=== Migration Complete ===");
Console.WriteLine($"Success: {successCount}");
Console.WriteLine($"Failed: {failCount}");
Console.WriteLine($"Total: {enemyTypes.Length}");

Log.CloseAndFlush();
