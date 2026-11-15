using Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModel;
using Catalog.Contracts.SiteModels.Public;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.GetPublicSiteModel;

internal static class GetPublicSiteModelEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("model/{modelSlug}", Handle)
            .WithName("getPublicSiteModel")
            .WithSummary("The model page")
            .WithDescription("A current model slug, or the slug of an on-site product shown as a model of its own, gives the page. The page lists the axis values its products have and each product's values, so the site can hide combinations no product has and offer in-stock neighbours. Texts of `ru` fall back to `uk` field by field.")
            .Produces<PublicSiteModelDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<PublicSiteModelAddressDto>(StatusCodes.Status404NotFound);

    /// <summary>The model page; a 404 body with an address means a permanent redirect there.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] string lang, [FromRoute] string modelSlug, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicSiteModelQuery(lang, modelSlug), cancellationToken);
        if (!result.IsSuccess)
            return result.ToProblem(http);

        return result.Value.Model is { } model
            ? TypedResults.Ok(model)
            : result.Value.Redirect is { } redirect
                ? TypedResults.NotFound(redirect)
                : TypedResults.NotFound();
    }
}
