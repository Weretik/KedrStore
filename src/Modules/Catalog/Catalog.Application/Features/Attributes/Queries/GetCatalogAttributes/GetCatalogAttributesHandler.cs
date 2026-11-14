using Catalog.Application.Features.Attributes.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Application.Features.Attributes.Shared.Readers;

namespace Catalog.Application.Features.Attributes.Queries.GetCatalogAttributes;

public sealed class GetCatalogAttributesHandler(CatalogAttributeReader reader)
    : IQueryHandler<GetCatalogAttributesQuery, Result<List<AdminCatalogAttributeListItemDto>>>
{
    public async ValueTask<Result<List<AdminCatalogAttributeListItemDto>>> Handle(GetCatalogAttributesQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await reader.ListAsync(cancellationToken));
}
