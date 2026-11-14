using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.UpdateGalleryItem;

public sealed record UpdateGalleryItemCommand(long Id, GalleryTextsInput Texts)
    : ICommand<Result<IReadOnlyList<AdminGalleryItem>>>;
