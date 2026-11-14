using Catalog.Application.Features.SiteModels.Commands.AddSiteModelProducts;
using Catalog.Contracts.SiteModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.SiteModels;

internal static class AddAdminSiteModelProductsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("{id:int}/products", Handle)
            .WithName("addAdminSiteModelProducts")
            .Produces<SiteModelDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] AddSiteModelProductsRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new AddSiteModelProductsCommand(id, request), cancellationToken)).ToHttpResult(http);
}
