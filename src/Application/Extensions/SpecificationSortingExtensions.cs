namespace Application.Extensions;

public static class SpecificationSortingExtensions
{
    public static IOrderedSpecificationBuilder<TEntity> ApplySortingStrict<TEntity>(
        this ISpecificationBuilder<TEntity> specification,
        ISortMap<TEntity> map,
        string? sort)
        where TEntity : class
    {

        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(map);

        var tokens = SortParser.ParseStrict(sort, map.DefaultKey);
        IOrderedSpecificationBuilder<TEntity>? ordered = null;

        foreach (var token in tokens)
        {
            if (!map.Keys.TryGetValue(token.Key, out var expr))
                continue;

            ordered = ordered is null
                ? token.Direction == SortDirection.Desc ? specification.OrderByDescending(expr) : specification.OrderBy(expr)
                : token.Direction == SortDirection.Desc ? ordered.ThenByDescending(expr) : ordered.ThenBy(expr);
        }

        ordered ??= specification.OrderBy(map.Keys[map.DefaultKey]);
        return ordered;
    }
}
