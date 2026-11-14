using Content.Application.Features.Home.Queries.GetPublicHomeBlocks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Home;

internal static class GetPublicHomeBlocksEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{lang}/home", Handle)
            .WithName("getPublicHomeBlocks")
            .Produces<PublicHomeBlocksResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicHomeBlocksQuery(lang), cancellationToken);
        return result.Map(HomeResponseMapper.ToResponse).ToHttpResult(http);
    }
}
