using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.DeleteGalleryItem;

/// <summary>Removes a card from the gallery; the picture stays in the media library.</summary>
public sealed record DeleteGalleryItemCommand(long Id) : ICommand<Result<IReadOnlyList<AdminGalleryItem>>>;
