using Content.Application.Features.Wholesale.Common;

namespace Content.Application.Features.Wholesale.Public.Get;

/// <summary>The page «Співпраця» in one language, as the site renders it.</summary>
public sealed record GetPublicWholesalePageQuery(string Language) : IQuery<Result<PublicWholesalePage>>;
