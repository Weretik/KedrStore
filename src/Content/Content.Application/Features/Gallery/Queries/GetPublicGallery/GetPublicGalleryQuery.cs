using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Queries.GetPublicGallery;

public sealed record GetPublicGalleryQuery(string Language) : IQuery<Result<IReadOnlyList<PublicGalleryItem>>>;
