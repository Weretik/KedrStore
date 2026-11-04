using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.GetList;

public sealed record GetAdminGalleryQuery : IQuery<Result<IReadOnlyList<AdminGalleryItem>>>;
