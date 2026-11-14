using Catalog.Application.Features.Groups.Queries.GetProductGroupForProduct;
using Catalog.Contracts.Groups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Products.GetAdminProductGroupOfProduct;

internal static class GetAdminProductGroupOfProductEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}/group", Handle)
            .WithName("getAdminProductGroupOfProduct")
            .Produces<AdminProductGroupMembershipDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetProductGroupForProductQuery(id), cancellationToken)).ToHttpResult(http);
}
