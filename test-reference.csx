using System;
using System.IO;
using Game.ContentBuilder.Services;
using Newtonsoft.Json.Linq;

var baseDir = AppDomain.CurrentDomain.BaseDirectory;
var solutionRoot = Directory.GetParent(baseDir)?.Parent?.Parent?.Parent?.FullName;
var dataPath = Path.Combine(solutionRoot, "Game.Data", "Data", "Json");

var resolver = new Game.ContentBuilder.Services.ReferenceResolverService(dataPath);

var categories = resolver.GetAvailableCategories("classes", ".");
Console.WriteLine("Categories found: " + string.Join(", ", categories));

if (categories.Count > 0)
{
    var refs = resolver.GetAvailableReferences("classes", ".", categories[0]);
    Console.WriteLine($"References in {categories[0]}: " + string.Join(", ", refs));
    
    if (refs.Count > 0)
    {
        Console.WriteLine($"Testing reference: {refs[0]}");
        var result = resolver.ResolveReference(refs[0]);
        Console.WriteLine($"Result: {(result != null ? "SUCCESS" : "NULL")}");
    }
}
