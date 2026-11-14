using Catalog.Application.Features.Groups.Shared;
using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.Queries.GetProductGroupForProduct;

public sealed class GetProductGroupForProductHandler(ProductGroupReader reader)
    : IQueryHandler<GetProductGroupForProductQuery, Result<AdminProductGroupMembershipDto>>
{
    public async ValueTask<Result<AdminProductGroupMembershipDto>> Handle(GetProductGroupForProductQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadForProductAsync(ProductId.From(query.ProductId), cancellationToken) is { } membership
            ? Result.Success(membership)
            : Result.NotFound();
}
