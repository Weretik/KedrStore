using Content.Application.Contracts.Gallery;
using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.PreviewYoutube;

/// <summary>The title comes from YouTube when it answers; otherwise the manager types it.</summary>
public sealed class PreviewYoutubeVideoQueryHandler(IReadContentDbContext db, IYoutubeVideoInfo youtube)
    : IQueryHandler<PreviewYoutubeVideoQuery, Result<YoutubePreview>>
{
    public async ValueTask<Result<YoutubePreview>> Handle(PreviewYoutubeVideoQuery query,
        CancellationToken cancellationToken)
    {
        var youtubeId = YoutubeVideoId.Parse(query.Url);
        var exists = await db.GalleryItems.AnyAsync(item => item.YoutubeId == youtubeId, cancellationToken);
        var title = await youtube.GetTitleAsync(youtubeId, cancellationToken);
        return Result.Success(new YoutubePreview(youtubeId, title, YoutubeThumbnails.Url(youtubeId), exists));
    }
}
