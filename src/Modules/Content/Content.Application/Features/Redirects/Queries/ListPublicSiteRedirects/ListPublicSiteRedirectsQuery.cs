using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Queries.ListPublicSiteRedirects;

/// <summary>Every redirect for the site, which keeps them in memory and answers matching requests with a 301.</summary>
public sealed record ListPublicSiteRedirectsQuery : IQuery<Result<IReadOnlyList<PublicSiteRedirect>>>;
