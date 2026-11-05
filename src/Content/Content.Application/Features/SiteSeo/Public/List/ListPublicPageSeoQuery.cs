using Content.Application.Features.SiteSeo.Common;

namespace Content.Application.Features.SiteSeo.Public.List;

/// <summary>The editor-set search texts of the site pages in one language.</summary>
public sealed record ListPublicPageSeoQuery(string Language) : IQuery<Result<IReadOnlyList<PublicPageSeo>>>;
