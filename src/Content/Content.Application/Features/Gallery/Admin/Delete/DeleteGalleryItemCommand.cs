using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.Delete;

/// <summary>Removes a card from the gallery; the picture stays in the media library.</summary>
public sealed record DeleteGalleryItemCommand(long Id) : ICommand<Result<IReadOnlyList<AdminGalleryItem>>>;
