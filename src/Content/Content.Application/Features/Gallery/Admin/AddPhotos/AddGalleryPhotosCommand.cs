using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.AddPhotos;

/// <summary>Adds library pictures to the end of the gallery in the given order.</summary>
public sealed record AddGalleryPhotosCommand(IReadOnlyList<long> MediaFileIds)
    : ICommand<Result<IReadOnlyList<AdminGalleryItem>>>;
