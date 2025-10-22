namespace Application.Catalog.Sorting;

public sealed class ProductSortMap : ISortMap<Product>
{
    public IReadOnlyDictionary<string, Expression<Func<Product, object?>>> Keys { get; } =
        new Dictionary<string, Expression<Func<Product, object?>>>(StringComparer.OrdinalIgnoreCase)
        {
            ["id"]           = p => p.Id.Value,
            ["name"]         = p => p.Name,
            ["price"]        = p => p.Price.Amount,
            ["category"]     = p => p.CategoryId.Value,
        };

    public string DefaultKey => "name";
}
