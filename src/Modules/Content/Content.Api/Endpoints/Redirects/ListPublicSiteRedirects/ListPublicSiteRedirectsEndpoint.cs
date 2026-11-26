using Content.Application.Features.Redirects.Queries.ListPublicSiteRedirects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Content.Api.Endpoints.Redirects.Shared;

namespace Content.Api.Endpoints.Redirects.ListPublicSiteRedirects;

internal static class ListPublicSiteRedirectsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("redirects", Handle)
            .WithName("listPublicSiteRedirects")
            .WithSummary("List every site redirect")
            .WithDescription("Public, because the site answers anonymous visitors with a 301; it keeps the list in memory for a few minutes. `from` is a decoded, lower-case path without a trailing slash; `to` is a site path or a full URL. At most 2000 entries.")
            .Produces<IReadOnlyList<PublicSiteRedirectResponse>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListPublicSiteRedirectsQuery(), cancellationToken);
        return result.Map(SiteRedirectResponseMapper.ToResponse).ToHttpResult(http);
    }
}
