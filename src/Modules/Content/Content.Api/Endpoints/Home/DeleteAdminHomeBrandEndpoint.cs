using Content.Application.Features.Home.Commands.DeleteHomeBrand;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Home;

internal static class DeleteAdminHomeBrandEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("brands/{id:long}", Handle)
            .WithName("deleteAdminHomeBrand")
            .Produces<AdminHomeBlocksResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>Removes a brand; its logo stays in the media library.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new DeleteHomeBrandCommand(id), cancellationToken))
            .Map(HomeResponseMapper.ToResponse)
            .ToHttpResult(http);
}
