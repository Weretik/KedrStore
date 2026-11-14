using Content.Application.Abstractions.Integrations;
using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Queries.PreviewYoutubeVideo;

/// <summary>The title comes from YouTube when it answers; otherwise the manager types it.</summary>
public sealed class PreviewYoutubeVideoHandler(IReadContentDbContext db, IYoutubeVideoInfo youtube)
    : IQueryHandler<PreviewYoutubeVideoQuery, Result<YoutubePreview>>
{
    public async ValueTask<Result<YoutubePreview>> Handle(PreviewYoutubeVideoQuery query,
        CancellationToken cancellationToken)
    {
        var parsed = YoutubeVideoId.Parse(query.Url);
        if (parsed.IsFailure)
            return parsed.Error!.ToResult<YoutubePreview>();
        var youtubeId = parsed.Value;
        var exists = await db.GalleryItems.AnyAsync(item => item.YoutubeId == youtubeId, cancellationToken);
        var title = await youtube.GetTitleAsync(youtubeId, cancellationToken);
        return Result.Success(new YoutubePreview(youtubeId, title, YoutubeThumbnails.Url(youtubeId), exists));
    }
}
