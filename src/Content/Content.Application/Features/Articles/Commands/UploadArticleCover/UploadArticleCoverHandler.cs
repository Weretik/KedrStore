using Content.Application.Abstractions.Media;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.UploadArticleCover;

/// <summary>
/// Converts the picture to a web-sized WebP and stores it on the CDN under <c>articles/</c>, in the library folder
/// «Статті». Nothing about an article changes until it is saved with the returned ID.
/// </summary>
public sealed class UploadArticleCoverHandler(ICoverImageProcessor processor, IMediaImageWriter writer)
    : ICommandHandler<UploadArticleCoverCommand, Result<UploadedArticleCover>>
{
    public const string StorageDirectory = "articles";
    public const string LibraryFolder = "Статті";

    public async ValueTask<Result<UploadedArticleCover>> Handle(UploadArticleCoverCommand command,
        CancellationToken cancellationToken)
    {
        var processed = await processor.ProcessAsync(command.File, cancellationToken);
        if (!processed.IsSuccess)
            return processed.Map(_ => default(UploadedArticleCover)!);

        var name = Path.GetFileNameWithoutExtension(command.File.FileName);
        var fileName = $"{(string.IsNullOrWhiteSpace(name) ? "cover" : name)}.webp";
        var stored = await writer.UploadAsync(
            new MediaImageUpload(StorageDirectory, LibraryFolder, fileName, processed.Value.Data), cancellationToken);
        return stored.Map(image => new UploadedArticleCover(image.MediaFileId, image.Url));
    }
}
