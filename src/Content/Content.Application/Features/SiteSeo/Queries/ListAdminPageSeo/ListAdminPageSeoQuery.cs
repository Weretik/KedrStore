using Content.Application.Features.SiteSeo.Shared;

namespace Content.Application.Features.SiteSeo.Queries.ListAdminPageSeo;

/// <summary>Every site page in menu order, with its texts or empty values when none were saved.</summary>
public sealed record ListAdminPageSeoQuery : IQuery<Result<IReadOnlyList<AdminPageSeo>>>;
