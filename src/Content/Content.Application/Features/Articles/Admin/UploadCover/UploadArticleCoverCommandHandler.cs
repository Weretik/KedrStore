using Content.Application.Contracts.Images;
using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.UploadCover;

/// <summary>Converts the picture to a web-sized WebP and stores it as the article cover with a new version.</summary>
public sealed class UploadArticleCoverCommandHandler(
    IContentRepository<Article> articles,
    IContentRepository<ArticleCover> covers,
    ICoverImageProcessor processor,
    TimeProvider clock)
    : ICommandHandler<UploadArticleCoverCommand, Result<AdminArticleDetails>>
{
    public async ValueTask<Result<AdminArticleDetails>> Handle(UploadArticleCoverCommand command,
        CancellationToken cancellationToken)
    {
        var id = ArticleId.Create(command.ArticleId);
        var article = await articles.FirstOrDefaultAsync(new ArticleByIdSpec(id), cancellationToken);
        if (article is null)
            return Result.NotFound();

        var processed = await processor.ProcessAsync(command.File, cancellationToken);
        if (!processed.IsSuccess)
            return processed.Map(_ => ArticleMapper.ToAdmin(article));

        var cover = await covers.FirstOrDefaultAsync(new ArticleCoverSpec(id), cancellationToken);
        if (cover is null)
            await covers.AddAsync(ArticleCover.Create(id, processed.Value.Data, processed.Value.ContentType), cancellationToken);
        else
            cover.Replace(processed.Value.Data, processed.Value.ContentType);

        article.ReplaceCover(clock.GetUtcNow());
        await articles.SaveChangesAsync(cancellationToken);
        return Result.Success(ArticleMapper.ToAdmin(article));
    }
}
