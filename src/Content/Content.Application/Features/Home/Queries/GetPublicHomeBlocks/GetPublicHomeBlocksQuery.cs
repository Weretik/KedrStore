using Content.Application.Features.Home.Shared;

namespace Content.Application.Features.Home.Queries.GetPublicHomeBlocks;

public sealed record GetPublicHomeBlocksQuery(string Language) : IQuery<Result<PublicHomeBlocks>>;
