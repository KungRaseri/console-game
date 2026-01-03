using FluentAssertions;
using RealmEngine.Data.Services;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace RealmEngine.Core.Tests.Services;

/// <summary>
/// Integration tests for GameDataCache that validate ALL production JSON files load successfully.
/// </summary>
[Trait("Category", "Integration")]
public class GameDataCacheProductionDataTests
{
    private readonly ITestOutputHelper _output;
    private readonly string _productionDataPath;

    public GameDataCacheProductionDataTests(ITestOutputHelper output)
    {
        _output = output;
        _productionDataPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../package/Data/Json"));
    }

    [Fact]
    public void Should_Load_All_Production_JSON_Files_Without_Errors()
    {
        // Arrange
        _output.WriteLine($"Testing production data from: {_productionDataPath}");
        _output.WriteLine($"Directory exists: {Directory.Exists(_productionDataPath)}");
        
        Directory.Exists(_productionDataPath).Should().BeTrue("production data directory must exist");
        
        var cache = new GameDataCache(_productionDataPath);

        // Act - this should throw if ANY file fails to load
        Action loadAction = () => cache.LoadAllData();

        // Assert
        loadAction.Should().NotThrow("all production JSON files must load without errors");
        
        // Verify we loaded files
        var stats = cache.GetStats();
        _output.WriteLine($"Loaded {stats.TotalFiles} files");
        stats.TotalFiles.Should().BeGreaterThan(0, "should have loaded data from files");
    }

    [Fact]
    public void Should_Report_Expected_Production_File_Count()
    {
        // Arrange
        var expectedFiles = Directory.GetFiles(_productionDataPath, "*.json", SearchOption.AllDirectories).Length;
        _output.WriteLine($"Expected file count: {expectedFiles}");
        
        expectedFiles.Should().BeGreaterThan(100, "production data should contain a significant number of JSON files");
        
        var cache = new GameDataCache(_productionDataPath);

        // Act
        cache.LoadAllData();

        // Assert
        var stats = cache.GetStats();
        _output.WriteLine($"Loaded {stats.TotalFiles} files (expected: {expectedFiles})");
        
        // All JSON files should be loaded
        stats.TotalFiles.Should().Be(expectedFiles, "all JSON files should be loaded into cache");
    }

    [Fact]
    public void Should_Load_All_Major_Domains()
    {
        // Arrange
        var expectedDomains = new[]
        {
            "abilities",
            "classes",
            "enemies",
            "items",
            "npcs",
            "organizations",
            "quests",
            "social",
            "world",
            "general"
        };
        
        var cache = new GameDataCache(_productionDataPath);

        // Act
        cache.LoadAllData();

        // Assert - verify each domain has files loaded
        foreach (var domain in expectedDomains)
        {
            var domainFiles = cache.GetFilesByDomain(domain).ToList();
            _output.WriteLine($"{domain}: {domainFiles.Count} files");
            domainFiles.Should().NotBeEmpty($"{domain} domain should have files loaded");
        }
    }
}
