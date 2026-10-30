namespace Catalog.Application.Features.Category.Stats;

public sealed record CategoryStats(int ProductCount, decimal? MinPrice, decimal? MaxPrice)
{
    public static readonly CategoryStats Empty = new(0, null, null);

    public CategoryStats Add(CategoryStats other) => new(
        ProductCount + other.ProductCount,
        Min(MinPrice, other.MinPrice),
        Max(MaxPrice, other.MaxPrice));

    private static decimal? Min(decimal? left, decimal? right)
        => left is null ? right : right is null ? left : Math.Min(left.Value, right.Value);

    private static decimal? Max(decimal? left, decimal? right)
        => left is null ? right : right is null ? left : Math.Max(left.Value, right.Value);
}
