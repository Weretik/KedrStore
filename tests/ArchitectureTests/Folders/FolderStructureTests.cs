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

    /// <summary>
    /// <c>src/</c> holds the hosts, the shared code and the business modules, each module in
    /// <c>src/Modules/&lt;Module&gt;/</c> with its <c>Api</c>, <c>Application</c>, <c>Contracts</c>, <c>Domain</c> and
    /// <c>Infrastructure</c> projects.
    /// </summary>
    [Fact]
    public void Source_HasHostsSharedCodeAndModules_AndEveryModuleProjectSitsInModules()
    {
        var src = Path.Combine(FindRepositoryRoot(), "src");
        var topFolders = Directory.EnumerateDirectories(src).Select(Path.GetFileName).Order().ToArray();
        Assert.Equal(["Bootstrapper", "BuildingBlocks", "Modules"], topFolders);

        var misplaced = ApplicationAssemblies.Concat(ApiAssemblies)
            .Select(project => project[..project.IndexOf('.')])
            .Distinct()
            .Where(module => !Directory.Exists(Path.Combine(src, "Modules", module)))
            .ToArray();
        Assert.Empty(misplaced);
    }

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

    /// <summary>
    /// The folder axiom: a code folder holds at most six code files; past that it is split by meaning into named
    /// subfolders. No folder is empty, and none is a wrapper (no files and a single subfolder) unless it is a level
    /// of the layer layout. Generated code (EF <c>Migrations</c>, <c>Generated</c>) is exempt.
    /// </summary>
    [Theory]
    [InlineData("src")]
    [InlineData("tests")]
    public void CodeFolders_HoldAtMostSixFiles_AndAreNeitherEmptyNorWrappers(string top)
    {
        var root = Path.Combine(FindRepositoryRoot(), top);
        var violations = new List<string>();
        foreach (var folder in CodeFolders(root))
        {
            var relative = Path.GetRelativePath(root, folder).Replace('\\', '/');
            var files = Directory.GetFiles(folder);
            var subfolders = CodeFolders(folder, recursive: false).ToArray();
            var codeFiles = files.Count(file => file.EndsWith(".cs", StringComparison.Ordinal));

            if (codeFiles > MaxCodeFilesPerFolder)
                violations.Add($"{top}/{relative}: {codeFiles} code files, split it into named subfolders");
            if (files.Length == 0 && subfolders.Length == 0)
                violations.Add($"{top}/{relative}: empty folder, delete it");
            if (files.Length == 0 && subfolders.Length == 1 && !IsLayoutLevel(folder, subfolders[0]) &&
                Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories).Any())
                violations.Add($"{top}/{relative}: wrapper around {Path.GetFileName(subfolders[0])}/, flatten it");
        }

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations.Order()));
    }

    private const int MaxCodeFilesPerFolder = 6;

    private static readonly string[] LayoutLevels = ["Commands", "Queries", "Jobs", "Adapters", "Integrations"];

    private static bool IsLayoutLevel(string folder, string onlySubfolder)
        => LayoutLevels.Contains(Path.GetFileName(folder)) || LayoutLevels.Contains(Path.GetFileName(onlySubfolder));

    private static IEnumerable<string> CodeFolders(string root, bool recursive = true)
        => Directory.EnumerateDirectories(root, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(path => !Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar)
                .Any(segment => segment is "bin" or "obj" or "Migrations" or "Generated" or "TestResults" ||
                                segment.StartsWith('.')));

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

    /// <summary>
    /// Platform spec 005, R-001: a module's HTTP code is <c>Endpoints/&lt;Feature&gt;/</c> (endpoints, their
    /// requests and responses, and HTTP-only pieces such as the Work activity middleware), or registration.
    /// No module has controllers: MVC is not registered (TS-007).
    /// </summary>
    [Fact]
    public void Endpoints_LiveInFeatureFolders_AndNoModuleHasControllers()
    {
        var violations = ApiAssemblies.Select(Assembly.Load)
            .SelectMany(assembly =>
            {
                var name = assembly.GetName().Name!;
                var types = assembly.GetTypes().Where(type => !type.IsNested && type.Namespace is not null).ToArray();
                var endpoints = types.Where(type => type.Name.EndsWith("Endpoint", StringComparison.Ordinal)
                                                    || type.Name.EndsWith("Endpoints", StringComparison.Ordinal))
                    .ToArray();
                var misplaced = endpoints
                    .Where(type => !type.Namespace!.StartsWith($"{name}.Endpoints.", StringComparison.Ordinal) ||
                                   (type.Name.EndsWith("Endpoints", StringComparison.Ordinal)
                                       ? !EndpointFeatureNamespace.IsMatch(type.Namespace!)
                                       : type.Namespace != EndpointUseCaseNamespace(type)))
                    .Select(type => type.Name.EndsWith("Endpoints", StringComparison.Ordinal)
                        ? $"{type.FullName} is outside {name}.Endpoints.<Feature>"
                        : $"{type.FullName} is outside {name}.Endpoints.<Feature>.<UseCase> named by it");
                var leftovers = types.Where(type => typeof(ControllerBase).IsAssignableFrom(type))
                    .Select(type => $"{type.FullName} is a controller; modules expose Minimal API endpoints");
                return misplaced.Concat(leftovers);
            })
            .Order()
            .ToArray();

        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static readonly Regex EndpointFeatureNamespace =
        new(@"^\w+\.Api\.Endpoints\.\w+$", RegexOptions.CultureInvariant);

    private static string EndpointUseCaseNamespace(Type endpoint)
    {
        var feature = string.Join('.', endpoint.Namespace!.Split('.').Take(4));
        return $"{feature}.{endpoint.Name[..^"Endpoint".Length]}";
    }

    /// <summary>
    /// An HTTP feature folder is a table of contents, not a pile: <c>Endpoints/&lt;Feature&gt;/</c> holds only the
    /// route group <c>&lt;Feature&gt;Endpoints.cs</c>; each endpoint sits with its own request and response in
    /// <c>&lt;UseCase&gt;/</c>, and what several endpoints of the feature share sits in <c>Shared/</c>.
    /// </summary>
    [Fact]
    public void EndpointFeatureFolders_HoldOnlyTheRouteGroup_AndOneFolderPerEndpoint()
    {
        var modules = Path.Combine(FindRepositoryRoot(), "src", "Modules");
        var violations = Directory.EnumerateDirectories(modules, "Endpoints", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(segment => segment is "bin" or "obj"))
            .SelectMany(Directory.EnumerateDirectories)
            .SelectMany(feature => Directory.EnumerateFiles(feature, "*.cs")
                .Where(file => Path.GetFileName(file) != $"{Path.GetFileName(feature)}Endpoints.cs" &&
                               Directory.EnumerateFiles(feature, "*Endpoint.cs", SearchOption.AllDirectories).Any()))
            .Select(file => Path.GetRelativePath(modules, file).Replace('\\', '/'))
            .Order()
            .ToArray();

        Assert.True(violations.Length == 0,
            "Move each file into the endpoint's <UseCase>/ folder or the feature's Shared/: " +
            string.Join(", ", violations));
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
            return !(segments.Length >= 2 && segments[^2].EndsWith(".Infrastructure", StringComparison.Ordinal));
        }

        // A large aggregate splits its own value objects, enums and errors out of the entity's folder.
        if (name is "ValueObjects" or "Enums" && segments.Length >= 3 &&
            segments[^3].EndsWith(".Domain", StringComparison.Ordinal))
        {
            return false;
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
