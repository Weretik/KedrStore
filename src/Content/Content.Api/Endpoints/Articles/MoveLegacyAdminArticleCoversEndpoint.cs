using Content.Application.Features.Articles.Commands.MoveLegacyArticleCovers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Articles;

internal static class MoveLegacyAdminArticleCoversEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("covers/move-legacy", Handle)
            .WithName("moveLegacyAdminArticleCovers")
            .Produces<LegacyArticleCoversMovedResponse>()
            .Produces(StatusCodes.Status503ServiceUnavailable);

    /// <summary>
    /// Moves covers saved before Content 008 from the database to the CDN folder <c>articles</c> and the library
    /// folder «Статті». Safe to repeat; answers how many were moved by this call.
    /// </summary>
    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MoveLegacyArticleCoversCommand(), cancellationToken);
        return result.Map(moved => new LegacyArticleCoversMovedResponse(moved.Moved)).ToHttpResult(http);
    }
}
