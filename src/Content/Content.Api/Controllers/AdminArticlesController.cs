using Content.Api.Articles;
using Content.Application.Contracts.Images;
using Content.Application.Features.Articles.Admin.Create;
using Content.Application.Features.Articles.Admin.Delete;
using Content.Application.Features.Articles.Admin.GetById;
using Content.Application.Features.Articles.Admin.GetList;
using Content.Application.Features.Articles.Admin.RemoveCover;
using Content.Application.Features.Articles.Admin.Update;
using Content.Application.Features.Articles.Admin.UploadCover;
using Content.Application.Features.Articles.Common;
using Identity.Application.Security.Policies;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Controllers;

/// <summary>Admin management of site articles, drafts included.</summary>
[ApiController]
[Route("api/admin/articles")]
[Authorize(Policy = PolicyNames.CanManageContent)]
public sealed class AdminArticlesController(ISender sender) : ControllerBase
{
    private const long CoverRequestLimit = UploadArticleCoverCommandValidator.MaxUploadBytes + 64 * 1024;

    [HttpGet(Name = "getAdminArticles")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminArticleSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminArticleSummaryResponse>>> GetList(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminArticlesQuery(), cancellationToken);
        return this.ToActionResult(result.Map(items =>
            (IReadOnlyList<AdminArticleSummaryResponse>)items.Select(ArticleResponseMapper.ToResponse).ToList()));
    }

    [HttpGet("{id:long}", Name = "getAdminArticle")]
    [ProducesResponseType(typeof(AdminArticleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminArticleResponse>> GetById([FromRoute] long id, CancellationToken cancellationToken)
        => Map(await sender.Send(new GetAdminArticleQuery(id), cancellationToken));

    [HttpPost(Name = "createAdminArticle")]
    [ProducesResponseType(typeof(AdminArticleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AdminArticleResponse>> Create(
        [FromBody] SaveArticleRequest request, CancellationToken cancellationToken)
        => Map(await sender.Send(new CreateArticleCommand(ArticleResponseMapper.ToInput(request)), cancellationToken));

    [HttpPut("{id:long}", Name = "updateAdminArticle")]
    [ProducesResponseType(typeof(AdminArticleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminArticleResponse>> Update(
        [FromRoute] long id, [FromBody] SaveArticleRequest request, CancellationToken cancellationToken)
        => Map(await sender.Send(new UpdateArticleCommand(id, ArticleResponseMapper.ToInput(request)), cancellationToken));

    [HttpDelete("{id:long}", Name = "deleteAdminArticle")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteArticleCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    [HttpPut("{id:long}/cover", Name = "uploadAdminArticleCover")]
    [RequestSizeLimit(CoverRequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = CoverRequestLimit)]
    [ProducesResponseType(typeof(AdminArticleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    public async Task<ActionResult<AdminArticleResponse>> UploadCover(
        [FromRoute] long id, IFormFile file, CancellationToken cancellationToken)
    {
        var upload = new CoverUpload(file.FileName, file.ContentType, file.Length, file.OpenReadStream);
        return Map(await sender.Send(new UploadArticleCoverCommand(id, upload), cancellationToken));
    }

    [HttpDelete("{id:long}/cover", Name = "removeAdminArticleCover")]
    [ProducesResponseType(typeof(AdminArticleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminArticleResponse>> RemoveCover([FromRoute] long id, CancellationToken cancellationToken)
        => Map(await sender.Send(new RemoveArticleCoverCommand(id), cancellationToken));

    private ActionResult<AdminArticleResponse> Map(Result<AdminArticleDetails> result)
        => this.ToActionResult(result.Map(ArticleResponseMapper.ToResponse));
}
