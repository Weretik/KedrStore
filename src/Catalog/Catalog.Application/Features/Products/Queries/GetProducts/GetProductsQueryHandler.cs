using Catalog.Application.Features.Shared;
using Catalog.Application.Persistance;
using Catalog.Application.Shared;
using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler(ICatalogReadRepository<Product> productRepository)
    : IQueryHandler<GetProductsQuery, Result<GetProductsQueryResult<ProductDto>>>
{
    public async ValueTask<Result<GetProductsQueryResult<ProductDto>>> Handle(
        GetProductsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var countSpec = new ProductsForCountSpec(query.Filter, query.PricingOptions);
        var total = await productRepository.CountAsync(countSpec, cancellationToken);
        if (total == 0) return Result.NotFound();

        var pageSpec  = new ProductsPageSpec(
            query.Filter,
            query.Pagination,
            query.Sorter,
            query.PricingOptions);

        var items = await productRepository.ListAsync(pageSpec, cancellationToken);

        return Result.Success(new GetProductsQueryResult<ProductDto>(items, total, query.Pagination));
    }
}
