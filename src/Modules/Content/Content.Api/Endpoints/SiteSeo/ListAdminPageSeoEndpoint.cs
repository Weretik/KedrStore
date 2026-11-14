using Content.Application.Features.SiteSeo.Queries.ListAdminPageSeo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.SiteSeo;

internal static class ListAdminPageSeoEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("listAdminPageSeo")
            .Produces<IReadOnlyList<AdminPageSeoResponse>>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListAdminPageSeoQuery(), cancellationToken);
        return result.Map(PageSeoResponseMapper.ToResponse).ToHttpResult(http);
    }
}
