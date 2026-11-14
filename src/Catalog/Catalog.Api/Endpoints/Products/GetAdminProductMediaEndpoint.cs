using Catalog.Application.Features.ProductMedia.Queries.GetProductMedia;
using Catalog.Contracts.Products.Media;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Products;

internal static class GetAdminProductMediaEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}/media", Handle)
            .WithName("getAdminProductMedia")
            .RequireAuthorization(PolicyNames.CanManageMedia)
            .Produces<ProductMediaDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProductMediaQuery(id), cancellationToken);
        return result.ToHttpResult(http);
    }
}
