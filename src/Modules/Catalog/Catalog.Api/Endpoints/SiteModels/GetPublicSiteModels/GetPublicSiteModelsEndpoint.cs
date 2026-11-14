using Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModels;
using Catalog.Contracts.SiteModels.Public;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.SiteModels.Shared;

namespace Catalog.Api.Endpoints.SiteModels.GetPublicSiteModels;

internal static class GetPublicSiteModelsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("models", Handle)
            .WithName("getPublicSiteModels")
            .Produces<PublicSiteModelListDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    /// <summary>The search over all models (without attribute filters).</summary>
    private static async Task<IResult> Handle(
        [FromRoute] string lang, [AsParameters] GetPublicSiteModelsParameters parameters, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetPublicSiteModelsQuery(lang, null, parameters.ToRequest()), cancellationToken))
            .ToHttpResult(http);
}
