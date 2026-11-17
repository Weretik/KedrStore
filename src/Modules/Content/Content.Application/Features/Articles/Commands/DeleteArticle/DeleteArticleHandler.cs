using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Reading;

namespace Content.Application.Features.Articles.Commands.DeleteArticle;

public sealed class DeleteArticleHandler(
    IContentRepository<Article> articles)
    : ICommandHandler<DeleteArticleCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteArticleCommand command, CancellationToken cancellationToken)
    {
        var id = ArticleId.Create(command.ArticleId);
        var article = await articles.FirstOrDefaultAsync(new ArticleByIdSpec(id), cancellationToken);
        if (article is null)
            return Result.NotFound();

        // The legacy cover row goes with the article (cascade on the foreign key).
        await articles.DeleteAsync(article, cancellationToken);
        await articles.SaveChangesAsync(cancellationToken);
        return Result.NoContent();
    }
}
