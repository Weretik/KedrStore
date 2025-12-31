using Catalog.Application.Features.Shared;
using Catalog.Application.Persistance;
using Catalog.Application.Shared;
using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler(ICatalogReadRepository<Product> productRepository)
    : IQueryHandler<GetProductByIdQuery, Result<ProductDto>>
{
    public async ValueTask<Result<ProductDto>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        var spec = new GetProductByIdSpec(query.Id);
        var product = await productRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (product is null)
            return Result.NotFound();

        return Result.Success(product);
    }
}
