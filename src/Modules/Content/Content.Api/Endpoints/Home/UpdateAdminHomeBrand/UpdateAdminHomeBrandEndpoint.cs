using Content.Application.Features.Home.Commands.UpdateHomeBrand;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Home.Shared;

namespace Content.Api.Endpoints.Home.UpdateAdminHomeBrand;

internal static class UpdateAdminHomeBrandEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("brands/{id:long}", Handle)
            .WithName("updateAdminHomeBrand")
            .Produces<AdminHomeBlocksResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] HomeBrandRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new UpdateHomeBrandCommand(id, HomeResponseMapper.ToInput(request)),
            cancellationToken)).Map(HomeResponseMapper.ToResponse).ToHttpResult(http);
}
