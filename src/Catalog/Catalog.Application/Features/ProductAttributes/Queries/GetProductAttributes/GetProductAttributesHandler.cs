using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.ProductAttributes.Queries.GetProductAttributes;

public sealed class GetProductAttributesHandler(ProductAttributesReader reader)
    : IQueryHandler<GetProductAttributesQuery, Result<AdminProductAttributesDto>>
{
    public async ValueTask<Result<AdminProductAttributesDto>> Handle(GetProductAttributesQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadAsync(ProductId.From(query.ProductId), cancellationToken) is { } dto
            ? Result.Success(dto)
            : Result.NotFound();
}
