using Content.Application.Features.SiteSeo.Shared;

namespace Content.Application.Features.SiteSeo.Queries.ListPublicPageSeo;

/// <summary>The editor-set search texts of the site pages in one language.</summary>
public sealed record ListPublicPageSeoQuery(string Language) : IQuery<Result<IReadOnlyList<PublicPageSeo>>>;
