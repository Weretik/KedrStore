using Content.Application.Features.Home.Commands.UpdateHomeFaqItem;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Home;

internal static class UpdateAdminHomeFaqItemEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("faq/{id:long}", Handle)
            .WithName("updateAdminHomeFaqItem")
            .Produces<AdminHomeBlocksResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] HomeFaqItemRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new UpdateHomeFaqItemCommand(id, HomeResponseMapper.ToInput(request)),
            cancellationToken)).Map(HomeResponseMapper.ToResponse).ToHttpResult(http);
}
