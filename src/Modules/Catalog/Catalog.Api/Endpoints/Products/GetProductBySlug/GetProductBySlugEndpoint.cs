using Catalog.Application.Features.Products.Queries.GetProductBySlug;
using Catalog.Contracts.Category;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Products.GetProductBySlug;

internal static class GetProductBySlugEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("product/{productSlug}", Handle)
            .WithName("getPublicProductBySlug")
            .WithSummary("Get a public product by slug")
            .WithDescription("A former slug returns 200 with `productSlug` set to the current slug, so the client can redirect permanently. A product of a deleted category is not public.")
            .Produces<ProductBySlugDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<CatalogRedirect>(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, [FromRoute] string productSlug, ISender sender, HttpContext http,
        [FromQuery] int priceTypeId = 11, CancellationToken cancellationToken = default)
    {
        var request = new GetProductBySlugRequest(productSlug, priceTypeId, lang);
        var query = new GetProductBySlugQuery(request);
        var result = await sender.Send(query, cancellationToken);

        return result.ToLookupHttpResult(http);
    }
}
