using Catalog.Application.Features.Groups.Common;
using Catalog.Contracts.Groups;
using Catalog.Domain.ValueObjects;

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
