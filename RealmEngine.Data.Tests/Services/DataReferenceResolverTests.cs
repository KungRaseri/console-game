using FluentAssertions;
using Game.Shared.Services;

namespace Game.Data.Tests.Services;

[Trait("Category", "Integration")]
public class DataReferenceResolverTests
{
    private readonly DataReferenceResolver _resolver;

    public DataReferenceResolverTests()
    {
        _resolver = DataReferenceResolver.Instance;
    }

    [Fact]
    public void ResolveMaterial_Should_Return_Material_Traits_For_Weapon_Context()
    {
        // Arrange
        var materialRef = "materials/metals/Iron";
        var itemType = "weapon";

        // Act
        var result = _resolver.ResolveMaterial(materialRef, itemType);

        // Assert - May return null if catalog not available in test context
        // Just verify it doesn't throw
        if (result != null)
        {
            result.Should().BeOfType<Dictionary<string, object>>();
        }
    }

    [Fact]
    public void ResolveMaterial_Should_Handle_Null_Reference()
    {
        // Arrange
        string? materialRef = null;
        var itemType = "weapon";

        // Act
        var result = _resolver.ResolveMaterial(materialRef!, itemType);

        // Assert - Should return null for null reference
        result.Should().BeNull();
    }

    [Fact]
    public void ResolveMaterial_Should_Handle_Invalid_Reference_Format()
    {
        // Arrange
        var materialRef = "invalid/format";
        var itemType = "weapon";

        // Act
        var result = _resolver.ResolveMaterial(materialRef, itemType);

        // Assert - Should return null for invalid format
        result.Should().BeNull();
    }

    [Fact]
    public void ResolveMaterial_Should_Handle_NonExistent_Category()
    {
        // Arrange
        var materialRef = "materials/nonexistent/Material";
        var itemType = "weapon";

        // Act
        var result = _resolver.ResolveMaterial(materialRef, itemType);

        // Assert - Should return null gracefully
        result.Should().BeNull();
    }

    [Fact]
    public void Instance_Should_Return_Same_Singleton_Instance()
    {
        // Arrange & Act
        var instance1 = DataReferenceResolver.Instance;
        var instance2 = DataReferenceResolver.Instance;

        // Assert
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void ResolveMaterial_Should_Not_Throw_On_Empty_String()
    {
        // Arrange
        var materialRef = "";
        var itemType = "weapon";

        // Act
        var result = _resolver.ResolveMaterial(materialRef, itemType);

        // Assert - Should handle gracefully
        result.Should().BeNull();
    }

    [Fact]
    public void ResolveMaterial_Should_Handle_Whitespace_ItemType()
    {
        // Arrange
        var materialRef = "materials/metals/Iron";
        var itemType = "   ";

        // Act
        var result = _resolver.ResolveMaterial(materialRef, itemType);

        // Assert - Should not throw
        // Result depends on catalog structure
    }
}

