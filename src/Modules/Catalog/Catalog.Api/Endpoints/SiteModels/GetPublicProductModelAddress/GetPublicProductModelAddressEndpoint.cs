using Catalog.Application.Features.SiteModels.Queries.GetPublicProductModelAddress;
using Catalog.Contracts.Category;
using Catalog.Contracts.SiteModels.Public;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.GetPublicProductModelAddress;

internal static class GetPublicProductModelAddressEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("product/{productSlug}/model", Handle)
            .WithName("getPublicProductModelAddress")
            .Produces<PublicSiteModelAddressDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<CatalogRedirect>(StatusCodes.Status404NotFound);

    /// <summary>The model address of a current or former product slug, for the permanent redirect (M-11).</summary>
    private static async Task<IResult> Handle(
        [FromRoute] string lang, [FromRoute] string productSlug, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new GetPublicProductModelAddressQuery(productSlug),
            cancellationToken)).ToLookupHttpResult(http);
}
