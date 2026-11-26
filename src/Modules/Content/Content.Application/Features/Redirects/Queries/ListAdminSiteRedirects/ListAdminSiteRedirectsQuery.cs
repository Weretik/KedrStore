using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Queries.ListAdminSiteRedirects;

/// <summary>One page of redirects, newest change first; every word of <c>Search</c> must appear in an address.</summary>
public sealed record ListAdminSiteRedirectsQuery(string? Search, int Page = 1, int PageSize = 25)
    : IQuery<Result<AdminSiteRedirectPage>>;
