namespace Application.Catalog.Queries.GetProducts;

public class GetProductsQueryHandler(
    ICatalogReadRepository<Product> productRepository)
    : IQueryHandler<GetProductsQuery, Result<PageResponse<ProductDto>>>
{
    public async ValueTask<Result<PageResponse<ProductDto>>> Handle(
        GetProductsQuery query, CancellationToken cancellationToken)
    {
        var pageSpec  = new ProductsPageSpecification(
            query.SearchTerm,
            query.CategoryId,
            query.MinPrice,
            query.MaxPrice,
            query.Manufacturer,
            query.Sort,
            query.PageNumber,
            query.PageSize);

        var countSpec = ProductsPageSpecification.ForCount(
            query.SearchTerm,
            query.CategoryId,
            query.MinPrice,
            query.MaxPrice,
            query.Manufacturer);

        var items = await productRepository.ListAsync(pageSpec,  cancellationToken);
        var total = await productRepository.CountAsync(countSpec, cancellationToken);

        if (total == 0)
        {
            return Result.NotFound("Нічого не знайдено за заданими фільтрами.");
        }

        var response = new PageResponse<ProductDto>(items, total, query.PageNumber, query.PageSize);
        return Result.Success(response);


    }
}
