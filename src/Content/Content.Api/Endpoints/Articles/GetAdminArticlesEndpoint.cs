using Content.Application.Features.Articles.Queries.GetAdminArticles;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Articles;

internal static class GetAdminArticlesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminArticles")
            .Produces<IReadOnlyList<AdminArticleSummaryResponse>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminArticlesQuery(), cancellationToken);
        return result.Map(items =>
            (IReadOnlyList<AdminArticleSummaryResponse>)items.Select(ArticleResponseMapper.ToResponse).ToList())
            .ToHttpResult(http);
    }
}
