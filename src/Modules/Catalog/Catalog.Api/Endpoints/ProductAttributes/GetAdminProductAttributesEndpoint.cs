using Catalog.Application.Features.ProductAttributes.Queries.GetProductAttributes;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.ProductAttributes;

internal static class GetAdminProductAttributesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("attributes", Handle)
            .WithName("getAdminProductAttributes")
            .Produces<AdminProductAttributesDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int productId, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetProductAttributesQuery(productId), cancellationToken)).ToHttpResult(http);
}
