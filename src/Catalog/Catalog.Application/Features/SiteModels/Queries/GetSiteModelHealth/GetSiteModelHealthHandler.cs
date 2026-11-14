using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Queries.GetSiteModelHealth;

/// <summary>Counts of spec catalog/007 R-08 for «Огляд».</summary>
public sealed class GetSiteModelHealthHandler(SiteModelStateReader reader)
    : IQueryHandler<GetSiteModelHealthQuery, Result<SiteModelHealthDto>>
{
    public async ValueTask<Result<SiteModelHealthDto>> Handle(GetSiteModelHealthQuery query,
        CancellationToken cancellationToken)
        => Result.Success((await reader.ReadAsync(cancellationToken)).Health());
}
