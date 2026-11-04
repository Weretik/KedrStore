using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.GetBlocks;

public sealed class GetAdminHomeBlocksQueryHandler(IReadContentDbContext db, IMediaImageReader images)
    : IQueryHandler<GetAdminHomeBlocksQuery, Result<AdminHomeBlocks>>
{
    public async ValueTask<Result<AdminHomeBlocks>> Handle(GetAdminHomeBlocksQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
}
