using Catalog.Application.Features.Products.GetById.DTOs;
using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Persistence;

namespace Catalog.Application.Features.Products.GetById;

public class ProductBySlugQueryHandler(IReadCatalogDbContext catalogDbContext)
    : IQueryHandler<ProductBySlugQuery, Result<ProductBySlugDto>>
{
    public async ValueTask<Result<ProductBySlugDto>> Handle(ProductBySlugQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var slug = query.Slug;



        if (product is null)
            return Result.NotFound();

        return Result.Success(product);
    }
}
