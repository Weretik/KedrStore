using System.Reflection;
using BuildingBlocks.Domain.Abstractions;

namespace ArchitectureTests.Layers;

/// <summary>
/// Domain model rules (module rules «Domain model (DDD)»): every business module marks its roots with
/// <see cref="IAggregateRoot"/>; a root is created by a static factory, never by a public constructor; and a root holds
/// no other root, as a child or as a navigation, so children change only through their root and other aggregates are
/// referenced by id.
/// </summary>
public sealed class AggregateRootTests
{
    private static readonly string[] BusinessDomains =
        ["Catalog.Domain", "Content.Domain", "Media.Domain", "Sales.Domain", "Scheduling.Domain", "Work.Domain"];

    private static readonly Type[] Roots = BusinessDomains.Select(Assembly.Load)
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IAggregateRoot).IsAssignableFrom(type))
        .ToArray();

    [Fact]
    public void EveryBusinessModule_MarksItsRoots()
    {
        var withoutRoots = BusinessDomains
            .Where(domain => Roots.All(root => root.Assembly.GetName().Name != domain))
            .ToArray();

        Assert.Empty(withoutRoots);
    }

    [Fact]
    public void Roots_HaveNoPublicConstructor()
    {
        var violations = Roots
            .Where(root => root.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Length > 0)
            .Select(root => root.FullName!)
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void Roots_HoldNoOtherRoot()
    {
        var violations = Roots
            .SelectMany(root => root.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(property => HeldType(property.PropertyType) is { } held && held != root &&
                                   typeof(IAggregateRoot).IsAssignableFrom(held))
                .Select(property => $"{root.FullName}.{property.Name}"))
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    /// <summary>The domain class a property holds, alone or as the element of a collection.</summary>
    private static Type? HeldType(Type type)
    {
        var element = type.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(type)
            ? type.GetGenericArguments()[0]
            : type;
        return element.IsClass && element != typeof(string) ? element : null;
    }
}
