using Catalog.Application.Features.Groups.Commands.SetProductGroup;
using Catalog.Contracts.Groups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Products.SetAdminProductGroupOfProduct;

internal static class SetAdminProductGroupOfProductEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/group", Handle)
            .WithName("setAdminProductGroupOfProduct")
            .Produces<AdminProductGroupMembershipDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>Moves the product to a group, out of groups, or back to automatic grouping (spec catalog/006).</summary>
    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] SetProductGroupRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new SetProductGroupCommand(id, request), cancellationToken)).ToHttpResult(http);
}
