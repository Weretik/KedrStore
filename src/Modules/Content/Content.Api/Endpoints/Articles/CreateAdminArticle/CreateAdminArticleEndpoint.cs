using Content.Application.Features.Articles.Commands.CreateArticle;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Articles.Shared;

namespace Content.Api.Endpoints.Articles.CreateAdminArticle;

internal static class CreateAdminArticleEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("createAdminArticle")
            .WithSummary("Create an article")
            .WithDescription("An empty `slug` is generated from the Ukrainian title (KMU-2010 transliteration, `-2`, `-3`… on a clash). `bodyHtml` is sanitized to p, br, strong, em, ul, ol, li, h2, h3, blockquote, and a[href] (http, https, or a path starting with `/`). Not idempotent: repeating the request creates another article.")
            .Produces<AdminArticleResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromBody] SaveArticleRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(new CreateArticleCommand(ArticleResponseMapper.ToInput(request)), cancellationToken))
            .Map(ArticleResponseMapper.ToResponse)
            .ToHttpResult(http);
}
