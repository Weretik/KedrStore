using Content.Application.Features.Home.Commands.DeleteHomeFaqItem;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Home.Shared;

namespace Content.Api.Endpoints.Home.DeleteAdminHomeFaqItem;

internal static class DeleteAdminHomeFaqItemEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("faq/{id:long}", Handle)
            .WithName("deleteAdminHomeFaqItem")
            .Produces<AdminHomeBlocksResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new DeleteHomeFaqItemCommand(id), cancellationToken))
            .Map(HomeResponseMapper.ToResponse)
            .ToHttpResult(http);
}
