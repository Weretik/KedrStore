using Content.Application.Abstractions.Media;
using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Queries.GetAdminGallery;

public sealed class GetAdminGalleryHandler(IReadContentDbContext db, IMediaImageReader images)
    : IQueryHandler<GetAdminGalleryQuery, Result<IReadOnlyList<AdminGalleryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminGalleryItem>>> Handle(GetAdminGalleryQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await GalleryAdminList.LoadAsync(db, images, cancellationToken));
}
