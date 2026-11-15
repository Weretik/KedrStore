using Content.Application.Features.Home.Queries.GetPublicHomeBlocks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Home.Shared;

namespace Content.Api.Endpoints.Home.GetPublicHomeBlocks;

internal static class GetPublicHomeBlocksEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{lang}/home", Handle)
            .WithName("getPublicHomeBlocks")
            .WithSummary("Get the home page FAQ and brands in one language")
            .WithDescription("Public, in site order. A brand whose logo file was deleted from the library is left out. An empty `brands` list means the admin has not added any.")
            .Produces<PublicHomeBlocksResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicHomeBlocksQuery(lang), cancellationToken);
        return result.Map(HomeResponseMapper.ToResponse).ToHttpResult(http);
    }
}
