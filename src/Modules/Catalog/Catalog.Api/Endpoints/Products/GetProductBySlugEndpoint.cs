using Catalog.Application.Features.Products.Queries.GetProductBySlug;
using Catalog.Contracts.Category;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Products;

internal static class GetProductBySlugEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("product/{productSlug}", Handle)
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
