using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.Update;

public sealed record UpdateGalleryItemCommand(long Id, GalleryTextsInput Texts)
    : ICommand<Result<IReadOnlyList<AdminGalleryItem>>>;
