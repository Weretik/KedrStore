using Catalog.Application.Features.SiteModels.Commands.RemoveSiteModelProduct;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.SiteModels.RemoveAdminSiteModelProduct;

internal static class RemoveAdminSiteModelProductEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("{id:int}/products/{productId:int}", Handle)
            .WithName("removeAdminSiteModelProduct")
            .Produces<SiteModelDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromRoute] int productId, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new RemoveSiteModelProductCommand(id, productId), cancellationToken)).ToHttpResult(http);
}
