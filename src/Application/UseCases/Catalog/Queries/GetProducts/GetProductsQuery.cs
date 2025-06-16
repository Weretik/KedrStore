namespace Application.UseCases.Catalog.Queries.GetProducts;

public sealed record GetProductsQuery : PagedQuery<AppResult<PagedResult<ProductDto>>>
{
    public string? SearchTerm { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int? CategoryId { get; init; }
    public string? Manufacturer { get; init; }

    public GetProductsQuery() { }

    public GetProductsQuery(
        string? searchTerm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int? categoryId = null,
        string? manufacturer = null,
        int pageNumber = 1,
        int pageSize = 20,
        string? sortBy = null,
        SortDirection sortDirection = default)
        : base(pageNumber, pageSize, sortBy, sortDirection)
    {
        SearchTerm = searchTerm;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
        CategoryId = categoryId;
        Manufacturer = manufacturer;
    }
}
