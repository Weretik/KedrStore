using Content.Application.Contracts.Html;
using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.Update;

/// <summary>Replaces the slug, date, publication flag, and both texts of an article.</summary>
public sealed class UpdateArticleCommandHandler(
    IContentRepository<Article> articles,
    IReadContentDbContext db,
    IArticleHtmlSanitizer sanitizer,
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

        article.Update(slug, input.PublishedOn, input.IsPublished,
            ArticleTexts.Sanitized(input.Texts, sanitizer), clock.GetUtcNow());
        await articles.SaveChangesAsync(cancellationToken);
        return Result.Success(ArticleMapper.ToAdmin(article));
    }
}
