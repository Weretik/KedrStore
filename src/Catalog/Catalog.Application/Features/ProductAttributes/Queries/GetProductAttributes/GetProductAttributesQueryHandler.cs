using Catalog.Application.Features.ProductAttributes.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.ProductAttributes.Queries.GetProductAttributes;

public sealed class GetProductAttributesQueryHandler(ProductAttributesReader reader)
    : IQueryHandler<GetProductAttributesQuery, Result<AdminProductAttributesDto>>
{
    public async ValueTask<Result<AdminProductAttributesDto>> Handle(GetProductAttributesQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadAsync(ProductId.From(query.ProductId), cancellationToken) is { } dto
            ? Result.Success(dto)
            : Result.NotFound();
}
