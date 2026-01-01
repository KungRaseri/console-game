using FluentAssertions;
using RealmEngine.Data.Services;
using Microsoft.Extensions.Caching.Memory;

namespace RealmEngine.Data.Tests.Services;

[Trait("Category", "Unit")]
public class GameDataCacheDomainHierarchyTests : IDisposable
{
    private readonly GameDataCache _cache;
    private readonly string _testDataPath;

    public GameDataCacheDomainHierarchyTests()
    {
        // Get the solution root and navigate to the data directory
        var currentDir = Directory.GetCurrentDirectory();
        var solutionRoot = currentDir;
        while (solutionRoot != null && !File.Exists(Path.Combine(solutionRoot, "RealmEngine.sln")))
        {
            solutionRoot = Directory.GetParent(solutionRoot)?.FullName;
        }

        if (solutionRoot == null)
            throw new InvalidOperationException("Could not find solution root");

        _testDataPath = Path.Combine(solutionRoot, "RealmEngine.Data", "Data", "Json");

        if (!Directory.Exists(_testDataPath))
            throw new DirectoryNotFoundException($"Data directory not found: {_testDataPath}");

        _cache = new GameDataCache(_testDataPath, new MemoryCache(new MemoryCacheOptions()));
        _cache.LoadAllData();
    }

    [Fact]
    public void GetAllDomains_Should_Return_Expected_Domains()
    {
        // Act
        var domains = _cache.GetAllDomains();

        // Assert
        domains.Should().NotBeEmpty();
        domains.Should().Contain("abilities");
        domains.Should().Contain("npcs");
        domains.Should().Contain("items");
    }

    [Fact]
    public void GetSubdomainsForDomain_Should_Return_Subdomains_For_Abilities()
    {
        // Act
        var subdomains = _cache.GetSubdomainsForDomain("abilities");

        // Assert
        subdomains.Should().NotBeEmpty();
        subdomains.Should().Contain("active");
        subdomains.Should().Contain("passive");
    }

    [Fact]
    public void GetSubdomainsForDomain_Should_Return_Subdomains_For_Npcs()
    {
        // Act
        var subdomains = _cache.GetSubdomainsForDomain("npcs");

        // Assert
        subdomains.Should().NotBeEmpty();
        subdomains.Should().Contain("common");
        subdomains.Should().Contain("noble");
    }

    [Fact]
    public void GetSubdomainsForDomain_Should_Return_Empty_For_NonExistent_Domain()
    {
        // Act
        var subdomains = _cache.GetSubdomainsForDomain("nonexistent");

        // Assert
        subdomains.Should().BeEmpty();
    }

    [Fact]
    public void GetFilesBySubdomain_Should_Return_Files_For_Abilities_Active()
    {
        // Act
        var files = _cache.GetFilesBySubdomain("abilities", "active");

        // Assert
        files.Should().NotBeEmpty();
        files.All(f => f.Domain == "abilities").Should().BeTrue();
        files.All(f => f.Subdomain == "active").Should().BeTrue();
    }

    [Fact]
    public void GetFilesBySubdomain_Should_Return_Empty_For_NonExistent_Combination()
    {
        // Act
        var files = _cache.GetFilesBySubdomain("abilities", "nonexistent");

        // Assert
        files.Should().BeEmpty();
    }

    [Fact]
    public void GetDomainHierarchy_Should_Return_Complete_Structure()
    {
        // Act
        var hierarchy = _cache.GetDomainHierarchy();

        // Assert
        hierarchy.Should().NotBeEmpty();
        hierarchy.Should().ContainKey("abilities");

        if (hierarchy.TryGetValue("abilities", out var abilitiesSubdomains))
        {
            abilitiesSubdomains.Should().ContainKey("active");
            abilitiesSubdomains.Should().ContainKey("passive");
        }
    }

    [Fact]
    public void CachedJsonFile_Should_Have_Subdomain_Property()
    {
        // Act
        var file = _cache.GetFile("abilities/active/offensive/catalog.json");

        // Assert
        file.Should().NotBeNull();
        file!.Domain.Should().Be("abilities");
        file.Subdomain.Should().Be("active");
    }

    public void Dispose()
    {
        _cache?.Dispose();
    }
}