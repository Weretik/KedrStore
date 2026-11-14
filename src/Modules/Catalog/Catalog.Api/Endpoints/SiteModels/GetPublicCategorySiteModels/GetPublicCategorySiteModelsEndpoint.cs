using Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModels;
using Catalog.Contracts.SiteModels.Public;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.SiteModels.Shared;

namespace Catalog.Api.Endpoints.SiteModels.GetPublicCategorySiteModels;

internal static class GetPublicCategorySiteModelsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{categorySlug}/models", Handle)
            .WithName("getPublicCategorySiteModels")
            .Produces<PublicSiteModelListDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>Models of a category tree; attribute filters with counts only in a final category.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] string lang, [FromRoute] string categorySlug,
        [AsParameters] GetPublicSiteModelsParameters parameters, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetPublicSiteModelsQuery(lang, categorySlug, parameters.ToRequest()),
            cancellationToken)).ToHttpResult(http);
}
