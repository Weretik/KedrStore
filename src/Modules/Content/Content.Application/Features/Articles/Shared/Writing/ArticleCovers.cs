using Content.Application.Abstractions.Media;
using BuildingBlocks.Domain.Abstractions.Errors;
using Content.Application.Features.Articles.Shared.Reading;
namespace Content.Application.Features.Articles.Shared.Writing;

/// <summary>Reads cover URLs from the media library and applies the cover part of the admin input.</summary>
internal static class ArticleCovers
{
    /// <summary>CDN directory and media-library folder of article covers.</summary>
    public const string StorageDirectory = "articles";
    public const string LibraryFolder = "Статті";

    public static async Task<IReadOnlyDictionary<long, MediaImage>> ImagesAsync(IMediaImageReader images,
        IEnumerable<long?> mediaFileIds, CancellationToken cancellationToken)
    {
        var ids = mediaFileIds.OfType<long>().Distinct().ToList();
        return ids.Count == 0
            ? new Dictionary<long, MediaImage>()
            : await images.GetImagesAsync(ids, cancellationToken);
    }

    /// <summary>A deleted library file leaves the article without a cover instead of a broken picture.</summary>
    public static ArticleCoverView View(long? mediaFileId, int? legacyVersion,
        IReadOnlyDictionary<long, MediaImage> images)
    {
        if (mediaFileId is { } id)
            return images.TryGetValue(id, out var image) ? new(id, image.Url, null) : new(id, null, null);
        return legacyVersion is { } version ? new(null, null, version) : ArticleCoverView.None;
    }

    public static async Task<ArticleCoverView> ViewAsync(IMediaImageReader images, Article article,
        CancellationToken cancellationToken)
        => View(article.CoverMediaFileId, article.CoverVersion,
            await ImagesAsync(images, [article.CoverMediaFileId], cancellationToken));

    /// <summary>
    /// Sets or removes the cover by the input; a legacy cover row is deleted once it is no longer used.
    /// 400 when the chosen picture is not in the media library.
    /// </summary>
    public static async Task<DomainError?> ApplyAsync(Article article, ArticleInput input, IMediaImageReader images,
        IContentRepository<ArticleCover> covers, DateTimeOffset now, string editor, CancellationToken cancellationToken)
    {
        if (input.CoverMediaFileId is { } mediaFileId)
        {
            if (mediaFileId != article.CoverMediaFileId)
            {
                var found = await images.GetImagesAsync([mediaFileId], cancellationToken);
                if (!found.ContainsKey(mediaFileId))
                    return new DomainError("Content.Article.Cover.NotFound", "Цього зображення вже немає в медіатеці.",
                        Field: "coverMediaFileId");
            }
            var cover = article.SetCover(mediaFileId, now, editor);
            if (cover.IsFailure)
                return cover.Error;
        }
        else if (input.RemoveCover)
        {
            article.RemoveCover(now, editor);
        }
        else
        {
            return null;
        }

        var legacy = await covers.FirstOrDefaultAsync(new ArticleCoverSpec(article.Id), cancellationToken);
        if (legacy is not null)
            await covers.DeleteAsync(legacy, cancellationToken);
        return null;
    }
}
