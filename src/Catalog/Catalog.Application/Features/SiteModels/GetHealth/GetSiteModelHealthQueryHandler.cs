using Catalog.Application.Features.SiteModels.Common;
using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.GetHealth;

/// <summary>Counts of spec catalog/007 R-08 for «Огляд».</summary>
public sealed class GetSiteModelHealthQueryHandler(SiteModelStateReader reader)
    : IQueryHandler<GetSiteModelHealthQuery, Result<SiteModelHealthDto>>
{
    public async ValueTask<Result<SiteModelHealthDto>> Handle(GetSiteModelHealthQuery query,
        CancellationToken cancellationToken)
        => Result.Success((await reader.ReadAsync(cancellationToken)).Health());
}
