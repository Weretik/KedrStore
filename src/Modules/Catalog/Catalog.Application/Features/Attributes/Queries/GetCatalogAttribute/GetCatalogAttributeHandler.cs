using Catalog.Application.Features.Attributes.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Application.Features.Attributes.Shared.Readers;
using Catalog.Domain.Attributes.ValueObjects;

namespace Catalog.Application.Features.Attributes.Queries.GetCatalogAttribute;

public sealed class GetCatalogAttributeHandler(CatalogAttributeReader reader)
    : IQueryHandler<GetCatalogAttributeQuery, Result<AdminCatalogAttributeDto>>
{
    public async ValueTask<Result<AdminCatalogAttributeDto>> Handle(GetCatalogAttributeQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadAsync(AttributeId.Create(query.AttributeId), cancellationToken) is { } dto
            ? Result.Success(dto)
            : Result.NotFound();
}
