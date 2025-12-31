using Game.ContentBuilder.Services;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

var baseDir = @"C:\code\console-game\Game.Data\Data\Json";
var resolver = new ReferenceResolverService(baseDir);

var reference = "@abilities/active/offensive:Infernal Flames";
Console.WriteLine($"Testing: {reference}");

var components = resolver.ParseReference(reference);
Console.WriteLine($"Parsed - Domain: {components.Domain}, Path: {components.Path}, Category: {components.Category}, Item: {components.ItemName}");

var result = resolver.ResolveReference(reference);
Console.WriteLine($"Result: {(result == null ? "NULL" : result["name"]?.ToString() ?? "NO NAME")}");
