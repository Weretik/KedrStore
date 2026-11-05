using Catalog.Application.Features.Attributes.Common;
using Catalog.Contracts.Attributes;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Attributes.Get;

public sealed class GetCatalogAttributeQueryHandler(CatalogAttributeReader reader)
    : IQueryHandler<GetCatalogAttributeQuery, Result<AdminCatalogAttributeDto>>
{
    public async ValueTask<Result<AdminCatalogAttributeDto>> Handle(GetCatalogAttributeQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadAsync(AttributeId.Create(query.AttributeId), cancellationToken) is { } dto
            ? Result.Success(dto)
            : Result.NotFound();
}
