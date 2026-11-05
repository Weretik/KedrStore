using Content.Application.Features.SiteSeo.Common;

namespace Content.Application.Features.SiteSeo.Admin.List;

/// <summary>Every site page in menu order, with its texts or empty values when none were saved.</summary>
public sealed record ListAdminPageSeoQuery : IQuery<Result<IReadOnlyList<AdminPageSeo>>>;
