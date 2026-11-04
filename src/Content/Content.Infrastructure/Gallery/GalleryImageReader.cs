using Content.Application.Contracts.Gallery;
using Media.Contracts.Files;

namespace Content.Infrastructure.Gallery;

/// <summary>Gallery photos come from the Media module through its published lookup, never its tables.</summary>
internal sealed class GalleryImageReader(IMediaFileLookup media) : IGalleryImageReader
{
    public async Task<IReadOnlyDictionary<long, GalleryImage>> GetImagesAsync(
        IReadOnlyCollection<long> mediaFileIds, CancellationToken cancellationToken)
    {
        var files = await media.GetFilesAsync(mediaFileIds, cancellationToken);
        return files.ToDictionary(pair => pair.Key,
            pair => new GalleryImage(pair.Value.Url, pair.Value.FileName, pair.Value.Width, pair.Value.Height));
    }
}
