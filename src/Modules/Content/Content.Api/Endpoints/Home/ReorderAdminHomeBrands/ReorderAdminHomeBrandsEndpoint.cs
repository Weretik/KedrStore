using Content.Application.Features.Home.Commands.ReorderHomeBrands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Home.Shared;

namespace Content.Api.Endpoints.Home.ReorderAdminHomeBrands;

internal static class ReorderAdminHomeBrandsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("brands/order", Handle)
            .WithName("reorderAdminHomeBrands")
            .WithSummary("Save the order of all brands")
            .WithDescription("Requires `CanManageContent`. `ids` must hold every current brand exactly once; a stale list answers 409.")
            .Produces<AdminHomeBlocksResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

    /// <summary>Saves the order of all brands. 409 when the list does not match the current brands.</summary>
    private static async Task<IResult> Handle(
        [FromBody] ReorderHomeRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new ReorderHomeBrandsCommand(request.Ids ?? []), cancellationToken))
            .Map(HomeResponseMapper.ToResponse)
            .ToHttpResult(http);
}
