using Game.Core.Generators;
using Game.Core.Services;
using Bogus;
using Serilog;

// Simple test to verify v4 pattern generation with @materialRef tokens
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Console.WriteLine("Testing V4 Pattern-Based Weapon Generation\n");
Console.WriteLine("===========================================\n");

var dataService = new GameDataService();
var itemGenerator = new ItemGenerator(dataService);
var faker = new Faker();

Console.WriteLine("Generating 10 weapons with @materialRef/weapon patterns:\n");

for (int i = 0; i < 10; i++)
{
    var weaponName = itemGenerator.GenerateWeaponName(faker);
    Console.WriteLine($"{i + 1}. {weaponName}");
}

Console.WriteLine("\n\nPattern execution test complete!");
Log.CloseAndFlush();
