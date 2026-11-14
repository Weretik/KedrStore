using Content.Application.Features.Home.Commands.ReorderHomeBrands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Home;

internal static class ReorderAdminHomeBrandsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("brands/order", Handle)
            .WithName("reorderAdminHomeBrands")
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
