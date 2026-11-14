using Content.Application.Features.Wholesale.Shared;

namespace Content.Application.Features.Wholesale.Queries.GetAdminWholesalePage;

/// <summary>Both languages of the page «Співпраця» with who saved the last change.</summary>
public sealed record GetAdminWholesalePageQuery : IQuery<Result<AdminWholesalePage>>;
