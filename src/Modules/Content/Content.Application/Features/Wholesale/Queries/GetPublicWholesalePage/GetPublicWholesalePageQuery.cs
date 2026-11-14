using Content.Application.Features.Wholesale.Shared;

namespace Content.Application.Features.Wholesale.Queries.GetPublicWholesalePage;

/// <summary>The page «Співпраця» in one language, as the site renders it.</summary>
public sealed record GetPublicWholesalePageQuery(string Language) : IQuery<Result<PublicWholesalePage>>;
