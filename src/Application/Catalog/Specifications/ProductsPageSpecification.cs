namespace Application.Catalog.Specifications;

public sealed class ProductsPageSpecification : Specification<Product, ProductDto>
{
    public ProductsPageSpecification(
        string? search,
        CategoryId? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? manufacturer,
        string? sort,
        int page,
        int pageSize)
    {
        Query.AsNoTracking();

        ApplyCommonFilters(Query, search, categoryId, minPrice, maxPrice, manufacturer);

        var ordered = Query.ApplySortingStrict(new ProductSortMap(), sort);
        ordered.ThenBy(p => p.Id);

        Query.Skip((page - 1) * pageSize).Take(pageSize);

        Query.Select(p => new ProductDto
        {
            Id           = p.Id.Value,
            Name         = p.Name,
            Manufacturer = p.Manufacturer,
            Amount       = p.Price.Amount,
            Currency     = p.Price.Currency,
            CategoryId   = p.CategoryId.Value,
            Photo        = p.Photo
        });
    }

    public static Specification<Product> ForCount(
        string? search,
        CategoryId? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? manufacturer)
    {
        var spec = new Specification<Product>();
        spec.Query.AsNoTracking();

        ApplyCommonFilters(spec.Query, search, categoryId, minPrice, maxPrice, manufacturer);

        return spec;
    }

    private static void ApplyCommonFilters(
        ISpecificationBuilder<Product> query,
        string? search,
        CategoryId? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? manufacturer)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var tokens = term.Split(' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && id > 0)
            {
                query.Where(p => p.Id == id);
            }
            else
            {
                foreach (var raw in tokens)
                {
                    var text = EscapeLike(raw);
                    var pattern = $"%{text}%";
                    query.Where(p =>
                        EF.Functions.ILike(p.Name, pattern, @"\") ||
                        EF.Functions.ILike(p.Manufacturer, pattern, @"\"));
                }
            }
        }

        if (categoryId is not null)
            query.Where(p => p.CategoryId == categoryId);

        if (minPrice.HasValue)
            query.Where(p => p.Price.Amount >= minPrice.Value);

        if (maxPrice.HasValue)
            query.Where(p => p.Price.Amount <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(manufacturer))
            query.Search(p => p.Manufacturer, $"%{manufacturer.Trim()}%");
    }

    static string EscapeLike(string text)
    {
        return text
            .Replace(@"\", @"\\", StringComparison.Ordinal)
            .Replace("%", @"\%",StringComparison.Ordinal)
            .Replace("_", @"\_",StringComparison.Ordinal);
    }
}
