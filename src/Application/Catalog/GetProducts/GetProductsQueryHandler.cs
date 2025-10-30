using Application.Catalog.Shared;
using Domain.Catalog.Entities;

namespace Application.Catalog.GetProducts;

public class GetProductsQueryHandler(ICatalogReadRepository<Product> productRepository)
    : IQueryHandler<GetProductsQuery, Result<PaginationResult<ProductDto>>>
{
    public async ValueTask<Result<PaginationResult<ProductDto>>> Handle(
        GetProductsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var countSpec = new ProductsForCountSpec(query.Filter, query.PriceTypeId);
        var total = await productRepository.CountAsync(countSpec, cancellationToken);
        if (total == 0) return Result.NotFound();

        var pageSpec  = new ProductsPageSpec(query.Filter, query.ProductPagination, query.Sorter, query.PriceTypeId);
        var items = await productRepository.ListAsync(pageSpec, cancellationToken);

        return Result.Success(new PaginationResult<ProductDto>(items, total, query.ProductPagination));
    }
}
