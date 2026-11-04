using Content.Application.Features.Home.Common;

namespace Content.Application.Features.Home.Public.GetBlocks;

public sealed record GetPublicHomeBlocksQuery(string Language) : IQuery<Result<PublicHomeBlocks>>;
