namespace Application.Catalog.Queries;

public sealed class GetProductsByCategoryQueryHandler(
    IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetProductsByCategoryQuery, AppResult<List<ProductDto>>>
{
    public async Task<AppResult<List<ProductDto>>> Handle(
        GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var products = await productRepository.GetByCategoryIdAsync(
            request.CategoryId, cancellationToken);

        if (!products.Any())
        {
            return AppResult.Failure<List<ProductDto>>(
                AppErrors.System.NotFound
                    .WithDetails($"Product {request.CategoryId.Value.ToString()}"));
        }
        var result = mapper.Map<List<ProductDto>>(products);

        return AppResult.Success(result);
    }
}

