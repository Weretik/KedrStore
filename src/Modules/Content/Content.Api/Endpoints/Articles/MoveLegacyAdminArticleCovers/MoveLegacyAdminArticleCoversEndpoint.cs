using Content.Application.Features.Articles.Commands.MoveLegacyArticleCovers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Articles.Shared;

namespace Content.Api.Endpoints.Articles.MoveLegacyAdminArticleCovers;

internal static class MoveLegacyAdminArticleCoversEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("covers/move-legacy", Handle)
            .WithName("moveLegacyAdminArticleCovers")
            .WithSummary("Move covers saved before Content 008 to the CDN")
            .WithDescription("Uploads each legacy cover to the CDN under `articles/` (library folder «Статті», named after the slug), sets it as the article cover, and deletes the stored bytes. Safe to repeat: a second call moves nothing. Stops at the first failed upload and keeps what was already moved.")
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
