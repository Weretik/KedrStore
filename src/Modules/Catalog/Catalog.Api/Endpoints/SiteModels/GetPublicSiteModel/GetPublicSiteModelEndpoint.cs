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
