using System.Globalization;

namespace Application.Catalog.Specifications;

public sealed class ProductsPagedSpec : Specification<Product, ProductDto>
{
    public ProductsPagedSpec(
        string? search,
        CategoryId? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        string? manufacturer,
        string? sort,
        int page,
        int pageSize)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            Query.Search(p => p.Name, $"%{term}%")
                .Search(p => p.Manufacturer, $"%{term}%");

            if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pid))
                Query.Where(p => p.Id.Value == pid);
        }


        if (categoryId is not null)
            Query.Where(p => p.CategoryId == categoryId);

        if (minPrice.HasValue)
            Query.Where(p => p.Price.Amount >= minPrice.Value);

        if (maxPrice.HasValue)
            Query.Where(p => p.Price.Amount <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(manufacturer))
            Query.Search(p => p.Manufacturer, $"%{manufacturer!.Trim()}%");

        IOrderedSpecificationBuilder<Product>? ordered = null;
        foreach (var term in SortParser.Parse(sort))
        {
            var key = ProductSortMap.Keys[term.Field];
            ordered = ordered is null
                ? (term.isDesc ? Query.OrderByDescending(key) : Query.OrderBy(key))
                : (term.isDesc ? ordered.ThenByDescending(key) : ordered.ThenBy(key));
        }

        (ordered ?? Query.OrderBy(ProductSortMap.Keys[ProductSortMap.DefaultField]))
            .ThenBy(p => p.Id.Value); // стабильность

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
        string? search, CategoryId? categoryId,
        decimal? minPrice, decimal? maxPrice,
        string? manufacturer)
    {
        var spec = new Specification<Product>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            spec.Query.Search(p => p.Name, $"%{term}%")
                .Search(p => p.Manufacturer, $"%{term}%");

            if (int.TryParse(term, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pid))
                spec.Query.Where(p => p.Id.Value == pid);
        }


        if (categoryId is not null)
            spec.Query.Where(p => p.CategoryId == categoryId);

        if (minPrice.HasValue)
            spec.Query.Where(p => p.Price.Amount >= minPrice.Value);

        if (maxPrice.HasValue)
            spec.Query.Where(p => p.Price.Amount <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(manufacturer))
            spec.Query.Search(p => p.Manufacturer, $"%{manufacturer!.Trim()}%");

        return spec;
    }
}
