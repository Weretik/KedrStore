using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Public.GetList;

public sealed record GetPublicGalleryQuery(string Language) : IQuery<Result<IReadOnlyList<PublicGalleryItem>>>;
