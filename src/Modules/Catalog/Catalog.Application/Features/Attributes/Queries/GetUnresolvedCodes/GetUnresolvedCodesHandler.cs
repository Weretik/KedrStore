using Catalog.Application.Features.Attributes.Shared;
using Catalog.Contracts.Attributes;
using Catalog.Application.Features.Attributes.Shared.Readers;

namespace Catalog.Application.Features.Attributes.Queries.GetUnresolvedCodes;

public sealed class GetUnresolvedCodesHandler(UnresolvedCodeReader reader)
    : IQueryHandler<GetUnresolvedCodesQuery, Result<List<AdminUnresolvedCodeDto>>>
{
    public async ValueTask<Result<List<AdminUnresolvedCodeDto>>> Handle(GetUnresolvedCodesQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await reader.ListAsync(cancellationToken));
}
