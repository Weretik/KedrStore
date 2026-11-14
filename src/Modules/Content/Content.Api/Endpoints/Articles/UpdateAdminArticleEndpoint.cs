using Content.Application.Features.Articles.Commands.UpdateArticle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Articles;

internal static class UpdateAdminArticleEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:long}", Handle)
            .WithName("updateAdminArticle")
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
