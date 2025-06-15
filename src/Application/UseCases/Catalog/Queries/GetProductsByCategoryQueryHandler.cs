namespace Application.UseCases.Catalog.Queries;

public sealed class GetProductsByCategoryQueryHandler(
    IProductRepository productRepository, IMapper mapper)
    : IQueryHandler<GetProductsByCategoryQuery, List<ProductDto>>
{
    public async Task<List<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var products = await productRepository.GetByCategoryIdAsync(
            request.CategoryId,
            cancellationToken);

        return mapper.Map<List<ProductDto>>(products);
    }
}

