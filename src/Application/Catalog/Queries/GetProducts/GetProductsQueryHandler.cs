namespace Application.Catalog.Queries.GetProducts;

public class GetProductsQueryHandler(
    ICatalogReadRepository<Product> productRepository)
    : IQueryHandler<GetProductsQuery, Result<PaginatedList<ProductDto>>>
{
    public async ValueTask<Result<PaginatedList<ProductDto>>> Handle(
        GetProductsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pageSpec  = new ProductsPageSpecification(query.Criteria);
        var countSpec = new ProductsForCountSpecification(query.Criteria);

        var items = await productRepository.ListAsync(pageSpec,  cancellationToken);
        var total = await productRepository.CountAsync(countSpec, cancellationToken);

        if (items.Count == 0)
        {
            return Result.NotFound();
        }

        var pageList = new PaginatedList<ProductDto>(
            items,
            total,
            query.Criteria.PageNumber,
            query.Criteria.PageSize);

        return Result.Success(pageList);


    }
}
