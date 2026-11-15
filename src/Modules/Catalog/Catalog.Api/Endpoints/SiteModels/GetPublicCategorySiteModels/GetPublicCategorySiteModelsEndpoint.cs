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
            .WithSummary("Models of a category")
            .WithDescription("Models of the category and every category below it. A final category (without children) takes attribute filters and returns them with counts in `filters`; a parent category gives `filters` empty and refuses attribute filters with 400. A former category slug still selects its category. A model matches when one of its on-site products matches every filter; the card's price, stock, sale, and new count only those products.")
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
