using System.Reflection;
using System.Runtime.CompilerServices;

namespace ArchitectureTests.Layers;

/// <summary>Domain state changes only through the aggregate's methods, so no Domain property has a public setter.</summary>
public sealed class DomainEncapsulationTests
{
    private static readonly string[] DomainAssemblies =
    [
        "BuildingBlocks.Domain", "Catalog.Domain", "Content.Domain", "Identity.Domain", "Media.Domain",
        "Sales.Domain", "Work.Domain",
    ];

    [Fact]
    public void DomainProperties_HaveNoPublicSetter()
    {
        var violations = DomainAssemblies.Select(Assembly.Load)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !IsRecord(type))
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(property => property.SetMethod is { IsPublic: true } setter && !IsInitOnly(setter))
                .Select(property => $"{type.FullName}.{property.Name}"))
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    private static bool IsInitOnly(MethodInfo setter)
        => setter.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(IsExternalInit));

    private static bool IsRecord(Type type) => type.GetMethod("<Clone>$") is not null;
}
