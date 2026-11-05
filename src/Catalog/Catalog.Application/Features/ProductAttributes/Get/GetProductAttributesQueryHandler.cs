using Catalog.Application.Features.ProductAttributes.Common;
using Catalog.Contracts.Attributes;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.ProductAttributes.Get;

public sealed class GetProductAttributesQueryHandler(ProductAttributesReader reader)
    : IQueryHandler<GetProductAttributesQuery, Result<AdminProductAttributesDto>>
{
    public async ValueTask<Result<AdminProductAttributesDto>> Handle(GetProductAttributesQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadAsync(ProductId.From(query.ProductId), cancellationToken) is { } dto
            ? Result.Success(dto)
            : Result.NotFound();
}
