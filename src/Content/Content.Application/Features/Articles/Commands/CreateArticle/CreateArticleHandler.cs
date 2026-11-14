using BuildingBlocks.Application.Users;
using Content.Application.Shared;
using Content.Application.Abstractions.Integrations;
using Content.Application.Abstractions.Media;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.CreateArticle;

/// <summary>Creates an article with its cover; without a slug one is built from the Ukrainian title.</summary>
public sealed class CreateArticleHandler(
    IContentRepository<Article> articles,
    IContentRepository<ArticleCover> covers,
    IReadContentDbContext db,
    IMediaImageReader images,
    IArticleHtmlSanitizer sanitizer,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<CreateArticleCommand, Result<AdminArticleDetails>>
{
    public async ValueTask<Result<AdminArticleDetails>> Handle(CreateArticleCommand command, CancellationToken cancellationToken)
    {
        var input = command.Input;
        var slug = input.Slug?.Trim();
        if (string.IsNullOrEmpty(slug))
        {
            var ukTitle = input.Texts.First(text => text.Language == "uk").Title;
            slug = await ArticleSlugs.FreeFromTitleAsync(db, ukTitle, cancellationToken);
        }
        else if (await ArticleSlugs.IsTakenAsync(db, slug, null, cancellationToken))
        {
            return ArticleSlugs.TakenResult(slug);
        }

        var now = clock.GetUtcNow();
        var editor = ContentEditor.Name(currentUser);
        var created = Article.Create(slug, input.PublishedOn, input.IsPublished,
            ArticleTexts.Sanitized(input.Texts, sanitizer), now, editor);
        if (created.IsFailure)
            return created.Error!.ToResult<AdminArticleDetails>();
        var article = created.Value;
        var coverError = await ArticleCovers.ApplyAsync(article, input, images, covers, now, editor, cancellationToken);
        if (coverError is not null)
            return coverError.ToResult<AdminArticleDetails>();

        await articles.AddAsync(article, cancellationToken);
        return Result.Success(ArticleMapper.ToAdmin(article,
            await ArticleCovers.ViewAsync(images, article, cancellationToken)));
    }
}
