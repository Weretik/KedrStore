using Catalog.Application.Features.Attributes.Common;
using Catalog.Contracts.Attributes;

namespace Catalog.Application.Features.Attributes.GetUnresolvedCodes;

public sealed class GetUnresolvedCodesQueryHandler(UnresolvedCodeReader reader)
    : IQueryHandler<GetUnresolvedCodesQuery, Result<List<AdminUnresolvedCodeDto>>>
{
    public async ValueTask<Result<List<AdminUnresolvedCodeDto>>> Handle(GetUnresolvedCodesQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await reader.ListAsync(cancellationToken));
}
