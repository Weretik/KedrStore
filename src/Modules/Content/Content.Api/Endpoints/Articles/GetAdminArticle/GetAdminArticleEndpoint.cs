using Content.Application.Features.Articles.Queries.GetAdminArticle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Articles.Shared;

namespace Content.Api.Endpoints.Articles.GetAdminArticle;

internal static class GetAdminArticleEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{id:long}", Handle)
            .WithName("getAdminArticle")
            .Produces<AdminArticleResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new GetAdminArticleQuery(id), cancellationToken))
            .Map(ArticleResponseMapper.ToResponse)
            .ToHttpResult(http);
}
