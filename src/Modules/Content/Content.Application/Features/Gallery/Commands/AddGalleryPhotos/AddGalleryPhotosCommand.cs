using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.AddGalleryPhotos;

/// <summary>Adds library pictures to the end of the gallery in the given order.</summary>
public sealed record AddGalleryPhotosCommand(IReadOnlyList<long> MediaFileIds)
    : ICommand<Result<IReadOnlyList<AdminGalleryItem>>>;
