using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Queries.GetSiteModel;

public sealed class GetSiteModelQueryHandler(SiteModelReader reader)
    : IQueryHandler<GetSiteModelQuery, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(GetSiteModelQuery query, CancellationToken cancellationToken)
        => await reader.ReadAsync(query.ModelId, cancellationToken) is { } model
            ? Result.Success(model)
            : Result.NotFound();
}
