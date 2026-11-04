using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Admin.GetBlocks;

public sealed record GetAdminHomeBlocksQuery : IQuery<Result<AdminHomeBlocks>>;
