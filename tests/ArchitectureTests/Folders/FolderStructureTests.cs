using System.Reflection;
using Mediator;
using Microsoft.AspNetCore.Mvc;

using System.Text.RegularExpressions;

namespace ArchitectureTests.Folders;

/// <summary>
/// Folders are named by feature, aggregate or concern, never by type or as a dump; use cases sit in
/// <c>Features/&lt;Feature&gt;/Commands|Queries/&lt;UseCase&gt;/</c>, one handler per use case, named
/// <c>&lt;UseCase&gt;Handler</c>; controllers sit in <c>Features/&lt;Feature&gt;/</c> and use no Infrastructure type.
/// </summary>
public sealed class FolderStructureTests
{
    private static readonly string[] TypeOrDumpFolders =
    [
        "Helpers", "Utils", "Utilities", "Extensions", "Services", "Common", "Entities", "Enums", "ValueObjects",
        "Configurations", "Configuration", "DTOs", "Dtos", "Validators", "Controllers", "Repositories", "DataBase",
        "Database", "Models", "Interfaces", "Constants", "Exceptions",
    ];

    private static readonly string[] ApplicationAssemblies =
    [
        "Catalog.Application", "Content.Application", "Identity.Application", "Media.Application",
        "Sales.Application", "Work.Application",
    ];

    private static readonly string[] ApiAssemblies =
        ["Catalog.Api", "Content.Api", "Identity.Api", "Media.Api", "Sales.Api", "Work.Api"];

    [Fact]
    public void SourceFolders_AreNotNamedByTypeOrAsADump()
    {
        var src = Path.Combine(FindRepositoryRoot(), "src");
        var violations = Directory.EnumerateDirectories(src, "*", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(src, path).Replace('\\', '/'))
            .Where(path => !path.Split('/').Any(segment => segment is "bin" or "obj" or "Migrations" or "Generated"))
            .Where(path => IsForbidden(path.Split('/')))
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void UseCaseHandlers_SitOnePerUseCaseFolderAndAreNamedByIt()
    {
        var handlers = ApplicationAssemblies.Select(Assembly.Load)
            .SelectMany(assembly => assembly.GetTypes())
            .Where(IsUseCaseHandler)
            .ToArray();

        var misplaced = handlers
            .Where(type => UseCaseNamespace.Match(type.Namespace ?? "") is not { Success: true } match ||
                           type.Name != match.Groups["useCase"].Value + "Handler")
            .Select(type => type.FullName!);
        var crowded = handlers.GroupBy(type => type.Namespace)
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key}: {group.Count()} handlers");

        var violations = misplaced.Concat(crowded).Order().ToArray();
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void UseCases_DoNotDependOnAnotherUseCase()
    {
        var src = Path.Combine(FindRepositoryRoot(), "src");
        var violations = Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
            .Select(path => (Path: path, Folder: UseCaseFolder.Match(path.Replace('\\', '/'))))
            .Where(file => file.Folder.Success)
            .SelectMany(file => UseCaseReference.Matches(File.ReadAllText(file.Path))
                .Where(reference => reference.Groups["useCase"].Value != file.Folder.Groups["useCase"].Value)
                .Select(reference => $"{Path.GetRelativePath(src, file.Path)} -> {reference.Value}"))
            .Order()
            .ToArray();

        Assert.True(violations.Length == 0,
            "Move what several use cases share to the feature's Shared/ folder: " + string.Join(", ", violations));
    }

    private static readonly Regex UseCaseFolder =
        new(@"/\w+\.Application/Features/\w+/(Commands|Queries)/(?<useCase>\w+)/[^/]+\.cs$", RegexOptions.CultureInvariant);

    private static readonly Regex UseCaseReference =
        new(@"\b\w+\.Application\.Features\.\w+\.(Commands|Queries)\.(?<useCase>\w+)\b", RegexOptions.CultureInvariant);

    [Fact]
    public void Controllers_LiveInFeatureFolders()
    {
        var violations = ApiAssemblies.Select(Assembly.Load)
            .SelectMany(assembly => assembly.GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract)
                .Where(type => type.Namespace?.StartsWith($"{assembly.GetName().Name}.Features.",
                    StringComparison.Ordinal) != true))
            .Select(type => type.FullName!)
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void ApiCode_UsesInfrastructureOnlyForRegistration()
    {
        var violations = ApiAssemblies.Select(Assembly.Load)
            .SelectMany(assembly => NetArchTest.Rules.Types.InAssembly(assembly)
                .That().DoNotResideInNamespaceEndingWith(".DependencyInjection")
                .ShouldNot().HaveDependencyOnAny(ApiAssemblies.Select(api => api.Replace(".Api", ".Infrastructure"))
                    .ToArray())
                .GetResult().FailingTypeNames ?? [])
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    private static readonly Regex UseCaseNamespace =
        new(@"^\w+\.Application\.Features\.\w+\.(Commands|Queries)\.(?<useCase>\w+)$", RegexOptions.CultureInvariant);

    private static bool IsForbidden(string[] segments)
    {
        var name = segments[^1];
        if (name == "Contracts")
        {
            // A module's own Contracts project, or Infrastructure/Contracts/ that implements it, is allowed.
            return !(segments.Length == 3 && segments[1].EndsWith(".Infrastructure", StringComparison.Ordinal));
        }

        return TypeOrDumpFolders.Contains(name, StringComparer.Ordinal);
    }

    private static bool IsUseCaseHandler(Type type)
        => type is { IsAbstract: false, IsInterface: false } && type.GetInterfaces().Any(i => i.IsGenericType &&
            (i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) ||
             i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)));

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "KedrStore.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("KedrStore.sln was not found.");
    }
}
