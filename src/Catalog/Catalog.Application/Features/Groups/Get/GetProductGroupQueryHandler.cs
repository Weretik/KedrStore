using Catalog.Application.Features.Groups.Common;
using Catalog.Contracts.Groups;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Groups.Get;

public sealed class GetProductGroupQueryHandler(ProductGroupReader reader)
    : IQueryHandler<GetProductGroupQuery, Result<AdminProductGroupDto>>
{
    public async ValueTask<Result<AdminProductGroupDto>> Handle(GetProductGroupQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadAsync(ProductGroupId.Create(query.GroupId), cancellationToken) is { } group
            ? Result.Success(group)
            : Result.NotFound();
}
