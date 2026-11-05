using Content.Application.Features.Wholesale.Common;

namespace Content.Application.Features.Wholesale.Admin.Get;

/// <summary>Both languages of the page «Співпраця» with who saved the last change.</summary>
public sealed record GetAdminWholesalePageQuery : IQuery<Result<AdminWholesalePage>>;
