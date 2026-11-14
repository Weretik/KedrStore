using Catalog.Application.Features.SiteModels.Queries.GetSiteModels;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.SiteModels.Shared;

namespace Catalog.Api.Endpoints.SiteModels.GetAdminSiteModels;

internal static class GetAdminSiteModelsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminSiteModels")
            .Produces<SiteModelListDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [AsParameters] GetSiteModelsParameters parameters, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetSiteModelsQuery(parameters.ToModelsRequest()), cancellationToken))
            .ToHttpResult(http);
}
