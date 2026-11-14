using Content.Application.Features.Articles.Queries.GetPublicArticles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Articles;

internal static class GetArticlesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{lang}/articles", Handle)
            .WithName("getArticles")
            .Produces<IReadOnlyList<ArticleSummaryResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicArticlesQuery(lang), cancellationToken);
        return result.Map(items =>
            (IReadOnlyList<ArticleSummaryResponse>)items.Select(ArticleResponseMapper.ToResponse).ToList())
            .ToHttpResult(http);
    }
}
