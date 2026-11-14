using Catalog.Application.Features.Attributes.Shared;
using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.Queries.GetCatalogAttributes;

public sealed class GetCatalogAttributesQueryHandler(CatalogAttributeReader reader)
    : IQueryHandler<GetCatalogAttributesQuery, Result<List<AdminCatalogAttributeListItemDto>>>
{
    public async ValueTask<Result<List<AdminCatalogAttributeListItemDto>>> Handle(GetCatalogAttributesQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await reader.ListAsync(cancellationToken));
}
