using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RealmEngine.Core.Generators;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json.Linq;

namespace RealmEngine.Core.Tests.Generators;

/// <summary>
/// Tests for the NameComposer utility that handles pattern-based name generation.
/// </summary>
public class NameComposerTests
{
    private readonly ITestOutputHelper _output;
    private readonly NameComposer _composer;

    public NameComposerTests(ITestOutputHelper output)
    {
        _output = output;
        _composer = new NameComposer(NullLogger<NameComposer>.Instance);
    }

    [Fact]
    public void Should_Compose_Simple_Base_Name()
    {
        // Arrange
        var components = JToken.Parse(@"
        {
            ""base"": [
                { ""value"": ""Wolf"", ""rarityWeight"": 100 }
            ]
        }");

        // Act
        var name = _composer.ComposeName("base", components, out var componentValues);

        // Assert
        name.Should().Be("Wolf");
        componentValues.Should().ContainKey("base");
        componentValues["base"].Should().Be("Wolf");
        
        _output.WriteLine($"Name: {name}");
        _output.WriteLine($"Components: {string.Join(", ", componentValues.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }

    [Fact]
    public void Should_Compose_Name_With_Multiple_Tokens()
    {
        // Arrange
        var components = JToken.Parse(@"
        {
            ""size"": [
                { ""value"": ""Giant"", ""rarityWeight"": 50 }
            ],
            ""type"": [
                { ""value"": ""Frost"", ""rarityWeight"": 30 }
            ],
            ""base"": [
                { ""value"": ""Wolf"", ""rarityWeight"": 100 }
            ]
        }");

        // Act
        var name = _composer.ComposeName("{size} {type} {base}", components, out var componentValues);

        // Assert
        name.Should().Be("Giant Frost Wolf");
        componentValues.Should().ContainKey("size");
        componentValues.Should().ContainKey("type");
        componentValues.Should().ContainKey("base");
        componentValues["size"].Should().Be("Giant");
        componentValues["type"].Should().Be("Frost");
        componentValues["base"].Should().Be("Wolf");
        
        _output.WriteLine($"Name: {name}");
        _output.WriteLine($"Components: {string.Join(", ", componentValues.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }

    [Fact]
    public void Should_Handle_Missing_Optional_Tokens()
    {
        // Arrange
        var components = JToken.Parse(@"
        {
            ""base"": [
                { ""value"": ""Wolf"", ""rarityWeight"": 100 }
            ],
            ""title"": [
                { ""value"": ""the Devourer"", ""rarityWeight"": 50 }
            ]
        }");

        // Act - pattern includes {type} but it's not in components
        var name = _composer.ComposeName("{type} {base} {title}", components, out var componentValues);

        // Assert
        name.Should().Be("Wolf the Devourer");
        componentValues.Should().ContainKey("base");
        componentValues.Should().ContainKey("title");
        componentValues.Should().NotContainKey("type"); // Missing token not added
        
        _output.WriteLine($"Name: {name}");
        _output.WriteLine($"Components: {string.Join(", ", componentValues.Select(kv => $"{kv.Key}={kv.Value}"))}");
    }

    [Fact]
    public void Should_Select_Random_Pattern_By_Weight()
    {
        // Arrange
        var patterns = JToken.Parse(@"[
            { ""pattern"": ""base"", ""rarityWeight"": 50 },
            { ""pattern"": ""{size} {base}"", ""rarityWeight"": 40 },
            { ""pattern"": ""{type} {base}"", ""rarityWeight"": 30 }
        ]");

        // Act
        var results = new Dictionary<string, int>();
        for (int i = 0; i < 100; i++)
        {
            var pattern = _composer.GetRandomWeightedPattern(patterns);
            var patternStr = pattern?["pattern"]?.Value<string>() ?? "";
            
            if (!results.ContainsKey(patternStr))
                results[patternStr] = 0;
            results[patternStr]++;
        }

        // Assert
        results.Should().ContainKey("base");
        results.Should().ContainKey("{size} {base}");
        results.Should().ContainKey("{type} {base}");
        
        // More common patterns should appear more often (not strict since it's random)
        _output.WriteLine("\nPattern selection distribution (100 samples):");
        foreach (var result in results.OrderByDescending(kv => kv.Value))
        {
            _output.WriteLine($"  {result.Key}: {result.Value} times");
        }
    }

    [Fact]
    public void Should_Compose_Ability_Name_With_Power_And_School()
    {
        // Arrange
        var components = JToken.Parse(@"
        {
            ""power"": [
                { ""value"": ""Greater"", ""rarityWeight"": 30 }
            ],
            ""school"": [
                { ""value"": ""Frost"", ""rarityWeight"": 50 }
            ],
            ""base"": [
                { ""value"": ""Bolt"", ""rarityWeight"": 100 }
            ]
        }");

        // Act
        var name = _composer.ComposeName("{power} {school} {base}", components, out var componentValues);

        // Assert
        name.Should().Be("Greater Frost Bolt");
        componentValues["power"].Should().Be("Greater");
        componentValues["school"].Should().Be("Frost");
        componentValues["base"].Should().Be("Bolt");
        
        _output.WriteLine($"Ability Name: {name}");
    }

    [Fact]
    public void Should_Compose_NPC_Name_With_Title_Prefix_And_Suffix()
    {
        // Arrange
        var components = JToken.Parse(@"
        {
            ""title_prefix"": [
                { ""value"": ""Master"", ""rarityWeight"": 40 }
            ],
            ""base"": [
                { ""value"": ""Garrick"", ""rarityWeight"": 100 }
            ],
            ""title_suffix"": [
                { ""value"": ""the Wise"", ""rarityWeight"": 30 }
            ]
        }");

        // Act
        var name = _composer.ComposeName("{title_prefix} {base} {title_suffix}", components, out var componentValues);

        // Assert
        name.Should().Be("Master Garrick the Wise");
        componentValues["title_prefix"].Should().Be("Master");
        componentValues["base"].Should().Be("Garrick");
        componentValues["title_suffix"].Should().Be("the Wise");
        
        _output.WriteLine($"NPC Name: {name}");
    }
}
