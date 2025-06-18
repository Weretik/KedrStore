namespace Application.Catalog.Queries.GetProducts;

public class GetProductsQueryHandler(
    IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetProductsQuery, AppResult<PagedResult<ProductDto>>>
{
    public async Task<AppResult<PagedResult<ProductDto>>> Handle(
        GetProductsQuery request, CancellationToken cancellationToken)
    {
        var specification = new ProductFilterSpecification(
            searchTerm: request.SearchTerm,
            minPrice: request.MinPrice,
            maxPrice: request.MaxPrice,
            categoryId: request.CategoryId.HasValue ? new CategoryId(request.CategoryId.Value) : null,
            manufacturer: request.Manufacturer
        );

        var query = productRepository.GetAllProductAsync()
            .ApplySpecification(specification)
            .ApplySort(request.SortBy, request.SortDirection);

        var pagedResult = await query.ToPagedResultAsync<Product, ProductDto>(
            mapper,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return AppResult<PagedResult<ProductDto>>.Success(pagedResult);
    }
}
