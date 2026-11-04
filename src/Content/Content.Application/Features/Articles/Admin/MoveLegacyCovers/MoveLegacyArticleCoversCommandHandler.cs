using BuildingBlocks.Application.Contracts;
using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Articles.Admin.UploadCover;
using Content.Application.Features.Articles.Common;
using Content.Application.Features.Editing;

namespace Content.Application.Features.Articles.Admin.MoveLegacyCovers;

/// <summary>
/// Uploads each legacy cover as <c>&lt;slug&gt;.webp</c>, sets it as the article cover, and deletes the database bytes,
/// one article at a time, so a failure keeps what was already moved. Stops at the first failed upload (503 without
/// CDN credentials); an article whose bytes are missing just loses the legacy version.
/// </summary>
public sealed class MoveLegacyArticleCoversCommandHandler(
    IContentRepository<Article> articles,
    IContentRepository<ArticleCover> covers,
    IMediaImageWriter writer,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<MoveLegacyArticleCoversCommand, Result<LegacyArticleCoversMoved>>
{
    public async ValueTask<Result<LegacyArticleCoversMoved>> Handle(MoveLegacyArticleCoversCommand command,
        CancellationToken cancellationToken)
    {
        var legacy = await articles.ListAsync(new ArticlesWithLegacyCoverSpec(), cancellationToken);
        var editor = ContentEditor.Name(currentUser);
        var moved = 0;
        foreach (var article in legacy)
        {
            var cover = await covers.FirstOrDefaultAsync(new ArticleCoverSpec(article.Id), cancellationToken);
            if (cover is null)
            {
                article.RemoveCover(clock.GetUtcNow(), editor);
                await articles.SaveChangesAsync(cancellationToken);
                continue;
            }

            var stored = await writer.UploadAsync(new MediaImageUpload(UploadArticleCoverCommandHandler.StorageDirectory,
                UploadArticleCoverCommandHandler.LibraryFolder, $"{article.Slug}.webp", cover.Data), cancellationToken);
            if (!stored.IsSuccess)
                return stored.Map(_ => default(LegacyArticleCoversMoved)!);

            article.SetCover(stored.Value.MediaFileId, clock.GetUtcNow(), editor);
            await covers.DeleteAsync(cover, cancellationToken);
            await articles.SaveChangesAsync(cancellationToken);
            moved++;
        }

        return Result.Success(new LegacyArticleCoversMoved(moved));
    }
}
