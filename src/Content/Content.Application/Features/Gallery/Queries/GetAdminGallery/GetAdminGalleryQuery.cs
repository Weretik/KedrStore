using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Queries.GetAdminGallery;

public sealed record GetAdminGalleryQuery : IQuery<Result<IReadOnlyList<AdminGalleryItem>>>;
