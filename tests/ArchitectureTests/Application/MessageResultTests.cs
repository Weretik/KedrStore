using System.Reflection;
using Mediator;

namespace ArchitectureTests.Application;

/// <summary>
/// Every command and query answers an <c>Ardalis.Result</c>, so business failures and validation errors travel as
/// results; the validation behavior never has to fall back to throwing.
/// </summary>
public sealed class MessageResultTests
{
    private static readonly string[] ApplicationAssemblies =
    [
        "Catalog.Application", "Content.Application", "Identity.Application", "Media.Application",
        "Sales.Application", "Work.Application",
    ];

    [Fact]
    public void EveryCommandAndQuery_ReturnsAResult()
    {
        var violations = ApplicationAssemblies.Select(Assembly.Load)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(i => i == typeof(ICommand) || i.IsGenericType &&
                    (i.GetGenericTypeDefinition() == typeof(ICommand<>) || i.GetGenericTypeDefinition() == typeof(IQuery<>)))
                .Where(i => !i.IsGenericType || !IsResult(i.GetGenericArguments()[0]))
                .Select(_ => type.FullName!))
            .Distinct()
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    private static bool IsResult(Type type)
        => type == typeof(Ardalis.Result.Result) ||
           type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Ardalis.Result.Result<>);
}
