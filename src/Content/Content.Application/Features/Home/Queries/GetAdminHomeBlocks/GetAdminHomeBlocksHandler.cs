using Content.Application.Abstractions.Media;
using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Queries.GetAdminHomeBlocks;

public sealed class GetAdminHomeBlocksHandler(IReadContentDbContext db, IMediaImageReader images)
    : IQueryHandler<GetAdminHomeBlocksQuery, Result<AdminHomeBlocks>>
{
    public async ValueTask<Result<AdminHomeBlocks>> Handle(GetAdminHomeBlocksQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await HomeAdminBlocks.LoadAsync(db, images, cancellationToken));
}
