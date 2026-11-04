using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.AddVideo;

/// <summary>Adds a YouTube video by a pasted link (or bare ID) to the end of the gallery.</summary>
public sealed record AddGalleryVideoCommand(string Url, GalleryTextsInput? Texts)
    : ICommand<Result<IReadOnlyList<AdminGalleryItem>>>;
