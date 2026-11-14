using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Queries.GetAdminHomeBlocks;

public sealed record GetAdminHomeBlocksQuery : IQuery<Result<AdminHomeBlocks>>;
