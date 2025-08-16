namespace Application.Common.Sorting;

public static class ProductSortMap
{
    public static readonly IReadOnlyDictionary<string, Expression<Func<Product, object?>>> Keys
        = new Dictionary<string, Expression<Func<Product, object?>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"]         = p => p.Name,
            ["price"]        = p => p.Price.Amount,
            ["manufacturer"] = p => p.Manufacturer,
        };

    public const string DefaultField = "name";
}
