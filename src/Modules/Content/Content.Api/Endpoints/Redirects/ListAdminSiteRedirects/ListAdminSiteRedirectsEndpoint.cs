using Content.Application.Features.Redirects.Queries.ListAdminSiteRedirects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Content.Api.Endpoints.Redirects.Shared;

namespace Content.Api.Endpoints.Redirects.ListAdminSiteRedirects;

internal static class ListAdminSiteRedirectsEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("listAdminSiteRedirects")
            .WithSummary("List site redirects")
            .WithDescription("Requires `CanManageContent` (Manager, Seo, or Admin). Newest change first; every word of `search` must appear in the old or the new address. `pageSize` is 1–100.")
            .Produces<AdminSiteRedirectPageResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromQuery] string? search, [FromQuery] int? page, [FromQuery] int? pageSize, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListAdminSiteRedirectsQuery(search, page ?? 1, pageSize ?? 25),
            cancellationToken);
        return result.Map(SiteRedirectResponseMapper.ToResponse).ToHttpResult(http);
    }
}
