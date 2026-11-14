using Catalog.Application.Features.Categories.Shared.Stats;

namespace Catalog.Application.Features.Categories.Shared.Tree;

/// <summary>Builds the category tree with product counts, shared by the public and admin category reads.</summary>
internal static class CategoryTree
{
    public static IReadOnlyList<CategoryTreeDto> Build(
        IEnumerable<ProductCategory> categories,
        string lang,
        IReadOnlyDictionary<int, CategoryStats> stats,
        bool hideEmpty)
    {
        int CountOf(int id) => stats.TryGetValue(id, out var value) ? value.ProductCount : 0;

        var items = categories.ToList()
            .Select(c => new
            {
                Id = c.Id.Value,
                c.Slug,
                c.Name,
                ParentId = c.ParentId?.Value,
                c.ShortNameUk,
                c.ShortNameRu,
                c.SortOrder,
                c.Level,
                Path = c.Path.Value
            })
            .ToList();

        static string? ParentKey(string path)
        {
            var i = path.LastIndexOf('.');
            return i >= 0 ? path[..i] : null;
        }
        var byPath = items.ToDictionary(i => i.Path, StringComparer.Ordinal);
        var lookup = items.ToLookup(i => ParentKey(i.Path));

        IReadOnlyList<CategoryTreeDto> BuildBranch(string? parentKey, HashSet<string> ancestors)
        {
            return lookup[parentKey]
                .Where(i => !hideEmpty || CountOf(i.Id) > 0)
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .Select(i => new CategoryTreeDto(
                    Id: i.Id,
                    Slug: i.Slug,
                    Name: string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase) ? i.ShortNameRu : i.ShortNameUk,
                    ParentId: i.ParentId ?? (ParentKey(i.Path) is { } parentPath && byPath.TryGetValue(parentPath, out var parent) ? parent.Id : null),
                    ShortNameUk: i.ShortNameUk,
                    ShortNameRu: i.ShortNameRu,
                    SortOrder: i.SortOrder,
                    Level: i.Level,
                    Children: ancestors.Contains(i.Path)
                        ? []
                        : BuildBranch(i.Path, new HashSet<string>(ancestors, StringComparer.Ordinal) { i.Path }),
                    ProductCount: CountOf(i.Id)
                ))
                .ToList();
        }

        return BuildBranch(null, new HashSet<string>(StringComparer.Ordinal));
    }
}
