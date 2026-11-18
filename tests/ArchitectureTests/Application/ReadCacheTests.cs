using System.Text.RegularExpressions;
using ArchitectureTests.TestSupport;

namespace ArchitectureTests.Application;

/// <summary>
/// Caching goes through one port (ADR-0016): modules and hosts use <c>IReadCache</c>; only
/// <c>BuildingBlocks.Infrastructure/Caching</c> touches <c>HybridCache</c>, <c>IMemoryCache</c> or <c>IDistributedCache</c>.
/// That keeps one way to name, expire, invalidate and trace a cached read, and lets an L2 be added in one place.
/// </summary>
public sealed partial class ReadCacheTests
{
    private static readonly string CachingAdapter =
        Path.Combine("src", "BuildingBlocks", "BuildingBlocks.Infrastructure", "Caching") + Path.DirectorySeparatorChar;

    [Fact]
    public void OnlyTheCachingAdapter_UsesCacheLibrariesDirectly()
    {
        var root = RepositoryRoot.Find();
        var violations = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(root, path))
            .Where(path => !path.StartsWith(CachingAdapter, StringComparison.Ordinal))
            .Where(path => File.ReadLines(Path.Combine(root, path))
                .Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal))
                .Any(line => CacheLibrary().IsMatch(line)))
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    /// <summary>
    /// The cached site models state (<c>SiteModelStateReader.Tag</c>) shows products, models, attributes and categories,
    /// so every Catalog command drops it, except those that change nothing it shows.
    /// </summary>
    [Fact]
    public void EveryCatalogCommand_DropsTheSiteModelsState()
    {
        string[] changeNothingItShows =
        [
            "UploadAttributeSwatchCommand.cs", // stores a picture file; the state holds no swatch
            "SaveCategoryContentCommand.cs", // SEO texts of a category; the state holds its names only
        ];
        var root = RepositoryRoot.Find();
        var features = Path.Combine(root, "src", "Modules", "Catalog", "Catalog.Application", "Features");
        var violations = Directory.EnumerateFiles(features, "*Command.cs", SearchOption.AllDirectories)
            .Where(path => !changeNothingItShows.Contains(Path.GetFileName(path)))
            .Where(path => !File.ReadAllText(path).Contains("SiteModelStateReader.Tag", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(root, path))
            .Order()
            .ToArray();

        Assert.Empty(violations);
    }

    [GeneratedRegex(@"\b(IMemoryCache|IDistributedCache|HybridCache|AddMemoryCache|AddDistributedMemoryCache)\b|Microsoft\.Extensions\.Caching")]
    private static partial Regex CacheLibrary();
}
