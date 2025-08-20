namespace Application.Extensions;

public static class SpecificationSortingExtensions
{
    public static IOrderedSpecificationBuilder<TEntity>? ApplySorting<TEntity>(
        this ISpecificationBuilder<TEntity> specification,
        ISortMap<TEntity> map,
        string? sort)
    {
        IOrderedSpecificationBuilder<TEntity>? ordered = null;
        foreach (var (key, desc) in SortParser.Parse(sort, map.DefaultKey))
        {
            if (!map.Keys.TryGetValue(key, out var expr)) continue;

            ordered = ordered is null
                ? (desc ? specification.OrderByDescending(expr) : specification.OrderBy(expr))
                : (desc ? ordered.ThenByDescending(expr) : ordered.ThenBy(expr));
        }

        return ordered;
    }
}
