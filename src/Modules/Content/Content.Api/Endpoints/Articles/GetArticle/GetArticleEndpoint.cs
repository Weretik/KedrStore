using Content.Application.Features.Articles.Queries.GetPublicArticle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Articles.Shared;

namespace Content.Api.Endpoints.Articles.GetArticle;

internal static class GetArticleEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{lang}/articles/{slug}", Handle)
            .WithName("getArticle")
            .Produces<ArticleResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, [FromRoute] string slug, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicArticleQuery(lang, slug), cancellationToken);
        return result.Map(ArticleResponseMapper.ToResponse).ToHttpResult(http);
    }
}
