namespace Application.Extensions;

public static class SortMapExtensions
{
    public static bool IsValidSort<TEntity>(this string? sort, ISortMap<TEntity> map)
    {
        ArgumentNullException.ThrowIfNull(map);
        if (string.IsNullOrWhiteSpace(sort)) return true;
        var parsed = SortParser.ParseStrict(sort, map!.DefaultKey!);

        return parsed.Any();
    }
}
