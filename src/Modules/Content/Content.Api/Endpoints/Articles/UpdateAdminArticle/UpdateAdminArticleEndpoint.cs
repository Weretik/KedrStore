using Content.Application.Features.Articles.Commands.UpdateArticle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Articles.Shared;

namespace Content.Api.Endpoints.Articles.UpdateAdminArticle;

internal static class UpdateAdminArticleEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", Handle)
            .WithName("updateAdminArticle")
            .WithSummary("Replace an article's slug, date, publication flag, and both texts")
            .WithDescription("Full replacement; `slug` is required and must not belong to another article. Repeating it is safe.")
            .Produces<AdminArticleResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] SaveArticleRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new UpdateArticleCommand(id, ArticleResponseMapper.ToInput(request)), cancellationToken))
            .Map(ArticleResponseMapper.ToResponse)
            .ToHttpResult(http);
}
