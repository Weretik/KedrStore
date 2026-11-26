using System.Reflection;
using FluentValidation;
using Mediator;

namespace ArchitectureTests.Application;

/// <summary>
/// Every command or query that carries client input has a FluentValidation validator in its module, so the server
/// checks input itself whatever the client did. Messages without input or with only flags need none; the cache tags a command drops are not input.
/// </summary>
public sealed class ValidatorCoverageTests
{
    private static readonly string[] ApplicationAssemblies =
    [
        "Catalog.Application", "Content.Application", "Identity.Application", "Media.Application",
        "Sales.Application", "Scheduling.Application", "Work.Application",
    ];

    [Fact]
    public void EveryMessageWithInput_HasAValidator()
    {
        var missing = new List<string>();

        foreach (var assembly in ApplicationAssemblies.Select(Assembly.Load))
        {
            var types = assembly.GetTypes();
            var validated = types
                .Where(type => type is { IsAbstract: false, IsGenericTypeDefinition: false })
                .SelectMany(type => type.GetInterfaces())
                .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(type => type.GetGenericArguments()[0])
                .ToHashSet();

            missing.AddRange(types
                .Where(IsMessage)
                .Where(HasInput)
                .Where(type => !validated.Contains(type))
                .Select(type => type.FullName!));
        }

        Assert.True(missing.Count == 0, "Add a <UseCase>Validator for: " + string.Join(", ", missing.Order()));
    }

    private static bool IsMessage(Type type)
        => type is { IsAbstract: false, IsInterface: false }
           && type.GetInterfaces().Any(i => i == typeof(ICommand) || i == typeof(IQuery<>)
               || i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(ICommand<>)
                                      || i.GetGenericTypeDefinition() == typeof(IQuery<>)));

    private static bool HasInput(Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            // The cache tags a command drops are fixed by the code, not sent by the client.
            .Where(property => property.Name is not ("EqualityContract" or "InvalidatedCacheTags"))
            .Any(property => property.PropertyType != typeof(bool) && property.PropertyType != typeof(bool?));
}
