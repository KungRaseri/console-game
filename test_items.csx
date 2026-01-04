using RealmEngine.Data.Services;
using RealmEngine.Core.Generators.Modern;
using Microsoft.Extensions.Logging;
using Moq;

var basePath = Path.Combine(Directory.GetCurrentDirectory(), "RealmEngine.Data", "Data", "Json");
var dataCache = new GameDataCache(basePath);
var mockLogger = new Mock&lt;ILogger&lt;ReferenceResolverService&gt;&gt;();
var referenceResolver = new ReferenceResolverService(dataCache, mockLogger.Object);
var itemLogger = new Mock&lt;ILogger&lt;ItemGenerator&gt;&gt;();
var mockLoggerFactory = new Mock&lt;ILoggerFactory&gt;&gt;();
var generator = new ItemGenerator(dataCache, referenceResolver, itemLogger.Object, mockLoggerFactory.Object);

dataCache.LoadAllData();
var items = await generator.GenerateItemsAsync("weapons", 5);

Console.WriteLine($"Generated {items.Count} items:");
foreach (var item in items)
{
    Console.WriteLine($"- {item.Name}");
    Console.WriteLine($"  Material: '{item.Material ?? "NULL"}'");
    Console.WriteLine($"  MaterialTraits: {item.MaterialTraits.Count}");
    Console.WriteLine($"  Prefixes: {item.Prefixes.Count}");
    Console.WriteLine($"  TotalRarityWeight: {item.TotalRarityWeight}");
}
