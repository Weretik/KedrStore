using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Category.Stats;

public static class CategoryStatsRollup
{
    /// <summary>Adds each category's direct stats to itself and every ancestor on its path (R-015).</summary>
    public static Dictionary<int, CategoryStats> Rollup(
        IEnumerable<ProductCategory> categories,
        IReadOnlyDictionary<int, CategoryStats> direct)
    {
        var categoryList = categories.ToList();
        var known = categoryList.Select(category => category.Id.Value).ToHashSet();
        var result = categoryList.ToDictionary(category => category.Id.Value, _ => CategoryStats.Empty);

        foreach (var category in categoryList)
        {
            if (!direct.TryGetValue(category.Id.Value, out var stats))
                continue;

            foreach (var ancestorId in PathIds(category.Path))
            {
                if (known.Contains(ancestorId))
                    result[ancestorId] = result[ancestorId].Add(stats);
            }
        }

        return result;
    }

    private static IEnumerable<int> PathIds(CategoryPath path)
        => path.Value.Split('.')
            .Select(segment => segment.TrimStart('n'))
            .Select(segment => int.TryParse(segment, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : 0)
            .Where(id => id > 0)
            .Distinct();
}
