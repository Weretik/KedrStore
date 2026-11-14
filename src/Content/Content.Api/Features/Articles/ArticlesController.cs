using Content.Application.Features.Articles.Queries.GetPublicArticle;
using Content.Application.Features.Articles.Queries.GetArticleCover;
using Content.Application.Features.Articles.Queries.GetPublicArticles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Net.Http.Headers;

namespace Content.Api.Features.Articles;

/// <summary>Public site articles: only published ones are listed or opened.</summary>
[ApiController]
[Route("api/content")]
[AllowAnonymous]
public sealed class ArticlesController(ISender sender) : ControllerBase
{
    [HttpGet("{lang}/articles", Name = "getArticles")]
    [ProducesResponseType(typeof(IReadOnlyList<ArticleSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ArticleSummaryResponse>>> GetList(
        [FromRoute] string lang, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicArticlesQuery(lang), cancellationToken);
        return this.ToActionResult(result.Map(items =>
            (IReadOnlyList<ArticleSummaryResponse>)items.Select(ArticleResponseMapper.ToResponse).ToList()));
    }

    [HttpGet("{lang}/articles/{slug}", Name = "getArticle")]
    [ProducesResponseType(typeof(ArticleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArticleResponse>> GetBySlug(
        [FromRoute] string lang, [FromRoute] string slug, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicArticleQuery(lang, slug), cancellationToken);
        return this.ToActionResult(result.Map(ArticleResponseMapper.ToResponse));
    }

    /// <summary>Cover bytes. The URL carries the cover version, so a matching request is cached for a year.</summary>
    [HttpGet("articles/{id:long}/cover", Name = "getArticleCover")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "image/webp")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCover([FromRoute] long id, [FromQuery] int? v, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetArticleCoverQuery(id), cancellationToken);
        if (!result.IsSuccess)
            return this.ToProblem(result);

        Response.Headers[HeaderNames.CacheControl] = v == result.Value.Version
            ? "public, max-age=31536000, immutable"
            : "public, max-age=300";
        return File(result.Value.Data, result.Value.ContentType);
    }
}
