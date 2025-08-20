namespace Application.Catalog.Sorting;

public sealed class ProductSortMap : ISortMap<Product>
{
    public IReadOnlyDictionary<string, Expression<Func<Product, object?>>> Keys { get; } =
        new Dictionary<string, Expression<Func<Product, object?>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = p => p.Name,
            ["price"] = p => p.Price.Amount,
            ["manufacturer"] = p => p.Manufacturer,
        };

    public string DefaultKey => "name";
}
