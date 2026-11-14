using Catalog.Application.Features.Groups.Shared;
using Catalog.Contracts.Groups;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.Groups.Queries.GetProductGroup;

public sealed class GetProductGroupHandler(ProductGroupReader reader)
    : IQueryHandler<GetProductGroupQuery, Result<AdminProductGroupDto>>
{
    public async ValueTask<Result<AdminProductGroupDto>> Handle(GetProductGroupQuery query,
        CancellationToken cancellationToken)
        => await reader.ReadAsync(ProductGroupId.Create(query.GroupId), cancellationToken) is { } group
            ? Result.Success(group)
            : Result.NotFound();
}
