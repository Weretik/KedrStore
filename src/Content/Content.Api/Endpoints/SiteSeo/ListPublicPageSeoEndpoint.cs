using Content.Application.Features.SiteSeo.Queries.ListPublicPageSeo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.SiteSeo;

internal static class ListPublicPageSeoEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("{lang}/page-seo", Handle)
            .WithName("listPublicPageSeo")
            .Produces<IReadOnlyList<PublicPageSeoResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<IResult> Handle(
        [FromRoute] string lang, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListPublicPageSeoQuery(lang), cancellationToken);
        return result.Map(PageSeoResponseMapper.ToResponse).ToHttpResult(http);
    }
}
