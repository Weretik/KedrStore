using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.AddGalleryVideo;

/// <summary>Adds a YouTube video by a pasted link (or bare ID) to the end of the gallery.</summary>
public sealed record AddGalleryVideoCommand(string Url, GalleryTextsInput? Texts)
    : ICommand<Result<IReadOnlyList<AdminGalleryItem>>>;
