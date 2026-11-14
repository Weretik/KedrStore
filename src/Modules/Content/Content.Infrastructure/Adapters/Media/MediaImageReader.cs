using Content.Application.Abstractions.Media;
using Media.Contracts.Files;

namespace Content.Infrastructure.Adapters.Media;

/// <summary>Gallery photos and brand logos come from the Media module through its published lookup, never its tables.</summary>
internal sealed class MediaImageReader(IMediaFileLookup media) : IMediaImageReader
{
    public async Task<IReadOnlyDictionary<long, MediaImage>> GetImagesAsync(
        IReadOnlyCollection<long> mediaFileIds, CancellationToken cancellationToken)
    {
        var files = await media.GetFilesAsync(mediaFileIds, cancellationToken);
        return files.ToDictionary(pair => pair.Key,
            pair => new MediaImage(pair.Value.Url, pair.Value.FileName, pair.Value.Width, pair.Value.Height));
    }
}
