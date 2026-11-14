using Catalog.Application.Features.Attributes.Common;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

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
