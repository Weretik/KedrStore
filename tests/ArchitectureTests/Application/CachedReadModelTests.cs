using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using ArchitectureTests.TestSupport;
using BuildingBlocks.Domain.Abstractions;

namespace ArchitectureTests.Application;

/// <summary>
/// The read cache keeps CQRS reads only (ADR-0016): a cached value is a read model, never a domain entity. Every cached
/// type is marked <c>[ImmutableObject(true)]</c> (HybridCache then shares the instance), and nothing reachable from it may
/// be an aggregate or another entity class of a <c>*.Domain</c> assembly. Immutable domain values (records, enums,
/// structs such as typed ids) are allowed, the same as in responses.
/// </summary>
public sealed partial class CachedReadModelTests
{
    private static readonly Assembly[] CachingAssemblies =
    [
        typeof(Catalog.Application.CatalogApplicationAssemblyMarker).Assembly,
    ];

    [Fact]
    public void EveryCachedType_IsInACheckedAssembly()
    {
        var root = RepositoryRoot.Find();
        var marked = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"))
            .Sum(path => ImmutableMark().Count(File.ReadAllText(path)));

        // A mismatch means a new [ImmutableObject(true)] type lives in an assembly missing from CachingAssemblies.
        Assert.Equal(marked, CachedTypes().Count());
    }

    [Fact]
    public void CachedTypes_HoldNoDomainEntities()
    {
        var violations = CachedTypes()
            .SelectMany(type => Reachable(type).Where(IsEntity).Select(entity => $"{type.FullName} -> {entity.FullName}"))
            .Distinct()
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    private static IEnumerable<Type> CachedTypes() => CachingAssemblies
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => type.GetCustomAttribute<ImmutableObjectAttribute>()?.Immutable == true);

    private static bool IsEntity(Type type) =>
        typeof(IAggregateRoot).IsAssignableFrom(type)
        || (type.IsClass && type.Assembly.GetName().Name!.EndsWith(".Domain", StringComparison.Ordinal) && !IsRecord(type));

    private static bool IsRecord(Type type) => type.GetMethod("<Clone>$") is not null;

    private static HashSet<Type> Reachable(Type start)
    {
        var seen = new HashSet<Type>();
        var queue = new Queue<Type>([start]);
        while (queue.TryDequeue(out var type))
        {
            if (type.IsArray) { queue.Enqueue(type.GetElementType()!); continue; }
            if (type.IsGenericType) foreach (var argument in type.GetGenericArguments()) queue.Enqueue(argument);
            if (type.Namespace?.StartsWith("System", StringComparison.Ordinal) == true || !seen.Add(type)) continue;
            const BindingFlags members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var property in type.GetProperties(members)) queue.Enqueue(property.PropertyType);
            foreach (var field in type.GetFields(members)) queue.Enqueue(field.FieldType);
        }

        return seen;
    }

    [GeneratedRegex(@"\[(System\.ComponentModel\.)?ImmutableObject\(true\)\]")]
    private static partial Regex ImmutableMark();
}
