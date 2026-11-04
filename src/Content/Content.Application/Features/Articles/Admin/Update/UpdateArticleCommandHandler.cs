using BuildingBlocks.Application.Contracts;
using Content.Application.Features.Editing;
using Content.Application.Contracts.Html;
using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.Update;

/// <summary>Replaces the slug, date, publication flag, both texts, and (when sent) the cover of an article.</summary>
public sealed class UpdateArticleCommandHandler(
    IContentRepository<Article> articles,
    IContentRepository<ArticleCover> covers,
    IReadContentDbContext db,
    IMediaImageReader images,
    IArticleHtmlSanitizer sanitizer,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdateArticleCommand, Result<AdminArticleDetails>>
{
    public async ValueTask<Result<AdminArticleDetails>> Handle(UpdateArticleCommand command, CancellationToken cancellationToken)
    {
        var id = ArticleId.Create(command.ArticleId);
        var article = await articles.FirstOrDefaultAsync(new ArticleByIdSpec(id), cancellationToken);
        if (article is null)
            return Result.NotFound();

        var input = command.Input;
        var slug = input.Slug!.Trim();
        if (await ArticleSlugs.IsTakenAsync(db, slug, id, cancellationToken))
            return ArticleSlugs.TakenResult(slug);

        var now = clock.GetUtcNow();
        var editor = ContentEditor.Name(currentUser);
        article.Update(slug, input.PublishedOn, input.IsPublished, ArticleTexts.Sanitized(input.Texts, sanitizer), now, editor);
        var coverError = await ArticleCovers.ApplyAsync(article, input, images, covers, now, editor, cancellationToken);
        if (coverError is not null)
            return Result.Invalid(coverError);

        await articles.SaveChangesAsync(cancellationToken);
        return Result.Success(ArticleMapper.ToAdmin(article,
            await ArticleCovers.ViewAsync(images, article, cancellationToken)));
    }
}
