using Content.Application.Contracts.Html;
using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.Create;

/// <summary>Creates an article; without a slug one is built from the Ukrainian title.</summary>
public sealed class CreateArticleCommandHandler(
    IContentRepository<Article> articles,
    IReadContentDbContext db,
    IArticleHtmlSanitizer sanitizer,
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

        var article = Article.Create(slug, input.PublishedOn, input.IsPublished,
            ArticleTexts.Sanitized(input.Texts, sanitizer), clock.GetUtcNow());
        await articles.AddAsync(article, cancellationToken);
        return Result.Success(ArticleMapper.ToAdmin(article));
    }
}
