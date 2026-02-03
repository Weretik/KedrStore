using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Products.GetById.DTOs;
using Catalog.Application.Features.Products.GetById.Extensions;

namespace Catalog.Application.Features.Products.GetById;

public class GetProductBySlugQueryHandler(IReadCatalogDbContext catalogDbContext)
    : IQueryHandler<GetProductBySlugQuery, Result<ProductBySlugDto>>
{
    public async ValueTask<Result<ProductBySlugDto>> Handle(GetProductBySlugQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var request = query.Request;

        var queryResult = catalogDbContext.Products
            .AsNoTracking()
            .JoinPricesAndCategoryForProductBySlug(
                catalogDbContext.ProductPrices.AsNoTracking(),
                catalogDbContext.Categories.AsNoTracking(),
                request);

        var result = await queryResult.FirstOrDefaultAsync(cancellationToken);

        if (result is null)
            return Result.NotFound();

        return Result.Success(result);
    }
}
