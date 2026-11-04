using Content.Application.Contracts.Gallery;
using Content.Application.Features.Gallery.Common;

namespace Content.Application.Features.Gallery.Admin.GetList;

public sealed class GetAdminGalleryQueryHandler(IReadContentDbContext db, IGalleryImageReader images)
    : IQueryHandler<GetAdminGalleryQuery, Result<IReadOnlyList<AdminGalleryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminGalleryItem>>> Handle(GetAdminGalleryQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await GalleryAdminList.LoadAsync(db, images, cancellationToken));
}
