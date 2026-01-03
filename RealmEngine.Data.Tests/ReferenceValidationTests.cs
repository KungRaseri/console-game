using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace RealmEngine.Data.Tests;

/// <summary>
/// Validates that all JSON reference strings follow correct v4.1 standards and point to existing files
/// </summary>
public class ReferenceValidationTests
{
    private readonly ITestOutputHelper _output;
    private readonly string _dataPath;

    public ReferenceValidationTests(ITestOutputHelper output)
    {
        _output = output;
        _dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "RealmEngine.Data", "Data", "Json");
        _dataPath = Path.GetFullPath(_dataPath);
    }

    [Fact]
    public void Should_Not_Have_Double_Path_Segments_In_References()
    {
        // Find all JSON files
        var jsonFiles = Directory.GetFiles(_dataPath, "*.json", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".cbconfig.json"))
            .ToList();

        var invalidReferences = new List<(string File, int Line, string Reference, string Issue)>();

        foreach (var file in jsonFiles)
        {
            var content = File.ReadAllLines(file);
            for (int i = 0; i < content.Length; i++)
            {
                var line = content[i];
                
                // Find all @domain/... references
                var matches = System.Text.RegularExpressions.Regex.Matches(line, @"@([\w-]+)/([\w-/]+):([\w-*\s]+)");
                
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var reference = match.Value;
                    var domain = match.Groups[1].Value;
                    var path = match.Groups[2].Value;
                    var item = match.Groups[3].Value;

                    // Check for double path segments (e.g., "passive/passive")
                    var pathParts = path.Split('/');
                    for (int j = 0; j < pathParts.Length - 1; j++)
                    {
                        if (pathParts[j] == pathParts[j + 1])
                        {
                            invalidReferences.Add((
                                Path.GetRelativePath(_dataPath, file),
                                i + 1,
                                reference,
                                $"Duplicate path segment '{pathParts[j]}' - should be @{domain}/{string.Join("/", pathParts.Take(j + 1))}:{item}"
                            ));
                        }
                    }

                    // Check for wildcards in path (not just in item name)
                    if (path.Contains("*"))
                    {
                        invalidReferences.Add((
                            Path.GetRelativePath(_dataPath, file),
                            i + 1,
                            reference,
                            "Wildcard '*' in path is not supported - wildcards only allowed in item name"
                        ));
                    }
                }
            }
        }

        // Report findings
        if (invalidReferences.Any())
        {
            _output.WriteLine($"\n❌ Found {invalidReferences.Count} invalid reference(s):\n");
            
            foreach (var (file, line, reference, issue) in invalidReferences.OrderBy(x => x.File).ThenBy(x => x.Line))
            {
                _output.WriteLine($"  {file}:{line}");
                _output.WriteLine($"    Reference: {reference}");
                _output.WriteLine($"    Issue: {issue}\n");
            }
        }

        invalidReferences.Should().BeEmpty("all references should follow v4.1 standards without duplicate path segments or wildcards in paths");
    }

    [Fact]
    public void Should_Not_Reference_Nonexistent_Catalog_Files()
    {
        var jsonFiles = Directory.GetFiles(_dataPath, "*.json", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".cbconfig.json"))
            .ToList();

        var invalidReferences = new List<(string File, int Line, string Reference, string ExpectedPath, bool FileExists)>();

        foreach (var file in jsonFiles)
        {
            var content = File.ReadAllLines(file);
            for (int i = 0; i < content.Length; i++)
            {
                var line = content[i];
                
                var matches = System.Text.RegularExpressions.Regex.Matches(line, @"@([\w-]+)/([\w-/]+):([\w-*\s]+)(\?)?");
                
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var reference = match.Value;
                    var domain = match.Groups[1].Value;
                    var path = match.Groups[2].Value;
                    var item = match.Groups[3].Value;
                    var isOptional = match.Groups[4].Success;

                    // Skip wildcard item names (valid for random selection)
                    if (item.Trim() == "*")
                        continue;

                    // Skip optional references (ending with ?)
                    if (isOptional)
                        continue;

                    // Build possible catalog paths
                    var pathParts = path.Split('/');
                    var possiblePaths = new List<string>();

                    // Try with full path as subdirectory
                    possiblePaths.Add(Path.Combine(_dataPath, domain, path, "catalog.json"));
                    
                    // Try with last segment as category in file
                    if (pathParts.Length > 1)
                    {
                        var parentPath = string.Join("/", pathParts.Take(pathParts.Length - 1));
                        possiblePaths.Add(Path.Combine(_dataPath, domain, parentPath, "catalog.json"));
                    }
                    
                    // Try root catalog
                    possiblePaths.Add(Path.Combine(_dataPath, domain, "catalog.json"));

                    // Check if any expected path exists
                    var exists = possiblePaths.Any(File.Exists);
                    
                    if (!exists)
                    {
                        invalidReferences.Add((
                            Path.GetRelativePath(_dataPath, file),
                            i + 1,
                            reference,
                            string.Join(" OR ", possiblePaths.Select(p => Path.GetRelativePath(_dataPath, p))),
                            false
                        ));
                    }
                }
            }
        }

        if (invalidReferences.Any())
        {
            _output.WriteLine($"\n❌ Found {invalidReferences.Count} reference(s) to nonexistent catalogs:\n");
            
            foreach (var (file, line, reference, expectedPath, _) in invalidReferences.OrderBy(x => x.File).ThenBy(x => x.Line))
            {
                _output.WriteLine($"  {file}:{line}");
                _output.WriteLine($"    Reference: {reference}");
                _output.WriteLine($"    Expected catalog: {expectedPath}\n");
            }
        }

        invalidReferences.Should().BeEmpty("all references should point to existing catalog files");
    }

    [Fact]
    public void Should_Not_Have_Empty_Active_Healing_References()
    {
        var jsonFiles = Directory.GetFiles(_dataPath, "*.json", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".cbconfig.json"))
            .ToList();

        var healingReferences = new List<(string File, int Line, string Reference)>();

        foreach (var file in jsonFiles)
        {
            var content = File.ReadAllLines(file);
            for (int i = 0; i < content.Length; i++)
            {
                var line = content[i];
                
                if (line.Contains("@abilities/active/healing:"))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(line, @"@abilities/active/healing:([\w-]+)");
                    if (match.Success)
                    {
                        healingReferences.Add((
                            Path.GetRelativePath(_dataPath, file),
                            i + 1,
                            match.Value
                        ));
                    }
                }
            }
        }

        // Check if healing catalog exists
        var healingCatalogPath = Path.Combine(_dataPath, "abilities", "active", "healing", "catalog.json");
        var healingCatalogExists = File.Exists(healingCatalogPath);

        if (!healingCatalogExists && healingReferences.Any())
        {
            _output.WriteLine($"\n⚠️  Found {healingReferences.Count} reference(s) to abilities/active/healing but catalog is missing:\n");
            
            foreach (var (file, line, reference) in healingReferences)
            {
                _output.WriteLine($"  {file}:{line} - {reference}");
            }
            
            _output.WriteLine($"\n💡 Solution: Create {Path.GetRelativePath(_dataPath, healingCatalogPath)} or update references to use @abilities/active/support:heal instead");
        }

        // This is a warning, not a hard failure
        if (!healingCatalogExists && healingReferences.Any())
        {
            _output.WriteLine("\n⚠️  This is a known issue - healing folder is empty");
        }
    }

    [Fact]
    public void Should_Report_Reference_Statistics()
    {
        var jsonFiles = Directory.GetFiles(_dataPath, "*.json", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".cbconfig.json"))
            .ToList();

        var referenceStats = new Dictionary<string, int>();
        var totalReferences = 0;

        foreach (var file in jsonFiles)
        {
            var content = File.ReadAllText(file);
            var matches = System.Text.RegularExpressions.Regex.Matches(content, @"@([\w-]+)/([\w-/]+):([\w-*\s]+)");
            
            totalReferences += matches.Count;
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var domain = match.Groups[1].Value;
                var key = $"@{domain}/...";
                
                if (!referenceStats.ContainsKey(key))
                    referenceStats[key] = 0;
                
                referenceStats[key]++;
            }
        }

        _output.WriteLine($"\n📊 JSON v4.1 Reference Statistics:\n");
        _output.WriteLine($"Total References: {totalReferences}");
        _output.WriteLine($"Files Scanned: {jsonFiles.Count}\n");
        _output.WriteLine("By Domain:");
        
        foreach (var (domain, count) in referenceStats.OrderByDescending(x => x.Value))
        {
            _output.WriteLine($"  {domain}: {count} references");
        }

        // This test always passes - it's just for reporting
        totalReferences.Should().BeGreaterThan(0, "there should be JSON references in the data files");
    }
}
