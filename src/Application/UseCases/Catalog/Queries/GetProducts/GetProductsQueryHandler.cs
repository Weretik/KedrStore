namespace Application.UseCases.Catalog.Queries.GetProducts;

public class GetProductsQueryHandler(
    IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetProductsQuery, AppResult<PagedResult<ProductDto>>>
{
    public async Task<AppResult<PagedResult<ProductDto>>> Handle(
        GetProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Создаем спецификацию для фильтрации
            var specification = new ProductFilterSpecification(
                searchTerm: request.SearchTerm,
                minPrice: request.MinPrice,
                maxPrice: request.MaxPrice,
                categoryId: request.CategoryId.HasValue ? new CategoryId(request.CategoryId.Value) : null,
                manufacturer: request.Manufacturer
            );

            // Получаем базовый запрос из репозитория
            var query = productRepository.GetAllProductAsync();

            // Применяем спецификацию
            query = query.ApplySpecification(specification);

            // Применяем сортировку
            query = query.ApplySort(request.SortBy, request.SortDirection);

            // Применяем пагинацию с маппингом в DTO
            var pagedResult = await query.ToPagedResultAsync<Product, ProductDto>(
                mapper,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            return AppResult<PagedResult<ProductDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return AppResult<PagedResult<ProductDto>>.Failure(
                AppErrors.System.Unexpected("Failed to retrieve products")
                    .WithDetails(ex.Message));
        }
    }
}
