using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.DeleteArticle;

public sealed class DeleteArticleHandler(
    IContentRepository<Article> articles,
    IContentRepository<ArticleCover> covers)
    : ICommandHandler<DeleteArticleCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteArticleCommand command, CancellationToken cancellationToken)
    {
        var id = ArticleId.Create(command.ArticleId);
        var article = await articles.FirstOrDefaultAsync(new ArticleByIdSpec(id), cancellationToken);
        if (article is null)
            return Result.NotFound();

        var cover = await covers.FirstOrDefaultAsync(new ArticleCoverSpec(id), cancellationToken);
        if (cover is not null)
            await covers.DeleteAsync(cover, cancellationToken);
        await articles.DeleteAsync(article, cancellationToken);
        return Result.NoContent();
    }
}
