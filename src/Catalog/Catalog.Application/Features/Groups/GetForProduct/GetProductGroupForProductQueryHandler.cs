using Catalog.Application.Features.Groups.Common;
using Catalog.Contracts.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Groups.GetForProduct;

public sealed class GetProductGroupForProductQueryHandler(ProductGroupReader reader)
    : IQueryHandler<GetProductGroupForProductQuery, Result<AdminProductGroupMembershipDto>>
{
    public async ValueTask<Result<AdminProductGroupMembershipDto>> Handle(GetProductGroupForProductQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadForProductAsync(ProductId.From(query.ProductId), cancellationToken) is { } membership
            ? Result.Success(membership)
            : Result.NotFound();
}
