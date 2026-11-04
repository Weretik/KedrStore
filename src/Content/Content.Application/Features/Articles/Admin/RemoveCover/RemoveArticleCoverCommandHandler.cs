using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.RemoveCover;

public sealed class RemoveArticleCoverCommandHandler(
    IContentRepository<Article> articles,
    IContentRepository<ArticleCover> covers,
    TimeProvider clock)
    : ICommandHandler<RemoveArticleCoverCommand, Result<AdminArticleDetails>>
{
    public async ValueTask<Result<AdminArticleDetails>> Handle(RemoveArticleCoverCommand command,
        CancellationToken cancellationToken)
    {
        var id = ArticleId.Create(command.ArticleId);
        var article = await articles.FirstOrDefaultAsync(new ArticleByIdSpec(id), cancellationToken);
        if (article is null)
            return Result.NotFound();

        var cover = await covers.FirstOrDefaultAsync(new ArticleCoverSpec(id), cancellationToken);
        if (cover is not null)
            await covers.DeleteAsync(cover, cancellationToken);

        article.RemoveCover(clock.GetUtcNow());
        await articles.SaveChangesAsync(cancellationToken);
        return Result.Success(ArticleMapper.ToAdmin(article));
    }
}
