namespace Application.Catalog.Queries.GetProducts;

public class GetProductsQueryHandler(
    ICatalogReadRepository<Product> productRepository)
    : IQueryHandler<GetProductsQuery, Result<PaginatedList<ProductDto>>>
{
    public async ValueTask<Result<PaginatedList<ProductDto>>> Handle(
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

        if (items.Count == 0)
        {
            return Result.NotFound();
        }

        var pageList = new PaginatedList<ProductDto>(
            items,
            total,
            query.PageNumber,
            query.PageSize);

        return Result.Success(pageList);


    }
}
